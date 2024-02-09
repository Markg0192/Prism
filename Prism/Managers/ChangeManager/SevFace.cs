using System.Collections.Generic;
using Tekla.Structures.Geometry3d;
using Tekla.Structures.Model;

namespace Prism
{
    internal class SevFace
    {
        public Tekla.Structures.Geometry3d.Vector Normal;
        public List<Point> Vertices;

        public static List<SevFace> GetAllFaces(ModelObject obj)
        {
          
            var faceList = new List<SevFace>();

            if (obj is Part part)
            {
                var solid = part.GetSolid();

                var faces = solid.GetFaceEnumerator();
                while (faces.MoveNext())
                {
                    var face = faces.Current;
                    var normal = face.Normal;
                    var loops = face.GetLoopEnumerator();
                    while (loops.MoveNext())
                    { 
                        List<Point> verticesList = new List<Point>();
                        var sevFace = new SevFace
                        {
                            Normal = normal
                        };
                        var loop = loops.Current;
                        var vertices = loop.GetVertexEnumerator();

                        while (vertices.MoveNext())
                        {
                            var vertex = vertices.Current;
                            verticesList.Add(vertex);
                        }
                        //sevFace.GetPolygons();

                        sevFace.Vertices = verticesList;
                        faceList.Add(sevFace);
                    }
                }
            }
            return faceList;
        }
    }
}