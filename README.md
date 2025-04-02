# Semantic Kernel Multi-Agent Demo: Automated Software Development Workflow

Welcome! This project demonstrates a collaborative multi-agent system built using the experimental agent features in Microsoft Semantic Kernel. It simulates a simplified software development lifecycle, showcasing how specialized AI agents can work together to take a feature request from user input to documented code.

This repository contains the code demonstrated during the Global AI bootcamp session "Semantic Kernel Multi-Agent Scenarios" (2025-04-11) and serves as a practical example for the code deep-dive.

## ✨ Overview

Ever wondered how multiple AI agents could collaborate on a complex task like software development? This demo brings that concept to life! We've set up a team of AI agents, each with a specific role in the development process:

1.  **User Request:** A user specifies a new feature requirement.
2.  **Requirements Engineering:** An AI agent analyzes the request and generates a formal requirements document.
3.  **Development:** Another agent takes the requirements and writes the C# code for the feature, including unit tests.
4.  **Code Review:** A reviewer agent assesses the code against the requirements and best practices, providing feedback.
5.  **Iteration (if needed):** The developer agent refines the code based on the review feedback.
6.  **Documentation:** Once the code is approved, a documentation specialist agent generates API docs, user guide snippets, and changelog entries.

This entire workflow is orchestrated using Semantic Kernel's agent capabilities, showing how prompts, tools, and automated chat management can create sophisticated, goal-oriented interactions.

## 🚀 Key Features Demonstrated

*   **Multi-Agent Collaboration:** Utilizes `AgentGroupChat` for structured interaction between specialized agents.
*   **Specialized AI Roles:** Employs `ChatCompletionAgent` with distinct instructions and goals (Requirements Engineer, Developer, Reviewer, Documenter).
*   **Dynamic Orchestration:** Leverages prompt-based `KernelFunctionSelectionStrategy` and `KernelFunctionTerminationStrategy` to guide the conversation flow and determine completion.
*   **Tool Integration:** Shows agents using Semantic Kernel Plugins (`FileSystemPlugin`) to perform actions like writing requirement, code, and documentation files.
*   **Configurable Backends:** Supports various AI models via `KernelFactory` (Azure OpenAI, OpenAI, Ollama, Gemini - configurable in `appsettings.json`).
*   **Practical Workflow Simulation:** Models a realistic, albeit simplified, software development lifecycle.

## ⚙️ The Agent Team

*   **RequirementsEngineer:** Acts as the bridge between the user and the development team, translating requests into structured requirements (`/tmp/requirements.md`).
*   **SeniorDeveloper:** Implements the features based on the requirements, writing C# code and unit tests (`/tmp/code.md`).
*   **CodeReviewer:** Evaluates the developer's code for quality, completeness, and adherence to standards, providing feedback and ratings.
*   **DocumentationSpecialist:** Creates API documentation, user manual sections, and changelog entries based on the approved code (`/tmp/documentation.md`).

## 🛠️ Technologies Used

*   .NET 8
*   Semantic Kernel SDK (including Agents, Connectors)
*   Azure OpenAI / OpenAI / Ollama / Gemini (configurable)
*   .NET Logging Extension (for console logging)

## 🏗️ Project Structure
    /src
    ├── Workshop.SemanticKernel.MultiAgent.csproj # Project file with dependencies
    ├── Program.cs                 # Application entry point, initializes and runs the scenario
    ├── Agents.cs                  # Agent initialization logic
    ├── Scenarios.cs               # Defines and manages AgentGroupChat interactions
    ├── KernelFactory.cs           # Creates Kernel instances for different AI backends
    ├── Tools.cs                   # ToolFactory registers and provides tools (Plugins) to agents, FileSystemPlugin as example tool for filesystem interaction
    ├── Settings.cs                # Configuration management
    └── appsettings.json           # Configuration for agents, tools, scenarios, backends
    /docs
    ├── AGENTS.md                  # Detailed Agent description and instructions (in-line with appsettings.json instructions)
    └── SCENARIO.md                # Scenario description, agent selection prompt and termination prompt

## 🏁 Getting Started

### Prerequisites

*   .NET 8 SDK: [Download Link](https://dotnet.microsoft.com/download/dotnet/8.0)
*   Access to an AI Model provider (Azure OpenAI, OpenAI, Ollama, or Gemini) and corresponding API keys/endpoints.

### Configuration

1.  **Clone the repository:**
    ```bash
    git clone https://github.com/mplogas/sk-multiagent.git
    cd sk-multiagent
    ```
2.  **Configure AI Backend:**
    *   You need to provide credentials for your chosen AI service. The recommended way is using .NET User Secrets. Initialize user secrets for the project:
        ```bash
        dotnet user-secrets init
        ```
    *   Add your secrets (replace `YourProvider` and keys/endpoints accordingly, e.g., for Azure OpenAI):
        ```bash
        dotnet user-secrets set "AzureOpenAI:ApiKey" "YOUR_AZURE_OPENAI_API_KEY"
        dotnet user-secrets set "AzureOpenAI:Endpoint" "YOUR_AZURE_OPENAI_ENDPOINT"
        # Add other provider secrets if needed (OpenAI:ApiKey, GoogleAI:ApiKey etc.)
        ```
    *   Alternatively, you can directly edit the `appsettings.json` file, but **be careful not to commit your secrets** to version control.
    *   Ensure the `backend` and `model` properties for agents and scenarios in `appsettings.json` match your configured provider.
3.  **Configure FileSystem Plugin Path (Optional):**
    *   The `FileSystemPlugin` writes files to `/tmp/` by default (see `tools` section in `appsettings.json`). You can change the `basepath` parameter if needed (ensure the directory exists and the application has write permissions).

### Build

```bash
dotnet build
```

## ▶️ Running the Demo 

1. Run the application:
    ```bash
    dotnet run
    ```
2. Select Scenario: The application will list available scenarios (e.g., "development"). Type the name and press Enter.
3. Enter Prompt: Provide a feature request for the agents. For example:

    ``` Build a Blazor WASM-based calculator that runs all its calculations in the web assembly and does not require a backend. The calculator only needs to be able to add positive numbers between 0 and 255 ```
4. Observe: Watch the console output as the agents interact, perform their tasks, and potentially iterate based on reviews.
5. Check Output: Look in the configured `basepath` (default `/tmp/`) for the generated files: `requirements.md`, `code.md`, and `documentation.md`.

## 🔍 Code Deep-Dive Highlights

For those interested in exploring the code further: 

- Agent Definition (`appsettings.json`, `Agents.cs`): See how agent names, instructions, tools, and model configurations are defined and loaded.
- Tooling (`FileSystemPlugin.cs`, `ToolFactory.cs`): Understand how Semantic Kernel Plugins are created and made available to agents. Note the use of `[KernelFunction]` and `[Description]` attributes.
- Orchestration (`Scenarios.cs`): Examine how `AgentGroupChat` is set up with custom selection and termination strategies defined by prompts (found in `appsettings.json` under the scenario definition). Follow the `InvokeAsync` loop.
- Kernel Setup (`KernelFactory.cs`): See how different AI backends are configured and chosen based on settings.
- Configuration (`Settings.cs`, `appsettings.json`): Explore how the application manages settings for different components.
     