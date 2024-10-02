
using AsriATS.Application;
using AsriATS.Application.DTOs.Email;
using AsriATS.Application.Options;
using AsriATS.Persistance;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.FileProviders;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace AsriATS.WebAPI;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Add services to the container.
        builder.Services.ConfigurePersistance(builder.Configuration);
        builder.Services.ConfigureApplication();
        builder.Services.ConfigureIdentity();

        var configuration = builder.Configuration;

        builder.Services.AddAuthentication(opt => {
            opt.DefaultAuthenticateScheme =
            opt.DefaultChallengeScheme =
            opt.DefaultForbidScheme =
            opt.DefaultScheme =
            opt.DefaultSignInScheme =
            opt.DefaultSignOutScheme = JwtBearerDefaults.AuthenticationScheme;
        }).AddJwtBearer(opt => {
            opt.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidIssuer = configuration["JWT:Issuer"],
                ValidateAudience = true,
                ValidAudience = configuration["JWT:Audience"],
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["JWT:SigningKey"]!)),
            };
        });

        // MailSettings options
        builder.Services.Configure<MailOptions>(builder.Configuration.GetSection(MailOptions.MailSettings));

        builder.Services.AddControllers();
        // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();

        var app = builder.Build();

        // Configure the HTTP request pipeline.
        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        app.UseHttpsRedirection();
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
