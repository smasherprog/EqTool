using EQTool.Models;
using System.Collections.Generic;

namespace EQTool.Services.WLD.Fragments
{
    /// <summary>
    /// LegacyMesh (0x2C)
    /// Internal name: _DMSPRITEDEF
    /// This fragment is only found in the gequip archives and while it exists and is functional, it is not used.
    /// It looks like an earlier version of the Mesh fragment with fewer data points.
    /// </summary>
    public class LegacyMesh : WldFragment
    {
        public List<vec3> Vertices = new List<vec3>();
        public List<vec2> TexCoords = new List<vec2>();
        public List<vec3> Normals = new List<vec3>();
        public List<Polygon> Polygons = new List<Polygon>();
        public List<ivec2> VertexTex = new List<ivec2>();
        public List<RenderGroup> RenderGroups = new List<RenderGroup>();
        public MaterialList MaterialList;
        public Dictionary<int, MobVertexPiece> MobPieces { get; private set; }

        public override void Initialize(int index, int size, byte[] data, List<WldFragment> fragments, Dictionary<int, string> stringHash,
            bool isNewWldFormat)
        {
            base.Initialize(index, size, data, fragments, stringHash, isNewWldFormat);
            Name = stringHash[-Reader.ReadInt32()];
            var flags = Reader.ReadInt32();
            var vertexCount = Reader.ReadInt32();
            var texCoordCount = Reader.ReadInt32();
            var normalsCount = Reader.ReadInt32();
            var colorsCount = Reader.ReadInt32(); // size4
            var polygonCount = Reader.ReadInt32();
            int size6 = Reader.ReadInt16();
            _ = Reader.ReadInt16();
            var vertexPieceCount = Reader.ReadInt32(); // -1
            MaterialList = fragments[Reader.ReadInt32() - 1] as MaterialList;
            _ = Reader.ReadInt32();
            _ = Reader.ReadSingle();
            _ = Reader.ReadSingle();
            _ = Reader.ReadSingle();
            _ = Reader.ReadInt32();
            _ = Reader.ReadInt32();
            _ = Reader.ReadInt32();

            for (var i = 0; i < vertexCount; ++i)
            {
                Vertices.Add(new vec3(Reader.ReadSingle(), Reader.ReadSingle(), Reader.ReadSingle()));
            }

            for (var i = 0; i < texCoordCount; ++i)
            {
                TexCoords.Add(new vec2(Reader.ReadSingle(), Reader.ReadSingle()));
            }

            for (var i = 0; i < normalsCount; ++i)
            {
                Normals.Add(new vec3(Reader.ReadSingle(), Reader.ReadSingle(), Reader.ReadSingle()));
            }

            Reader.BaseStream.Position += colorsCount * sizeof(int);

            for (var i = 0; i < polygonCount; ++i)
            {
                _ = Reader.ReadInt16();
                _ = Reader.ReadInt16();
                _ = Reader.ReadInt16();
                _ = Reader.ReadInt16();
                _ = Reader.ReadInt16();

                int i1 = Reader.ReadInt16();
                int i2 = Reader.ReadInt16();
                int i3 = Reader.ReadInt16();
                Polygons.Add(new Polygon
                {
                    IsSolid = true,
                    Vertex1 = i1,
                    Vertex2 = i2,
                    Vertex3 = i3
                });
            }

            for (var i = 0; i < size6; ++i)
            {

                var datatype = Reader.ReadInt32();

                if (datatype != 4)
                {
                    _ = Reader.ReadInt32();
                    _ = Reader.ReadInt16();
                    _ = Reader.ReadInt16();
                }
                else
                {
                    _ = Reader.ReadSingle();
                    _ = Reader.ReadInt32();
                }
            }

            MobPieces = new Dictionary<int, MobVertexPiece>();
            var mobStart = 0;
            for (var i = 0; i < vertexPieceCount; ++i)
            {
                var mobVertexPiece = new MobVertexPiece
                {
                    Count = Reader.ReadInt16(),
                    Start = Reader.ReadInt16()
                };

                mobStart += mobVertexPiece.Count;

                MobPieces[mobVertexPiece.Start] = mobVertexPiece;
            }

            var ba = new BitAnalyzer(flags);

            if (ba.IsBitSet(9))
            {
                var size8 = Reader.ReadInt32();

                Reader.BaseStream.Position += size8 * 4;
            }

            if (ba.IsBitSet(11))
            {
                var polygonTexCount = Reader.ReadInt32();

                for (var i = 0; i < polygonTexCount; ++i)
                {
                    RenderGroups.Add(new RenderGroup
                    {
                        PolygonCount = Reader.ReadInt16(),
                        MaterialIndex = Reader.ReadInt16()
                    });
                }
            }

            if (ba.IsBitSet(12))
            {
                var vertexTexCount = Reader.ReadInt32();

                for (var i = 0; i < vertexTexCount; ++i)
                {
                    VertexTex.Add(new ivec2
                    {
                        x = Reader.ReadInt16(),
                        y = Reader.ReadInt16()
                    });
                }
            }

            if (ba.IsBitSet(13))
            {
                _ = Reader.ReadInt32();
                _ = Reader.ReadInt32();
                _ = Reader.ReadInt32();
            }
        }
    }
}