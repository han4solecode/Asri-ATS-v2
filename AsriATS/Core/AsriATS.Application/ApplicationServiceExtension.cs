using AsriATS.Application.Contracts;
using AsriATS.Application.Services;
using Microsoft.Extensions.DependencyInjection;

namespace AsriATS.Application
{
    public static class ApplicationServiceExtension
    {
        public static void ConfigureApplication(this IServiceCollection services)
        {
            services.AddScoped<IRoleService, RoleService>();
            services.AddScoped<IAuthService, AuthService>();
            services.AddScoped<IWorkflowService, WorkflowService>();
            services.AddScoped<IUserService, UserService>();
            services.AddScoped<IRecruiterRegistrationRequestService, RecruiterRegistrationRequestService>();
            services.AddScoped<ICompanyService, CompanyService>();
            services.AddTransient<IEmailService, EmailService>();
            services.AddScoped<IJobPostRequestService, JobPostRequestService>();
            services.AddScoped<IJobPostService, JobPostService>();
            services.AddScoped<IJobPostTemplateRequestService, JobPostTemplateRequestService>();
        }
    }
}