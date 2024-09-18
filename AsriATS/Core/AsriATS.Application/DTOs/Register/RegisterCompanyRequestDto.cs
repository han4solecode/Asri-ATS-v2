namespace AsriATS.Application.DTOs.Register
{
    public class RegisterCompanyRequestDto
    {
        public string Email { get; set; } = null!;

        public string Name { get; set; } = null!;
        
        public string Address { get; set; } = null!;
    }
}