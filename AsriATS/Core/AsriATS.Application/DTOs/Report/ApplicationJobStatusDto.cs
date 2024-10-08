﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AsriATS.Application.DTOs.Report
{
    public class ApplicationJobStatusDto
    {
        public int ApplicationJobId { get; set; }
        public int JobPostId { get; set; }
        public bool InterviewScheduled {  get; set; }
        public DateTime? InterviewDate { get; set; }
        public DateTime UploadedDate { get; set; }
        public string Status { get; set; }
    }
}
