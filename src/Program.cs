using Microsoft.Extensions.Logging; 

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
                    .SetMinimumLevel(LogLevel.Information)
                    .AddConsole();
                // Optional: Add filters if needed later
                builder.AddFilter("Microsoft.SemanticKernel", LogLevel.Debug);
            });
            
            var logger = loggerFactory.CreateLogger<Program>();
            logger.LogInformation("Simple console logging configured.");
            
            var settings = new Settings();
            var agents = new Agents();
            var tools = new ToolFactory(loggerFactory, settings);
            agents.InitializeAgents(loggerFactory, settings, tools);
            
            var scenarios = new Scenarios();
            scenarios.Initialize(loggerFactory, settings, agents.GetAvailableAgents());
     
            Console.WriteLine("The following scenarios are available:");
            var availableScenarios = scenarios.GetAvailableScenarios().ToList();
            for (var i = 0; i < availableScenarios.Count; i++)
            {
                Console.WriteLine($"\t{i + 1}. {availableScenarios[i]}");
            }
            string? scenarioName = null;
            var attemptsLeft = 3;
            while (attemptsLeft > 0)
            {
                Console.WriteLine($"Please enter your choice. (Attempts left: {attemptsLeft}):");
                var input = Console.ReadLine();

                if (int.TryParse(input, out int scenarioNumber) &&
                    scenarioNumber > 0 && scenarioNumber <= availableScenarios.Count)
                {
                    scenarioName = availableScenarios[scenarioNumber - 1];
                    break;
                }
            
                attemptsLeft--;
                logger.LogWarning("Invalid selection. Enter a valid scenario number.");
            }

            if (scenarioName is null)
            {
                logger.LogWarning("No valid selection after 3 attempts. Exiting.");
                return;
            }

            Console.WriteLine("How would you like to provide your prompt?");
            Console.WriteLine("\t1. Enter prompt via command line");
            Console.WriteLine("\t2. Provide a path to a markdown file");
            string? prompt = null;
            var promptAttemptsLeft = 3;

            while (promptAttemptsLeft > 0)
            {
                Console.WriteLine($"Please enter your choice. (Attempts left: {promptAttemptsLeft})");
                var optionInput = Console.ReadLine();

                if (optionInput == "1")
                {
                    Console.WriteLine("Please enter your prompt:");
                    prompt = Console.ReadLine();
                    if (!string.IsNullOrWhiteSpace(prompt))
                    {
                        break;
                    }
                    logger.LogWarning("Prompt cannot be empty.");
                }
                else if (optionInput == "2")
                {
                    Console.WriteLine("Please enter the full path to the markdown (.md) file:");
                    var filePath = Console.ReadLine();

                    if (!string.IsNullOrWhiteSpace(filePath) && 
                        File.Exists(filePath) && 
                        Path.GetExtension(filePath).Equals(".md", StringComparison.OrdinalIgnoreCase))
                    {
                        prompt = await File.ReadAllTextAsync(filePath);
                        if (!string.IsNullOrWhiteSpace(prompt))
                        {
                            break;
                        }
                        logger.LogWarning("Markdown file is empty.");
                    }
                    else
                    {
                        logger.LogWarning("Invalid file path, file does not exist, or file is not a markdown file.");
                    }
                }
                else
                {
                    logger.LogWarning("Invalid choice. Please select option 1 or 2.");
                }

                promptAttemptsLeft--;
            }

            if (string.IsNullOrWhiteSpace(prompt))
            {
                logger.LogWarning("No valid prompt provided after 3 attempts. Exiting.");
                return;
            }

            await scenarios.ExecuteAsync(scenarioName, prompt);


            Console.ReadLine();
        }
         
    }
}