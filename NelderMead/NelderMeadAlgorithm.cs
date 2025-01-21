using System.Diagnostics;

namespace NelderMead
{
    /// <summary>
    /// An implementation of the method described at https://codesachin.wordpress.com/2016/01/16/nelder-mead-optimization/ 
    /// </summary>
    public class NelderMead<T> where T : IComparable<T>
    {
        public TransformationConfig TransformationConfig { get; protected set; }

        public TerminationConfig<T> TerminationConfig { get; protected set; }

        /// <summary>
        /// The function to be optimized
        /// </summary>
        protected Func<double[], T> _optimizationFunction;


        //TODO: move parameter config stuff to own class?

        public double[] InitialGuesses { get; }
        /// <summary>
        /// When initializing simplex, the first point will be the initial guess and each subsequent point i will take a step away
        /// from the intial point in the direction of the jth unit vector where j = i-1. The following parameter defines the step size.
        /// </summary>
        public double[] StepSize { get; }
        public double[] UpperBounds { get; }
        public double[] LowerBounds { get; }


        public NelderMeadVariation Variant { get; }

        public event EventHandler<IterationEventArgs<T>>? OnIterationCompleted;

        public NelderMead(Func<double[], T> function, double[] initialGuesses, double[] stepSizes, double[] upperBounds,
            double[] lowerBounds, TerminationConfig<T> terminationConfig, TransformationConfig? transformationConfig = null,
            NelderMeadVariation variant = NelderMeadVariation.Normal)
        {
            _optimizationFunction = function;

            InitialGuesses = initialGuesses;
            StepSize = stepSizes;
            LowerBounds = lowerBounds;
            UpperBounds = upperBounds;

            TerminationConfig = terminationConfig;
            TransformationConfig = transformationConfig ?? TransformationConfig.Default;
            Variant = variant;
        }


        /// <summary>
        /// Builds an initial simplex from which to start the optimization. Provide an array of guesses for each parameter
        /// and an array of step sizes that will determine the size of the intial step taken along that parameter.
        /// </summary>
        static OrderedSimplex<T> CreateInitialSimplex(double[] guesses, double[] stepSizes, Func<double[], T> function, 
            bool minimize)
        {
            var guessPoint = new SimplexPoint<T>(guesses, function(guesses));
            var simplexPoints = new List<SimplexPoint<T>>() { guessPoint };

            for (int i = 0; i < guesses.Length; i++)
            {
                double[] v = new double[guesses.Length];
                guesses.CopyTo(v, 0);
                v[i] += stepSizes[i];
                simplexPoints.Add(new SimplexPoint<T>(v, function(v)));
            }

            return new OrderedSimplex<T>(simplexPoints, minimize);
        }


        public Result Minimize(CancellationToken? token = null)
        {
            var initialSimplex = CreateInitialSimplex(InitialGuesses, StepSize, _optimizationFunction, true);
            return Optimize(initialSimplex, token);
        }

        public Result Maximize(CancellationToken? token = null)
        {
            var initialSimplex = CreateInitialSimplex(InitialGuesses, StepSize, _optimizationFunction, false);
            return Optimize(initialSimplex, token);
        }

        Result Optimize(OrderedSimplex<T> initialSimplex, CancellationToken? token = null)
        {
            var simplex = initialSimplex;

            SimplexPoint<T> solution;
            TerminationReason terminationReason;
            int iterations = 0;

            Stopwatch watch = new Stopwatch();
            watch.Start();

            while (true)
            {
                simplex = TransformSimplex(simplex, out var action);

                solution = simplex.Best();
                iterations++;

                var e = new IterationEventArgs<T>(simplex, iterations, action, watch.ElapsedMilliseconds);
                OnIterationCompleted?.Invoke(this, e);

                if (ShouldStop(simplex, watch, iterations, out var reason, token))
                {
                    terminationReason = reason;
                    watch.Stop();
                    break;
                }

            }
            return new Result(solution, iterations, watch.ElapsedMilliseconds, terminationReason);
        }

        public class Result
        {
            public double[] Inputs;
            public T Output;
            public int SolveIterations;
            public long SolveDurationMs;
            public TerminationReason TerminationReason;

            public Result(double[] inputs, T output, int solveIterations, long solveDurationMs, TerminationReason terminationReason)
            {
                Inputs = inputs;
                Output = output;
                SolveIterations = solveIterations;
                SolveDurationMs = solveDurationMs;
                TerminationReason = terminationReason;
            }

            public Result(SimplexPoint<T> point, int solveIterations, long solveDurationMs, TerminationReason terminationReason) :
                this(point.Inputs, point.Output, solveIterations, solveDurationMs, terminationReason)
            { }
        }

        #region Transformations

        SimplexPoint<T> ReflectOverCentroid(SimplexPoint<T> point, double[] centroid)
        {
            var newInputs = new double[point.Inputs.Length];
            for (int i = 0; i < point.Inputs.Length; i++)
            {
                var r = centroid[i] + TransformationConfig.ReflectCoefficient * (centroid[i] - point.Inputs[i]);
                newInputs[i] = r.Clamp(LowerBounds[i], UpperBounds[i]);
            }
            return new SimplexPoint<T>(newInputs, _optimizationFunction(newInputs));
        }

        SimplexPoint<T> ExpandFromCentroid(SimplexPoint<T> point, double[] centroid)
        {
            var newInputs = new double[point.Inputs.Length];
            for (int i = 0; i < point.Inputs.Length; i++)
            {
                var e = centroid[i] + TransformationConfig.ExpandCoefficient * (point.Inputs[i] - centroid[i]);
                newInputs[i] = e.Clamp(LowerBounds[i], UpperBounds[i]);
            }
            return new SimplexPoint<T>(newInputs, _optimizationFunction(newInputs));
        }

        SimplexPoint<T> ContractTowardsCentroid(SimplexPoint<T> point, double[] centroid)
        {
            var newInputs = new double[point.Inputs.Length];
            for (int i = 0; i < point.Inputs.Length; i++)
            {
                var c = centroid[i] + TransformationConfig.ContractCoefficient * (point.Inputs[i] - centroid[i]);
                newInputs[i] = c.Clamp(LowerBounds[i], UpperBounds[i]);
            }
            return new SimplexPoint<T>(newInputs, _optimizationFunction(newInputs));
        }

        SimplexPoint<T> ShrinkTowardsBest(SimplexPoint<T> point, SimplexPoint<T> bestPoint)
        {
            var newInputs = new double[point.Inputs.Length];
            for (int i = 0; i < point.Inputs.Length; i++)
            {
                var b = bestPoint.Inputs[i] + TransformationConfig.ShrinkCoefficient * (point.Inputs[i] - bestPoint.Inputs[i]);
                newInputs[i] = b.Clamp(LowerBounds[i], UpperBounds[i]);
            }
            return new SimplexPoint<T>(newInputs, _optimizationFunction(newInputs));
        }

        OrderedSimplex<T> Shrink(OrderedSimplex<T> orderedSimplex)
        {
            var bestPoint = orderedSimplex.Best();
            var withoutBest = orderedSimplex.CopyWithoutBest();
            var shrunkenPoints = new List<SimplexPoint<T>>();
            foreach (var point in withoutBest.Points)
            {
                var newPoint = ShrinkTowardsBest(point, bestPoint);
                shrunkenPoints.Add(newPoint);
            }
            shrunkenPoints.Add(bestPoint);
            return new OrderedSimplex<T>(shrunkenPoints, orderedSimplex.Minimized);
        }

        /// <summary>
        /// The algorithm by which the simplex is transformed. More explanation in <see cref="TransformationConfig">.
        /// </summary>
        OrderedSimplex<T> TransformSimplex(OrderedSimplex<T> orderedSimplex, out IterationAction action)
        {
            // The worst point in the simplex, which will drive our transformation
            var worstPoint = orderedSimplex.Worst();

            // Make a copy of the simplex without the worst point
            var newSimplex = orderedSimplex.CopyWithoutWorst();

            // REFLECTION - reflect worst point over the centroid of the new simplex
            var centroid = newSimplex.Centroid();
            var reflected = ReflectOverCentroid(worstPoint, centroid);

            // if new point is better than current best, EXPAND
            if (orderedSimplex.IsBetterThan(reflected, orderedSimplex.Best()))
            {
                var expanded = ExpandFromCentroid(reflected, centroid);

                // if expanded point is even better than the reflected one, use it
                if (orderedSimplex.IsBetterThan(expanded, reflected))
                {
                    action = IterationAction.Expand;
                    newSimplex.AddPoint(expanded);
                    return newSimplex;
                }
                else
                {
                    // if doing greedy expansion, use exapnded point if it is better than best, else use reflected
                    if (Variant == NelderMeadVariation.GreedyExpansion)
                    {
                        if (orderedSimplex.IsBetterThan(expanded, orderedSimplex.Best()))
                        {
                            action = IterationAction.GreedyExpand;
                            newSimplex.AddPoint(expanded);
                            return newSimplex;
                        }
                        else
                        {
                            action = IterationAction.Reflect;
                            newSimplex.AddPoint(reflected);
                            return newSimplex;
                        }
                    }
                    // if not greedy expansion, just use reflected
                    else
                    {
                        action = IterationAction.Reflect;
                        newSimplex.AddPoint(reflected);
                        return newSimplex;
                    }
                }
            }

            // if reflected point is worse than the worst point in the new simplex, CONTRACT
            if (orderedSimplex.IsBetterThan(newSimplex.Worst(), reflected))
            {
                var contracted = ContractTowardsCentroid(reflected, centroid);

                // I the contracted point is better than the worst point, use it
                if (orderedSimplex.IsBetterThan(contracted, worstPoint))
                {
                    action = IterationAction.Contract;
                    newSimplex.AddPoint(contracted);
                    return newSimplex;
                }

                // otherwise, shrink the entire simplex, keeping only the best point
                action = IterationAction.Shrink;
                return Shrink(orderedSimplex);

            }

            // if we neither contracted nor expanded, just use reflected
            action = IterationAction.Reflect;
            newSimplex.AddPoint(reflected);
            return newSimplex;
        }

        #endregion

        #region StoppingCriteria
        
        /// <summary>
        /// Calculates the simplex size along all parameters and returns the largest
        /// This is the diameter of an n-sphere that encloses the simplex
        /// </summary>
        /// <returns></returns>
        double BoundingDiameter(Simplex<T> simplex)
        {
            var centroid = simplex.Centroid();
            double maxRadiusSquared = 0;
            foreach (var p in simplex.Points)
            {
                double r = 0;
                for (int i = 0; i < p.Inputs.Length; i++)
                {
                    r += (p.Inputs[i] - centroid[i]).Square();
                }
                if (r > maxRadiusSquared)
                {
                    maxRadiusSquared = r;
                }
            }
            return Math.Sqrt(maxRadiusSquared) * 2;
        }


        bool ShouldStop(OrderedSimplex<T> simplex, Stopwatch watch, int iterations, out TerminationReason reason, CancellationToken? token = null)
        {
            // Canceled
            if (token.HasValue && token.Value.IsCancellationRequested)
            {
                reason = TerminationReason.Canceled;
                return true;
            }

            // Timeout
            if (TerminationConfig.MaxDurationMs.HasValue && TerminationConfig.MaxDurationMs.Value > watch.ElapsedMilliseconds)
            {
                reason = TerminationReason.Timeout;
                return true;
            }

            // Max iterations reached
            if (TerminationConfig.MaxIterations.HasValue && iterations > TerminationConfig.MaxIterations.Value)
            {
                reason = TerminationReason.MaxIterations;
                return true;
            }

            // Simplex got too small
            if (TerminationConfig.SimplexDiameterTolerance.HasValue && BoundingDiameter(simplex) < TerminationConfig.SimplexDiameterTolerance.Value)
            {
                reason = TerminationReason.SimplexDiameter;
                return true;
            }

            // Value converged
            bool? converged = TerminationConfig.ValueConverged?.Invoke(simplex.Best().Output, simplex.Worst().Output);
            if (converged.HasValue && converged.Value)
            {
                reason = TerminationReason.ValueConvergence;
                return true;
            }

            reason = TerminationReason.None;
            return false;

        }

        #endregion
    }

    public enum NelderMeadVariation
    {
        Normal = 0,
        GreedyExpansion = 1,
    }
}


