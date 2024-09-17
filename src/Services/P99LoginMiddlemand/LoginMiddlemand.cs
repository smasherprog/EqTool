using EQTool.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace EQTool.Services.P99LoginMiddlemand
{
    public class Packet
    {
        public bool IsFragment;
        public int Length;
        public byte[] Data;
    }

    public class Sequence
    {
        public Packet[] Packets;
        public int Capacity;
        public int Count;
        public int FragStart;
        public int FragCount;
        public short SeqToLocal;    // What the client thinks the next sequence from the login server should be
        public short SeqFromRemote; // The next "real" sequence we expect from the login server
    }
    public struct FirstFrag
    {
        public ushort protocolOpcode;
        public ushort sequence;
        public uint totalLen;
        public ushort appOpcode;
    }
    public struct Frag
    {
        public short protocolOpcode;
        public short sequence;
    }

    public class Connection : IDisposable
    {
        public Socket Socket;
        public bool InSession;
        public DateTime LastRecvTime;
        public IPEndPoint LocalAddr;
        public IPEndPoint RemoteAddr;
        public byte[] Buffer = new byte[2048];
        public Sequence Sequence;
        private const string SERVER_NAME_PREFIX = "Project 1999";
        private const int SizeOfFirstFrag = 10;
        private const int SizeOfFrag = 4;

        public Connection()
        {
            sequence_init();
        }

        public void Dispose()
        {
            try
            {
                Socket?.Close();
            }
            catch (Exception e)
            {
                Debug.WriteLine(e);
            }

            Socket = null;
        }
        public void sequence_init()
        {
            Sequence = new Sequence
            {
                Packets = new Packet[0],
                Capacity = 0,
                Count = 0,
                FragStart = 0,
                FragCount = 0,
                SeqToLocal = 0,
                SeqFromRemote = 0
            };
        }

        private void sequence_free()
        {
            sequence_init();
        }

        public void Open(int port)
        {
            // Set up socket
            Socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);

            // Set up local endpoint
            LocalAddr = new IPEndPoint(IPAddress.Any, port);

            // Bind the socket
            Socket.Bind(LocalAddr);

            // Resolve the remote server's address
            var remoteHostEntry = Dns.GetHostEntry("login.eqemulator.net");
            var remoteIPAddress = remoteHostEntry.AddressList[0];
            RemoteAddr = new IPEndPoint(remoteIPAddress, 5998);

            InSession = false;
            LastRecvTime = DateTime.MinValue;
        }

        private void sequence_adjust_combined(int len, int startindex)
        {
            var pos = 2 + startindex;
            if (len < 4)
            {
                return;
            }

            while (true)
            {
                var sublen = Buffer[pos];
                pos++;
                if ((pos + sublen) > len || sublen == 0)
                {
                    return;
                }

                if (IPAddress.NetworkToHostOrder(BitConverter.ToInt16(Buffer, pos)) == 0x15)
                {
                    sequence_adjust_ack(Buffer, pos, sublen);
                }

                pos += sublen;
                if (pos >= len)
                {
                    return;
                }
            }
        }

        private static void debug_write_packet(byte[] buf, int startIndex, int len, bool loginToClient)
        {
            var newbuff = new byte[len];
            Array.Copy(buf, startIndex, newbuff, 0, len);
            debug_write_packet(buf, len, loginToClient);
        }

        private static void debug_write_packet(byte[] buf, int len, bool loginToClient)
        {
            var i = 0;
            Debug.Write($"{DateTime.Now.Ticks} ");
            if (loginToClient)
            {
                Debug.WriteLine($"LOGIN to CLIENT (len {len}):");
            }
            else
            {
                Debug.WriteLine($"CLIENT to LOGIN (len {len}):");
            }

            int k;
            while ((i + 64) < len)
            {
                var j = i;
                i += 64;
                for (k = j; k < i; k++)
                {
                    Debug.Write($"{buf[k]:X2} ");
                }

                Debug.Write("  ");
                for (k = j; k < i; k++)
                {
                    Debug.Write(char.IsLetterOrDigit((char)buf[k]) ? (char)buf[k] : '.');
                }

                Debug.WriteLine("");
            }

            if (i == len)
            {
                return;
            }

            for (k = i; k < len; k++)
            {
                Debug.Write($"{buf[k]:X2} ");
            }

            Debug.Write("  ");
            var padding = 16 - (len % 16);
            if (padding == 16)
            {
                padding = 0;
            }

            for (k = 0; k < padding; k++)
            {
                Debug.Write("   ");
            }

            for (k = i; k < len; k++)
            {
                Debug.Write(char.IsLetterOrDigit((char)buf[k]) ? (char)buf[k] : '.');
            }
            Debug.WriteLine("");
        }

        private void connection_send(byte[] data, int startIndex, int len, bool toRemote)
        {
            EndPoint addr = toRemote ? RemoteAddr : LocalAddr;

#if DEBUG
            debug_write_packet(data, startIndex, len, !toRemote);
#endif

            _ = Socket.SendTo(data, startIndex, len, SocketFlags.None, addr);
        }

        private void sequence_adjust_ack(byte[] data, int startindex, int len)
        {
            if (len < 4)
            {
                return;
            }

            BitConverter.GetBytes(IPAddress.HostToNetworkOrder((short)(Sequence.SeqFromRemote - 1))).CopyTo(data, 2 + startindex);
        }

        private void recv_from_local(int len)
        {
            switch (get_protocol_opcode(Buffer, 0))
            {
                case 0x03: /* OP_Combined */
                    sequence_adjust_combined(len, 0);
                    break;
                case 0x05: /* OP_SessionDisconnect */
                    InSession = false;
                    sequence_free();
                    break;
                case 0x15: /* OP_Ack */
                    /* Rewrite client-to-server ack sequence values, since we will be desynchronizing them */
                    sequence_adjust_ack(Buffer, 0, len);
                    break;
                default:
                    break;
            }

            /* Forward everything */
            connection_send(Buffer, 0, len, true);
        }

        public bool connection_read()
        {
            EndPoint addr = new IPEndPoint(IPAddress.Any, 0);
            var len = Socket.ReceiveFrom(Buffer, ref addr);

            if (len < 2)
            {
                return true;
            }

            var recvTime = DateTime.Now;

            // Check if packet is from remote server
            if (addr.Equals(RemoteAddr))
            {
                recv_from_remote(Buffer, 0, len);
            }
            else
            {
                // If not from remote server and not in session or session timed out, reset
                if (!InSession || (recvTime - LastRecvTime).TotalSeconds > 60)
                {
                    LocalAddr = (IPEndPoint)addr;
                    sequence_free();
                }

                recv_from_local(len);
            }

            LastRecvTime = recvTime;
            return true;
        }

        private short get_protocol_opcode(byte[] data, int startIndex)
        {
            var opcode = BitConverter.ToInt16(data, startIndex);
            var val = IPAddress.NetworkToHostOrder(opcode);
            Debug.WriteLine($"get_protocol_opcode: {val}");
            return val;
        }

        private short get_sequence(byte[] data, int startIndex)
        {
            return IPAddress.NetworkToHostOrder(BitConverter.ToInt16(data, startIndex + 2));
        }

        private void copy_fragment(Packet p, byte[] data, int startIndex, int len)
        {
            var copy = new byte[len];
            Array.Copy(data, startIndex, copy, 0, len);
            p.Data = copy;
        }

        private void sequence_recv_fragment(byte[] data, int startIndex, int len)
        {
            var val = get_sequence(data, startIndex);
            var p = get_packet_space(val, len);

            p.IsFragment = true;
            copy_fragment(p, data, startIndex, len);

            if (val == Sequence.SeqFromRemote)
            {
                _ = process_first_fragment(data);
            }
            else if (Sequence.FragCount > 0)
            {
                check_fragment_finished();
            }
        }

        private void recv_from_remote(byte[] buffer, int startIndex, int len)
        {
            switch (get_protocol_opcode(buffer, startIndex))
            {
                case 0x02: /* OP_SessionResponse */
                    InSession = true;
                    sequence_free();
                    break;
                case 0x03: /* OP_Combined */
                    sequence_recv_combined(buffer, startIndex, len);
                    return; /* Pieces will be forwarded individually */
                case 0x09: /* OP_Packet */
                    sequence_recv_packet(buffer, startIndex, len);
                    break;
                case 0x0d: /* OP_Fragment -- must be one of the server list packets */
                    sequence_recv_fragment(buffer, startIndex, len);
                    return; /* Don't forward, whole point is to filter this */
                default:
                    break;
            }

            /* Forward anything that isn't a fragment */
            connection_send(buffer, startIndex, len, false);
        }

        private void grow(int index)
        {
            var seq = Sequence;
            var cap = 32;

            while (cap <= index)
            {
                cap *= 2;
            }

            var array = new Packet[cap];
            for (var i = 0; i < cap; i++)
            {
                array[i] = new Packet();
            }
            if (seq.Capacity != 0)
            {
                Array.Copy(seq.Packets, array, seq.Capacity);
            }

            seq.Capacity = cap;
            seq.Packets = array;
        }

        public Packet get_packet_space(short sequence, int len)
        {
            var seq = Sequence;
            if (sequence >= seq.Count)
            {
                seq.Count = sequence + 1;
            }

            if (sequence >= seq.Capacity)
            {
                grow(sequence + 1);
            }

            var p = seq.Packets[sequence];
            p.Length = len;
            p.Data = null;

            return p;
        }


        public bool process_first_fragment(byte[] data)
        {
            var seq = Sequence;
            var frag = new
            {
                totalLen = BitConverter.ToUInt32(data, 4),
                appOpcode = BitConverter.ToUInt16(data, 8)
            };

            if (frag.appOpcode != 0x18) // OP_ServerListResponse
            {
                return false;
            }

            seq.FragStart = get_sequence(data, 0);
            seq.FragCount = ((IPAddress.NetworkToHostOrder((int)frag.totalLen) - (512 - 8)) / (512 - 4)) + 2;
            return true;
        }

        private void check_fragment_finished()
        {
            var seq = Sequence;
            var index = seq.FragStart;
            var n = seq.FragCount;
            var count = 1;

            var p = seq.Packets[index];
            var got = p.Length - SizeOfFirstFrag + 2; /* AppOpcode is counted */

            while (count < n)
            {
                index++;
                if (index >= seq.Count)
                {
                    return;
                }

                p = seq.Packets[index];
                if (p.Data == null)
                {
                    return;
                }

                got += p.Length - SizeOfFrag;
                count++;
            }

            filter_server_list();
        }

        private void sequence_recv_packet(byte[] buffer, int startIndex, int len)
        {
            var val = get_sequence(buffer, startIndex);
            var p = get_packet_space(val, len);

            p.IsFragment = false;

            /* Correct the sequence for the client */
            BitConverter.GetBytes(IPAddress.HostToNetworkOrder(Sequence.SeqToLocal++)).CopyTo(buffer, 2 + startIndex);

            if (val != Sequence.SeqFromRemote)
            {
                return;
            }

            for (var i = val; i < Sequence.Count; i++)
            {
                if (Sequence.Packets[i].Length > 0)
                {
                    Sequence.SeqFromRemote++;

                    if (Sequence.Packets[i].IsFragment && process_first_fragment(Sequence.Packets[i].Data))
                    {
                        check_fragment_finished();
                        break;
                    }
                }
            }
        }

        private static int strlen(byte[] array, int offset)
        {
            if (array == null || array.Length == 0 || offset >= array.Length)
            {
                return 0;
            }

            var startoffset = offset;
            while (array[offset] != 0)
            {
                offset++;
            }

            return offset - startoffset;
        }

        private void filter_server_list()
        {
            var index = Sequence.FragStart;
            var p = Sequence.Packets[index];
            if (p.Length == 0)
            {
                return;
            }

            var packetstosend = Sequence.Packets.Skip(index)
                .Take(Sequence.FragCount)
                .OrderBy(a => IPAddress.NetworkToHostOrder(BitConverter.ToInt16(a.Data, 2)))
                .ToList();

            foreach (var packet in packetstosend)
            {
                Sequence.SeqToLocal++;
                connection_send(packet.Data, 0, packet.Length, false);
            }

            Sequence.SeqFromRemote = (short)(Sequence.FragStart + Sequence.FragCount + 1);
            Sequence.FragCount = 0;
            Sequence.FragStart = 0;
        }

        private void sequence_recv_combined(byte[] buffer, int startIndex, int len)
        {
            int sublen;
            var pos = 2 + startIndex;

            if (len < 4)
            {
                return;
            }

            for (; ; )
            {
                sublen = buffer[pos];
                pos++;
                if ((pos + sublen) > len || sublen == 0)
                {
                    return;
                }

                recv_from_remote(buffer, pos, sublen);

                pos += sublen;
                if (pos >= len)
                {
                    return;
                }
            }
        }
    }

    public class LoginMiddlemand : IDisposable
    {
        private Connection connection;
        private Thread thread;
        public bool Running = false;
        private readonly object connectionLock = new object();
        private readonly EQToolSettings toolSettings;
        private readonly LoggingService loggingService;
        private int Port = 5998;
        public LoginMiddlemand(EQToolSettings toolSettings, LoggingService loggingService)
        {
            this.toolSettings = toolSettings;
            this.loggingService = loggingService;
        }

        public void Dispose()
        {
            StopListening();
        }

        public void StopListening()
        {
            try
            {
                Running = false;
                connection?.Dispose();
                thread?.Join();
                thread = null;
                connection = null;
            }
            catch (Exception ex)
            {
                loggingService.Log(ex.ToString(), EQToolShared.Enums.EventType.LoginMiddleMand, null);
                Debug.WriteLine(ex);
            }
        }

        public void StartListening()
        {
            try
            {
                lock (connectionLock)
                {
                    if (Running)
                    {
                        return;
                    }

                    Running = true;
                }
                connection?.Dispose();
                thread?.Join();
                connection = new Connection();
                thread = new Thread(ListenForConnections);
                thread.Start();
            }
            catch (Exception ex)
            {
                loggingService.Log(ex.ToString(), EQToolShared.Enums.EventType.LoginMiddleMand, null);
                StopListening();
                Debug.WriteLine(ex);
            }
        }

        public bool IsConfiguredCorrectly()
        {
            return GetPort(toolSettings.DefaultEqDirectory).HasValue;
        }

        public void ApplyDefaultConfiguration()
        {
            var lines = new List<string>
            {
                "[LoginServer]",
                "Host=localhost:5998"
            };
            System.IO.File.WriteAllLines(toolSettings.DefaultEqDirectory + "/eqhost.txt", lines);
        }

        public void RevertDefaultConfiguration()
        {
            var lines = new List<string>
            {
                "[LoginServer]",
                "Host=login.eqemulator.net:5998"
            };
            System.IO.File.WriteAllLines(toolSettings.DefaultEqDirectory + "/eqhost.txt", lines);
        }


        private static int? GetPort(string eqdirectory)
        {
            try
            {
                var lines = System.IO.File.ReadAllLines(eqdirectory + "/eqhost.txt");
                var hostline = lines.FirstOrDefault(x => x.StartsWith("host", StringComparison.OrdinalIgnoreCase));
                if (!string.IsNullOrWhiteSpace(hostline))
                {
                    if (hostline.IndexOf("=localhost", StringComparison.OrdinalIgnoreCase) == -1)
                    {
                        return null;
                    }
                    var charindex = hostline.LastIndexOf(':');
                    if (charindex != -1)
                    {
                        return int.Parse(hostline.Substring(charindex + 1));
                    }
                }
            }
            catch (Exception e)
            {
                Debug.WriteLine(e.Message);
            }
            return null;
        }

        private bool Listen()
        {
            try
            {
                var port = GetPort(toolSettings.DefaultEqDirectory);
                if (port == null)
                {
                    Running = false;
                    return false;
                }
                Port = port.Value;
                connection.Open(Port);
            }
            catch (Exception e)
            {
                loggingService.Log(e.ToString(), EQToolShared.Enums.EventType.LoginMiddleMand, null);
                Debug.WriteLine(e.Message);
            }
            return true;
        }

        private void ListenForConnections()
        {
            if (!Listen())
            {
                return;
            }
            Debug.WriteLine("Starting ListenForConnections");
            try
            {
                var innerkeeprunning = true;
                while (innerkeeprunning)
                {
                    try
                    {
                        innerkeeprunning = connection.connection_read();
                    }
                    catch (Exception e)
                    {
                        connection.Dispose();
                        connection = new Connection();
                        if (!Listen())
                        {
                            Debug.WriteLine("Exit ListenForConnections");
                            return;
                        }
                        innerkeeprunning = true;
                        Thread.Sleep(1000);
                        Debug.WriteLine(e.Message);
                    }
                    if (!Running)
                    {
                        Debug.WriteLine("Exit ListenForConnections");
                        return;
                    }
                    else if (!innerkeeprunning)
                    {
                        connection.Dispose();
                        connection = new Connection();
                        if (!Listen())
                        {
                            Debug.WriteLine("Exit ListenForConnections");
                            return;
                        }
                        innerkeeprunning = true;
                        Thread.Sleep(1000);
                    }
                }
            }
            catch (Exception e)
            {
                loggingService.Log(e.ToString(), EQToolShared.Enums.EventType.LoginMiddleMand, null);
                Debug.WriteLine(e.Message);
            }
            Debug.WriteLine("Exit ListenForConnections");
        }
    }
}
