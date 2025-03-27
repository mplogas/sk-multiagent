using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Agents;
using Microsoft.SemanticKernel.Agents.Chat;
using Microsoft.SemanticKernel.ChatCompletion;

namespace Workshop.SemanticKernel.MultiAgent
{
    public class Scenarios
    {
        private bool isInitialized = false;
        private Dictionary<string, Scenario> activeScenarios = new Dictionary<string, Scenario>();

        private class Scenario
        {
            private AgentGroupChat chat = new();

            public void Initialize(Settings.ScenarioSettings scenarioSettings, Kernel scenarioKernel, List<ChatCompletionAgent> agents)
            {
                
                var terminateFunction = AgentGroupChat.CreatePromptFunctionForStrategy(scenarioSettings.TerminationPrompt);
                var selectionFunction = AgentGroupChat.CreatePromptFunctionForStrategy(scenarioSettings.SelectionPrompt);
            
                chat.ExecutionSettings = new AgentGroupChatSettings 
                {
                    TerminationStrategy = new KernelFunctionTerminationStrategy(terminateFunction, scenarioKernel)
                    {
                        Agents = agents.Where(a => scenarioSettings.TerminationAgents.Contains(a.Name)).ToList(),
                        HistoryVariableName = "history",
                        ResultParser = (result) => result.GetValue<string>()?.Contains(scenarioSettings.TerminationSuccess, StringComparison.InvariantCultureIgnoreCase) ?? false // add more advanced parsing logic here
                    },
                    SelectionStrategy = new KernelFunctionSelectionStrategy(selectionFunction, scenarioKernel)
                    {
                        AgentsVariableName = "agents",
                        HistoryVariableName = "history"
                    }
                };
                
                foreach (var agent in agents)
                {
                    chat.AddAgent(agent);
                    Console.WriteLine($"Agent {agent.Name} configured in scenario {scenarioSettings.Name}");
                }
            }
            
            public async Task Execute(string prompt)
            {
                chat.AddChatMessage(new ChatMessageContent(AuthorRole.User, prompt));
                await foreach (var content in chat.InvokeAsync())
                {
                   Console.WriteLine();
                    Console.WriteLine($"# {content.Role} - {content.AuthorName ?? "*"}: '{content.Content}'");
                    Console.WriteLine();
                }
            
                Console.WriteLine($"# IS COMPLETE: {chat.IsComplete}");
            }
        }
        
        public void Initialize(Settings settings, List<ChatCompletionAgent> agents)
        {
            var scenarios = settings.GetSettings<List<Settings.ScenarioSettings>>("scenarios");
            foreach (var scenario in scenarios)
            {
                if (!scenario.Enabled || scenario.Agents.Count == 0)
                {
                    Console.WriteLine($"Scenario {scenario.Name} is not enabled or has no agents.");
                    continue;
                }
                
                // get all agents for this scenario
                var activeAgents = new List<ChatCompletionAgent>();
                foreach (var agent in scenario.Agents)
                {
                    var agentSettings = agents.FirstOrDefault(a => a.Name == agent);
                    if (agentSettings == null)
                    {
                        Console.WriteLine($"Agent {agent} not found.");
                        continue;
                    }
                    
                    activeAgents.Add(agentSettings);
                }
                
                var s = new Scenario();
                s.Initialize(scenario, KernelFactory.CreateKernel(settings, scenario.Model, KernelFactory.ConvertFrom(scenario.Backend)), activeAgents);
                activeScenarios.Add(scenario.Name, s);
            }
            
            isInitialized = true;
        }

        public Task ExecuteAsync(string scenarioName, string prompt)
        {
            if (!isInitialized)
            {
                throw new InvalidOperationException("Scenarios not initialized. Call Initialize() first.");
            }

            if (activeScenarios.TryGetValue(scenarioName, out var scenario))
            {
                return scenario.Execute(prompt);
            }
            else
            {
                throw new ArgumentException($"Scenario {scenarioName} not found.");
            }
        }

        public List<string> GetAvailableScenarios()
        {
            if (!isInitialized)
            {
                throw new InvalidOperationException("Scenarios not initialized. Call Initialize() first.");
            }

            return activeScenarios.Keys.ToList();
        }
    }
}