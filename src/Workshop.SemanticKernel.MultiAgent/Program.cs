using Microsoft.Extensions.Logging; // Core logging interfaces
using Microsoft.SemanticKernel;
using System; // For AppContext

namespace Workshop.SemanticKernel.MultiAgent
{

    public class Program
    {
        public static async Task Main(string[] args)
        {
            // --- Simple Logging Setup ---
            // normally you'd go with DI and preferably OpenTelemetry
            
            // Create a logger factory specifically for console output
            using var loggerFactory = LoggerFactory.Create(builder =>
            {
                builder
                    .SetMinimumLevel(LogLevel.Trace) // Set level to Trace for maximum detail
                    .AddConsole(); // Add the simple console logger provider
                // Optional: Add filters if needed later
                // builder.AddFilter("Microsoft.SemanticKernel", LogLevel.Debug);
            });
            // Still needed even with basic logging if you want connector details.
            AppContext.SetSwitch("Microsoft.SemanticKernel.Experimental.GenAI.EnableOTelDiagnosticsSensitive", true); 
            var logger = loggerFactory.CreateLogger<Program>();
            logger.LogInformation("Simple console logging configured.");
            
            var settings = new Settings();
            var agents = new Agents();
            var tools = new ToolFactory(settings);
            agents.InitializeAgents(settings, tools, loggerFactory);
            
            var scenarios = new Scenarios();
            scenarios.Initialize(loggerFactory, settings, agents.AvailableAgents);
            
     
            // get the prompt from console.readline and run on a predefined scenario
            // that obviously doesn't make a lot of sense for a console application but can be easily extended for a webapi / desktop application
            Console.WriteLine("The following scenarios are available:");
            foreach (var availableScenario in scenarios.GetAvailableScenarios())
            {
                Console.WriteLine($"\t{availableScenario}");
            }
            Console.WriteLine("Please select a scenario:");
            var scenarioName = Console.ReadLine();
            if (string.IsNullOrEmpty(scenarioName) || !scenarios.GetAvailableScenarios().Contains(scenarioName))
            {
                Console.WriteLine("No scenario valid selected. Exiting.");
                return;
            }
            
            Console.WriteLine("Please enter a prompt:");
            var prompt = Console.ReadLine();
            if (string.IsNullOrEmpty(prompt))
            {
                Console.WriteLine("No prompt entered. Exiting.");
                return;
            }
            
            await scenarios.ExecuteAsync(scenarioName, prompt);



            Console.ReadLine();
        }
         
    }
}