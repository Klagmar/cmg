using System;
using System.Collections.Generic;
using FluentAssertions;
using Moq.AutoMock;
using NUnit.Framework;
using SensorsEvaluator.Objects;
using SensorsEvaluator.SensorEvaluators;

namespace SensorsEvaluatorUnitTests.SensorEvaluators
{
    /// <summary>
    /// Tests for <see cref="HumiditySensorEvaluator"/>
    /// </summary>
    [TestFixture]
    public class HumiditySensorEvaluatorTests
    {
        private static string DateTimeString = DateTime.Now.ToString("yyy-mm-ddThh:mm");
        private AutoMocker _mocker = new AutoMocker();
        private HumiditySensorEvaluator _humiditySensorEvaluator;

        [SetUp]
        public void Setup()
        {
            _humiditySensorEvaluator = _mocker.CreateInstance<HumiditySensorEvaluator>();
        }

        [Test]
        public void EvaluateSensor_EmptyReadingsList_ThrowsException()
        {
            // Arrange
            RoomEnvironment roomEnvironment = new RoomEnvironment
            {
                Temperature = 10,
                Humidity = 25,
                CoConcentration = 3,
            };
            List<string> readingsList = new List<string>();

            // Act & Assert
            ArgumentException exception = Assert.Throws<ArgumentException>(
                () => _humiditySensorEvaluator.EvaluateSensor(roomEnvironment, readingsList));
            exception.Should().NotBeNull();
            exception.Message.Should().Contain("at least one");
        }

        [Test]
        public void EvaluateSensor_InvalidReadingFormat_ThrowsException()
        {
            // Arrange
            RoomEnvironment roomEnvironment = new RoomEnvironment
            {
                Temperature = 10,
                Humidity = 25,
                CoConcentration = 3,
            };
            List<string> readingsList = new List<string>
            {
                "this is invalid too many spaces",
            };

            // Act & Assert
            ArgumentException exception = Assert.Throws<ArgumentException>(
                () => _humiditySensorEvaluator.EvaluateSensor(roomEnvironment, readingsList));
            exception.Should().NotBeNull();
            exception.Message.Should().Contain("2 parts");
        }

        [TestCase("notanumber")]
        [TestCase("-1")]
        [TestCase("101")]
        public void EvaluateSensor_InvalidPercentage_ThrowsException(string percentage)
        {
            // Arrange
            RoomEnvironment roomEnvironment = new RoomEnvironment
            {
                Temperature = 10,
                Humidity = 25,
                CoConcentration = 3,
            };
            List<string> readingsList = new List<string>
            {
                $"{DateTimeString} {percentage}",
            };

            // Act & Assert
            ArgumentException exception = Assert.Throws<ArgumentException>(
                () => _humiditySensorEvaluator.EvaluateSensor(roomEnvironment, readingsList));
            exception.Should().NotBeNull();
            exception.Message.Should().Contain("percentage");
        }

        [TestCase(27)]
        [TestCase(23)]
        [TestCase(26.1)]
        [TestCase(23.9)]
        [TestCase(0)]
        [TestCase(100)]
        public void EvaluateSensor_OneReadingAboveTolerance_ReturnsDiscard(double percentage)
        {
            // Arrange
            RoomEnvironment roomEnvironment = new RoomEnvironment
            {
                Temperature = 10,
                Humidity = 25,
                CoConcentration = 3,
            };
            List<string> readingsList = new List<string>
            {
                $"{DateTimeString} {percentage}",
            };

            // Act
            string result = _humiditySensorEvaluator.EvaluateSensor(roomEnvironment, readingsList);

            // Assert
            result.Should().Be("discard");
        }

        [TestCase(25)]
        [TestCase(26)]
        [TestCase(24)]
        [TestCase(25.9)]
        [TestCase(24.1)]
        public void EvaluateSensor_OneReadingWithinTolerance_ReturnsKeep(double percentage)
        {
            // Arrange
            RoomEnvironment roomEnvironment = new RoomEnvironment
            {
                Temperature = 10,
                Humidity = 25,
                CoConcentration = 3,
            };
            List<string> readingsList = new List<string>
            {
                $"{DateTimeString} {percentage}",
            };

            // Act
            string result = _humiditySensorEvaluator.EvaluateSensor(roomEnvironment, readingsList);

            // Assert
            result.Should().Be("keep");
        }

        [Test]
        public void EvaluateSensor_MultipleReadingsWithOneAboveTolerance_ReturnsDiscard()
        {
            // Arrange
            RoomEnvironment roomEnvironment = new RoomEnvironment
            {
                Temperature = 10,
                Humidity = 25,
                CoConcentration = 3,
            };
            List<string> readingsList = new List<string>
            {
                $"{DateTimeString} 24.5",
                $"{DateTimeString} 25.5",
                $"{DateTimeString} 26.5",
                $"{DateTimeString} 25",
            };

            // Act
            string result = _humiditySensorEvaluator.EvaluateSensor(roomEnvironment, readingsList);

            // Assert
            result.Should().Be("discard");
        }

        [Test]
        public void EvaluateSensor_MultipleReadingsWithNoneAboveTolerance_ReturnsKeep()
        {
            // Arrange
            RoomEnvironment roomEnvironment = new RoomEnvironment
            {
                Temperature = 10,
                Humidity = 25,
                CoConcentration = 3,
            };
            List<string> readingsList = new List<string>
            {
                "{DateTimeString} 24.5",
                "{DateTimeString} 25.5",
                "{DateTimeString} 25.0",
                "{DateTimeString} 25",
            };

            // Act
            string result = _humiditySensorEvaluator.EvaluateSensor(roomEnvironment, readingsList);

            // Assert
            result.Should().Be("keep");
        }

        [Test]
        public void EvaluateSensor_MultipleReadingsWithInvalidDates_StillReturnsKeep()
        {
            // Arrange
            RoomEnvironment roomEnvironment = new RoomEnvironment
            {
                Temperature = 10,
                Humidity = 25,
                CoConcentration = 5,
            };
            List<string> readingsList = new List<string>
            {
                "notadate 24",
                "87439857 26",
                "*&^&*^ 24",
                "iwishiwasadate 25",
            };

            // Act
            string result = _humiditySensorEvaluator.EvaluateSensor(roomEnvironment, readingsList);

            // Assert
            result.Should().Be("keep");
        }
    }
}