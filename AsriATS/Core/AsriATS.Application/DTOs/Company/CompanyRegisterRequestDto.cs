namespace AsriATS.Application.DTOs.Company
{
    public class CompanyRegisterRequestDto
    {
        public string Email { get; set; } = null!;

        public string CompanyName { get; set; } = null!;

        public string CompanyAddress { get; set; } = null!;

        public string FirstName { get; set; } = null!;

        public string LastName { get; set; } = null!;

        public string UserAddress { get; set; } = null!;

        public DateOnly Dob { get; set; }

        public string Sex { get; set; } = null!;

        public string PhoneNumber { get; set; } = null!;
    }
}