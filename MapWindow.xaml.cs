using EQTool.Models;
using EQTool.Services;
using EQTool.Services.WLD;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Media3D;

namespace EQTool
{
    /// <summary>
    /// Interaction logic for MapWindow.xaml
    /// </summary>
    public partial class MapWindow : Window
    {
        public MapWindow()
        {
            InitializeComponent();



            var myModel3DGroup = new Model3DGroup();
            var myGeometryModel = new GeometryModel3D();
            var myModelVisual3D = new ModelVisual3D();
            var myPCamera = new PerspectiveCamera
            {
                // Specify where in the 3D scene the camera is.
                Position = new Point3D(0, 0, 2),

                // Specify the direction that the camera is pointing.
                LookDirection = new Vector3D(0, 0, -1),

                // Define camera's horizontal field of view in degrees.
                FieldOfView = 60
            };

            // Asign the camera to the viewport
            viewport.Camera = myPCamera;
            // Define the lights cast in the scene. Without light, the 3D object cannot
            // be seen. Note: to illuminate an object from additional directions, create
            // additional lights.
            var myDirectionalLight = new DirectionalLight
            {
                Color = Colors.White,
                Direction = new Vector3D(-0.61, -0.5, -0.61)
            };

            myModel3DGroup.Children.Add(myDirectionalLight);

            var r = new S3dReader();
            var data = r.Read();
            var reader = new WLDReader();
            reader.Read(data.FirstOrDefault(a => a.Name == "freportn.wld"));
            var stuff = reader.ExportZone();

            var usedVertices = new HashSet<int>();
            var newIndices = new List<Polygon>();

            var currentPolygon = 0;
            foreach (var mesh in stuff.mesh)
            {
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
                var myTextureCoordinatesCollection = new PointCollection();
                for (var i = 0; i < mesh.TextureUvCoordinates.Count; i++)
                {
                    if (!usedVertices.Contains(i))
                    {
                        continue;
                    }
                    var textureUv = mesh.TextureUvCoordinates[i];
                    myTextureCoordinatesCollection.Add(new Point
                    {
                        X = textureUv.x,
                        Y = textureUv.y
                    });
                }
                myMeshGeometry3D.TextureCoordinates = myTextureCoordinatesCollection;
                var myNormalCollection = new Vector3DCollection()
                for (var i = 0; i < mesh.Normals.Count; i++)
                {
                    if (!usedVertices.Contains(i))
                    {
                        continue;
                    }

                    var normal = mesh.Normals[i];
                    _export.Append("n");
                    _export.Append(",");
                    _export.Append(normal.x);
                    _export.Append(",");
                    _export.Append(normal.y);
                    _export.Append(",");
                    _export.Append(normal.z);
                    _export.AppendLine();
                }

                myMeshGeometry3D.Normals = myNormalCollection;
                for (var i = 0; i < mesh.Colors.Count; i++)
                {
                    if (!usedVertices.Contains(i))
                    {
                        continue;
                    }

                    var vertexColor = mesh.Colors[i];
                    _export.Append("c");
                    _export.Append(",");
                    _export.Append(vertexColor.B);
                    _export.Append(",");
                    _export.Append(vertexColor.G);
                    _export.Append(",");
                    _export.Append(vertexColor.R);
                    _export.Append(",");
                    _export.Append(vertexColor.A);
                    _export.AppendLine();
                }



                // Create a collection of triangle indices for the MeshGeometry3D.
                var myTriangleIndicesCollection = new Int32Collection
            {
                0,
                1,
                2,
                3,
                4,
                5
            };
                myMeshGeometry3D.TriangleIndices = myTriangleIndicesCollection;

                // Apply the mesh to the geometry model.
                myGeometryModel.Geometry = myMeshGeometry3D;

                myModel3DGroup.Children.Add(myGeometryModel);

            }


            myModelVisual3D.Content = myModel3DGroup;

            viewport.Children.Add(myModelVisual3D);
        }
    }
}
