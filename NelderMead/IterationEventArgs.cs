using System.Numerics;

namespace NelderMead
{
    /// <summary>
    /// Arguments for an event that fires after each iteration of the solve, containing information about the solve for debugging and
    /// visualization purposes
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class IterationEventArgs<T> : EventArgs where T : IComparable<T>
    {
        public readonly OrderedSimplex<T> CurrentSimplex;
        public readonly int Iteration;
        public readonly IterationAction Action;
        public readonly long ElapsedMs;

        public IterationEventArgs(OrderedSimplex<T> currentSimplex, int iteration, IterationAction action, 
            long elapsedMs)
        {
            CurrentSimplex = currentSimplex;
            Iteration = iteration;
            Action = action;
            ElapsedMs = elapsedMs;
        }
    }

    public enum IterationAction
    {
        Reflect = 1,
        Expand = 2,
        Contract = 3,
        Shrink = 4,
        GreedyExpand = 5,
    }
}
