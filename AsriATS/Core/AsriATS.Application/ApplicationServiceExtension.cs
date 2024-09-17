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
        }
    }
}