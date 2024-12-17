using AsriATS.Application.DTOs.Helpers;
using AsriATS.Application.DTOs.JobPost;
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
        Task<IEnumerable<JobPost>> SearchJobPostAsync(QueryObject queryObject, Pagination pagination);
        Task<object> SearchJobPostsAsync(JobPostSearch searchParams);
        Task<JobPostDetail> JobPostDetails(int jobPostId);
    }
}
