using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AsriATS.Domain.Entities
{
    public class RoleChangeRequest
    {
        [Key]
        public int RoleChangeRequestId { get; set; }

        // reference to AppUser
        public string UserId { get; set; } = null!;
        [ForeignKey("UserId")]
        public virtual AppUser UserIdNavigation { get; set; } = null!;

        public bool? IsApproved { get; set; }
    }
}