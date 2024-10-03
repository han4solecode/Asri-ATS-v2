﻿using AsriATS.Application.Persistance;
using AsriATS.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace AsriATS.Persistance.Repositories
{
    public class InterviewSchedulingRepository : IInterviewSchedulingRepository
    {
        private readonly AppDbContext _context;

        public InterviewSchedulingRepository(AppDbContext appDbContext)
        {
            _context = appDbContext;
        }

        public async Task CreateAsync(InterviewScheduling entity)
        {
            await _context.InterviewScheduling.AddAsync(entity);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(InterviewScheduling entity)
        {
            _context.InterviewScheduling.Remove(entity);
            await _context.SaveChangesAsync();
        }

        public async Task<IEnumerable<InterviewScheduling>> GetAllAsync()
        {
            var interviewSchedules = await _context.InterviewScheduling.ToListAsync();

            return interviewSchedules;
        }

        public async Task<InterviewScheduling?> GetByIdAsync(int id)
        {
            var interviewSchedule = await _context.InterviewScheduling.FindAsync(id);

            return interviewSchedule;
        }

        public async Task UpdateAsync(InterviewScheduling entity)
        {
            _context.InterviewScheduling.Update(entity);
            await _context.SaveChangesAsync();
        }
    }
}