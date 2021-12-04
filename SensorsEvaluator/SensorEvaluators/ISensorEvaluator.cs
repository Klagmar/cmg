using System.Collections.Generic;
using System.Runtime.CompilerServices;
using SensorsEvaluator.Objects;

[assembly: InternalsVisibleTo("DynamicProxyGenAssembly2")]

namespace SensorsEvaluator.SensorEvaluators
{
    /// <summary>
    /// The specification for an evaluator of home sensors.
    /// </summary>
    internal interface ISensorEvaluator
    {
        /// <summary>
        /// Evaluates a sensor.
        /// </summary>
        /// <param name="roomEnvironment">The stable <see cref="RoomEnvironment"/>.</param>
        /// <param name="readingsList">The list of readings for the sensor being evaluated.</param>
        /// <returns>The evaluation result.</returns>
        string EvaluateSensor(RoomEnvironment roomEnvironment, List<string> readingsList);
    }
}
