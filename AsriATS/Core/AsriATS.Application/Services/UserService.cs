using AsriATS.Application.Contracts;
using AsriATS.Application.DTOs.ChangePassword;
using AsriATS.Application.DTOs;
using AsriATS.Application.DTOs.Update;
using AsriATS.Domain.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using AsriATS.Application.DTOs.User;
using AsriATS.Application.Persistance;
using AsriATS.Application.DTOs.Report;
using System.Reflection.Metadata;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;

namespace AsriATS.Application.Services
{
    public class UserService : IUserService
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly RoleManager<AppRole> _roleManager;
        private readonly ICompanyRepository _companyRepository;
        public readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IDocumentSupportRepository _documentSupportRepository;

        public UserService(UserManager<AppUser> userManager, RoleManager<AppRole> roleManager, IHttpContextAccessor httpContextAccessor, IDocumentSupportRepository documentSupportRepository, ICompanyRepository companyRepository)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _httpContextAccessor = httpContextAccessor;
            _documentSupportRepository = documentSupportRepository;
            _companyRepository = companyRepository;
        }

        // update user based on login user and roles
        public async Task<UpdateResponseDto> UpdateUserAsync(UpdateRequestDto update)
        {
            // Get the username of the currently logged-in user
            var currentUser = _httpContextAccessor.HttpContext?.User.Identity!.Name;

            if (currentUser == null)
            {
                return new UpdateResponseDto
                {
                    Status = "Error",
                    Message = "User is not logged in."
                };
            }

            // Get the roles of the currently logged-in user
            var currentUserRoles = _httpContextAccessor.HttpContext?.User.Claims
                .Where(c => c.Type == ClaimTypes.Role)
                .Select(c => c.Value)
                .ToList();

            // Find the user to be updated by username
            var userToUpdate = await _userManager.FindByNameAsync(update.Username!);

            if (userToUpdate == null)
            {
                return new UpdateResponseDto
                {
                    Status = "Error",
                    Message = "User not found!"
                };
            }

            // Get the currently logged-in user object to check their CompanyId
            var currentUserObject = await _userManager.FindByNameAsync(currentUser);

            if (currentUserObject == null)
            {
                return new UpdateResponseDto
                {
                    Status = "Error",
                    Message = "Current user not found!"
                };
            }

            // Ensure new username is not already taken
            if (!string.IsNullOrEmpty(update.Username) && update.Username != userToUpdate.UserName)
            {
                var existingUserByUsername = await _userManager.FindByNameAsync(update.Username);
                if (existingUserByUsername != null)
                {
                    return new UpdateResponseDto
                    {
                        Status = "Error",
                        Message = "Username is already taken!"
                    };
                }
            }

            // Ensure new email is not already taken
            if (!string.IsNullOrEmpty(update.Email) && update.Email != userToUpdate.Email)
            {
                var existingUserByEmail = await _userManager.FindByEmailAsync(update.Email);
                if (existingUserByEmail != null)
                {
                    return new UpdateResponseDto
                    {
                        Status = "Error",
                        Message = "Email is already taken!"
                    };
                }
            }

            // Check if the current user has the rights to update the target user
            if (currentUserRoles.Contains("Administrator"))
            {
                // Administrator can update anyone
                UpdateUserFields(userToUpdate, update);
            }
            else if (currentUserRoles.Contains("HR Manager"))
            {
                if (update.Username == currentUser ||
                    (await _userManager.IsInRoleAsync(userToUpdate, "Recruiter") && currentUserObject.CompanyId == userToUpdate.CompanyId))
                {
                    // HR Manager can update their own details or those of recruiters within the same company
                    UpdateUserFields(userToUpdate, update);
                }
                else if (await _userManager.IsInRoleAsync(userToUpdate, "HR Manager"))
                {
                    // HR Manager can update another HR Manager only if they belong to the same company
                    if (currentUserObject.CompanyId == userToUpdate.CompanyId)
                    {
                        UpdateUserFields(userToUpdate, update);
                    }
                    else
                    {
                        return new UpdateResponseDto
                        {
                            Status = "Error",
                            Message = "HR Manager can only update another HR Manager from the same company."
                        };
                    }
                }
                else
                {
                    return new UpdateResponseDto
                    {
                        Status = "Error",
                        Message = "HR Manager can only update their own account or recruiters from the same company."
                    };
                }
            }
            else if (currentUserRoles.Contains("Recruiter"))
            {
                if (update.Username == currentUser)
                {
                    // Recruiter can only update their own account
                    UpdateUserFields(userToUpdate, update);
                }
                else
                {
                    return new UpdateResponseDto
                    {
                        Status = "Error",
                        Message = "Recruiter can only update their own account."
                    };
                }
            }
            else if (currentUserRoles.Contains("Applicant"))
            {
                if (update.Username == currentUser)
                {
                    // Applicant can only update their own account
                    UpdateUserFields(userToUpdate, update);
                }
                else
                {
                    return new UpdateResponseDto
                    {
                        Status = "Error",
                        Message = "Applicant can only update their own account."
                    };
                }
            }
            else
            {
                return new UpdateResponseDto
                {
                    Status = "Error",
                    Message = "User does not have permission to update this account."
                };
            }

            // Save the changes
            var result = await _userManager.UpdateAsync(userToUpdate);

            if (!result.Succeeded)
            {
                return new UpdateResponseDto
                {
                    Status = "Error",
                    Message = "User update failed! Please check the details and try again."
                };
            }

            return new UpdateResponseDto
            {
                Status = "Success",
                Message = "User details updated successfully."
            };
        }


        // Get user info by login
        public async Task<object> GetUserInfo()
        {
            // Get the username of the currently logged-in user
            var userName = _httpContextAccessor.HttpContext?.User.Identity!.Name;

            if (userName == null)
            {
                throw new UnauthorizedAccessException("User is not logged in.");
            }

            // Find the user by username (or you could use user ID if needed)
            var user = await _userManager.FindByNameAsync(userName);

            if (user == null)
            {
                throw new Exception("User not found.");
            }

            // Get the roles associated with the user
            var roles = await _userManager.GetRolesAsync(user);
            
            // Get Company Name for User
            var company = user?.CompanyId != null ? await _companyRepository.GetByIdAsync(user.CompanyId.Value) : null;

            // Return the user information along with roles
            return new
            {
                userId = user.Id,
                UserName = user.UserName,
                Email = user.Email,
                PhoneNumber = user.PhoneNumber,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Address = user.Address,
                Dob = user.Dob,
                Sex = user.Sex,
                Roles = roles,
                CompanyName = company?.Name ?? ""
            };
        }

        // delete user based on login and user roles
        public async Task<bool> DeleteUserAsync(string userName)
        {
            // Get the username of the currently logged-in user
            var currentUser = _httpContextAccessor.HttpContext?.User.Identity!.Name;

            if (currentUser == null)
            {
                throw new UnauthorizedAccessException("User is not logged in.");
            }

            // Get the roles of the currently logged-in user
            var currentUserRoles = _httpContextAccessor.HttpContext?.User.Claims
                .Where(c => c.Type == ClaimTypes.Role)
                .Select(c => c.Value)
                .ToList();

            // Find the user to be deleted
            var userToDelete = await _userManager.FindByNameAsync(userName);

            if (userToDelete == null)
            {
                throw new Exception("User not found.");
            }

            // Get the currently logged-in user object
            var currentUserObject = await _userManager.FindByNameAsync(currentUser);

            if (currentUserObject == null)
            {
                throw new Exception("Current user not found.");
            }

            // Ensure user can only delete their own account or has proper permissions
            if (currentUserRoles.Contains("Administrator"))
            {
                // Administrator can delete anyone, regardless of company
                await _userManager.DeleteAsync(userToDelete);
            }
            else if (currentUserRoles.Contains("HR Manager"))
            {
                // HR Manager can delete their own account or recruiter accounts if they belong to the same company
                if (userName == currentUser ||
                    (await _userManager.IsInRoleAsync(userToDelete, "Recruiter") && currentUserObject.CompanyId == userToDelete.CompanyId))
                {
                    await _userManager.DeleteAsync(userToDelete);
                }
                else if (await _userManager.IsInRoleAsync(userToDelete, "HR Manager"))
                {
                    if (currentUserObject.CompanyId != userToDelete.CompanyId)
                    {
                        throw new UnauthorizedAccessException("HR Manager can only delete another HR Manager from the same company.");
                    }
                    await _userManager.DeleteAsync(userToDelete);
                }
                else
                {
                    throw new UnauthorizedAccessException("HR Manager can only delete their own account or recruiters from the same company.");
                }
            }
            else if (currentUserRoles.Contains("Recruiter"))
            {
                // Recruiter can only delete their own account and cannot delete anyone else
                if (userName == currentUser)
                {
                    await _userManager.DeleteAsync(userToDelete);
                }
                else
                {
                    throw new UnauthorizedAccessException("Recruiter can only delete their own account.");
                }
            }
            else if (currentUserRoles.Contains("Applicant"))
            {
                // Applicant can only delete their own account
                if (userName == currentUser)
                {
                    await _userManager.DeleteAsync(userToDelete);
                }
                else
                {
                    throw new UnauthorizedAccessException("Applicant can only delete their own account.");
                }
            }
            else
            {
                throw new UnauthorizedAccessException("User does not have permission to delete this account.");
            }

            return true;
        }

        // Method for view HR or Recruiter in same company
        public async Task<List<UserResponseDto>> GetUsersInSameCompanyAsync()
        {
            // Get the username of the currently logged-in user
            var currentUser = _httpContextAccessor.HttpContext?.User.Identity!.Name;

            if (currentUser == null)
            {
                throw new UnauthorizedAccessException("User is not logged in.");
            }

            // Get the roles of the currently logged-in user
            var currentUserRoles = _httpContextAccessor.HttpContext?.User.Claims
                .Where(c => c.Type == ClaimTypes.Role)
                .Select(c => c.Value)
                .ToList();

            // Check if the user is a Recruiter or HR Manager
            if (!currentUserRoles.Contains("HR Manager") && !currentUserRoles.Contains("Recruiter"))
            {
                throw new UnauthorizedAccessException("Only HR Managers and Recruiters can view users within the same company.");
            }

            // Get the current user's details to retrieve their CompanyId
            var currentUserObject = await _userManager.FindByNameAsync(currentUser);

            if (currentUserObject == null)
            {
                throw new Exception("Current user not found.");
            }

            if (currentUserObject.CompanyId == null)
            {
                throw new Exception("Company ID not found for current user.");
            }

            // Fetch all users in the same company
            var usersInSameCompany = await _userManager.Users
                .Where(u => u.CompanyId == currentUserObject.CompanyId)
                .ToListAsync();

            var userResponseList = usersInSameCompany.Select(user => new UserResponseDto
            {
                userId = user.Id,
                UserName = user.UserName,
                Email = user.Email,
                FirstName = user.FirstName,
                LastName = user.LastName,
                PhoneNumber = user.PhoneNumber,
                Address = user.Address,
                Dob = user.Dob,
                Sex = user.Sex,
                CompanyId = user.CompanyId
            }).ToList();

            return userResponseList;
        }

        public async Task<IEnumerable<object>> GetAllUserInfoAsync()
        {
            var userInfos = await _userManager.Users.Include(u => u.CompanyIdNavigation).Select(u => new
            {
                UserId = u.Id,
                UserName = u.UserName,
                Email = u.Email,
                FirstName = u.FirstName,
                LastName = u.LastName,
                PhoneNumber = u.PhoneNumber,
                Address = u.Address,
                Dob = u.Dob,
                Sex = u.Sex,
                Company = u.CompanyIdNavigation!.Name
            }).ToListAsync();

            return userInfos;
        }

        private void UpdateUserFields(AppUser user, UpdateRequestDto update)
        {
            // Update the fields that are allowed to be modified
            user.UserName = update.Username;
            user.Email = update.Email;
            user.PhoneNumber = update.PhoneNumber;
            user.FirstName = update.Firstname;
            user.LastName = update.Lastname;
            user.Address = update.Address;
            user.Dob = update.Dob;
            user.Sex = update.Sex;
        }

        public async Task<BaseResponseDto> UploadDocumentAsync(IFormFile file)
        {
            var userName = _httpContextAccessor.HttpContext!.User.Identity!.Name;
            var user = await _userManager.FindByNameAsync(userName!);

            if (!file.ContentType.Equals("application/pdf", StringComparison.CurrentCultureIgnoreCase) && !file.ContentType.Equals("application/vnd.openxmlformats-officedocument.wordprocessingml.document", StringComparison.CurrentCultureIgnoreCase))
            {
                return new BaseResponseDto
                {
                    Status = "Error",
                    Message = "File type must be pdf or docx"
                };
            }

            try
            {
                // store the document in storage
                var uploadPath = Path.Combine(Directory.GetCurrentDirectory(), "uploads");
                if (!Directory.Exists(uploadPath))
                {
                    Directory.CreateDirectory(uploadPath);
                }
                // Create a user-specific folder inside "uploads" based on the UserId
                var userUploadPath = Path.Combine(uploadPath, user.Id.ToString()); // Use UserId as folder name
                if (!Directory.Exists(userUploadPath))
                {
                    Directory.CreateDirectory(userUploadPath); // Create directory for the user if it doesn't exist
                }

                var fileExtension = Path.GetExtension(file.FileName); // Get file extension
                var originalFileName = Path.GetFileNameWithoutExtension(file.FileName); // Get original file name without extension
                var fileName = $"{Guid.NewGuid()}_{originalFileName}{fileExtension}";
                var filePath = Path.Combine(userUploadPath, fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

                // create new SupportingDocument object
                var newDocument = new SupportingDocument
                {
                    UserId = user!.Id,
                    DocumentName = fileName,
                    FilePath = filePath,
                    UploadedDate = DateTime.UtcNow,
                };

                // save newDocument
                await _documentSupportRepository.CreateAsync(newDocument);

                return new BaseResponseDto
                {
                    Status = "Success",
                    Message = "Document uploaded successfuly"
                };

            }
            catch (System.Exception)
            {
                return new BaseResponseDto
                {
                    Status = "Error",
                    Message = "Document upload error"
                };
            }
        }

        public async Task<BaseResponseDto> DeleteDocumentAsync(int id)
        {
            var userName = _httpContextAccessor.HttpContext!.User.Identity!.Name;
            var user = await _userManager.FindByNameAsync(userName!);

            var document = await _documentSupportRepository.GetByIdAsync(id);

            if (document == null)
            {
                return new BaseResponseDto
                {
                    Status = "Error",
                    Message = "Document not found"
                };
            }

            if (document.UserId != user!.Id)
            {
                return new BaseResponseDto
                {
                    Status = "Error",
                    Message = "Unauthorized to delete document"
                };
            }

            try
            {
                // delete document in storage
                // check if documetn exist
                if (!File.Exists(document.FilePath))
                {
                    return new BaseResponseDto
                    {
                        Status = "Error",
                        Message = "Document does not exist in storage"
                    };
                }

                File.Delete(document.FilePath);

                // delete document in database
                await _documentSupportRepository.DeleteAsync(document);

                return new BaseResponseDto
                {
                    Status = "Success",
                    Message = "Document deleted successfuly"
                };
            }
            catch (System.Exception)
            {
                return new BaseResponseDto
                {
                    Status = "Error",
                    Message = "Document delete error"
                };

                throw;
            }
        }

        public async Task<IEnumerable<DemographicOverviewDto>> GetDemographicOverviewAsync(string address)
        {
            var userDemographic = await _userManager.Users
                .Where(u => u.Address.Contains(address) && u.CompanyId == null) // Check if address contains the string and has no CompanyId
                .Select(u => new DemographicOverviewDto
                {
                    UserId = u.Id,
                    FullName = u.FirstName + " " + u.LastName,
                    Dob = u.Dob,
                    Address = u.Address,
                    Email = u.Email,
                    PhoneNumber = u.PhoneNumber
                })
                .ToListAsync();

            return userDemographic;
        }
    }
}
