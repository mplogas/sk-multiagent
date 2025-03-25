using Microsoft.SemanticKernel;

namespace Workshop.SemanticKernel.MultiAgent {

    public static class KernelFactory
    {
        public static Kernel CreateKernel(Settings.TransformerBackendSettings settings, string model)
        {
            if (settings is Settings.OpenAISettings oai) return Kernel.CreateBuilder().AddOpenAIChatCompletion(model, settings.ApiKey, oai.Organization).Build();
            else if(settings is Settings.AzureOpenAISettings aoai) return Kernel.CreateBuilder().AddAzureOpenAIChatCompletion(model, aoai.Endpoint, settings.ApiKey).Build(); 
            else if (settings is Settings.OllamaSettings ollama) return Kernel.CreateBuilder().AddOllamaChatCompletion(model, new Uri(ollama.Endpoint)).Build(); //if you have an api key, add it as the 3rd parameter
            else throw new ArgumentNullException(nameof(settings), "Settings are required for the specified backend.");
        }
    }

}