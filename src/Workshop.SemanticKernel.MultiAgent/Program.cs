using Microsoft.SemanticKernel.Agents;

namespace Workshop.SemanticKernel.MultiAgent
{

    public class Program
    {
        public static async Task Main(string[] args)
        {
            var settings = new Settings();
            var agents = new Agents();
            agents.InitializeAgents(settings);
            
            var scenarios = new Scenarios();
            scenarios.Initialize(settings, agents.AvailableAgents);
            
     
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