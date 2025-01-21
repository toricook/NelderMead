using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NelderMead
{
    public struct SimplexPoint<T>
    {
        public double[] Inputs;
        public T Output;

        public SimplexPoint(double[] inputs, T output)
        {
            Inputs = inputs;
            Output = output;
        }
    }
}
