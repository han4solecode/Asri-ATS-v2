using AsriATS.Application.Persistance.Common;
using AsriATS.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AsriATS.Application.Persistance
{
    public interface IApplicationJobRepository:IBaseRepository<ApplicationJob>
    {
        Task<IEnumerable<ApplicationJob>> GetAllToStatusAsync(int companyId, string userRole);
        Task<IEnumerable<ApplicationJob>> GetAllByApplicantAsync(string applicantId);
    }
}
