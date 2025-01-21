namespace NelderMead
{
    /// <summary>
    /// Options for terminating the solve for reasons other than convergence of the function value
    /// </summary>
    public class TerminationConfig<T>
    {
        /// <summary>
        /// How much the simplex diameter of the simplex must be reduced before the solve will terminate
        /// (e.g. how similar the inputs must be)
        /// </summary>
        public double? SimplexDiameterTolerance { get; }

        /// <summary>
        /// How close the best and worst values must be before the solve will terminate
        /// </summary>
        public double? ValueConvergenceTolerance { get; }

        /// <summary>
        /// The maximum number of iterations the solver will perform before termination
        /// </summary>
        public double? MaxIterations { get; } 

        /// <summary>
        /// The maximum number of milliseconds before each solve will terminate
        /// </summary>
        public double? MaxDurationMs { get; }

        public Func<T, T, bool> ValueConverged { get; }

        public TerminationConfig(double? diameterTolerance, double? maxIterations, double? maxDurationMs,
            Func<T, T, bool> valueConverged)
        {
            SimplexDiameterTolerance = diameterTolerance;
            MaxIterations = maxIterations;
            MaxDurationMs = maxDurationMs;
            ValueConverged = valueConverged;
        }

    }

    public enum TerminationReason
    {
        None,
        Canceled,
        Timeout,
        MaxIterations,
        SimplexDiameter,
        ValueConvergence

    }
}
