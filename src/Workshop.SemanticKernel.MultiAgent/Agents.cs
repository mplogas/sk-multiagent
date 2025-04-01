using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel.Agents;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel;


namespace Workshop.SemanticKernel.MultiAgent
{
    public class Agents
    {
        public List<ChatCompletionAgent> AvailableAgents { get; set; } = new List<ChatCompletionAgent>();

        public void InitializeAgents(Settings settings, ToolFactory toolFactory, ILoggerFactory loggerFactory, bool update = false)
        {
            if(!update)
            {
                AvailableAgents.Clear();
            }
            var logger = loggerFactory.CreateLogger<Agents>();
            
            var agentSettings = settings.GetSettings<List<Settings.AgentSettings>>("agents");
            
            foreach (var agentSetting in agentSettings)
            {
                var kernel = KernelFactory.CreateKernel(loggerFactory, settings, agentSetting.Model, KernelFactory.ConvertFrom(agentSetting.Backend));

                ChatCompletionAgent configuredAgent;
                if(agentSetting.Tools.Count > 0)
                {
                    foreach (var toolName in agentSetting.Tools)
                    {
                        var tool = toolFactory.GetTool(toolName);
                        if (tool != null)
                        {
                            kernel.Plugins.Add(tool);
                            logger.LogInformation($"Tool '{toolName}' added to agent.");
                        }
                        else
                        {
                            logger.LogWarning($"Tool '{toolName}' not found.");
                        }
                    }
                    
                    configuredAgent = new ChatCompletionAgent
                    {
                        Name = agentSetting.Name,
                        Description = agentSetting.Description,
                        Instructions = agentSetting.Instructions,
                        Kernel = kernel,
                        InstructionsRole = (agentSetting.Model.Equals("o1-mini", StringComparison.InvariantCultureIgnoreCase) || agentSetting.Model.Equals("o3-mini", StringComparison.InvariantCultureIgnoreCase)) ? AuthorRole.User : AuthorRole.System,
                        Arguments = new KernelArguments(KernelFactory.GetExecutionSettings(KernelFactory.ConvertFrom(agentSetting.Backend)))
                    };
                }
                else
                {
                    // no tools, so we can use the default agent
                    configuredAgent = new ChatCompletionAgent
                    {
                        Name = agentSetting.Name,
                        Description = agentSetting.Description,
                        Instructions = agentSetting.Instructions,
                        Kernel = kernel,
                        InstructionsRole = (agentSetting.Model.Equals("o1-mini", StringComparison.InvariantCultureIgnoreCase) || agentSetting.Model.Equals("o3-mini", StringComparison.InvariantCultureIgnoreCase)) ? AuthorRole.User : AuthorRole.System,
                    };
                }
                
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