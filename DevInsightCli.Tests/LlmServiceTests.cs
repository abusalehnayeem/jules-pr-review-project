using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Extensions.Configuration;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Extensions.Configuration;
using Moq; // Assumes Moq is available as a testing utility
using DevInsightCli.Services;
using System;
using System.Collections.Generic;
using System.Threading.Tasks; // Added for Task
using Microsoft.SemanticKernel; // Added for FunctionResult

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

        [TestMethod]
        public async Task ReviewCodeAsync_WithValidCodeSnippet_ReturnsReview()
        {
            // Arrange
            var mockConfigurationSectionOllama = new Mock<IConfigurationSection>();
            mockConfigurationSectionOllama.Setup(s => s["Endpoint"]).Returns("http://localhost:11434");
            mockConfigurationSectionOllama.Setup(s => s["ModelId"]).Returns("testmodel");
            mockConfigurationSectionOllama.Setup(s => s["ApiKey"]).Returns("");

            var mockConfiguration = new Mock<IConfiguration>();
            mockConfiguration.Setup(c => c.GetSection("Ollama"))
                             .Returns(mockConfigurationSectionOllama.Object);

            // Mock FunctionResult and its underlying value.
            // FunctionResult's ToString() method returns the string representation of its ValueObject.
            var expectedReview = "This is a mock code review.";
            // We need a KernelFunction instance for FunctionResult constructor.
            // For testing purposes, we can create a simple one if its properties are not deeply accessed.
            // However, FunctionResult has constructors that can take value and metadata directly,
            // or a function and a value. Let's try to create a simple FunctionResult.
            // Looking at Semantic Kernel source, FunctionResult's ToString() returns Value.ToString().
            // So we need to ensure Value is set.
            // A simple way to create FunctionResult for testing:
            var functionResult = new Microsoft.SemanticKernel.FunctionResult(function: null, value: expectedReview);


            // Mock LlmService itself to intercept the call to InvokeKernelPromptAsync
            // Pass the real configuration object to the constructor.
            // CallBase must be true so the constructor of LlmService runs and initializes _kernel,
            // otherwise _kernel would be null if InvokeKernelPromptAsync was not virtual and we tried to use _kernel.
            var mockLlmService = new Mock<LlmService>(mockConfiguration.Object) { CallBase = true };

            mockLlmService.Setup(s => s.InvokeKernelPromptAsync(It.IsAny<string>()))
                          .ReturnsAsync(functionResult);

            var service = mockLlmService.Object;
            var codeSnippet = "public void HelloWorld() { Console.WriteLine(\"Hello\"); }";

            // Act
            var review = await service.ReviewCodeAsync(codeSnippet);

            // Assert
            Assert.AreEqual(expectedReview, review);
            mockLlmService.Verify(s => s.InvokeKernelPromptAsync(It.Is<string>(prompt => prompt.Contains(codeSnippet))), Times.Once);
        }
    }
}
