using AsriATS.Application.Persistance;
using AsriATS.Domain.Entities;
using AsriATS.Persistance.Repositories;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace AsriATS.Persistance
{
    public static class PersistanceServiceExtension
    {
        public static void ConfigurePersistance(this IServiceCollection services, IConfiguration configuration)
        {
            var connection = configuration.GetConnectionString("DefaultConnection");
            services.AddDbContext<AppDbContext>(opt => {
                opt.UseNpgsql(connection);
            });

            services.AddScoped<ICompanyRepository, CompanyRepository>();
            services.AddScoped<IWorkflowRepository, WorkflowRepository>();
            services.AddScoped<IWorkflowSequenceRepository, WorkflowSequenceRepository>();
            services.AddScoped<INextStepRuleRepository, NextStepRuleRepository>();
            services.AddScoped<ICompanyRequestRepository, CompanyRequestRepository>();
            services.AddScoped<IRecruiterRegistrationRequestRepository, RecruiterRegistrationRequestRepository>();
            services.AddScoped<IRoleChangeRequestRepository, RoleChangeRequestRepository>();
            services.AddScoped<IJobPostRepository, JobPostRepository>();
            services.AddScoped<IJobPostRequestRepository, JobPostRequestRepository>();
            services.AddScoped<IProcessRepository, ProcessRepository>();
            services.AddScoped<IWorkflowActionRepository, WorkflowActionRepository>();
            services.AddScoped<IJobTemplateRequestRepository, JobPostTemplateRequestRepository>();
            services.AddScoped<IJobPostTemplateRepository, JobPostTemplateRepository>();
            services.AddScoped<IApplicationJobRepository, ApplicationJobRepository>();
            services.AddScoped<IDocumentSupportRepository, DocumentSupportRepository>();
            services.AddScoped<IInterviewSchedulingRepository, InterviewSchedulingRepository>();
        }

        public static void ConfigureIdentity(this IServiceCollection services)
        {
            services.AddIdentity<AppUser, AppRole>(opt => {
                opt.SignIn.RequireConfirmedEmail = true;
                opt.Password.RequireLowercase = true;
                opt.Password.RequireUppercase = true;
                opt.Password.RequireNonAlphanumeric = false;
                opt.Password.RequiredLength = 8;
            }).AddEntityFrameworkStores<AppDbContext>()
            .AddDefaultTokenProviders();
        }
    }
}