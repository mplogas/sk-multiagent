using Microsoft.SemanticKernel.Agents;
using Microsoft.SemanticKernel.ChatCompletion;

namespace Workshop.SemanticKernel.MultiAgent
{
    public class Agents
    {
        public List<ChatCompletionAgent> AvailableAgents { get; set; } = new List<ChatCompletionAgent>();

        public void InitializeAgents(Settings settings, bool update = false)
        {
            if(!update)
            {
                AvailableAgents.Clear();
            }
            
            var agentSettings = settings.GetSettings<List<Settings.AgentSettings>>("agents");
            
            foreach (var agentSetting in agentSettings)
            {
                TransformerBackend backend;
                switch (agentSetting.Backend)
                {
                    case "openai":
                        backend = TransformerBackend.OpenAI;
                        break;
                    case "azureopenai":
                        backend = TransformerBackend.AzureOpenAI;
                        break;
                    case "ollama":
                        backend = TransformerBackend.Ollama;
                        break;
                    default:
                        Console.WriteLine($"Unknown backend: {agentSetting.Backend}");
                        continue;
                }
                
                var kernel = KernelFactory.CreateKernel(settings, agentSetting.Model, backend);
                var configuredAgent = new ChatCompletionAgent
                {
                    Name = agentSetting.Name,
                    Description = agentSetting.Description,
                    Instructions = agentSetting.Instructions,
                    Kernel = kernel,
                    InstructionsRole = (agentSetting.Model.Equals("o1-mini", StringComparison.InvariantCultureIgnoreCase) || agentSetting.Model.Equals("o3-mini", StringComparison.InvariantCultureIgnoreCase)) ? AuthorRole.User : AuthorRole.System
                    
                };
                
                if (update)
                {
                    var existingAgent = AvailableAgents.FirstOrDefault(a => a.Name == configuredAgent.Name);
                    if (existingAgent != null)
                    {
                        // unfortunately, ChatCompletionAgent does not implement live update of its properties
                        // this may create a race condition if the agent is used in multiple threads
                        AvailableAgents.Remove(existingAgent);
                    }
                }

                AvailableAgents.Add(configuredAgent);
            }
        }
    }
}