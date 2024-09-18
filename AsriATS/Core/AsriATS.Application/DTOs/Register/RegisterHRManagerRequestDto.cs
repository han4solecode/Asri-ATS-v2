namespace AsriATS.Application.DTOs.Register
{
    public class RegisterHRManagerRequestDto
    {
        public string Email { get; set; } = null!;

        public string Username { get; set; } = null!;

        public string Firstname { get; set; } = null!;

        public string Lastname { get; set; } = null!;
        
        public string Address { get; set; } = null!;

        public DateOnly Dob { get; set; }

        public string Sex { get; set; } = null!;

        public string PhoneNumber { get; set; } = null!;

        public int CompanyId { get; set; }
    }
}