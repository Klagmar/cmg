using System;
using System.Collections.Generic;
using FluentAssertions;
using HomeSensors;
using Moq;
using Newtonsoft.Json;
using NUnit.Framework;
using SensorsEvaluator.Objects;
using SensorsEvaluator.SensorEvaluators;

namespace SensorsEvaluatorUnitTests
{
    /// <summary>
    /// Tests for <see cref="LogEvaluator"/>
    /// </summary>
    [TestFixture]
    public class LogEvaluatorTests
    {
        private static string DateTimeString = DateTime.Now.ToString("yyy-mm-ddThh:mm");
        private Mock<ISensorEvaluator> _mockThermometerEvaluator;
        private Mock<ISensorEvaluator> _mockHumiditySensorEvaluator;
        private Mock<ISensorEvaluator> _mockCarbonMonoxideDetectorEvaluator;

        [SetUp]
        public void Setup()
        {
            _mockThermometerEvaluator = new Mock<ISensorEvaluator>();
            LogEvaluator.ThermometerEvaluator = _mockThermometerEvaluator.Object;
            _mockHumiditySensorEvaluator = new Mock<ISensorEvaluator>();
            LogEvaluator.HumiditySensorEvaluator = _mockHumiditySensorEvaluator.Object;
            _mockCarbonMonoxideDetectorEvaluator = new Mock<ISensorEvaluator>();
            LogEvaluator.CarbonMonoxideDetectorEvaluator = _mockCarbonMonoxideDetectorEvaluator.Object;
        }

        [TestCase(null)]
        [TestCase("")]
        [TestCase("           ")]
        public void EvaluateLogFile_EmptyFile_ThrowsException(string fileContents)
        {
            // Act & Assert
            ArgumentException exception = Assert.Throws<ArgumentException>(
                () => LogEvaluator.EvaluateLogFile(fileContents));
            exception.Should().NotBeNull();
            exception.Message.Should().Contain("must not be empty");
        }

        [Test]
        public void EvaluateLogFile_FirstLineDoesNotRepresentRoomEnvironment_ThrowsException()
        {
            // Act & Assert
            ArgumentException exception = Assert.Throws<ArgumentException>(
                () => LogEvaluator.EvaluateLogFile("very invalid string"));
            exception.Should().NotBeNull();
            exception.Message.Should().Contain("must represent a room environment");
        }

        [Test]
        public void EvaluateLogFile_InvalidReading_ThrowsException()
        {
            // Arrange
            string fileContents = $@"reference 70.0 45.0 6 
too many spaces in this";

            // Act & Assert
            ArgumentException exception = Assert.Throws<ArgumentException>(
                () => LogEvaluator.EvaluateLogFile(fileContents));
            exception.Should().NotBeNull();
            exception.Message.Should().Contain("expected format");
        }

        [Test]
        public void EvaluateLogFile_ReadingWithNoSensor_SkipsReading()
        {
            // Arrange
            string fileContents = $@"reference 70.0 45.0 6 
{DateTimeString} 45.0";

            // Act
            string result = LogEvaluator.EvaluateLogFile(fileContents);

            // Assert
            Assert.IsNotNull(result);
            var dictionary = DeserializeSafely(result);
            dictionary.Should().BeEmpty();
        }

        [Test]
        public void EvaluateLogFile_ReadingsForOneThermometer_CallsThermometerEvaluator()
        {
            // Arrange
            string fileContents = $@"reference 70.0 45.0 6
thermometer temp-1
{DateTimeString} 45.0
{DateTimeString} 46.0
{DateTimeString} 47.0";
            string expectedResult = "precise";
            RoomEnvironment capturedRoomEnvironment = default;
            List<string> capturedReadingsList = default;
            _mockThermometerEvaluator.Setup(t => t.EvaluateSensor(It.IsAny<RoomEnvironment>(), It.IsAny<List<string>>()))
                .Callback((RoomEnvironment receivedRoomEnvironment, List<string> receivedReadingsList) =>
                {
                    capturedRoomEnvironment = receivedRoomEnvironment;
                    capturedReadingsList = receivedReadingsList;
                })
                .Returns(expectedResult);

            // Act
            _ = LogEvaluator.EvaluateLogFile(fileContents);

            // Assert
            _mockThermometerEvaluator.Verify(t => t.EvaluateSensor(It.IsAny<RoomEnvironment>(), It.IsAny<List<string>>()), Times.Once);
            RoomEnvironment expectedRoomEnvironment = new RoomEnvironment
            {
                Temperature = 70.0,
                Humidity = 45.0,
                CoConcentration = 6
            };
            capturedRoomEnvironment.Should().BeEquivalentTo(expectedRoomEnvironment);
            List<string> expectedReadingsList = new List<string>
            {
                $"{DateTimeString} 45.0",
                $"{DateTimeString} 46.0",
                $"{DateTimeString} 47.0",
            };
            capturedReadingsList.Should().BeEquivalentTo(expectedReadingsList);
        }

        [Test]
        public void EvaluateLogFile_ReadingsForOneThermometer_IncludesInResult()
        {
            // Arrange
            string fileContents = $@"reference 70.0 45.0 6
thermometer temp-1
{DateTimeString} 45.0
{DateTimeString} 46.0
{DateTimeString} 47.0";
            string expectedResult = "precise";
            _mockThermometerEvaluator.Setup(t => t.EvaluateSensor(It.IsAny<RoomEnvironment>(), It.IsAny<List<string>>())).Returns(expectedResult);

            // Act
            string result = LogEvaluator.EvaluateLogFile(fileContents);

            // Assert
            Assert.IsNotNull(result);
            var dictionary = DeserializeSafely(result);
            Dictionary<string, string> expectedDictionary = new Dictionary<string, string>
            {
                ["temp-1"] = expectedResult,
            };
            dictionary.Should().BeEquivalentTo(expectedDictionary);
        }

        [Test]
        public void EvaluateLogFile_ReadingsForOneHumiditySensor_CallsHumiditySensorEvaluator()
        {
            // Arrange
            string fileContents = $@"reference 70.0 45.0 6
humidity hum-1
{DateTimeString} 44.4
{DateTimeString} 45.6
{DateTimeString} 47.6";
            string expectedResult = "keep";
            RoomEnvironment capturedRoomEnvironment = default;
            List<string> capturedReadingsList = default;
            _mockHumiditySensorEvaluator.Setup(t => t.EvaluateSensor(It.IsAny<RoomEnvironment>(), It.IsAny<List<string>>()))
                .Callback((RoomEnvironment receivedRoomEnvironment, List<string> receivedReadingsList) =>
                {
                    capturedRoomEnvironment = receivedRoomEnvironment;
                    capturedReadingsList = receivedReadingsList;
                })
                .Returns(expectedResult);

            // Act
            _ = LogEvaluator.EvaluateLogFile(fileContents);

            // Assert
            _mockHumiditySensorEvaluator.Verify(t => t.EvaluateSensor(It.IsAny<RoomEnvironment>(), It.IsAny<List<string>>()), Times.Once);
            RoomEnvironment expectedRoomEnvironment = new RoomEnvironment
            {
                Temperature = 70.0,
                Humidity = 45.0,
                CoConcentration = 6
            };
            capturedRoomEnvironment.Should().BeEquivalentTo(expectedRoomEnvironment);
            List<string> expectedReadingsList = new List<string>
            {
                $"{DateTimeString} 44.4",
                $"{DateTimeString} 45.6",
                $"{DateTimeString} 47.6",
            };
            capturedReadingsList.Should().BeEquivalentTo(expectedReadingsList);
        }

        [Test]
        public void EvaluateLogFile_ReadingsForOneHumiditySensor_IncludesInResult()
        {
            // Arrange
            string fileContents = $@"reference 70.0 45.0 6
humidity hum-1
{DateTimeString} 44.4
{DateTimeString} 45.6
{DateTimeString} 47.6";
            string expectedResult = "keep";
            _mockHumiditySensorEvaluator.Setup(t => t.EvaluateSensor(It.IsAny<RoomEnvironment>(), It.IsAny<List<string>>())).Returns(expectedResult);

            // Act
            string result = LogEvaluator.EvaluateLogFile(fileContents);

            // Assert
            Assert.IsNotNull(result);
            var dictionary = DeserializeSafely(result);
            Dictionary<string, string> expectedDictionary = new Dictionary<string, string>
            {
                ["hum-1"] = expectedResult,
            };
            dictionary.Should().BeEquivalentTo(expectedDictionary);
        }

        [Test]
        public void EvaluateLogFile_ReadingsForOneCarbonMonoxideDetector_CallsCarbonMonoxideDetectorEvaluator()
        {
            // Arrange
            string fileContents = $@"reference 70.0 45.0 6
monoxide mon-1
{DateTimeString} 5
{DateTimeString} 7
{DateTimeString} 9";
            string expectedResult = "keep";
            RoomEnvironment capturedRoomEnvironment = default;
            List<string> capturedReadingsList = default;
            _mockCarbonMonoxideDetectorEvaluator.Setup(t => t.EvaluateSensor(It.IsAny<RoomEnvironment>(), It.IsAny<List<string>>()))
                .Callback((RoomEnvironment receivedRoomEnvironment, List<string> receivedReadingsList) =>
                {
                    capturedRoomEnvironment = receivedRoomEnvironment;
                    capturedReadingsList = receivedReadingsList;
                })
                .Returns(expectedResult);

            // Act
            _ = LogEvaluator.EvaluateLogFile(fileContents);

            // Assert
            _mockCarbonMonoxideDetectorEvaluator.Verify(t => t.EvaluateSensor(It.IsAny<RoomEnvironment>(), It.IsAny<List<string>>()), Times.Once);
            RoomEnvironment expectedRoomEnvironment = new RoomEnvironment
            {
                Temperature = 70.0,
                Humidity = 45.0,
                CoConcentration = 6
            };
            capturedRoomEnvironment.Should().BeEquivalentTo(expectedRoomEnvironment);
            List<string> expectedReadingsList = new List<string>
            {
                $"{DateTimeString} 5",
                $"{DateTimeString} 7",
                $"{DateTimeString} 9",
            };
            capturedReadingsList.Should().BeEquivalentTo(expectedReadingsList);
        }

        [Test]
        public void EvaluateLogFile_ReadingsForOneCarbonMonoxideDetector_IncludesInResult()
        {
            // Arrange
            string fileContents = $@"reference 70.0 45.0 6
monoxide mon-1
{DateTimeString} 5
{DateTimeString} 7
{DateTimeString} 9";
            string expectedResult = "keep";
            _mockCarbonMonoxideDetectorEvaluator.Setup(t => t.EvaluateSensor(It.IsAny<RoomEnvironment>(), It.IsAny<List<string>>())).Returns(expectedResult);

            // Act
            string result = LogEvaluator.EvaluateLogFile(fileContents);

            // Assert
            Assert.IsNotNull(result);
            var dictionary = DeserializeSafely(result);
            Dictionary<string, string> expectedDictionary = new Dictionary<string, string>
            {
                ["mon-1"] = expectedResult,
            };
            dictionary.Should().BeEquivalentTo(expectedDictionary);
        }

        [Test]
        public void EvaluateLogFile_ReadingsForMultipleSensors_IncludesAllResults()
        {
            // Arrange
            string fileContents = $@"reference 70.0 45.0 6
thermometer temp-1
{DateTimeString} 45.0
{DateTimeString} 46.0
{DateTimeString} 47.0
humidity hum-1
{DateTimeString} 44.4
{DateTimeString} 45.6
{DateTimeString} 47.6
monoxide mon-1
{DateTimeString} 5
{DateTimeString} 7
{DateTimeString} 9
thermometer temp-2
{DateTimeString} 45.0
{DateTimeString} 46.0
{DateTimeString} 47.0
humidity hum-2
{DateTimeString} 44.4
{DateTimeString} 45.6
{DateTimeString} 47.6
monoxide mon-2
{DateTimeString} 5
{DateTimeString} 7
{DateTimeString} 9";
            string expectedThermometerResult = "precise";
            string expectedHumidityResult = "keep";
            string expectedMonoxideResult = "keep";
            _mockThermometerEvaluator.Setup(t => t.EvaluateSensor(It.IsAny<RoomEnvironment>(), It.IsAny<List<string>>())).Returns(expectedThermometerResult);
            _mockHumiditySensorEvaluator.Setup(t => t.EvaluateSensor(It.IsAny<RoomEnvironment>(), It.IsAny<List<string>>())).Returns(expectedHumidityResult);
            _mockCarbonMonoxideDetectorEvaluator.Setup(t => t.EvaluateSensor(It.IsAny<RoomEnvironment>(), It.IsAny<List<string>>())).Returns(expectedMonoxideResult);

            // Act
            string result = LogEvaluator.EvaluateLogFile(fileContents);

            // Assert
            Assert.IsNotNull(result);
            var dictionary = DeserializeSafely(result);
            Dictionary<string, string> expectedDictionary = new Dictionary<string, string>
            {
                ["temp-1"] = expectedThermometerResult,
                ["temp-2"] = expectedThermometerResult,
                ["hum-1"] = expectedHumidityResult,
                ["hum-2"] = expectedHumidityResult,
                ["mon-1"] = expectedMonoxideResult,
                ["mon-2"] = expectedMonoxideResult,
            };
            dictionary.Should().BeEquivalentTo(expectedDictionary);
        }

        private Dictionary<string, string> DeserializeSafely(string str)
        {
            try
            {
                return JsonConvert.DeserializeObject<Dictionary<string, string>>(str);
            }
            catch
            {
                Assert.Fail($"Could not deserialize {str}");
                return null;
            }
        }
    }
}
