using Microsoft.SemanticKernel;
using Microsoft.Extensions.Configuration; // Added for IConfiguration

namespace DevInsightCli.Services
{
    public class LlmService
    {
        private readonly Kernel _kernel;

        // Modified constructor to accept IConfiguration
        public LlmService(IConfiguration configuration)
        {
            var builder = Kernel.CreateBuilder();

            // Configure for Ollama using OpenAI-compatible API
            var ollamaEndpoint = configuration["Ollama:Endpoint"]; // e.g., "http://localhost:11434"
            var ollamaModelId = configuration["Ollama:ModelId"]; // e.g., "llama3"
            var ollamaApiKey = configuration["Ollama:ApiKey"]; // Optional, usually not required for local Ollama

            if (string.IsNullOrEmpty(ollamaEndpoint) || string.IsNullOrEmpty(ollamaModelId))
            {
                // Fallback or error handling if configuration is missing
                // For now, let's throw an exception or use a default placeholder if appropriate
                // This ensures the application doesn't run with an incomplete LLM setup.
                // Alternatively, could use a default in-memory model or disable LLM features.
                throw new InvalidOperationException("Ollama endpoint or model ID is not configured.");
            }

            // Construct the full endpoint for OpenAI-compatible chat completions
            // Typically, Ollama's OpenAI compatible endpoint is at /v1 of the base URL
            var fullOllamaEndpoint = ollamaEndpoint.TrimEnd('/') + "/v1";


            // Using AddOpenAIChatCompletion to connect to Ollama's OpenAI-compatible API
            // The first parameter is the model ID, the second is the API key (if any), and the third is the endpoint.
            // For local Ollama, apiKey is often not needed or can be a non-empty string if the server expects one.
            builder.AddOpenAIChatCompletion(
                modelId: ollamaModelId,
                apiKey: ollamaApiKey, // Pass API key if configured/needed
                endpoint: new System.Uri(fullOllamaEndpoint) // Pass the full URI
                );


            _kernel = builder.Build();
        }

        public async Task<string> AnalyzeCodeAsync(string codeSnippet)
        {
            // Logic to use Semantic Kernel for code analysis
            // This part remains the same, but will now use the configured Ollama model
            // Example: var result = await _kernel.InvokePromptAsync($"Analyze this code: {codeSnippet}");
            // For now, simulate analysis
            var result = await _kernel.InvokePromptAsync($"Provide a brief analysis of the following code snippet:\n```\n{codeSnippet}\n```");
            // Assuming the result is of a type that can be converted to string, e.g., FunctionResult.
            return result.ToString();
        }
    }
}
