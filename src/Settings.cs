using Microsoft.Extensions.Configuration;

namespace Workshop.SemanticKernel.MultiAgent
{
    public enum TransformerBackend
    {
        OpenAI,
        AzureOpenAI,
        Ollama,
        Gemini
    }

    public enum ChatHistoryLength
    {
        Full,
        Reduced
    }
    
    public class Settings
    {
        private readonly IConfigurationRoot _configRoot;

        public class TransformerBackendSettings
        {
            public string ApiKey { get; set; } = string.Empty;
            public string Endpoint { get; set; } = string.Empty;
            public TransformerBackend Type { get; set; } = TransformerBackend.AzureOpenAI;
        }
        
        public class OpenAISettings : TransformerBackendSettings
        {
            public string Organization { get; set; } = string.Empty;
        }
        
        public class ToolSettings
        {
            public string Name { get; set; } = string.Empty;
            public string Description { get; set; } = string.Empty;
            public Dictionary<string, string> Parameters { get; set; } = new ();
        }
        
        public class AgentSettings
        {
            public string Name { get; set; } = string.Empty;
            public string Description { get; set; } = string.Empty;
            public string Backend { get; set; } = string.Empty;
            public string Instructions { get; set; } = string.Empty;
            public string Model { get; set; } = string.Empty;
            public List<string> Tools { get; set; } = new List<string>();
        }
        
        public class ScenarioSettings
        {
            public string Name { get; set; } = string.Empty;
            public string Description { get; set; } = string.Empty;
            public List<string> Agents { get; set; } = new ();
            public string SelectionPrompt { get; set; } = string.Empty;
            public string TerminationPrompt { get; set; } = string.Empty;
            public List<string> TerminationAgents { get; set; } = new ();
            public string TerminationSuccess { get; set; } = string.Empty;
            public bool Enabled { get; set; } = true;
            public string History { get; set; } = string.Empty;
            public string Backend { get; set; } = string.Empty;
            public string Model { get; set; } = string.Empty;
        }

        public TSettings GetSettings<TSettings>(string name) where TSettings : new()
        {
            var section = this._configRoot.GetSection(name);
            return section.Exists() ? section.Get<TSettings>() ?? new TSettings() : new TSettings();
        }
        public TransformerBackendSettings GetTransformerBackendSettings(TransformerBackend backend)
        {
            switch (backend)
            {
                case TransformerBackend.OpenAI:
                    return this.GetSettings<OpenAISettings>("OpenAI"); 
                case TransformerBackend.AzureOpenAI:
                    return this.GetSettings<TransformerBackendSettings>("AzureOpenAI");
                case TransformerBackend.Gemini:
                    return this.GetSettings<TransformerBackendSettings>("GoogleAI");
                case TransformerBackend.Ollama:
                default:
                    return this.GetSettings<TransformerBackendSettings>("Ollama");
            }
        }
        
        public Settings()
        {
            var basePath = File.Exists("/config/appsettings.json") ? "/config" : AppContext.BaseDirectory;
            
            this._configRoot =
                new ConfigurationBuilder()
                    .SetBasePath(basePath)
                    .AddJsonFile("appsettings.json", false)
                    .AddEnvironmentVariables()
                    .AddUserSecrets<Program>()
                    .Build();
        }
    }
}