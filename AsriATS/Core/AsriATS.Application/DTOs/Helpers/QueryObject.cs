using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AsriATS.Application.DTOs.Helpers
{
    public class QueryObject
    {
        public string? JobTitle { get; set; }
        public string? Location { get; set; }
        public string? EmploymentType { get; set; }
        public decimal MinSalary { get; set; }
        public decimal MaxSalary { get; set; }
        public string? QueryOperators { get; set; }
    }
}
