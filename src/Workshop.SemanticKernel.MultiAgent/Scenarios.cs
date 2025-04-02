using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Agents;
using Microsoft.SemanticKernel.Agents.Chat;
using Microsoft.SemanticKernel.ChatCompletion;

namespace Workshop.SemanticKernel.MultiAgent
{
    public class Scenarios
    {
        private bool _isInitialized = false;
        private readonly Dictionary<string, Scenario> _activeScenarios = new Dictionary<string, Scenario>();

        private class Scenario
        {
            private AgentGroupChat chat = new();
            private readonly ILogger<Scenario> _logger;
            private readonly ILoggerFactory _loggerFactory;

            public Scenario(ILoggerFactory loggerFactory)
            {
                _logger = loggerFactory.CreateLogger<Scenario>();
                _loggerFactory = loggerFactory;
            }

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
                    _logger.LogInformation($"Agent {agent.Name} added to scenario {scenarioSettings.Name}");
                }
            }
            
            public async Task Execute(string prompt)
            {
                chat.AddChatMessage(new ChatMessageContent(AuthorRole.User, prompt));
                await foreach (var content in chat.InvokeAsync())
                {
                    _logger.LogInformation("----------------------------------------");
                    _logger.LogInformation($"# {content.Role} - {content.AuthorName ?? "*"}: '{content.Content}'");
                    _logger.LogInformation("----------------------------------------");
                }
                
                _logger.LogInformation($"# IS COMPLETE: {chat.IsComplete}");
            }
        }
        
        public void Initialize(ILoggerFactory loggerFactory, Settings settings, List<ChatCompletionAgent> agents)
        {
            var scenarios = settings.GetSettings<List<Settings.ScenarioSettings>>("scenarios");
            var logger = loggerFactory.CreateLogger<Scenarios>();
            foreach (var scenario in scenarios)
            {
                if (!scenario.Enabled || scenario.Agents.Count == 0)
                {
                    logger.LogWarning($"Scenario {scenario.Name} is not enabled or has no agents.");
                    continue;
                }
                
                // get all agents for this scenario
                var activeAgents = new List<ChatCompletionAgent>();
                foreach (var agent in scenario.Agents)
                {
                    var agentSettings = agents.FirstOrDefault(a => a.Name == agent);
                    if (agentSettings == null)
                    {
                        logger.LogWarning($"Agent {agent} not found.");
                        continue;
                    }
                    
                    activeAgents.Add(agentSettings);
                }
                
                var s = new Scenario(loggerFactory);
                s.Initialize(scenario, KernelFactory.CreateKernel(loggerFactory, settings, scenario.Model, KernelFactory.ConvertFrom(scenario.Backend)), activeAgents);
                _activeScenarios.Add(scenario.Name, s);
            }
            
            _isInitialized = true;
        }

        public Task ExecuteAsync(string scenarioName, string prompt)
        {
            if (!_isInitialized)
            {
                throw new InvalidOperationException("Scenarios not initialized. Call Initialize() first.");
            }

            if (_activeScenarios.TryGetValue(scenarioName, out var scenario))
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
            if (!_isInitialized)
            {
                throw new InvalidOperationException("Scenarios not initialized. Call Initialize() first.");
            }

            return _activeScenarios.Keys.ToList();
        }
    }
}