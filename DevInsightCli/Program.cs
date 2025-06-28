using System;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.Threading.Tasks;
using DevInsightCli.Services;
using DevInsightCli.Data;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;

namespace DevInsightCli
{
    class Program
    {
        static async Task<int> Main(string[] args)
        {
            var serviceProvider = ConfigureServices();

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

                // Basic GitHubService usage (will be expanded later)
                // For now, directly instantiating. Will move to DI for services that need it.
                var githubService = serviceProvider.GetRequiredService<GitHubService>();
                var prDetails = await githubService.GetPrDetailsAsync(prUrl);
                Console.WriteLine(prDetails);

                // Example of using WorkflowService (which uses DbContext via DI)
                var workflowService = serviceProvider.GetRequiredService<WorkflowService>();
                var workflow = await workflowService.CreateWorkflowAsync($"Analyze PR: {prUrl}");
                Console.WriteLine($"Created workflow ID: {workflow.Id} with name: {workflow.Name}");
                await workflowService.UpdateWorkflowStepAsync(workflow.Id, "PR URL Received");
                var retrievedWorkflow = await workflowService.GetWorkflowAsync(workflow.Id);
                Console.WriteLine($"Workflow {retrievedWorkflow?.Id} current step: {retrievedWorkflow?.CurrentStep}");

            }, prUrlArgument);

            return await rootCommand.InvokeAsync(args);
        }

        private static ServiceProvider ConfigureServices()
        {
            var services = new ServiceCollection();

            // Configure DbContext
            services.AddDbContext<DevInsightDbContext>(options =>
                options.UseSqlite("Data Source=devinsight.db")); // Connection string for SQLite

            // Register services
            services.AddTransient<GitHubService>(); // GitHubService can be transient for now
            services.AddTransient<LlmService>();     // LlmService can be transient
            services.AddTransient<WorkflowService>(); // WorkflowService depends on DbContext
            services.AddTransient<AgentService>();   // AgentService depends on LlmService (will handle later if direct DI needed)

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
