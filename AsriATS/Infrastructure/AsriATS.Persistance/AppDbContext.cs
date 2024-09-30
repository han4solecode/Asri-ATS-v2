using AsriATS.Domain.Entities;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

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
        public DbSet<WorkflowAction> WorkflowActions { get; set; }
        public DbSet<NextStepRule> NextStepsRules { get; set; }
        public DbSet<CompanyRequest> CompanyRequests { get; set; }
        public DbSet<RoleChangeRequest> RoleChangeRequests { get; set; }
        public DbSet<RecruiterRegistrationRequest> RecruiterRegistrationRequests { get; set; }
        public DbSet<JobPostRequest> JobPostRequests { get; set; }

        public DbSet<Company> Companies { get; set; }
        public DbSet<JobPost> JobPosts { get; set; }
        public DbSet<JobPostTemplateRequest> JobPostTemplateRequests { get; set; }
        public DbSet<JobPostTemplate> JobPostTemplates { get; set; }

        public DbSet<ApplicationJob> ApplicationJobs { get; set; }
        public DbSet<SupportingDocument> SupportingDocuments { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

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
            });

            modelBuilder.Entity<RecruiterRegistrationRequest>(entity =>
            {
                entity.HasOne(rr => rr.CompanyIdNavigation)
                      .WithMany(c => c.RecruiterRegistrationRequests)
                      .HasForeignKey(rr => rr.CompanyId)
                      .HasConstraintName("FK_Recruiter_Registration_Request_Company");
            });

            modelBuilder.Entity<WorkflowSequence>(entity =>
            {
                entity.HasOne(wfs => wfs.Workflow).WithMany(w => w.WorkflowSequences)
                     .HasForeignKey(wfs => wfs.WorkflowId)
                     .HasConstraintName("workflow_sequence_id_workflow_fkey");

                entity.HasOne(wfs => wfs.Role).WithMany()
                     .HasForeignKey(wfs => wfs.RequiredRole)
                     .HasConstraintName("workflow_sequence_id_Role_fkey")
                     .OnDelete(DeleteBehavior.Cascade);
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

            // Define the relationship between ApplicationJob and JobPost (JobPostNavigation)
            modelBuilder.Entity<ApplicationJob>()
                .HasOne(aj => aj.JobPostNavigation) // Navigation property
                .WithMany(jp => jp.ApplicationJobsNavigation) // Assuming one job post can have multiple applications
                .HasForeignKey(aj => aj.JobPostId)
                .OnDelete(DeleteBehavior.Cascade); // Optional, depending on deletion logic

            // Define the relationship between ApplicationJob and Process (ProcessIdNavigation)
            modelBuilder.Entity<ApplicationJob>()
                .HasOne(aj => aj.ProcessIdNavigation) // Navigation property
                .WithMany(p => p.ApplicationJobNavigation) // Assuming one process can have multiple applications
                .HasForeignKey(aj => aj.ProcessId)
                .OnDelete(DeleteBehavior.Cascade);

            // Configure ApplicationJob and AppUser relationship
            modelBuilder.Entity<ApplicationJob>()
                .HasOne(a => a.UserIdNavigation)
                .WithMany() // Assuming one user can have multiple jobs
                .HasForeignKey(a => a.UserId)
                .OnDelete(DeleteBehavior.Restrict); // Prevent user deletion from deleting related jobs

            // Configure SupportingDocument and AppUser relationship
            modelBuilder.Entity<SupportingDocument>()
                .HasOne(d => d.UserIdNavigation)
                .WithMany() // Assuming one user can upload multiple documents
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.Restrict); // Prevent user deletion from deleting documents
        }
    }
}