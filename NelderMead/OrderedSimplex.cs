using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Security;
using System.Text;
using System.Threading.Tasks;

namespace NelderMead
{
    /// <summary>
    /// A <see cref="Simplex{T}"/> where the points are ordered from "worst" to "best", 
    /// where the worst is the largest value if this is being used in a minimizing optimization
    /// and vice versa
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class OrderedSimplex<T> : Simplex<T> where T : IComparable<T>
    {
        public Func<SimplexPoint<T>, SimplexPoint<T>, bool> IsBetterThan;
        Action _sort;

        public bool Minimized { get; }
        public OrderedSimplex(List<SimplexPoint<T>> points, bool minimizing = true) : base(points)
        {
            Minimized = minimizing;
            if (minimizing)
            {
                IsBetterThan = (x, y) => x.Output.CompareTo(y.Output) == -1;
                _sort = () => { Points.Sort((a, b) => b.Output.CompareTo(a.Output)); };
            }
            else
            {
                IsBetterThan = (x, y) => x.Output.CompareTo(y.Output) == 1;
                _sort = () => { Points.Sort((a, b) => a.Output.CompareTo(b.Output)); };
            }

            _sort();
        }

        public void AddPoint(SimplexPoint<T> point)
        {
            Points.Add(point);
            _sort();
        }

        public void RemovePoint(SimplexPoint<T> point)
        {
            Points.Remove(point);
            _sort();
        }

        public SimplexPoint<T> Worst()
        {
            return Points.First();
        }

        public SimplexPoint<T> Best()
        {
            return Points.Last();
        }

        /// <summary>
        /// Creates a copy of this ordered simplex without the worst point
        /// </summary>
        public OrderedSimplex<T> CopyWithoutWorst()
        {
            var points = Points.Skip(1).ToList();
            return new OrderedSimplex<T>(points, Minimized);
        }

        /// <summary>
        /// Creates a copy of this ordered simplex without the best point
        /// </summary>
        public OrderedSimplex<T> CopyWithoutBest()
        {
            var points = new List<SimplexPoint<T>>(Points);
            points.Reverse();
            points = points.Skip(1).ToList();
            return new OrderedSimplex<T>(points, Minimized);
        }
    }
}
