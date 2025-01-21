using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NelderMead
{
    /// <summary>
    /// An n-dimensional shape with n+1 vertices.
    /// E.g. a 2D simplex is a triangle, 3D is a tetrahedron
    /// </summary>
    public class Simplex<T>
    {
        public List<SimplexPoint<T>> Points { get; protected set; }
        public int NumVertices { get; }
        public Simplex(List<SimplexPoint<T>> points)
        {
            if (points.Count < 2)
            {
                throw new ArgumentException("A simplex must contain at least 2 points");
            }
            Points = points;
            NumVertices = points[0].Inputs.Length;
        }

        /// <summary>
        /// Returns the centroid of the inputs of the points comprising this simplex
        /// </summary>
        public double[] Centroid()
        {
            var pointSum = new double[NumVertices];
            foreach (var point in Points)
            {
                for (int i = 0; i < NumVertices; i++)
                {
                    pointSum[i] += point.Inputs[i];
                }
            }

            for (int i = 0; i < NumVertices; i++)
            {
                pointSum[i] /= NumVertices;
            }

            return pointSum;
        }

    }
}
