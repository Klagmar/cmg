using System;
using System.Collections.Generic;
using System.Linq;
using SensorsEvaluator.Objects;

namespace SensorsEvaluator.SensorEvaluators
{
    /// <summary>
    /// An evaluator for carbon monoxide detectors.
    /// </summary>
    internal class CarbonMonoxideDetectorEvaluator : ISensorEvaluator
    {
        /// <summary>
        /// The tolerance value in ppm.
        /// </summary>
        private const int CoTolerance = 3;

        /// <summary>
        /// The result when the sensor should be kept.
        /// </summary>
        private const string KeepResult = "keep";

        /// <summary>
        /// The result when the sensor should be discarded.
        /// </summary>
        private const string DiscardResult = "discard";

        /// <inheritdoc/>
        public string EvaluateSensor(RoomEnvironment roomEnvironment, List<string> readingsList)
        {
            // Ensure there is at least one reading
            if (!readingsList.Any())
            {
                throw new ArgumentException("There should be at least one reading");
            }

            // Evaluate each reading
            foreach (string reading in readingsList)
            {
                // Ensure the reading has exactly 2 parts (date and read ppm)
                string[] parts = reading.Split(null);
                if (parts.Length != 2)
                {
                    throw new ArgumentException("Readings should have 2 parts separated by whitespace");
                }

                // We don't have anything to do with the first part (date) and so can ignore it for now

                // Ensure the ppm can be parsed as an int
                if (!int.TryParse(parts[1], out int ppm))
                {
                    throw new ArgumentException("Read ppm should be an integer");
                }

                // Calculate the deviation from the actual CO concentration
                double deviation = Math.Abs(roomEnvironment.CoConcentration - ppm);

                // If it's above the tolerance, reject the whole sensor and exit early
                if (deviation > CoTolerance)
                {
                    return DiscardResult;
                }
            }
            return KeepResult;
        }
    }
}
