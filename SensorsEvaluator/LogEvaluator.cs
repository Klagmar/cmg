using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using SensorsEvaluator.Objects;
using SensorsEvaluator.SensorEvaluators;

namespace HomeSensors
{
    /// <summary>
    /// An evaluator that operates on a log file representing the outputs of multiple sensors.
    /// </summary>
    public static class LogEvaluator
    {
        internal static ISensorEvaluator ThermometerEvaluator = new ThermometerEvaluator();
        internal static ISensorEvaluator HumiditySensorEvaluator = new HumiditySensorEvaluator();
        internal static ISensorEvaluator CarbonMonoxideDetectorEvaluator = new CarbonMonoxideDetectorEvaluator();

        /// <summary>
        /// Evaluates a log file of sensor data and outputs a serialized dictionary of the results for each sensor.
        /// </summary>
        /// <param name="logContentsStr">The log file to evaluate.</param>
        /// <returns>The mapping of sensor results, with sensor names mapped to results.</returns>
        /// <remarks>TODO: Update evaluations to run asynchronously, to let us parse and evaluate simultaneously.</remarks>
        public static string EvaluateLogFile(string logContentsStr)
        {
            // Ensure there are file contents
            if (string.IsNullOrWhiteSpace(logContentsStr))
            {
                throw new ArgumentException("Log file must not be empty");
            }

            // Ensure we have the first line that gives the room constants
            using StringReader reader = new StringReader(logContentsStr);
            string? line = reader.ReadLine();
            if (string.IsNullOrWhiteSpace(line))
            {
                throw new InvalidOperationException("Log file must contain a first line");
            }

            // Ensure it accurately represents the room environment
            if (!RoomEnvironment.TryParse(line, out RoomEnvironment? roomEnvironment))
            {
                throw new ArgumentException("Log file first line must represent a room environment");
            }

            // Read each line
            ConcurrentDictionary<string, string> resultsDictionary = new ConcurrentDictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            string? currentSensorName = default;
            SensorType currentSensorType = default;
            List<string> readingsList = new List<string>();
            while ((line = reader.ReadLine()) != null)
            {
                // No matter what the line represents, it should always have 2 sections separated by whitespace
                line = line.Trim();
                string[] parts = line.Split(null);
                if (parts.Length != 2)
                {
                    throw new ArgumentException($"Line did not have the expected format. Line: {line}");
                }

                // If the first part refers to to a new sensor, we can handle the old one and update
                if (Enum.TryParse(parts[0], true, out SensorType sensorType))
                {
                    // Ensure it wasn't Unknown
                    if (sensorType == SensorType.Unknown)
                    {
                        throw new InvalidOperationException("Sensor type should not be unknown");
                    }

                    // If the last sensor had any readings, evaluate it before updating with the new info
                    if (readingsList.Any() && !string.IsNullOrWhiteSpace(currentSensorName))
                    {
                        EvaluateSensor(resultsDictionary, currentSensorName, roomEnvironment, currentSensorType, readingsList);
                    }

                    // Update with the info for the new sensor
                    readingsList = new List<string>();
                    currentSensorName = parts[1];
                    currentSensorType = sensorType;
                }

                // Otherwise, add the line to the readings list (to be evaluated later)
                else
                {
                    readingsList.Add(line);
                }
            }

            // If the final sensor had any readings, evaluate it
            if (readingsList.Any() && !string.IsNullOrWhiteSpace(currentSensorName))
            {
                EvaluateSensor(resultsDictionary, currentSensorName, roomEnvironment, currentSensorType, readingsList);
            }

            // Return the json-serialized dictionary
            return JsonConvert.SerializeObject(resultsDictionary);
        }

        /// <summary>
        /// Evaluates a sensor.
        /// </summary>
        /// <param name="resultsDictionary">The dictionary to add the result to.</param>
        /// <param name="sensorName">The sensor name.</param>
        /// <param name="roomEnvironment">The <see cref="RoomEnvironment"/>.</param>
        /// <param name="sensorType">The <see cref="SensorType"/>.</param>
        /// <param name="readingsList">The list of readings gathered for that sensor.</param>
        private static void EvaluateSensor(
            ConcurrentDictionary<string, string> resultsDictionary,
            string sensorName,
            RoomEnvironment roomEnvironment,
            SensorType sensorType,
            List<string> readingsList)
        {
            // Use a different evaluator depending on the type
            ISensorEvaluator sensorEvaluator;
            switch (sensorType)
            {
                case SensorType.Thermometer:
                    sensorEvaluator = ThermometerEvaluator;
                    break;
                case SensorType.Humidity:
                    sensorEvaluator = HumiditySensorEvaluator;
                    break;
                case SensorType.Monoxide:
                    sensorEvaluator = CarbonMonoxideDetectorEvaluator;
                    break;
                default:
                    throw new InvalidOperationException($"Invalid sensor type {sensorType}");
            }

            // Evaluate and add the result to the dictionary
            string result = sensorEvaluator.EvaluateSensor(roomEnvironment, readingsList);
            resultsDictionary[sensorName] = result;
        }
    }
}
