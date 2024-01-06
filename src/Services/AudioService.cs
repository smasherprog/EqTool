using EQTool.ViewModels;
using System;
using System.IO;
using System.Linq;
using System.Windows.Media;

namespace EQTool.Services
{
    public class AudioService
    {
        private readonly LogParser logParser;
        private readonly ActivePlayer activePlayer;

        public AudioService(LogParser logParser, ActivePlayer activePlayer)
        {
            this.logParser = logParser;
            this.activePlayer = activePlayer;
            this.logParser.InvisEvent += LogParser_InvisEvent;
            this.logParser.EnrageEvent += LogParser_EnrageEvent;
            this.logParser.LevEvent += LogParser_LevEvent;
        }

        private void LogParser_LevEvent(object sender, LevParser.LevStatus e)
        {
            if (this.activePlayer?.Player?.LevFadingAudio == true)
            {
                this.PlayResource("levfading");
            }
        }

        private void PlayResource(string resourcename)
        {
            var audiodir = System.IO.Directory.GetCurrentDirectory() + "/audio";
            if (!System.IO.Directory.Exists(audiodir))
            {
                try
                {
                    _ = Directory.CreateDirectory(audiodir);
                }
                catch { }
            }
            var version = App.Version.Replace(".", string.Empty).Trim();
            if (!File.Exists($"{audiodir}/{resourcename}{version}.mp3"))
            {
                try
                {
                    var filetodelete = Directory.GetFiles(audiodir, $"{resourcename}*", SearchOption.TopDirectoryOnly).FirstOrDefault();
                    File.Delete(filetodelete);
                }
                catch (Exception)
                {

                }
                audiodir = $"{audiodir}/{resourcename}{version}.mp3";
                using (var stream = System.Reflection.Assembly.GetExecutingAssembly().GetManifestResourceStream("EQTool.audio." + resourcename + ".mp3"))
                {
                    using (var fileStream = new FileStream(audiodir, FileMode.Create, FileAccess.Write))
                    {
                        stream.CopyTo(fileStream);
                    }
                }
            }
            else
            {
                audiodir = $"{audiodir}/{resourcename}{version}.mp3";
            }

            System.Threading.Tasks.Task.Factory.StartNew(() =>
            {
                var player = new MediaPlayer();
                player.Open(new Uri(audiodir));
                player.Play();
            });
        }

        private void LogParser_EnrageEvent(object sender, EnrageParser.EnrageEvent e)
        {
            if (this.activePlayer?.Player?.EnrageAudio == true)
            {
                this.PlayResource("enraged");
            }
        }

        private void LogParser_InvisEvent(object sender, InvisParser.InvisStatus e)
        {
            if (this.activePlayer?.Player?.InvisFadingAudio == true)
            {
                this.PlayResource("invisfading");
            }
        }
    }
}
