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
    /// Tests for <see cref="ThermometerEvaluator"/>
    /// </summary>
    [TestFixture]
    public class ThermometerEvaluatorTests
    {
        private static string DateTimeString = DateTime.Now.ToString("yyy-mm-ddThh:mm");
        private AutoMocker _mocker = new AutoMocker();
        private ThermometerEvaluator _thermometerEvaluator;

        [SetUp]
        public void Setup()
        {
            _thermometerEvaluator = _mocker.CreateInstance<ThermometerEvaluator>();
        }

        [Test]
        public void EvaluateSensor_EmptyReadingsList_ThrowsException()
        {
            // Arrange
            RoomEnvironment roomEnvironment = new RoomEnvironment
            {
                Temperature = 10,
                Humidity = 25,
                CoConcentration = 5,
            };
            List<string> readingsList = new List<string>();

            // Act & Assert
            ArgumentException exception = Assert.Throws<ArgumentException>(
                () => _thermometerEvaluator.EvaluateSensor(roomEnvironment, readingsList));
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
                CoConcentration = 5,
            };
            List<string> readingsList = new List<string>
            {
                "this is invalid too many spaces",
            };

            // Act & Assert
            ArgumentException exception = Assert.Throws<ArgumentException>(
                () => _thermometerEvaluator.EvaluateSensor(roomEnvironment, readingsList));
            exception.Should().NotBeNull();
            exception.Message.Should().Contain("2 parts");
        }

        [TestCase("notanumber")]
        public void EvaluateSensor_InvalidTemperature_ThrowsException(string temperature)
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
                $"{DateTimeString} {temperature}",
            };

            // Act & Assert
            ArgumentException exception = Assert.Throws<ArgumentException>(
                () => _thermometerEvaluator.EvaluateSensor(roomEnvironment, readingsList));
            exception.Should().NotBeNull();
            exception.Message.Should().Contain("temperature");
        }

        [TestCase(9)]
        [TestCase(11)]
        [TestCase(10.6)]
        [TestCase(9.4)]
        public void EvaluateSensor_OneReadingOutsideOfTolerances_ReturnsPrecise(double temperature)
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
                $"{DateTimeString} {temperature}",
            };

            // Act
            string result = _thermometerEvaluator.EvaluateSensor(roomEnvironment, readingsList);

            // Assert
            result.Should().Be("precise");
        }

        [TestCase(10)]
        [TestCase(10.5)]
        [TestCase(9.5)]
        public void EvaluateSensor_WithinTolerances_ReturnsUltraPrecise(double temperature)
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
                $"{DateTimeString} {temperature}",
            };

            // Act
            string result = _thermometerEvaluator.EvaluateSensor(roomEnvironment, readingsList);

            // Assert
            result.Should().Be("ultra precise");
        }

        [Test]
        public void EvaluateSensor_MultipleReadingsWithOneWayOff_ReturnsPrecise()
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
                $"{DateTimeString} {10}",
                $"{DateTimeString} {10}",
                $"{DateTimeString} {100}",
                $"{DateTimeString} {10}",
            };

            // Act
            string result = _thermometerEvaluator.EvaluateSensor(roomEnvironment, readingsList);

            // Assert
            result.Should().Be("precise");
        }

        [Test]
        public void EvaluateSensor_MultipleReadingsWithStandardDeivationWithinVeryPreciseTolerance_ReturnsVeryPrecise()
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
                $"{DateTimeString} 13",
                $"{DateTimeString} 7",
                $"{DateTimeString} 13",
                $"{DateTimeString} 7",
            };

            // Act
            string result = _thermometerEvaluator.EvaluateSensor(roomEnvironment, readingsList);

            // Assert
            result.Should().Be("very precise");
        }

        [Test]
        public void EvaluateSensor_MultipleReadingsWithAllWithinTolerance_ReturnsUltraPrecise()
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
                $"{DateTimeString} 10",
                $"{DateTimeString} 10.1",
                $"{DateTimeString} 9.9",
                $"{DateTimeString} 10.5",
            };

            // Act
            string result = _thermometerEvaluator.EvaluateSensor(roomEnvironment, readingsList);

            // Assert
            result.Should().Be("ultra precise");
        }

        [Test]
        public void EvaluateSensor_MultipleReadingsWithInvalidDates_StillReturnsUltraPrecise()
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
                $"notadate 10",
                $"87439857 10.1",
                $"*&^&*^ 9.9",
                $"iwishiwasadate 10.5",
            };

            // Act
            string result = _thermometerEvaluator.EvaluateSensor(roomEnvironment, readingsList);

            // Assert
            result.Should().Be("ultra precise");
        }
    }
}