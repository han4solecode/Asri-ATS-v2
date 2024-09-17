using AsriATS.Domain.Entities;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace AsriATS.Persistance
{
    public class AppDbContext : IdentityDbContext<AppUser, AppRole, string>
    {
        // private readonly IConfiguration _configuration;

        public AppDbContext()
        {
            
        }

        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options)
        {
            // _configuration = configuration;
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            // var connection = _configuration.GetConnectionString("DefaultConnection");
            // optionsBuilder.UseNpgsql(connection);
            // optionsBuilder.UseLazyLoadingProxies();
        }

        // DBSets di bawah ini
        public DbSet<Workflow> Workflows { get; set; }
        public DbSet<Process> Processes { get; set; }
        public DbSet<WorkflowSequence> WorkflowSequences { get; set; }
        public DbSet<Request> Requests { get; set; }
        public DbSet<WorkflowAction> WorkflowActions { get; set; }
        public DbSet<NextStepRule> NextStepsRules { get; set; }

        public DbSet<Company> Companies { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<WorkflowSequence>()
                .HasOne(ws => ws.Role)
                .WithOne()
                .HasForeignKey<WorkflowSequence>(ws => ws.RequiredRole)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Process>(entity =>
            {
                entity.HasOne(p => p.Workflow)
                     .WithMany(w => w.Processes)
                     .HasForeignKey(p => p.WorkflowId)
                     .HasConstraintName("FK_Process_Workflow");

                entity.HasOne(p => p.Requester)
                     .WithMany(r => r.Processes)
                     .HasForeignKey(p => p.RequesterId)
                     .HasConstraintName("FK_Process_Requester");

                entity.HasOne(p => p.Request)
                     .WithOne(r => r.Process)
                     .HasForeignKey<Process>(p => p.RequestId)
                     .HasConstraintName("FK_Process_Request");
            });
            
            modelBuilder.Entity<WorkflowSequence>(entity =>
            {
                entity.HasOne(wfs => wfs.Workflow).WithMany(w => w.WorkflowSequences)
                     .HasForeignKey(wfs => wfs.WorkflowId)
                     .HasConstraintName("workflow_sequence_id_workflow_fkey");
            });
            modelBuilder.Entity<WorkflowAction>(entity =>
            {
                entity.HasOne(wf => wf.Process).WithMany(p => p.WorkflowActions)
                     .HasForeignKey(wf => wf.ProcessId)
                     .HasConstraintName("workflow_action_id_request_fkey");

                entity.HasOne(e => e.Actor).WithMany(u => u.WorkflowActions)
                     .HasForeignKey(e => e.ActorId)
                     .HasConstraintName("FK_WorkflowAction_User");
            });
            modelBuilder.Entity<NextStepRule>(entity =>
            {
                entity.HasOne(d => d.CurrentStep)
                     .WithMany()
                     .HasForeignKey(d => d.CurrentStepId)
                     .HasConstraintName("next_step_rule_id_currentstep_fkey")
                     .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(d => d.NextStep)
                     .WithMany()
                     .HasForeignKey(d => d.NextStepId)
                     .HasConstraintName("next_step_rule_id_nextstep_fkey")
                     .OnDelete(DeleteBehavior.Restrict);
            });
        }
    }
}