using System.ComponentModel;
using System.Diagnostics;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;

namespace Workshop.SemanticKernel.MultiAgent
{
    public class ToolFactory
    {
        private readonly Dictionary<string, Func<KernelPlugin>> _pluginRegistry = new();
        private readonly ILogger<ToolFactory> _logger;


        public ToolFactory(ILoggerFactory loggerFactory, Settings settings)
        {
            _logger = loggerFactory.CreateLogger<ToolFactory>();
            var availableTools = settings.GetSettings<List<Settings.ToolSettings>>("tools");
            foreach (var tool in availableTools)
            {
                if (_pluginRegistry.ContainsKey(tool.Name))
                {
                    _logger.LogWarning($"Tool '{tool.Name}' is already registered. Skipping registration.");
                    continue;
                }

                switch (tool.Name)
                {
                    case "FileSystem":
                        if(tool.Parameters.Count == 0 || !tool.Parameters.ContainsKey("basepath"))
                        {
                            _logger.LogWarning($"Tool '{tool.Name}' requires 'basePath' parameter. Skipping registration.");
                            continue;
                            
                        }
                        Register("FileSystem", () => KernelPluginFactory.CreateFromObject(new FileSystemPlugin( tool.Parameters["basepath"])));
                        break;
                    default:
                        _logger.LogWarning($"Tool '{tool.Name}' is not supported. Skipping registration.");
                        break;
                }
            }
        }
        
        public KernelPlugin GetTool(string toolName)
        {
            if (_pluginRegistry.TryGetValue(toolName, out var factory))
            {
                return factory();
            }
            else
            {
                _logger.LogWarning($"Tool '{toolName}' is not registered.");
                return null;
            }
        }

        private void Register(string toolName, Func<KernelPlugin> pluginFactory)
        {
            _pluginRegistry[toolName] = pluginFactory;
            _logger.LogInformation($"Tool '{toolName}' is registered.");
        }
    }
    
    public class FileSystemPlugin
    {
        private readonly string _allowedBaseDirectoryFullPath;

        // Constructor accepting the configured base path
        public FileSystemPlugin(string configuredBasePath)
        {
            // Ensure the base path is absolute and normalized
            _allowedBaseDirectoryFullPath = Path.GetFullPath(configuredBasePath);

            // Optionally, ensure the directory exists during initialization
            if (!Directory.Exists(_allowedBaseDirectoryFullPath))
            {
                throw new DirectoryNotFoundException($"The base directory '{_allowedBaseDirectoryFullPath}' does not exist.");
            }
        }
        
        [KernelFunction, Description("Reads the entire content of a specified file.")]
        public async Task<string> ReadFileAsync(
            [Description("The full path to the file to read.")] string filePath)
        {
            try
            {
                // Basic security check (adjust as needed for your environment)
                // Ensure the path is within an allowed directory if necessary.
                if (!IsPathAllowed(filePath)) // Implement IsPathAllowed based on your security needs
                {
                    return "Error: Access to the specified path is denied.";
                }
                return await File.ReadAllTextAsync(filePath);
            }
            catch (FileNotFoundException)
            {
                return $"Error: File not found at path '{filePath}'.";
            }
            catch (DirectoryNotFoundException)
            {
                 return $"Error: Directory not found for path '{filePath}'.";
            }
            catch (IOException ex)
            {
                return $"Error reading file: {ex.Message}";
            }
            catch (Exception ex)
            {
                // Catch other potential exceptions like security issues
                return $"An unexpected error occurred while reading the file: {ex.Message}";
            }
        }

        [KernelFunction, Description("Writes the given content to a specified file. Overwrites the file if it already exists.")]
        public async Task<string> WriteFileAsync(
            [Description("The full path to the file to write.")] string filePath,
            [Description("The content to write to the file.")] string content)
        {
             try
            {
                // Basic security check (adjust as needed)
                if (!IsPathAllowed(filePath))
                {
                    return "Error: Access to the specified path is denied.";
                }

                // Ensure directory exists
                var directory = Path.GetDirectoryName(filePath);
                if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                await File.WriteAllTextAsync(filePath, content);
                return $"Successfully wrote content to '{filePath}'.";
            }
            catch (IOException ex)
            {
                return $"Error writing file: {ex.Message}";
            }
            catch (Exception ex)
            {
                // Catch other potential exceptions
                return $"An unexpected error occurred while writing the file: {ex.Message}";
            }
        }


        private bool IsPathAllowed(string requestedPath)
        {
            try
            {
                // Critical step: Resolve the requested path to its absolute, canonical form
                string requestedFullPath = Path.GetFullPath(requestedPath);

                // Check if the canonical path starts with the allowed base directory
                // Use OrdinalIgnoreCase for cross-platform compatibility
                return requestedFullPath.StartsWith(_allowedBaseDirectoryFullPath, StringComparison.OrdinalIgnoreCase);
            }
            catch (ArgumentException)
            {
                // Path.GetFullPath can throw ArgumentException for invalid paths
                return false;
            }
            catch (PathTooLongException)
            {
                return false; // Reject overly long paths
            }
            catch (NotSupportedException)
            {
                // Paths with colons (other than drive letter) can cause this
                return false;
            }
            // Potentially catch other IO related exceptions if GetFullPath fails unexpectedly
            catch (Exception)
            {
                // Log this unexpected error
                return false;
            }
        }
    }
}
