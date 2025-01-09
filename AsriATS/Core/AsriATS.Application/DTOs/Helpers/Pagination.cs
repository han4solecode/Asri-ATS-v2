using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AsriATS.Application.DTOs.Helpers
{
    public class Pagination
    {
        public int? PageNumber { get; set; } = null;
        public int? PageSize { get; set; } = 20;
    }
}
