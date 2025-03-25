using Microsoft.SemanticKernel.Agents;

namespace Workshop.SemanticKernel.MultiAgent
{

    public class Program
    {
        public static void Main(string[] args)
        {
            var settings = new Settings();
            var scenarios = settings.GetSettings<List<Settings.ScenarioSettings>>("scenarios");
            var agents = settings.GetSettings<List<Settings.AgentSettings>>("agents");

            var activeScenarios = new List<Scenario>();
            foreach (var scenario in scenarios)
            {
                if(!scenario.Enabled || scenario.Agents.Count == 0)
                {
                    Console.WriteLine($"Scenario {scenario.Name} is not enabled or has no agents.");
                    continue;
                }
                
                var activeAgents = new List<ChatCompletionAgent>();
                foreach (var agent in scenario.Agents)
                {
                    var agentSettings = agents.FirstOrDefault(a => a.Name == agent);
                    if (agentSettings == null)
                    {
                        Console.WriteLine($"Agent {agent} not found.");
                        continue;
                    }
                    
                    Settings.TransformerBackendSettings backend;
                    switch (agentSettings.Backend)
                    {
                        case "openai":
                            backend = settings.OpenAI;
                            break;
                        case "azureopenai":
                            backend = settings.AzureOpenAI;
                            break;
                        case "ollama":
                            backend = settings.Ollama;
                            break;
                        default:
                            Console.WriteLine($"Unknown backend: {agentSettings.Backend}");
                            continue;
                    }
            
                    var kernel = KernelFactory.CreateKernel(backend, agentSettings.Model);
                    var configuredAgent = new ChatCompletionAgent
                    {
                        Name = agentSettings.Name,
                        Description = agentSettings.Description,
                        Instructions = agentSettings.Instructions,
                        Kernel = kernel
                    };
                    
                    activeAgents.Add(configuredAgent);
                }
                var newScenario = new Scenario();
                newScenario.Initialize(scenario, activeAgents);
                activeScenarios.Add(newScenario);
            }
            
            // get the prompt from console.readline and run all activeScenarios
            // that obviously doesn't make a lot of sense for a console application but can be easily extended for a webapi / desktop application
            
            
 
    
            Console.WriteLine("Hello, World!");
        }
         
    }
}