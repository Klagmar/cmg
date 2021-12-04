using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleToAttribute("SensorsEvaluatorUnitTests")]

namespace SensorsEvaluator.Objects
{
    /// <summary>
    /// A representation of the stable environment a room was held at.
    /// </summary>
    internal class RoomEnvironment
    {
        /// <summary>
        /// The reference string denoting the room environment.
        /// </summary>
        private const string ReferenceString = "reference";

        /// <summary>
        /// The temperature in degrees Celcius.
        /// </summary>
        public double Temperature;

        /// <summary>
        /// The humidity percentage.
        /// </summary>
        public double Humidity;

        /// <summary>
        /// The carbon monoxide concentration in ppm.
        /// </summary>
        public int CoConcentration;

        /// <summary>
        /// Initializes a new instance of <see cref="RoomEnvironment"/>.
        /// </summary>
        internal RoomEnvironment() { }

        /// <summary>
        /// Attempts to parse the given string as a <see cref="RoomEnvironment"/>.
        /// </summary>
        /// <param name="line">The line to parse.</param>
        /// <param name="roomEnvironment">The resulting <see cref="RoomEnvironment"/> if successful; otherwise null.</param>
        /// <returns>True if parsing was successful; otherwise false.</returns>
        internal static bool TryParse(string line, [NotNullWhen(true)] out RoomEnvironment? roomEnvironment)
        {
            // Ensure the line isn't null
            roomEnvironment = null;
            if (string.IsNullOrWhiteSpace(line))
            {
                return false;
            }

            // Ensure the line has exactly 4 parts
            line = line.Trim();
            string[] parts = line.Split(null);
            if (parts.Length != 4)
            {
                return false;
            }

            // The first part should be the reference string
            if (!string.Equals(parts[0], ReferenceString, StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            // The second part is the temperature and should be parsed as a double
            RoomEnvironment resultRoomEnvironment = new RoomEnvironment();
            if (!double.TryParse(parts[1], out double temperature))
            {
                return false;
            }
            resultRoomEnvironment.Temperature = temperature;

            // The third part is the humidity and should be parsed as a double from 0-100 inclusive
            if (!double.TryParse(parts[2], out double humidity) || humidity < 0 || humidity > 100)
            {
                return false;
            }
            resultRoomEnvironment.Humidity = humidity;

            // The fourth part is the CO concentration and should be parsed as an integer
            if (!int.TryParse(parts[3], out int coConcentration))
            {
                return false;
            }
            resultRoomEnvironment.CoConcentration = coConcentration;
            roomEnvironment = resultRoomEnvironment;
            return true;
        }
    }
}
