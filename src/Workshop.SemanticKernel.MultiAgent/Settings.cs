using Microsoft.Extensions.Configuration;

namespace Workshop.SemanticKernel.MultiAgent
{
    public class Settings
    {
        private readonly IConfigurationRoot configRoot;

        private AzureOpenAISettings azureOpenAI;
        private OpenAISettings openAI;
        private OllamaSettings ollama;

        public AzureOpenAISettings AzureOpenAI => this.azureOpenAI ??= this.GetSettings<Settings.AzureOpenAISettings>();
        public OpenAISettings OpenAI => this.openAI ??= this.GetSettings<Settings.OpenAISettings>();
        public OllamaSettings Ollama => this.ollama ??= this.GetSettings<Settings.OllamaSettings>();

        public class TransformerBackendSettings
        {
            public string ApiKey { get; set; } = string.Empty;
        }
        
        public class OpenAISettings : TransformerBackendSettings
        {
            public string Organization { get; set; } = string.Empty;
        }

        public class AzureOpenAISettings : TransformerBackendSettings
        {
            public string Endpoint { get; set; } = string.Empty;
        }
        
        public class OllamaSettings : TransformerBackendSettings
        {
            public string Endpoint { get; set; } = string.Empty;
        }
        
        public class AgentSettings
        {
            public string Name { get; set; } = string.Empty;
            public string Description { get; set; } = string.Empty;
            public string Backend { get; set; } = string.Empty;
            public string Instructions { get; set; } = string.Empty;
            public string Model { get; set; } = string.Empty;
        }
        
        public class ScenarioSettings
        {
            public string Name { get; set; } = string.Empty;
            public string Description { get; set; } = string.Empty;
            public List<string> Agents { get; set; } = new ();
            public string SelectionPrompt { get; set; } = string.Empty;
            public string TerminatePrompt { get; set; } = string.Empty;
            public List<string> TerminationAgents { get; set; } = new ();
            public int MaxIterations { get; set; } = 10;
            public bool Enabled { get; set; } = true;
            
        }

        // public TSettings GetSettings<TSettings>(string name) =>
        //     this.configRoot.GetRequiredSection(name).Get<TSettings>()!;
        public TSettings GetSettings<TSettings>(string name) where TSettings : new()
        {
            var section = this.configRoot.GetSection(name);
            return section.Exists() ? section.Get<TSettings>() ?? new TSettings() : new TSettings();
        }
        
        
        // private TSettings GetSettings<TSettings>() =>
        //     this.configRoot.GetRequiredSection(typeof(TSettings).Name).Get<TSettings>()!;
        private TSettings GetSettings<TSettings>() where TSettings : new()
        {
            var section = this.configRoot.GetSection(typeof(TSettings).Name);
            return section.Exists() ? section.Get<TSettings>() ?? new TSettings() : new TSettings();
        }
        
        public Settings()
        {
            var basePath = File.Exists("/config/appsettings.json") ? "/config" : AppContext.BaseDirectory;
            
            this.configRoot =
                new ConfigurationBuilder()
                    .SetBasePath(basePath)
                    .AddJsonFile("appsettings.json", false)
                    .AddEnvironmentVariables()
                    .AddUserSecrets<Program>()
                    .Build();
        }
    }
}