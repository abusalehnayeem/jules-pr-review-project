using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Extensions.Configuration;
using Moq; // Assumes Moq is available as a testing utility
using DevInsightCli.Services;
using System;
using System.Collections.Generic;

namespace DevInsightCli.Tests
{
    [TestClass]
    public class LlmServiceTests
    {
        [TestMethod]
        public void Constructor_WithValidConfiguration_ShouldNotThrow()
        {
            // Arrange
            var mockConfigurationSectionOllama = new Mock<IConfigurationSection>();
            mockConfigurationSectionOllama.Setup(s => s["Endpoint"]).Returns("http://localhost:11434");
            mockConfigurationSectionOllama.Setup(s => s["ModelId"]).Returns("testmodel");
            mockConfigurationSectionOllama.Setup(s => s["ApiKey"]).Returns(""); // Optional, can be empty

            var mockConfiguration = new Mock<IConfiguration>();
            mockConfiguration.Setup(c => c.GetSection("Ollama"))
                             .Returns(mockConfigurationSectionOllama.Object);

            // Act & Assert
            try
            {
                var llmService = new LlmService(mockConfiguration.Object);
                Assert.IsNotNull(llmService, "LlmService should be instantiated.");
            }
            catch (Exception ex)
            {
                Assert.Fail($"LlmService instantiation failed with valid configuration: {ex.Message}");
            }
        }

        [TestMethod]
        public void Constructor_WithMissingEndpointConfiguration_ShouldThrowInvalidOperationException()
        {
            // Arrange
            var mockConfigurationSectionOllama = new Mock<IConfigurationSection>();
            // Endpoint is missing
            mockConfigurationSectionOllama.Setup(s => s["ModelId"]).Returns("testmodel");

            var mockConfiguration = new Mock<IConfiguration>();
            mockConfiguration.Setup(c => c.GetSection("Ollama"))
                             .Returns(mockConfigurationSectionOllama.Object);

            // Act & Assert
            Assert.ThrowsException<InvalidOperationException>(() =>
            {
                var llmService = new LlmService(mockConfiguration.Object);
            }, "Expected InvalidOperationException when Ollama endpoint is missing.");
        }

        [TestMethod]
        public void Constructor_WithMissingModelIdConfiguration_ShouldThrowInvalidOperationException()
        {
            // Arrange
            var mockConfigurationSectionOllama = new Mock<IConfigurationSection>();
            mockConfigurationSectionOllama.Setup(s => s["Endpoint"]).Returns("http://localhost:11434");
            // ModelId is missing

            var mockConfiguration = new Mock<IConfiguration>();
            mockConfiguration.Setup(c => c.GetSection("Ollama"))
                             .Returns(mockConfigurationSectionOllama.Object);

            // Act & Assert
            Assert.ThrowsException<InvalidOperationException>(() =>
            {
                var llmService = new LlmService(mockConfiguration.Object);
            }, "Expected InvalidOperationException when Ollama ModelId is missing.");
        }

        // Note:
        // To run these tests, you would typically need:
        // 1. A separate test project (e.g., DevInsightCli.Tests.csproj).
        // 2. References to MSTest.TestAdapter, MSTest.TestFramework, Microsoft.NET.Test.Sdk.
        // 3. A reference to the DevInsightCli project.
        // 4. The Moq NuGet package for mocking.
        //
        // The AnalyzeCodeAsync method involves actual network calls and Semantic Kernel execution,
        // which is harder to unit test without more complex mocking of Kernel and its components.
        // Such tests would become integration tests. For now, the constructor tests cover
        // the configuration aspect of Ollama integration.
    }
}
