using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NelderMead
{
    // In each iteration of the algorithm, the simplex undergoes a transformation. The goal of the transformation is to 
    // move the least optimal point of the simplex to a different part of the search space in some predictable way. By
    // iteratively moving the worst point of the simplex to a better solution, we hope to converge on a (local) optimum.
    // The parameters in this config are used to compute 
    public struct TransformationConfig
    {
        // The first transformation reflects the worst point across the centroid, and the reflection vector is scaled by the
        // reflect coefficient (usually 1)
        public double ReflectCoefficient { get; private set; }
        
        // If the reflected point is better than the current BEST point, the point is moved further in the reflected direction by
        // scaling the reflection vector by the expand coefficient (usually 2)
        public double ExpandCoefficient { get; private set; }

        // If the reflected point is BETTER than the worst point xh that was originally reflected, but still worse than the second
        // worst point, we contract the reflected point back towards the centroid by the contract coefficient (usually 0.5)
        public double ContractCoefficient { get; private set; }

        // If the reflected point is WORST than the worst point xh that was originally reflected, only the best point of the
        // simplex is kept, and all other points are moved towards it by the shrink coefficient distance (usually 0.5)
        public double ShrinkCoefficient { get; private set; }

        public static TransformationConfig Default
        {
            get
            {
                return new TransformationConfig(1, 2, 0.5, 0.5);
            }
        }

        public TransformationConfig(double reflectCoefficient, double expandCoefficient, double contractCoefficient, double shrinkCoefficient)
        {
            ReflectCoefficient = reflectCoefficient;
            ExpandCoefficient = expandCoefficient;
            ContractCoefficient = contractCoefficient;
            ShrinkCoefficient = shrinkCoefficient;
        }
    }
}
