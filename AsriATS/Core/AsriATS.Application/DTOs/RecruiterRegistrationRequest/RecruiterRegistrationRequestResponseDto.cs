﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AsriATS.Application.DTOs.RecruiterRegistrationRequest
{
    public class RecruiterRegistrationRequestResponseDto : BaseResponseDto
    {
        public string? Email { get; set; }
        public string? Name { get; set; }
        public string? Address { get; set; }
        public int? CompanyId { get; set; }
    }
}
