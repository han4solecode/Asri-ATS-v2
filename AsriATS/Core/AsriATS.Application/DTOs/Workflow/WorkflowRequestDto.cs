﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AsriATS.Application.DTOs.Workflow
{
    public class WorkflowRequestDto
    {
        public string WorkflowName { get; set; } = null!;
        public string Description { get; set; } = null!;
    }
}