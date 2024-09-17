using System.ComponentModel.DataAnnotations;

namespace AsriATS.Domain.Entities
{
    public class Company
    {
        public int CompanyId { get; set; }

        [Required]
        public string Name { get; set; } = null!;

        [Required]
        public string Address { get; set; } = null!;

        // collection navigation to AppUser
        public ICollection<AppUser> AppUsers { get; set; } = [];
    }
}