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

        public DbSet<InterviewScheduling> InterviewScheduling { get; set; }

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

            // Configuring ApplicationJob relationships
            modelBuilder.Entity<ApplicationJob>(entity =>
            {
                // Foreign key to AspNetUsers (User)
                entity.HasOne(aj => aj.UserIdNavigation)
                    .WithMany(aj => aj.ApplicationJobs)
                    .HasForeignKey(aj => aj.UserId)
                    .OnDelete(DeleteBehavior.Restrict); 

                // Foreign key to JobPost
                entity.HasOne(aj => aj.JobPostNavigation)
                    .WithMany(jp => jp.ApplicationJobsNavigation) 
                    .HasForeignKey(aj => aj.JobPostId)
                    .OnDelete(DeleteBehavior.Cascade); // Cascade delete will remove the application if the job post is deleted

                // Foreign key to Process
                entity.HasOne(aj => aj.ProcessIdNavigation)
                    .WithMany(p => p.ApplicationJobNavigation) 
                    .HasForeignKey(aj => aj.ProcessId)
                    .OnDelete(DeleteBehavior.Cascade); // Cascade delete will remove the application if the process is deleted

                // Foreign key to SupportingDocuments (one-to-many relationship)
                entity.HasMany(aj => aj.SupportingDocumentsIdNavigation)
                    .WithMany(sd => sd.ApplicationJobNavigation) 
                    .UsingEntity(j => j.ToTable("ApplicationJobSupportingDocuments")); // Configuring the join table for many-to-many
            });

            // Configuring SupportingDocument relationships
            modelBuilder.Entity<SupportingDocument>(entity =>
            {
                // Foreign key to AspNetUsers (User)
                entity.HasOne(sd => sd.UserIdNavigation)
                    .WithMany(sd => sd.SupportingDocuments)
                    .HasForeignKey(sd => sd.UserId)
                    .OnDelete(DeleteBehavior.Restrict); // Use Restrict to avoid cascading delete on users
            });

            modelBuilder.Entity<InterviewScheduling>(entity =>
            {
                entity.HasOne(i => i.ApplicationIdNavigation)
                    .WithOne()
                    .HasForeignKey<InterviewScheduling>(i => i.ApplicationId)
                    .OnDelete(DeleteBehavior.Restrict); // Use Restrict to avoid cascading delete on users
            });
        }
    }
}