using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AsriATS.Application.DTOs.NextStepRule
{
    public class NextStepRuleRequestDto
    {
        public int CurrentStepId { get; set; }
        public int NextStepId { get; set; }
        [Required]
        public string ConditionType { get; set; } = null!;
        [Required]
        public string ConditionValue { get; set; } = null!;
    }
}
