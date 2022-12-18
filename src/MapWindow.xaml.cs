using EQTool.Models;
using EQTool.Services;
using EQTool.Services.WLD;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Media3D;

namespace EQTool
{
    /// <summary>
    /// Interaction logic for MapWindow.xaml
    /// </summary>
    public partial class MapWindow : Window
    {
        public PerspectiveCamera Camera = new PerspectiveCamera();

        public Point3D Position
        {
            get => Camera.Position;
            set => Camera.Position = value;
        }

        public Vector3D MovingDirection;

        public bool MovingDirectionIsLocked;

        public Vector3D LookDirection
        {
            get => Camera.LookDirection;
            set
            {
                Camera.LookDirection = value;
                if (!MovingDirectionIsLocked)
                {
                    MovingDirection = Camera.LookDirection;
                }
            }
        }

        public Vector3D UpDirection
        {
            get => Camera.UpDirection;
            set => Camera.UpDirection = value;
        }

        public Vector3D LeftDirection => Vector3D.CrossProduct(Camera.UpDirection, Camera.LookDirection);

        public MapWindow(S3dReader s3DReader)
        {
            InitializeComponent();

            Camera = new PerspectiveCamera
            {
                Position = new Point3D(0, 0, 1000),
                LookDirection = new Vector3D(0, 0, -1),
                FieldOfView = 60
            };

            viewport.Camera = Camera;

            var myDirectionalLight = new DirectionalLight
            {
                Color = Colors.White,
                Direction = new Vector3D(-0.61, -0.5, -0.61)
            };
            var data = s3DReader.Read();
            var reader = new WLDReader();
            reader.Read(data.FirstOrDefault(a => a.Name == "southkarana.wld"));
            var stuff = reader.ExportZone();

            var myModel3DGroup = new Model3DGroup();
            var myGeometryModel = new GeometryModel3D();
            var myModelVisual3D = new ModelVisual3D();
            myModel3DGroup.Children.Add(myDirectionalLight);
            var biggestverts = stuff.mesh.OrderByDescending(a => a.Vertices.Count).Take(1).ToList();
            foreach (var mesh in biggestverts)
            {
                var usedVertices = new HashSet<int>();
                var newIndices = new List<Polygon>();
                var currentPolygon = 0;
                foreach (var group in mesh.MaterialGroups)
                {
                    for (var i = 0; i < group.PolygonCount; ++i)
                    {
                        var polygon = mesh.Indices[currentPolygon];

                        newIndices.Add(polygon.GetCopy());
                        currentPolygon++;
                        _ = usedVertices.Add(polygon.Vertex1);
                        _ = usedVertices.Add(polygon.Vertex2);
                        _ = usedVertices.Add(polygon.Vertex3);
                    }
                }

                usedVertices.Clear();

                for (var i = 0; i < mesh.Vertices.Count; ++i)
                {
                    _ = usedVertices.Add(i);
                }

                var unusedVertices = 0;
                for (var i = mesh.Vertices.Count - 1; i >= 0; i--)
                {
                    if (usedVertices.Contains(i))
                    {
                        continue;
                    }

                    unusedVertices++;

                    foreach (var polygon in newIndices)
                    {
                        if (polygon.Vertex1 >= i && polygon.Vertex1 != 0)
                        {
                            polygon.Vertex1--;
                        }
                        if (polygon.Vertex2 >= i && polygon.Vertex2 != 0)
                        {
                            polygon.Vertex2--;
                        }
                        if (polygon.Vertex3 >= i && polygon.Vertex3 != 0)
                        {
                            polygon.Vertex3--;
                        }
                    }
                }

                var myMeshGeometry3D = new MeshGeometry3D();
                var myPositionCollection = new Point3DCollection();
                for (var i = 0; i < mesh.Vertices.Count; i++)
                {
                    if (!usedVertices.Contains(i))
                    {
                        continue;
                    }
                    var vertex = mesh.Vertices[i];
                    myPositionCollection.Add(new Point3D
                    {
                        X = vertex.x + mesh.Center.x,
                        Y = vertex.z + mesh.Center.z,
                        Z = vertex.y + mesh.Center.y,
                    });
                }
                myMeshGeometry3D.Positions = myPositionCollection;
                //var myTextureCoordinatesCollection = new PointCollection();
                //for (var i = 0; i < mesh.TextureUvCoordinates.Count; i++)
                //{
                //    if (!usedVertices.Contains(i))
                //    {
                //        continue;
                //    }
                //    var textureUv = mesh.TextureUvCoordinates[i];
                //    myTextureCoordinatesCollection.Add(new Point
                //    {
                //        X = textureUv.x,
                //        Y = textureUv.y
                //    });
                //}
                //myMeshGeometry3D.TextureCoordinates = myTextureCoordinatesCollection;
                var myNormalCollection = new Vector3DCollection();
                for (var i = 0; i < mesh.Normals.Count; i++)
                {
                    if (!usedVertices.Contains(i))
                    {
                        continue;
                    }
                    var normal = mesh.Normals[i];
                    myNormalCollection.Add(new Vector3D
                    {
                        Y = normal.y,
                        X = normal.x,
                        Z = normal.z
                    });
                }

                myMeshGeometry3D.Normals = myNormalCollection;

                //for (var i = 0; i < mesh.Colors.Count; i++)
                //{
                //    if (!usedVertices.Contains(i))
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
                currentPolygon = 0;
                foreach (var group in mesh.MaterialGroups)
                {
                    var myTriangleIndicesCollection = new Int32Collection();
                    for (var i = 0; i < group.PolygonCount; ++i)
                    {
                        var polygon = newIndices[currentPolygon];
                        currentPolygon++;
                        myTriangleIndicesCollection.Add(polygon.Vertex1);
                        Debug.Assert(polygon.Vertex1 < myPositionCollection.Count);
                        myTriangleIndicesCollection.Add(polygon.Vertex2);
                        Debug.Assert(polygon.Vertex2 < myPositionCollection.Count);
                        myTriangleIndicesCollection.Add(polygon.Vertex3);
                        Debug.Assert(polygon.Vertex3 < myPositionCollection.Count);
                    }
                    myGeometryModel.Transform = Transform3D.Identity;
                    var material = new DiffuseMaterial(new SolidColorBrush(Colors.Gray));
                    myGeometryModel.Material = material;
                    myMeshGeometry3D.TriangleIndices = myTriangleIndicesCollection;
                    myGeometryModel.Geometry = myMeshGeometry3D;

                    myModel3DGroup.Children.Add(myGeometryModel);
                }
            }

            myModelVisual3D.Content = myModel3DGroup;
            viewport.Children.Add(myModelVisual3D);
        }

        private Point prevPosition = new Point(double.NaN, 0);
        private void viewport_MouseDown(object sender, MouseButtonEventArgs e)
        {
            base.OnMouseMove(e);
            if (e.LeftButton != MouseButtonState.Pressed)
            {
                return;
            }

            var position = e.GetPosition(this);

            if (prevPosition.X != double.NaN)
            {
                //HandleMouseMove(prevPosition - position);
            }

            prevPosition = position;
        }

        //public void Rotate(Vector3D axis, double angle, Point3D center)
        //{
        //    Position = Vector3D.Subtract(center, Position);
        //    Rotate(axis, angle);
        //    Position = Position.Add(center);
        //}

        //public Quaternion Rotation(Vector3D axis, double angle)
        //{
        //    Position = Vector3D.Subtract(Position, center);
        //    Rotate(axis, angle);
        //    Position = Position.Add(center);
        //}


        //public void Rotate(Vector3D axis, double angle)
        //{
        //    var q = Rotation(axis, angle);
        //    Position = Transform(q, Position);
        //    UpDirection = Transform(q, UpDirection);
        //    LookDirection = Transform(q, LookDirection);
        //}
        public static Vector3D Transform(Quaternion q, Vector3D v)
        {
            var x2 = q.X + q.X;
            var y2 = q.Y + q.Y;
            var z2 = q.Z + q.Z;
            var wx2 = q.W * x2;
            var wy2 = q.W * y2;
            var wz2 = q.W * z2;
            var xx2 = q.X * x2;
            var xy2 = q.X * y2;
            var xz2 = q.X * z2;
            var yy2 = q.Y * y2;
            var yz2 = q.Y * z2;
            var zz2 = q.Z * z2;
            var x = (v.X * (1.0 - yy2 - zz2)) + (v.Y * (xy2 - wz2)) + (v.Z * (xz2 + wy2));
            var y = (v.X * (xy2 + wz2)) + (v.Y * (1.0 - xx2 - zz2)) + (v.Z * (yz2 - wx2));
            var z = (v.X * (xz2 - wy2)) + (v.Y * (yz2 + wx2)) + (v.Z * (1.0 - xx2 - yy2));
            return new Vector3D(x, y, z);
        }

        public static Point3D Transform(Quaternion q, Point3D v)
        {
            var x2 = q.X + q.X;
            var y2 = q.Y + q.Y;
            var z2 = q.Z + q.Z;
            var wx2 = q.W * x2;
            var wy2 = q.W * y2;
            var wz2 = q.W * z2;
            var xx2 = q.X * x2;
            var xy2 = q.X * y2;
            var xz2 = q.X * z2;
            var yy2 = q.Y * y2;
            var yz2 = q.Y * z2;
            var zz2 = q.Z * z2;
            var x = (v.X * (1.0 - yy2 - zz2)) + (v.Y * (xy2 - wz2)) + (v.Z * (xz2 + wy2));
            var y = (v.X * (xy2 + wz2)) + (v.Y * (1.0 - xx2 - zz2)) + (v.Z * (yz2 - wx2));
            var z = (v.X * (xz2 - wy2)) + (v.Y * (yz2 + wx2)) + (v.Z * (1.0 - xx2 - yy2));
            return new Point3D(x, y, z);
        }
        //public void ChangeHeading(double angle)
        //{
        //    Quaternion q = Math3D.RotationZ(angle);
        //    UpDirection = q.Transform(UpDirection);
        //    LookDirection = q.Transform(LookDirection);
        //}

        //protected void HandleMouseMove(Vector mouseMove)
        //{
        //    var angleX = mouseMove.X;
        //    var angleY = mouseMove.Y;


        //    if (MovingDirectionIsLocked)
        //    {
        //        Camera.ChangeHeading(angleX);
        //        Camera.ChangePitch(angleY);
        //    }
        //    else
        //    {
        //        Camera.ChangeRoll(-angleX);
        //        Camera.ChangePitch(angleY);
        //    }

        //}

        private void Grid_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            Camera.Position = new Point3D(Camera.Position.X - (e.Delta / 20), Camera.Position.Y - (e.Delta / 20), Camera.Position.Z - (e.Delta / 20));
            Debug.WriteLine($"{Camera.Position.X},{Camera.Position.Y},{Camera.Position.Z}");
        }
    }
}
