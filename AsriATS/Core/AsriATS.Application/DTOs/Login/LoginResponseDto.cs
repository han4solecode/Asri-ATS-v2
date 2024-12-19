using AsriATS.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AsriATS.Application.DTOs.Login
{
    public class LoginResponseDto : BaseResponseDto
    {

        public string? AccessToken { get; set; }

        public DateTime? AccessTokenExpiryTime { get; set; }

        public string? RefreshToken { get; set; }

        public DateTime? RefreshTokenExpiryTime { get; set; }
        public List<string> Roles { get; set; }
        public AppUser? User { get; set; }
    }
}
