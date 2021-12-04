using FluentAssertions;
using HomeSensors;
using NUnit.Framework;

namespace SensorsEvaluatorIntegrationTests
{
    /// <summary>
    /// Tests for <see cref="LogEvaluator"/>
    /// </summary>
    [TestFixture]
    public class LogEvaluatorTests
    {
        [Test]
        public void EvaluateLogFile_ExampleFromProblemSpec_ReturnsExpectedResult()
        {
            // Arrange
            string fileContents = $@"reference 70.0 45.0 6 
thermometer temp-1 
2007-04-05T22:00 72.4 
2007-04-05T22:01 76.0 
2007-04-05T22:02 79.1 
2007-04-05T22:03 75.6 
2007-04-05T22:04 71.2 
2007-04-05T22:05 71.4 
2007-04-05T22:06 69.2 
2007-04-05T22:07 65.2 
2007-04-05T22:08 62.8 
2007-04-05T22:09 61.4 
2007-04-05T22:10 64.0 
2007-04-05T22:11 67.5 
2007-04-05T22:12 69.4 
thermometer temp-2 
2007-04-05T22:01 69.5 
2007-04-05T22:02 70.1 
2007-04-05T22:03 71.3 
2007-04-05T22:04 71.5 
2007-04-05T22:05 69.8 
humidity hum-1 
2007-04-05T22:04 45.2 
2007-04-05T22:05 45.3 
2007-04-05T22:06 45.1 
humidity hum-2 
2007-04-05T22:04 44.4 
2007-04-05T22:05 43.9 
2007-04-05T22:06 44.9 
2007-04-05T22:07 43.8 
2007-04-05T22:08 42.1 
monoxide mon-1 
2007-04-05T22:04 5 
2007-04-05T22:05 7 
2007-04-05T22:06 9 
monoxide mon-2  
2007-04-05T22:04 2 
2007-04-05T22:05 4 
2007-04-05T22:06 10 
2007-04-05T22:07 8 
2007-04-05T22:08 6
";

            // Act
            string result = LogEvaluator.EvaluateLogFile(fileContents);

            // Assert
            string expectedResult = @"{ 
""temp-1"": ""precise"", 
""temp-2"": ""ultra precise"", 
""hum-1"": ""keep"", 
""hum-2"": ""discard"", 
""mon-1"": ""keep"", 
""mon-2"": ""discard""
}";
            result.Should().Equals(expectedResult);
        }

        // TODO - Add more larger end-to-end tests
    }
}