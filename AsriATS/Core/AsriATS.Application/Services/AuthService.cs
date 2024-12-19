using AsriATS.Application.Contracts;
using AsriATS.Application.DTOs;
using AsriATS.Application.DTOs.ChangePassword;
using AsriATS.Application.DTOs.Login;
using AsriATS.Application.DTOs.RefreshToken;
using AsriATS.Application.DTOs.Register;
using AsriATS.Application.DTOs.Update;
using AsriATS.Domain.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace AsriATS.Application.Services
{
    public class AuthService : IAuthService
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly RoleManager<AppRole> _roleManager;
        private readonly IConfiguration _configuration;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public AuthService(UserManager<AppUser> userManager, RoleManager<AppRole> roleManager, IConfiguration configuration, IHttpContextAccessor httpContextAccessor)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _configuration = configuration;
            _httpContextAccessor = httpContextAccessor;
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

            var res = await _userManager.CreateAsync(user, register.Password);

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

        // Register user and add a spesific roles to the user
        public async Task<RegisterResponseDto> RegisterUserAsync(RegisterUserRequestDto register, string rolename)
        {
            var userExist = await _userManager.FindByNameAsync(register.Username);

            if (userExist != null)
            {
                return new RegisterResponseDto
                {
                    Status = "Error",
                    Message = "User already exist!"
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
                Sex = register.Sex,
                CompanyId = register.CompanyId
            };

            var res = await _userManager.CreateAsync(user, register.Password);

            if (!res.Succeeded)
            {
                return new RegisterResponseDto
                {
                    Status = "Error",
                    Message = "User registration failed! Please check user details and try again."
                };
            }

            // add a spesific role to user 
            if (await _roleManager.RoleExistsAsync(rolename))
            {
                await _userManager.AddToRoleAsync(user, rolename);
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

        public async Task<RefreshTokenResponseDto> RefreshAccessTokenAsync(string refreshToken)
        {
            var user = await _userManager.Users.Where(u => u.RefreshToken == refreshToken).SingleOrDefaultAsync();

            if (user == null)
            {
                return new RefreshTokenResponseDto
                {
                    Status = "Error",
                    Message = $"User with {refreshToken} refresh token does not exist."
                };
            }

            if (user.RefreshTokenExpiryTime < DateTime.UtcNow)
            {
                return new RefreshTokenResponseDto
                {
                    Status = "Error",
                    Message = "Refresh token is not valid. Please log in.",
                };
            }
            else
            {
                var userRoles = await _userManager.GetRolesAsync(user);

                var authClaims = new List<Claim>
                {
                    new Claim(ClaimTypes.Name, user.UserName!),
                    new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
                };

                foreach (var userRole in userRoles)
                {
                    authClaims.Add(new Claim(ClaimTypes.Role, userRole));
                }

                var authSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JWT:SigningKey"]!));

                var token = new JwtSecurityToken(
                    issuer: _configuration["JWT:Issuer"],
                    audience: _configuration["JWT:Audience"],
                    expires: DateTime.Now.AddHours(3),
                    claims: authClaims,
                    signingCredentials: new SigningCredentials(authSigningKey, SecurityAlgorithms.HmacSha256)
                );

                return new RefreshTokenResponseDto
                {
                    Status = "Success",
                    Message = "Access token refreshed successfully!",
                    AccessToken = new JwtSecurityTokenHandler().WriteToken(token),
                    AccessTokenExpiryTime = token.ValidTo,
                    RefreshToken = user.RefreshToken,
                    RefreshTokenExpiryTime = user.RefreshTokenExpiryTime,
                    Roles = [.. userRoles],
                    Username = user.UserName
                };
            }
        }

        //login applicant
        public async Task<LoginResponseDto> LoginAsync(LoginRequestDto login)
        {
            var user = await _userManager.FindByNameAsync(login.Username);
            var pass = await _userManager.CheckPasswordAsync(user, login.Password);

            if (user != null && pass)
            {
                var userRoles = await _userManager.GetRolesAsync(user);

                var authClaims = new List<Claim>
                {
                    new Claim(ClaimTypes.Name, user.UserName!),
                    new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
                };

                foreach (var userRole in userRoles)
                {
                    authClaims.Add(new Claim(ClaimTypes.Role, userRole));
                }

                var authSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JWT:SigningKey"]!));

                var token = new JwtSecurityToken(
                    issuer: _configuration["JWT:Issuer"],
                    audience: _configuration["JWT:Audience"],
                    expires: DateTime.Now.AddHours(3),
                    claims: authClaims,
                    signingCredentials: new SigningCredentials(authSigningKey, SecurityAlgorithms.HmacSha256)
                );

                // check if refresh token is exist and valid
                if (user.RefreshToken != null && user.RefreshTokenExpiryTime > DateTime.UtcNow)
                {
                    return new LoginResponseDto
                    {
                        AccessToken = new JwtSecurityTokenHandler().WriteToken(token),
                        AccessTokenExpiryTime = token.ValidTo,
                        RefreshToken = user.RefreshToken,
                        RefreshTokenExpiryTime = user.RefreshTokenExpiryTime,
                        Roles = userRoles.ToList(),
                        Status = "Success",
                        Message = "Login successful!",
                        User = user,
                    };
                }

                // if refresh token is null or invalid, generate refresh token and update user
                var refreshToken = GenerateRefreshToken();
                var refreshTokenExpiryTime = DateTime.UtcNow.AddDays(7);

                user.RefreshToken = refreshToken;
                user.RefreshTokenExpiryTime = refreshTokenExpiryTime;
                await _userManager.UpdateAsync(user);

                return new LoginResponseDto
                {
                    AccessToken = new JwtSecurityTokenHandler().WriteToken(token),
                    AccessTokenExpiryTime = token.ValidTo,
                    RefreshToken = refreshToken,
                    RefreshTokenExpiryTime = refreshTokenExpiryTime,
                    Status = "Success",
                    Message = "Login successful!",
                    Roles = [.. userRoles],
                    User = user
                };
            }

            return new LoginResponseDto
            {
                Status = "Error",
                Message = "Username or password is not valid!"
            };
        }

        public async Task<BaseResponseDto> LogoutAsync()
        {
            var userName = _httpContextAccessor.HttpContext!.User.Identity!.Name!;
            var user = await _userManager.FindByNameAsync(userName);

            if (user == null)
            {
                return new BaseResponseDto
                {
                    Status = "Error",
                    Message = "User not found"
                };
            }

            if (user.RefreshToken == null)
            {
                return new BaseResponseDto
                {
                    Status = "Error",
                    Message = "User already logged out"
                };
            }

            user.RefreshToken = null;
            user.RefreshTokenExpiryTime = null;
            await _userManager.UpdateAsync(user);

            return new BaseResponseDto
            {
                Status = "Success",
                Message = "User logged out successfully"
            };
        }

        private static string GenerateRefreshToken()
        {
            var randNumber = new byte[32];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(randNumber);
                return Convert.ToBase64String(randNumber);
            }
        }

        // Update or manage own data for applicant 
        public async Task<UpdateResponseDto> UpdateApplicantAsync(UpdateRequestDto update)
        {
            // Find the applicant by username (or you could use user ID)
            var user = await _userManager.FindByNameAsync(update.Username);

            if (user == null)
            {
                return new UpdateResponseDto
                {
                    Status = "Error",
                    Message = "Applicant not found!"
                };
            }

            // Update the fields that are allowed to be modified
            user.UserName = update.Username;
            user.Email = update.Email;
            user.PhoneNumber = update.PhoneNumber;
            user.FirstName = update.Firstname;
            user.LastName = update.Lastname;
            user.Address = update.Address;
            user.Dob = update.Dob;
            user.Sex = update.Sex;

            // Save the changes
            var result = await _userManager.UpdateAsync(user);

            if (!result.Succeeded)
            {
                return new UpdateResponseDto
                {
                    Status = "Error",
                    Message = "Applicant update failed! Please check the details and try again."
                };
            }

            return new UpdateResponseDto
            {
                Status = "Success",
                Message = "Applicant details updated successfully."
            };
        }

        // change password
        public async Task<BaseResponseDto> ChangePasswordAsync(ChangePasswordRequestDto request)
        {
            var users = _httpContextAccessor.HttpContext?.User.Identity!.Name;
            // Find the applicant by username (or you could use user ID)
            var user = await _userManager.FindByNameAsync(users);

            if (user == null)
            {
                return new BaseResponseDto
                {
                    Status = "Error",
                    Message = "User not found!"
                };
            }

            // Generate a password reset token
            var resetToken = await _userManager.GeneratePasswordResetTokenAsync(user);

            // Change the user's password
            var result = await _userManager.ResetPasswordAsync(user, resetToken, request.NewPassword);

            if (!result.Succeeded)
            {
                return new BaseResponseDto
                {
                    Status = "Error",
                    Message = "Password change failed! Please check the details and try again."
                };
            }

            return new BaseResponseDto
            {
                Status = "Success",
                Message = "Password changed successfully."
            };
        }
    }
}
