using System;
using System.Collections.Generic;
using System.Linq;
using SensorsEvaluator.Objects;

namespace SensorsEvaluator.SensorEvaluators
{
    /// <summary>
    /// An evaluator for thermometers.
    /// </summary>
    internal class ThermometerEvaluator : ISensorEvaluator
    {
        /// <summary>
        /// The result when the sensor should be classified as ultra precise.
        /// </summary>
        private const string UltraPreciseResult = "ultra precise";

        /// <summary>
        /// The tolerance for the mean of the readings when the sensor should be classified as ultra precise, in degrees Celcius.
        /// </summary>
        private const double UltraPreciseMeanVariationTolerance = 0.5;

        /// <summary>
        /// The tolerance for the standard deviation of the readings when the sensor should be classified as ultra precise, in degrees Celcius.
        /// </summary>
        private const double UltraPreciseStandardDeviationTolerance = 3;

        /// <summary>
        /// The result when the sensor should be classified as very precise.
        /// </summary>
        private const string VeryPreciseResult = "very precise";

        /// <summary>
        /// The tolerance for the mean of the readings when the sensor should be classified as very precise, in degrees Celcius.
        /// </summary>
        private const double VeryPreciseMeanVariationTolerance = 0.5;

        /// <summary>
        /// The tolerance for the standard deviation of the readings when the sensor should be classified as very precise, in degrees Celcius.
        /// </summary>
        private const double VeryPreciseStandardDeviationTolerance = 5;

        /// <summary>
        /// The result when the sensor should be classified as precise.
        /// </summary>
        private const string PreciseResult = "precise";

        /// <inheritdoc/>
        public string EvaluateSensor(RoomEnvironment roomEnvironment, List<string> readingsList)
        {
            // Ensure there is at least one reading
            if (!readingsList.Any())
            {
                throw new ArgumentException("There should be at least one reading");
            }

            // Aggregate all the readings
            List<double> values = new List<double>();
            foreach (string reading in readingsList)
            {
                // Ensure the reading has exactly 2 parts (date and read temperature)
                string[] parts = reading.Split(null);
                if (parts.Length != 2)
                {
                    throw new ArgumentException("Readings should have 2 parts separated by whitespace");
                }

                // We don't have anything to do with the first part (date) and so can ignore it for now

                // Ensure the humidity can be read as a temperature
                if (!double.TryParse(parts[1], out double temp))
                {
                    throw new ArgumentException("Read temperature should be a decimal value");
                }

                values.Add(temp);
            }

            // Calculate the mean and standard deviation from the list of read values
            (double mean, double stdDeviation) = CalculateMeanAndStandardDeviation(values);

            // Calculate the mean inacuracy
            double meanInaccuracy = Math.Abs(roomEnvironment.Temperature - mean);

            // Return ultra precise if it's within its tolerances
            if (meanInaccuracy <= UltraPreciseMeanVariationTolerance && stdDeviation < UltraPreciseStandardDeviationTolerance)
            {
                return UltraPreciseResult;
            }

            // Otherwise return very precise if it's within its tolerances
            else if (meanInaccuracy <= VeryPreciseMeanVariationTolerance && stdDeviation < VeryPreciseStandardDeviationTolerance)
            {
                return VeryPreciseResult;
            }

            // Otherwise return precise
            return PreciseResult;
        }

        /// <summary>
        /// Calculates the mean and standard deviation of a list of values.
        /// </summary>
        /// <param name="data">The list of values.</param>
        /// <returns>
        ///     - The mean.
        ///     - The standard deviation.
        /// </returns>
        /// <remarks>Adapted from https://stackoverflow.com/questions/895929/how-do-i-determine-the-standard-deviation-stddev-of-a-set-of-values </remarks>
        private static (double, double) CalculateMeanAndStandardDeviation(List<double> data)
        {
            double sumAll = 0;
            double sumAllQ = 0;

            // Sum of x and sum of x²
            for (int i = 0; i < data.Count; i++)
            {
                double x = data[i];
                sumAll += x;
                sumAllQ += x * x;
            }

            // Mean
            double mean = sumAll / (double)data.Count;

            if (data.Count == 1)
            {
                return (mean, 0);
            }

            // Standard deviation
            double stdDev = Math.Sqrt(
                (sumAllQ -
                (sumAll * sumAll) / data.Count) *
                (1.0d / (data.Count - 1)));
            return (mean, stdDev);
        }
    }
}
