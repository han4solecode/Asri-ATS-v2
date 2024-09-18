using AsriATS.Application.Contracts;
using AsriATS.Application.DTOs;
using AsriATS.Application.DTOs.Register;
using AsriATS.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AsriATS.Application.Services
{
    public class AuthService:IAuthService
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly RoleManager<AppRole> _roleManager;
        private readonly IConfiguration _configuration;
    
        public AuthService(UserManager<AppUser> userManager, RoleManager<AppRole> roleManager, IConfiguration configuration)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _configuration = configuration;
        }

        public async Task<RegisterResponseDto> RegisterApplicantAsync(RegisterRequestDto register)
        {
            var userExist = await _userManager.FindByNameAsync(register.Username);

            if (userExist != null)
            {
                return new RegisterResponseDto
                {
                    Status = "Error",
                    Message = "Applicant already exist!"
                };
            }

            var user = new AppUser
            {
                UserName = register.Username,
                Email = register.Email,
                SecurityStamp = Guid.NewGuid().ToString(),
                PhoneNumber = register.PhoneNumber,
                FirstName = register.Firstname,
                LastName = register.Lastname,
                Address = register.Address,
                Dob = register.Dob,
                Sex = register.Sex
            };

            var res = await _userManager.CreateAsync(user);

            if (!res.Succeeded)
            {
                return new RegisterResponseDto
                {
                    Status = "Error",
                    Message = "Applicant registration failed! Please check user details and try again."
                };
            }
            if (await _roleManager.RoleExistsAsync("Applicant"))
            {
                await _userManager.AddToRoleAsync(user, "Applicant");
            }

            return new RegisterResponseDto
            {
                Status = "Success",
                Message = "Applicant Created successfully"
            };
        }

        public async Task<BaseResponseDto> RegisterHRManagerAsync(RegisterHRManagerRequestDto registerHRManagerRequest)
        {
            // TODO
            // get data from approved company request for its id
            // register new user as a HR Manager with specific company id

            throw new NotImplementedException();
        }
    }
}
