namespace NelderMead
{
    public static class Extensions
    {
        /// <summary>
        /// Sets every element of <paramref name="array"/> to <paramref name="value"/>
        /// </summary>
        public static void Populate<T>(this T[] array, T value)
        {
            for (int i = 0; i < array.Length; i++)
            {
                array[i] = value;
            }
        }

        /// <inheritdoc cref="Populate{T}(T[], T)"/>
        public static void Populate(this double[] array, double value)
        {
            Populate<double>(array, value);
        }

        /// <summary>
        /// Returns the value of <paramref name="x"/> multiplied by itself
        /// </summary>
        public static double Square(this double x)
        {
            return x * x;
        }

        /// <summary>
        /// Returns <paramref name="x"/> if <paramref name="x"/> is between <paramref name="min"/> and <paramref name="max"/>, 
        /// or returns <paramref name="min"/> or <paramref name="max"/> if <paramref name="x"/> is beyond one of those bounds
        /// </summary>
        public static double Clamp(this double x, double min, double max)
        {
            return Math.Clamp(x, min, max);
        }

        public static double[] AddElementwise(this double[] array, double[] otherArray)
        {
            if (array.Length != otherArray.Length)
            {
                throw new ArgumentException("Arrays must have the sanme length");
            }
            var newArray = new double[array.Length];
            for (int i = 0; i < newArray.Length; i++)
            {
                newArray[i] = array[i] + otherArray[i];
            }
            return newArray;
        }
    }
}
