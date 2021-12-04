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
    /// Tests for <see cref="CarbonMonoxideDetectorEvaluator"/>
    /// </summary>
    [TestFixture]
    public class CarbonMonoxideDetectorEvaluatorTests
    {
        private static string DateTimeString = DateTime.Now.ToString("yyy-mm-ddThh:mm");
        private AutoMocker _mocker = new AutoMocker();
        private CarbonMonoxideDetectorEvaluator _carbonMonoxideDetectorEvaluator;

        [SetUp]
        public void Setup()
        {
            _carbonMonoxideDetectorEvaluator = _mocker.CreateInstance<CarbonMonoxideDetectorEvaluator>();
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
                () => _carbonMonoxideDetectorEvaluator.EvaluateSensor(roomEnvironment, readingsList));
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
                () => _carbonMonoxideDetectorEvaluator.EvaluateSensor(roomEnvironment, readingsList));
            exception.Should().NotBeNull();
            exception.Message.Should().Contain("2 parts");
        }

        [TestCase("notanumber")]
        [TestCase("1.1")]
        [TestCase("1,")]
        public void EvaluateSensor_InvalidPpm_ThrowsException(string ppm)
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
                $"{DateTimeString} {ppm}",
            };

            // Act & Assert
            ArgumentException exception = Assert.Throws<ArgumentException>(
                () => _carbonMonoxideDetectorEvaluator.EvaluateSensor(roomEnvironment, readingsList));
            exception.Should().NotBeNull();
            exception.Message.Should().Contain("ppm");
        }

        [TestCase(1)]
        [TestCase(9)]
        [TestCase(0)]
        [TestCase(10)]
        public void EvaluateSensor_OneReadingAboveTolerance_ReturnsDiscard(int ppm)
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
                $"{DateTimeString} {ppm}",
            };

            // Act
            string result = _carbonMonoxideDetectorEvaluator.EvaluateSensor(roomEnvironment, readingsList);

            // Assert
            result.Should().Be("discard");
        }

        [TestCase(5)]
        [TestCase(4)]
        [TestCase(3)]
        [TestCase(2)]
        [TestCase(6)]
        [TestCase(7)]
        [TestCase(8)]
        public void EvaluateSensor_OneReadingWithinTolerance_ReturnsKeep(int ppm)
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
                $"{DateTimeString} {ppm}",
            };

            // Act
            string result = _carbonMonoxideDetectorEvaluator.EvaluateSensor(roomEnvironment, readingsList);

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
                CoConcentration = 5,
            };
            List<string> readingsList = new List<string>
            {
                $"{DateTimeString} 4",
                $"{DateTimeString} 6",
                $"{DateTimeString} 9",
                $"{DateTimeString} 5",
            };

            // Act
            string result = _carbonMonoxideDetectorEvaluator.EvaluateSensor(roomEnvironment, readingsList);

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
                CoConcentration = 5,
            };
            List<string> readingsList = new List<string>
            {
                $"{DateTimeString} 4",
                $"{DateTimeString} 6",
                $"{DateTimeString} 3",
                $"{DateTimeString} 5",
            };

            // Act
            string result = _carbonMonoxideDetectorEvaluator.EvaluateSensor(roomEnvironment, readingsList);

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
                "notadate 4",
                "87439857 6",
                "*&^&*^ 3",
                "iwishiwasadate 5",
            };

            // Act
            string result = _carbonMonoxideDetectorEvaluator.EvaluateSensor(roomEnvironment, readingsList);

            // Assert
            result.Should().Be("keep");
        }
    }
}