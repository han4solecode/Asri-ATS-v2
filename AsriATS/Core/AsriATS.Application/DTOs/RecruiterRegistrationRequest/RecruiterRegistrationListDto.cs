using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AsriATS.Application.DTOs.RecruiterRegistrationRequest
{
    public class RecruiterRegistrationListDto
    {
        public List<AllRecruiterRegistrationRequestDto> ToBeReviewed { get; set; } = new();
        public List<AllRecruiterRegistrationRequestDto> AlreadyReviewed { get; set; } = new();
    }
}
