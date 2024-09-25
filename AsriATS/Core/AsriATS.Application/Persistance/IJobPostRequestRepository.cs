﻿using AsriATS.Application.Persistance.Common;
using AsriATS.Domain.Entities;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace AsriATS.Application.Persistance
{
    public interface IJobPostRequestRepository : IBaseRepository<JobPostRequest>
    {
        Task<IEnumerable<JobPostRequest>> GetAllToBeReviewedAsync(int companyId, string userRoleId);
        Task<JobPostRequest?> GetFirstOrDefaultAsync(Expression<Func<JobPostRequest, bool>> expression);
    }
}
