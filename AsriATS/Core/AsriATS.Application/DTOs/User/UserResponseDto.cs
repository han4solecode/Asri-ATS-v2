using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AsriATS.Application.DTOs.User
{
    public class UserResponseDto
    {
        public string userId {  get; set; }
        public string UserName { get; set; }
        public string Email {  get; set; }
        public string PhoneNumber { get; set; }
        public string FirstName {  get; set; }
        public string LastName {  get; set; }
        public string Address {  get; set; }
        public string Sex {  get; set; }
        public DateOnly Dob { get; set; }
        public List<string> Roles { get; set; }
        public string? Company {  get; set; }
    }
}
