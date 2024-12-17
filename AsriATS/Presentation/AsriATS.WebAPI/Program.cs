
using AsriATS.Application;
using AsriATS.Application.DTOs.Email;
using AsriATS.Application.Options;
using AsriATS.Persistance;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.CookiePolicy;
using Microsoft.Extensions.FileProviders;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Reflection;
using System.Text;

namespace AsriATS.WebAPI;

public class Program
{
    public static void Main(string[] args)
    {
        var MyAllowSpecificOrigins = "_myAllowSpecificOrigins";
        var builder = WebApplication.CreateBuilder(args);

        // Add services to the container.
        builder.Services.ConfigurePersistance(builder.Configuration);
        builder.Services.ConfigureApplication();
        builder.Services.ConfigureIdentity();

        // CORS setup
        builder.Services.AddCors(options => options.AddPolicy(name: MyAllowSpecificOrigins, policy =>
        {
            policy.WithOrigins("http://localhost:5173").AllowAnyHeader().AllowAnyMethod().AllowCredentials().WithExposedHeaders("Content-Disposition");
        }));

        var configuration = builder.Configuration;

        builder.Services.AddAuthentication(opt =>
        {
            opt.DefaultAuthenticateScheme =
            opt.DefaultChallengeScheme =
            opt.DefaultForbidScheme =
            opt.DefaultScheme =
            opt.DefaultSignInScheme =
            opt.DefaultSignOutScheme = JwtBearerDefaults.AuthenticationScheme;
        }).AddJwtBearer(opt =>
        {
            opt.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidIssuer = configuration["JWT:Issuer"],
                ValidateAudience = true,
                ValidAudience = configuration["JWT:Audience"],
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["JWT:SigningKey"]!)),
            };

            opt.Events = new JwtBearerEvents
            {
                OnTokenValidated = context =>
                {
                    return Task.CompletedTask;
                },
                OnAuthenticationFailed = context =>
                {
                    context.Response.StatusCode = 401;
                    return Task.CompletedTask;
                },
                OnMessageReceived = context =>
                {
                    context.Token = context.Request.Cookies["AuthToken"];
                    return Task.CompletedTask;
                }
            };
        });

        // MailSettings options
        builder.Services.Configure<MailOptions>(builder.Configuration.GetSection(MailOptions.MailSettings));

        builder.Services.AddControllers();
        // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
        builder.Services.AddEndpointsApiExplorer();

        //Swagger Documentation Section
        var info = new OpenApiInfo()
        {
            Title = "Your API Documentation",
            Version = "v1",
            Description = "Description of your API",
            Contact = new OpenApiContact()
            {
                Name = "Your name",
                Email = "your@email.com",
            }

        };

        builder.Services.AddSwaggerGen(c =>
        {
            c.SwaggerDoc("v1", info);

            // Set the comments path for the Swagger JSON and UI.
            var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
            var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
            c.IncludeXmlComments(xmlPath);
        });

        // Cookie Configuration
        builder.Services.AddCookiePolicy(opt =>
        {
            opt.HttpOnly = HttpOnlyPolicy.Always;
            opt.Secure = CookieSecurePolicy.Always;
        });

        var app = builder.Build();

        // Configure the HTTP request pipeline.
        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger(u =>
            {
                u.RouteTemplate = "swagger/{documentName}/swagger.json";
            });
            app.UseSwaggerUI(c =>
            {
                c.RoutePrefix = "swagger";
                c.SwaggerEndpoint(url: "/swagger/v1/swagger.json", name: "Your API Title or Version");
            });
        }

        app.UseHttpsRedirection();
        // CORS
        app.UseCors(MyAllowSpecificOrigins);

        app.UseStaticFiles(new StaticFileOptions
        {
            FileProvider = new PhysicalFileProvider(Path.Combine(builder.Environment.ContentRootPath, "uploads")),
            RequestPath = "/uploads"
        });

        app.UseAuthentication();
        app.UseAuthorization();

        app.MapControllers();

        app.Run();
    }
}
