using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Agents;
using Microsoft.SemanticKernel.Agents.Chat;
using Microsoft.SemanticKernel.ChatCompletion;

namespace Workshop.SemanticKernel.MultiAgent
{
    public class Scenario
    {
        private AgentGroupChat chat = new();
        private bool isInitialized = false;
        
        public void Initialize(Settings.ScenarioSettings scenarioSettings, List<ChatCompletionAgent> agents)
        {
            foreach (var agent in agents)
            {
                chat.AddAgent(agent);
                Console.WriteLine($"Agent {agent.Name} configured.");
            }
            
            chat.ExecutionSettings = new AgentGroupChatSettings 
            {
                TerminationStrategy = new KernelFunctionTerminationStrategy(
                    KernelFunctionFactory.CreateFromPrompt(scenarioSettings.TerminatePrompt, KernelFactory.CreateKernel())
                    //TODO so close... 
                    )
            };

            /*
             * ExecutionSettings = new()
               {
                   TerminationStrategy = new KernelFunctionTerminationStrategy(terminateFunction, KernelCreator.CreateKernel(useAzureOpenAI))
                   {
                       Agents = [codeValidatorAgent],
                       ResultParser = (result) => result.GetValue<string>()?.Contains("yes", StringComparison.OrdinalIgnoreCase) ?? false,
                       HistoryVariableName = "history",
                       MaximumIterations = 10
                   },
                   SelectionStrategy = new KernelFunctionSelectionStrategy(selectionFunction, KernelCreator.CreateKernel(useAzureOpenAI))
                   {
                       AgentsVariableName = "agents",
                       HistoryVariableName = "history"
                   }
               }
             */
            
            isInitialized = true;
        }

        public async Task Execute(string prompt)
        {
            if(!isInitialized) throw new InvalidOperationException("Scenario not initialized. Call Initialize() first.");
            
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
}