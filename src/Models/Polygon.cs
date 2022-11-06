namespace EQTool.Models
{
    public class Polygon
    {
        public Polygon GetCopy()
        {
            return new Polygon
            {
                IsSolid = IsSolid,
                Vertex1 = Vertex1,
                Vertex2 = Vertex2,
                Vertex3 = Vertex3
            };
        }

        public bool IsSolid { get; set; }
        public int Vertex1 { get; set; }
        public int Vertex2 { get; set; }
        public int Vertex3 { get; set; }
    }
    public class RenderGroup
    {
        public int PolygonCount { get; set; }
        public int MaterialIndex { get; set; }
    }
    public class MobVertexPiece
    {
        public int Start { get; set; }
        public int Count { get; set; }
    }
}
