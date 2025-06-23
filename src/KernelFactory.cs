using Microsoft.Extensions.DependencyInjection;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel.Connectors.Google;

namespace Workshop.SemanticKernel.MultiAgent {

    public static class KernelFactory
    {
        public static Kernel CreateKernel(ILoggerFactory loggerFactory, Settings settings, string model, TransformerBackend backend)
        {
            var backendSettings = settings.GetTransformerBackendSettings(backend);
            var kernelBuilder = Kernel.CreateBuilder();
            kernelBuilder.Services.AddSingleton(loggerFactory);
            switch (backend)
            {
                case TransformerBackend.OpenAI:
                    var oaiSettings = backendSettings as Settings.OpenAISettings;
                    return kernelBuilder.AddOpenAIChatCompletion(model, oaiSettings.ApiKey, oaiSettings.Organization).Build();
                case TransformerBackend.AzureOpenAI:
                    return kernelBuilder.AddAzureOpenAIChatCompletion(model, backendSettings.Endpoint, backendSettings.ApiKey).Build(); 
                case TransformerBackend.Ollama:
                    return kernelBuilder.AddOllamaChatCompletion(model, new Uri(backendSettings.Endpoint)).Build(); //if you have an api key, add it as the 3rd parameter
                case TransformerBackend.Gemini:
                    return kernelBuilder.AddGoogleAIGeminiChatCompletion(model, backendSettings.ApiKey).Build();
                default:
                    throw new ArgumentOutOfRangeException(nameof(backend), backend, null);
            }
        }
        public static PromptExecutionSettings GetExecutionSettings(TransformerBackend backend)
        {
            switch (backend)
            {
                case TransformerBackend.OpenAI:
                case TransformerBackend.AzureOpenAI:
                    return new OpenAIPromptExecutionSettings
                    {
                        // Use FunctionChoiceBehavior for modern SK versions
                        FunctionChoiceBehavior = FunctionChoiceBehavior.Auto()
                        // OR if using older versions or needing specific OpenAI features:
                        // ToolCallBehavior = ToolCallBehavior.AutoInvokeKernelFunctions
                    };
                case TransformerBackend.Gemini:
                    return new GeminiPromptExecutionSettings
                    {
                        FunctionChoiceBehavior = FunctionChoiceBehavior.Auto()
                    };
                case TransformerBackend.Ollama:
                default:
                    return new PromptExecutionSettings()
                    {
                        FunctionChoiceBehavior = FunctionChoiceBehavior.Auto()
                    };
            }
        }
        
        public static TransformerBackend ConvertFrom(string backend) 
        {
            return backend.ToLower() switch
            {
                "openai" => TransformerBackend.OpenAI,
                "azureopenai" => TransformerBackend.AzureOpenAI,
                "ollama" => TransformerBackend.Ollama,
                "gemini" => TransformerBackend.Gemini,
                _ => throw new ArgumentOutOfRangeException(nameof(backend), backend, null)
            };
        }
    }

}