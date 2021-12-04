using FluentAssertions;
using NUnit.Framework;
using SensorsEvaluator.Objects;

namespace SensorsEvaluatorUnitTests
{
    /// <summary>
    /// Tests for <see cref="RoomEnvironment"/>.
    /// </summary>
    [TestFixture]
    public class RoomEnvironmentTests
    {
        [TestCase("needs more spaces")]
        [TestCase("needs less, exactly 3 spaces")]
        [TestCase("incorrect 70.0 45.0 6")]
        [TestCase("reference 70.0 notanum 6")]
        [TestCase("reference 70.0 -1 6")]
        [TestCase("reference 70.0 101 6")]
        [TestCase("reference 70.0 45.0 notanum")]
        public void TryParse_InvalidString_ReturnsFalse(string line)
        {
            // Act
            bool result = RoomEnvironment.TryParse(line, out RoomEnvironment roomEnvironment);

            // Assert
            result.Should().BeFalse();
            roomEnvironment.Should().BeNull();
        }

        [TestCase("reference 70.0 45.0 6")]
        [TestCase("reference 70 45.0 6")]
        [TestCase("reference -70 45.0 6")]
        [TestCase("reference 70.0 45 6")]
        [TestCase("reference 70.0 45.0 0")]
        [TestCase("reference 70.0 45.0 -1")]
        public void TryParse_ValidString_ReturnsTrue(string line)
        {
            // Act
            bool result = RoomEnvironment.TryParse(line, out _);

            // Assert
            result.Should().BeTrue();
        }

        [Test]
        public void TryParse_ValidString_ParsesCorrectly()
        {
            // Act
            _ = RoomEnvironment.TryParse("reference 70.0 45.0 6", out RoomEnvironment roomEnvironment);

            // Assert
            roomEnvironment.Should().NotBeNull();
            roomEnvironment.Temperature.Should().Be(70.0);
            roomEnvironment.Humidity.Should().Be(45.0);
            roomEnvironment.CoConcentration.Should().Be(6);
        }
    }
}
