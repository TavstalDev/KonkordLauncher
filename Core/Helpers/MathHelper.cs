namespace Tavstal.KonkordLauncher.Core.Helpers
{
    public static class MathHelper
    {
        private static Random? _random;
        private static readonly Lock syncObj = new();


        /// <summary>
        /// Returns a random integer between the specified minimum and maximum values (inclusive).
        /// </summary>
        /// <param name="min">The minimum value of the random range.</param>
        /// <param name="max">The maximum value of the random range.</param>
        /// <returns>A random integer between the specified minimum and maximum values.</returns>
        public static int Next(int min, int max)
        {
            lock (syncObj)
            {
                if (_random == null)
                    _random = new Random(); // Or exception...
                return _random.Next(min, max);
            }
        }

        /// <summary>
        /// Returns a random integer between 0 (inclusive) and the specified maximum value (exclusive).
        /// </summary>
        /// <param name="max">The exclusive maximum value of the random range.</param>
        /// <returns>A random integer between 0 (inclusive) and the specified maximum value (exclusive).</returns>
        public static int Next(int max)
        {
            lock (syncObj)
            {
                if (_random == null)
                    _random = new Random(); // Or exception...
                return _random.Next(0, max);
            }
        }

        /// <summary>
        /// Clamps the specified value between the specified minimum and maximum values.
        /// </summary>
        /// <param name="value">The value to clamp.</param>
        /// <param name="minValue">The minimum value to clamp to.</param>
        /// <param name="maxValue">The maximum value to clamp to.</param>
        /// <returns>The clamped value.</returns>
        public static int Clamp(int value, int minValue, int maxValue)
        {
            return maxValue < value ? maxValue : (value < minValue ? minValue : value);
        }

        /// <summary>
        /// Returns a random integer between 0 (inclusive) and the specified maximum value (exclusive).
        /// </summary>
        /// <returns>A random integer between 0 (inclusive) and the specified maximum value (exclusive).</returns>
        public static uint Next()
        {
            lock (syncObj)
            {
                if (_random == null)
                    _random = new Random();
                return (uint)(_random.Next(1 << 30)) << 2 | (uint)(_random.Next(1 << 2));
            }
        }

        /// <summary>
        /// Clamps the specified value between the specified minimum and maximum values.
        /// </summary>
        /// <param name="value">The value to clamp.</param>
        /// <param name="minValue">The minimum value to clamp to.</param>
        /// <param name="maxValue">The maximum value to clamp to.</param>
        /// <returns>The clamped value.</returns>
        public static uint Clamp(uint value, uint minValue, uint maxValue)
        {
            return maxValue < value ? maxValue : (value < minValue ? minValue : value);
        }

        /// <summary>
        /// Generates a random double value within the specified range.
        /// </summary>
        /// <param name="min">The minimum value of the range (inclusive).</param>
        /// <param name="max">The maximum value of the range (exclusive).</param>
        /// <returns>A random double value within the specified range.</returns>
        public static double Next(double min, double max)
        {
            lock (syncObj)
            {
                if (_random == null)
                    _random = new Random(); // Or exception...
                return (_random.NextDouble() * Math.Abs(max - min)) + min;
            }
        }

        /// <summary>
        /// Generates a random double value within the range of [0, max).
        /// </summary>
        /// <param name="max">The maximum value of the range (exclusive).</param>
        /// <returns>A random double value within the range of [0, max).</returns>
        public static double Next(double max)
        {
            lock (syncObj)
            {
                if (_random == null)
                    _random = new Random(); // Or exception...
                return (_random.NextDouble() * Math.Abs(max));
            }
        }

        /// <summary>
        /// Clamps a double value within the specified range.
        /// </summary>
        /// <param name="value">The value to clamp.</param>
        /// <param name="minValue">The minimum value of the range.</param>
        /// <param name="maxValue">The maximum value of the range.</param>
        /// <returns>The clamped double value.</returns>
        public static double Clamp(double value, double minValue, double maxValue)
        {
            return maxValue < value ? maxValue : (value < minValue ? minValue : value);
        }

        /// <summary>
        /// Generates a random float value within the specified range.
        /// </summary>
        /// <param name="min">The minimum value of the range.</param>
        /// <param name="max">The maximum value of the range.</param>
        /// <returns>A random float value within the specified range.</returns>
        public static float Next(float min, float max)
        {
            lock (syncObj)
            {
                if (_random == null)
                    _random = new Random(); // Or exception...
                return ((float)_random.NextDouble() * Math.Abs(max - min)) + min;
            }
        }

        /// <summary>
        /// Generates a random float value between 0 (inclusive) and the specified maximum value (exclusive).
        /// </summary>
        /// <param name="max">The maximum value (exclusive) of the range.</param>
        /// <returns>A random float value between 0 (inclusive) and the specified maximum value (exclusive).</returns>
        public static float Next(float max)
        {
            lock (syncObj)
            {
                if (_random == null)
                    _random = new Random(); // Or exception...
                return ((float)_random.NextDouble() * Math.Abs(max));
            }
        }

        /// <summary>
        /// Clamps a float value to the specified range.
        /// </summary>
        /// <param name="value">The float value to clamp.</param>
        /// <param name="minValue">The minimum value of the range.</param>
        /// <param name="maxValue">The maximum value of the range.</param>
        /// <returns>The clamped float value within the specified range.</returns>
        public static float Clamp(float value, float minValue, float maxValue)
        {
            return maxValue < value ? maxValue : (value < minValue ? minValue : value);
        }

        /// <summary>
        /// Converts a byte array representing metadata to an integer.
        /// </summary>
        /// <param name="metadata">The byte array representing the metadata.</param>
        /// <returns>The converted <see cref="int"/>.</returns>
        public static int ConvertByteToInt32(byte[] metadata)
        {
            return BitConverter.ToInt32(metadata, 0);
        }

        /// <summary>
        /// Converts a byte array representing metadata to an unsigned integer.
        /// </summary>
        /// <param name="metadata">The byte array representing the metadata.</param>
        /// <returns>The converted <see cref="uint"/>.</returns>
        public static uint ConvertByteToUInt32(byte[] metadata)
        {
            return BitConverter.ToUInt32(metadata, 0);
        }

        /// <summary>
        /// Converts an integer to a byte array representing metadata.
        /// </summary>
        /// <param name="value"></param>
        /// <returns>The byte array representing the converted <see cref="int"/>.</returns>
        public static byte[] ConvertInt32ToMetadata(uint value)
        {
            return BitConverter.GetBytes(value);
        }

        /// <summary>
        /// Converts an unsigned integer to a byte array representing metadata.
        /// </summary>
        /// <param name="value"></param>
        /// <returns>The byte array representing the converted <see cref="uint"/>.</returns>
        public static byte[] ConvertUInt32ToMetadata(uint value)
        {
            return BitConverter.GetBytes(value);
        }
    }
}
