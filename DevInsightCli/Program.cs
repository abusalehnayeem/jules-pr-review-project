using System;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.IO; // Required for Path.Combine
using System.Threading.Tasks;
using DevInsightCli.Services;
using DevInsightCli.Data;
using Microsoft.Extensions.Configuration; // Required for IConfiguration
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;

namespace DevInsightCli
{
    class Program
    {
        static async Task<int> Main(string[] args)
        {
            // Build configuration
            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory()) // Ensures appsettings.json is found
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .Build();

            var serviceProvider = ConfigureServices(configuration);

            var rootCommand = new RootCommand("DevInsight CLI - A tool for development insights.");

            // Define 'analyze' command
            var analyzeCommand = new Command("analyze", "Analyze various development artifacts.");
            rootCommand.AddCommand(analyzeCommand);

            // Define 'analyze pr' subcommand
            var prUrlArgument = new Argument<string>("pr-url", "The URL of the GitHub Pull Request to analyze.");
            var analyzePrCommand = new Command("pr", "Analyze a GitHub Pull Request.")
            {
                prUrlArgument
            };
            analyzeCommand.AddCommand(analyzePrCommand);

            analyzePrCommand.SetHandler(async (InvocationContext context) =>
            {
                var prUrl = context.ParseResult.GetValueForArgument(prUrlArgument);
                Console.WriteLine($"Analyzing PR: {prUrl}");

                var githubService = serviceProvider.GetRequiredService<GitHubService>();
                var prDetails = await githubService.GetPrDetailsAsync(prUrl);
                Console.WriteLine(prDetails);

                var workflowService = serviceProvider.GetRequiredService<WorkflowService>();
                var workflow = await workflowService.CreateWorkflowAsync($"Analyze PR: {prUrl}");
                Console.WriteLine($"Created workflow ID: {workflow.Id} with name: {workflow.Name}");
                await workflowService.UpdateWorkflowStepAsync(workflow.Id, "PR URL Received");
                var retrievedWorkflow = await workflowService.GetWorkflowAsync(workflow.Id);
                Console.WriteLine($"Workflow {retrievedWorkflow?.Id} current step: {retrievedWorkflow?.CurrentStep}");

                // Example of using LlmService directly
                Console.WriteLine("\n--- Direct LLM Analysis ---");
                try
                {
                    var llmService = serviceProvider.GetRequiredService<LlmService>();
                    var analysis = await llmService.AnalyzeCodeAsync($"Pull Request Details for direct analysis:\n{prDetails}");
                    Console.WriteLine($"LLM Analysis Result:\n{analysis}");
                }
                catch (Exception ex)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine($"Error during direct LLM analysis: {ex.Message}");
                    Console.ResetColor();
                    Console.WriteLine("Ensure Ollama is running and the model specified in appsettings.json is available.");
                }

                // Example of using AgentService for coordinated analysis
                Console.WriteLine("\n--- Agent-Based Analysis ---");
                try
                {
                    var agentService = serviceProvider.GetRequiredService<AgentService>();
                    // Use PR details as input for agent coordination
                    var agentAnalysisResults = await agentService.CoordinateAnalysisAsync(prDetails);
                    Console.WriteLine("Coordinated Agent Analysis Results:");
                    foreach (var result in agentAnalysisResults)
                    {
                        Console.WriteLine($"- {result.Key}: {result.Value}");
                    }
                }
                catch (Exception ex)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine($"Error during agent-based analysis: {ex.Message}");
                    Console.ResetColor();
                }

            }, prUrlArgument);

            return await rootCommand.InvokeAsync(args);
        }

        private static ServiceProvider ConfigureServices(IConfiguration configuration)
        {
            var services = new ServiceCollection();

            // Register IConfiguration
            services.AddSingleton(configuration);

            // Configure DbContext
            // Read connection string from configuration if available, otherwise default.
            var connectionString = configuration.GetConnectionString("DefaultConnection") ?? "Data Source=devinsight.db";
            services.AddDbContext<DevInsightDbContext>(options =>
                options.UseSqlite(connectionString));

            // Register services
            services.AddTransient<GitHubService>();
            // LlmService now requires IConfiguration, which is automatically injected by DI
            services.AddTransient<LlmService>();
            services.AddTransient<WorkflowService>();
            services.AddTransient<AgentService>();

            // Ensure migrations can run (this is a development-time concern, but good to have context)
            // In a real app, you might run migrations at startup or via a script.
            // For now, we assume the DB is created/migrated externally or by EF Core tools.

            var serviceProvider = services.BuildServiceProvider();

            // Automatically apply migrations (for development convenience)
            // In a production scenario, you might handle migrations differently.
            try
            {
                using (var scope = serviceProvider.CreateScope())
                {
                    var dbContext = scope.ServiceProvider.GetRequiredService<DevInsightDbContext>();
                    dbContext.Database.Migrate(); // Applies pending migrations or creates the DB if it doesn't exist
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred during database migration: {ex.Message}");
                // Depending on the policy, you might want to exit or log this.
            }


            return serviceProvider;
        }
    }
}
