using AsriATS.Application.DTOs.Helpers;
using AsriATS.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AsriATS.Application.Contracts
{
    public interface IJobPostService
    {
        Task<IEnumerable<JobPost>> GetAllJobPostAsync(Pagination pagination);
        Task<IEnumerable<JobPost>> SeachJobPostAsync(QueryObject queryObject, Pagination pagination);
    }
}
