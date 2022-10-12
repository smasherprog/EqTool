using EQTool.Models;
using EQTool.Services.WLD.Fragments;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace EQTool.Services.WLD
{
    public class WLDReader
    {
        public const int WldFileIdentifier = 0x54503D02;
        public const int WldFormatOldIdentifier = 0x00015500;
        public const int WldFormatNewIdentifier = 0x1000C800;

        /// <summary>
        /// A link of indices to fragments
        /// </summary>
        protected List<WldFragment> _fragments;

        /// <summary>
        /// The string has containing the index in the hash and the decoded string that is there
        /// </summary>
        private Dictionary<int, string> _stringHash;

        /// <summary>
        /// A collection of fragment lists that can be referenced by a fragment type
        /// </summary>
        //protected Dictionary<FragmentType, List<WldFragment>> _fragmentTypeDictionary;
        protected Dictionary<Type, List<WldFragment>> _fragmentTypeDictionary;

        /// <summary>
        /// A collection of fragment lists that can be referenced by a fragment type
        /// </summary>
        protected Dictionary<string, WldFragment> _fragmentNameDictionary;

        /// <summary>
        /// The shortname of the zone this WLD is from
        /// </summary>
        protected readonly string _zoneName;

        private bool _isNewWldFormat;

        public void Read(SubS3bFile subS3BFile)
        {
            _fragments = new List<WldFragment>();
            _fragmentTypeDictionary = new Dictionary<Type, List<WldFragment>>();
            _fragmentNameDictionary = new Dictionary<string, WldFragment>();

            var reader = new BinaryReader(new MemoryStream(subS3BFile.Bytes));

            var writeBytes = reader.ReadBytes(subS3BFile.Bytes.Length);
            reader.BaseStream.Position = 0;
            var writer = new BinaryWriter(new MemoryStream(writeBytes));
            var identifier = reader.ReadInt32();
            if (identifier != WldFileIdentifier)
            {
                return;
            }

            var version = reader.ReadInt32();

            switch (version)
            {
                case WldFormatOldIdentifier:
                    break;
                case WldFormatNewIdentifier:
                    _isNewWldFormat = true;
                    break;
                default:
                    return;
            }

            var fragmentCount = reader.ReadUInt32();
            var bspRegionCount = reader.ReadUInt32();
            // Should contain 0x000680D4
            var unknown = reader.ReadInt32();
            var stringHashSize = reader.ReadUInt32();
            var unknown2 = reader.ReadInt32();

            var stringHash = reader.ReadBytes((int)stringHashSize);


            ParseStringHash(WldStringDecoder.DecodeString(stringHash));

            for (var i = 0; i < fragmentCount; ++i)
            {
                var fragSize = reader.ReadUInt32();
                var fragId = reader.ReadInt32();

                var newFragment = !WldFragmentBuilder.Fragments.ContainsKey(fragId)
                    ? new UnknownWldFragment()
                    : WldFragmentBuilder.Fragments[fragId]();

                newFragment.Initialize(i, (int)fragSize, reader.ReadBytes((int)fragSize), _fragments, _stringHash, _isNewWldFormat);
                _fragments.Add(newFragment);

                if (!_fragmentTypeDictionary.ContainsKey(newFragment.GetType()))
                {
                    _fragmentTypeDictionary[newFragment.GetType()] = new List<WldFragment>();
                }

                if (!string.IsNullOrEmpty(newFragment.Name) && !_fragmentNameDictionary.ContainsKey(newFragment.Name))
                {
                    _fragmentNameDictionary[newFragment.Name] = newFragment;
                }

                _fragmentTypeDictionary[newFragment.GetType()].Add(newFragment);
            }
        }

        public List<T> GetFragmentsOfType<T>() where T : WldFragment
        {
            return !_fragmentTypeDictionary.ContainsKey(typeof(T)) ? new List<T>() : _fragmentTypeDictionary[typeof(T)].Cast<T>().ToList();
        }

        public T GetFragmentByName<T>(string fragmentName) where T : WldFragment
        {
            return !_fragmentNameDictionary.ContainsKey(fragmentName) ? default : _fragmentNameDictionary[fragmentName] as T;
        }


        private void ParseStringHash(string decodedHash)
        {
            _stringHash = new Dictionary<int, string>();
            var index = 0;
            var splitHash = decodedHash.Split('\0');

            foreach (var hashString in splitHash)
            {
                _stringHash[index] = hashString;

                // Advance the position by the length + the null terminator
                index += hashString.Length + 1;
            }
        }

        public (List<Mesh> mesh, List<MaterialList> materials) ExportZone()
        {
            var meshes = GetFragmentsOfType<Mesh>();
            var materials = GetFragmentsOfType<MaterialList>();
            return (meshes, materials);

            foreach (var mesh in meshes)
            {

                //var usedVertices = new HashSet<int>();
                //var newIndices = new List<Polygon>();

                //var currentPolygon = 0;

                //foreach (var group in mesh.MaterialGroups)
                //{
                //    for (var i = 0; i < group.PolygonCount; ++i)
                //    {
                //        var polygon = mesh.Indices[currentPolygon];

                //        newIndices.Add(polygon.GetCopy());
                //        currentPolygon++;
                //        _ = usedVertices.Add(polygon.Vertex1);
                //        _ = usedVertices.Add(polygon.Vertex2);
                //        _ = usedVertices.Add(polygon.Vertex3);
                //    }
                //}


                //usedVertices.Clear();

                //for (var i = 0; i < mesh.Vertices.Count; ++i)
                //{
                //    _ = usedVertices.Add(i);
                //}


                //var unusedVertices = 0;
                //for (var i = mesh.Vertices.Count - 1; i >= 0; i--)
                //{
                //    if (usedVertices.Contains(i))
                //    {
                //        continue;
                //    }

                //    unusedVertices++;

                //    foreach (var polygon in newIndices)
                //    {
                //        if (polygon.Vertex1 >= i && polygon.Vertex1 != 0)
                //        {
                //            polygon.Vertex1--;
                //        }
                //        if (polygon.Vertex2 >= i && polygon.Vertex2 != 0)
                //        {
                //            polygon.Vertex2--;
                //        }
                //        if (polygon.Vertex3 >= i && polygon.Vertex3 != 0)
                //        {
                //            polygon.Vertex3--;
                //        }
                //    }
                //}

                //if (!_isCollisionMesh && (_isFirstMesh || _useGroups))
                //{
                //    _export.Append("ml");
                //    _export.Append(",");
                //    _export.Append(FragmentNameCleaner.CleanName(mesh.MaterialList));
                //    _export.AppendLine();
                //    _isFirstMesh = false;
                //}

                //for (var i = 0; i < mesh.Vertices.Count; i++)
                //{
                //    if (!usedVertices.Contains(i))
                //    {
                //        continue;
                //    }

                //    var vertex = mesh.Vertices[i];
                //    _export.Append("v");
                //    _export.Append(",");
                //    _export.Append(vertex.x + mesh.Center.x);
                //    _export.Append(",");
                //    _export.Append(vertex.z + mesh.Center.z);
                //    _export.Append(",");
                //    _export.Append(vertex.y + mesh.Center.y);
                //    _export.AppendLine();
                //}

                //for (var i = 0; i < mesh.TextureUvCoordinates.Count; i++)
                //{
                //    if (!usedVertices.Contains(i) || _isCollisionMesh)
                //    {
                //        continue;
                //    }

                //    var textureUv = mesh.TextureUvCoordinates[i];
                //    _export.Append("uv");
                //    _export.Append(",");
                //    _export.Append(textureUv.x);
                //    _export.Append(",");
                //    _export.Append(textureUv.y);
                //    _export.AppendLine();
                //}

                //for (var i = 0; i < mesh.Normals.Count; i++)
                //{
                //    if (!usedVertices.Contains(i) || _isCollisionMesh)
                //    {
                //        continue;
                //    }

                //    var normal = mesh.Normals[i];
                //    _export.Append("n");
                //    _export.Append(",");
                //    _export.Append(normal.x);
                //    _export.Append(",");
                //    _export.Append(normal.y);
                //    _export.Append(",");
                //    _export.Append(normal.z);
                //    _export.AppendLine();
                //}

                //for (var i = 0; i < mesh.Colors.Count; i++)
                //{
                //    if (!usedVertices.Contains(i) || _isCollisionMesh)
                //    {
                //        continue;
                //    }

                //    var vertexColor = mesh.Colors[i];
                //    _export.Append("c");
                //    _export.Append(",");
                //    _export.Append(vertexColor.B);
                //    _export.Append(",");
                //    _export.Append(vertexColor.G);
                //    _export.Append(",");
                //    _export.Append(vertexColor.R);
                //    _export.Append(",");
                //    _export.Append(vertexColor.A);
                //    _export.AppendLine();
                //}

                //currentPolygon = 0;

                //foreach (var group in mesh.MaterialGroups)
                //{

                //    for (var i = 0; i < group.PolygonCount; ++i)
                //    {
                //        var polygon = newIndices[currentPolygon];

                //        currentPolygon++;

                //        _export.Append("i");
                //        _export.Append(",");
                //        _export.Append(group.MaterialIndex);
                //        _export.Append(",");
                //        _export.Append(_currentBaseIndex + polygon.Vertex1);
                //        _export.Append(",");
                //        _export.Append(_currentBaseIndex + polygon.Vertex2);
                //        _export.Append(",");
                //        _export.Append(_currentBaseIndex + polygon.Vertex3);
                //        _export.AppendLine();
                //    }
                //} 

                Debug.WriteLine("got here");
            }
        }
    }
}
