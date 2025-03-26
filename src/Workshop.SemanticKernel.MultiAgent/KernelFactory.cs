using Microsoft.SemanticKernel;

namespace Workshop.SemanticKernel.MultiAgent {

    public static class KernelFactory
    {
        public static Kernel CreateKernel(Settings settings, string model, TransformerBackend backend)
        {
            var backendSettings = settings.GetTransformerBackendSettings(backend);
            switch (backend)
            {
                case TransformerBackend.OpenAI:
                    var oaiSettings = backendSettings as Settings.OpenAISettings;
                    return Kernel.CreateBuilder().AddOpenAIChatCompletion(model, oaiSettings.ApiKey, oaiSettings.Organization).Build();
                case TransformerBackend.AzureOpenAI:
                    return Kernel.CreateBuilder().AddAzureOpenAIChatCompletion(model, backendSettings.Endpoint, backendSettings.ApiKey).Build(); 
                case TransformerBackend.Ollama:
                    return Kernel.CreateBuilder().AddOllamaChatCompletion(model, new Uri(backendSettings.Endpoint)).Build(); //if you have an api key, add it as the 3rd parameter
                default:
                    throw new ArgumentOutOfRangeException(nameof(backend), backend, null);
            }
        }
        
        public static TransformerBackend ConvertFrom(string backend) 
        {
            return backend.ToLower() switch
            {
                "openai" => TransformerBackend.OpenAI,
                "azureopenai" => TransformerBackend.AzureOpenAI,
                "ollama" => TransformerBackend.Ollama,
                _ => throw new ArgumentOutOfRangeException(nameof(backend), backend, null)
            };
        }
    }

}