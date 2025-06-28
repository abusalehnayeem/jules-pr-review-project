# DevInsight CLI

DevInsight CLI is a .NET-based command-line tool designed to provide development insights, initially focusing on GitHub Pull Request analysis with local LLM integration via Ollama and Semantic Kernel.

## Features

*   **Pull Request Analysis:** Fetches details for a given GitHub PR URL.
*   **LLM Integration:**
    *   Utilizes a locally running Ollama instance for language model capabilities.
    *   Supports any Ollama-compatible model (e.g., Llama 3, CodeLlama).
    *   Uses Semantic Kernel for LLM interaction and prompt engineering.
*   **Agent-Based Analysis:** Demonstrates an agent (`CodeReviewer`) that uses the LLM to provide specialized feedback.
*   **Workflow Tracking:** Basic workflow creation and step tracking (persisted via SQLite).
*   **Configurable:** LLM endpoint and model ID can be configured in `appsettings.json`.

## Prerequisites

1.  **.NET SDK:**
    *   Ensure you have the .NET SDK installed (compatible with the project, e.g., .NET 8.0 or .NET 6.0). You can download it from [https://dotnet.microsoft.com/download](https://dotnet.microsoft.com/download).

2.  **Ollama Installed and Running:**
    *   Download and install Ollama from [https://ollama.com/](https://ollama.com/).
    *   Verify Ollama is running (it usually runs as a background service). You can check its status by opening a terminal and running `ollama list`.

3.  **Ollama Model Downloaded:**
    *   The application uses the model specified in `DevInsightCli/appsettings.json` (under `Ollama:ModelId`). The default is often set to "llama3".
    *   Pull the required model using the Ollama CLI. For example:
        ```bash
        ollama pull llama3
        ```
    *   Replace `llama3` with the model ID you intend to use if you change it in `appsettings.json`. Other models like "codellama" can also be used.

## Setup & Configuration

1.  **Clone the Repository:**
    ```bash
    git clone <repository_url>
    cd <repository_directory>
    ```

2.  **Database Migration:**
    *   The application uses SQLite for storing workflow data. The database (`devinsight.db`) will be created, and migrations will be applied automatically when the application first runs.

3.  **Configure `appsettings.json`:**
    *   The main configuration file is `DevInsightCli/appsettings.json`.
    *   **Ollama Settings:**
        ```json
        {
          "Ollama": {
            "Endpoint": "http://localhost:11434", // Your local Ollama server endpoint
            "ModelId": "llama3",                 // The Ollama model to use (e.g., llama3, codellama)
            "ApiKey": ""                         // Usually empty for local Ollama
          },
          "ConnectionStrings": {
            "DefaultConnection": "Data Source=devinsight.db" // SQLite connection
          },
          "Logging": { // ... }
        }
        ```
    *   Ensure `Ollama:Endpoint` points to your Ollama server.
    *   Ensure `Ollama:ModelId` is a model you have downloaded via `ollama pull <model_id>`.

## How to Run

1.  **Navigate to the CLI project directory:**
    ```bash
    cd DevInsightCli
    ```

2.  **Run the application:**
    *   The primary command demonstrated is `analyze pr`.
    *   Use `dotnet run --` to pass arguments to the application.

    **Example:**
    ```bash
    dotnet run -- analyze pr https://github.com/owner/repo/pull/123
    ```
    Replace `https://github.com/owner/repo/pull/123` with an actual GitHub Pull Request URL.

## Output Explanation

When you run the `analyze pr` command, you can expect the following output:

1.  **PR Details:** Information fetched directly from the GitHub PR.
2.  **Workflow Information:** Updates on the workflow being created/updated for the analysis.
3.  **Direct LLM Analysis:**
    *   Headed by `--- Direct LLM Analysis ---`.
    *   Shows the raw analysis from the LLM based on the PR details and a general prompt.
4.  **Agent-Based Analysis:**
    *   Headed by `--- Agent-Based Analysis ---`.
    *   Shows results from different agents. Notably, the `CodeReviewer` agent will provide an LLM-generated code review based on a specialized prompt. Other agents might show simulated results.

## Testing

### Unit Tests
*   Unit tests for services like `LlmService` are located in the `DevInsightCli.Tests` directory (e.g., `LlmServiceTests.cs`).
*   These tests are written using MSTest and Moq (though actual execution in this environment might be limited).
*   To run these in a local dev environment with a proper test project setup:
    ```bash
    cd DevInsightCli.Tests
    dotnet test
    ```

### Manual Testing (Covered in "How to Run")
*   Follow the "How to Run" section to test the `analyze pr` command.
*   **Troubleshooting LLM Issues:**
    *   If you see errors like "Error during LLM analysis":
        *   Ensure Ollama service is running.
        *   Verify the `Ollama:ModelId` in `appsettings.json` is correct and the model is downloaded (`ollama list`).
        *   Check the `Ollama:Endpoint` is correct.
    *   You can test your Ollama API directly (e.g., with `curl`) to confirm it's operational:
        ```bash
        curl http://localhost:11434/v1/chat/completions -d '{
          "model": "llama3", # Or your configured model
          "messages": [{"role": "user", "content": "Test prompt"}]
        }'
        ```

This `README.md` provides a good overview, setup, configuration, and usage instructions, incorporating the Ollama and Semantic Kernel aspects.
Inline comments were added throughout the code development process in previous steps.

I will now mark this documentation step as complete.
