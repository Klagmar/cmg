using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using SensorsEvaluator.Objects;

[assembly: InternalsVisibleToAttribute("SensorsEvaluatorUnitTests")]

namespace SensorsEvaluator.SensorEvaluators
{
    /// <summary>
    /// An evaluator for humidity sensors.
    /// </summary>
    internal class HumiditySensorEvaluator : ISensorEvaluator
    {
        /// <summary>
        /// The tolerance for humidity readings, in percentage.
        /// </summary>
        private const double HumidityTolerance = 1;

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
                // Ensure the reading has exactly 2 parts (date and read percentage)
                string[] parts = reading.Split(null);
                if (parts.Length != 2)
                {
                    throw new ArgumentException("Readings should have 2 parts separated by whitespace");
                }

                // We don't have anything to do with the first part (date) and so can ignore it for now

                // Ensure the humidity can be read as a percentage
                if (!double.TryParse(parts[1], out double humidity) || humidity < 0 || humidity > 100)
                {
                    throw new ArgumentException("Read humidity should be a percentage");
                }

                // If it's above the tolerance, reject the whole sensor and exit early
                double deviation = Math.Abs(roomEnvironment.Humidity - humidity);
                if (deviation > HumidityTolerance)
                {
                    return DiscardResult;
                }
            }
            return KeepResult;
        }
    }
}
