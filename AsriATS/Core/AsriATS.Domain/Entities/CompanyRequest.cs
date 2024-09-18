using System.ComponentModel.DataAnnotations;

namespace AsriATS.Domain.Entities
{
    public class CompanyRequest
    {
        [Required]
        public string Email { get; set; } = null!;

        [Required]
        public string Name { get; set; } = null!;

        [Required]
        public string Address { get; set; } = null!;
    }
}