using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AsriATS.Application.DTOs.Login
{
    public class LoginRequestDto
    {
        public string Username { get; set; } = null!;

        public string Password { get; set; } = null!;
    }
}
