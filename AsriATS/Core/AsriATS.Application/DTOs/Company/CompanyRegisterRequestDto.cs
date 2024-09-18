namespace AsriATS.Application.DTOs.Company
{
    public class CompanyRegisterRequestDto
    {
        public string Email { get; set; } = null!;

        public string Name { get; set; } = null!;

        public string Address { get; set; } = null!;
    }
}