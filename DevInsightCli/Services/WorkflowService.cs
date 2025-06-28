using DevInsightCli.Data;
using Microsoft.EntityFrameworkCore;

namespace DevInsightCli.Services
{
    // Placeholder for a workflow entity
    public class Workflow
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string CurrentStep { get; set; }
        public bool IsCompleted { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }

    public class WorkflowService
    {
        private readonly DevInsightDbContext _dbContext;

        public WorkflowService(DevInsightDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<Workflow> CreateWorkflowAsync(string name)
        {
            var workflow = new Workflow
            {
                Name = name,
                CurrentStep = "Initial",
                IsCompleted = false,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
            _dbContext.Workflows.Add(workflow);
            await _dbContext.SaveChangesAsync();
            return workflow;
        }

        public async Task<Workflow?> GetWorkflowAsync(int id)
        {
            return await _dbContext.Workflows.FindAsync(id);
        }

        public async Task UpdateWorkflowStepAsync(int id, string newStep)
        {
            var workflow = await _dbContext.Workflows.FindAsync(id);
            if (workflow != null)
            {
                workflow.CurrentStep = newStep;
                workflow.UpdatedAt = DateTime.UtcNow;
                await _dbContext.SaveChangesAsync();
            }
        }

        public async Task CompleteWorkflowAsync(int id)
        {
            var workflow = await _dbContext.Workflows.FindAsync(id);
            if (workflow != null)
            {
                workflow.IsCompleted = true;
                workflow.CurrentStep = "Completed";
                workflow.UpdatedAt = DateTime.UtcNow;
                await _dbContext.SaveChangesAsync();
            }
        }
    }
}
