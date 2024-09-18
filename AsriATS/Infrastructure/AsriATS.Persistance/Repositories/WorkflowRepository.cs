﻿using AsriATS.Application.Persistance;
using AsriATS.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AsriATS.Persistance.Repositories
{
    public class WorkflowRepository : IWorkflowRepository
    {
        private readonly AppDbContext _context;

        public WorkflowRepository(AppDbContext appDbContext)
        {
            _context = appDbContext;
        }

        public async Task CreateAsync(Workflow entity)
        {
            await _context.Workflows.AddAsync(entity);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(Workflow entity)
        {
            _context.Workflows.Remove(entity);
            await _context.SaveChangesAsync();
        }

        public async Task<IEnumerable<Workflow>> GetAllAsync()
        {
            var workflows = await _context.Workflows.ToListAsync();

            return workflows;
        }

        public async Task<Workflow?> GetByIdAsync(int id)
        {
            var workflow = await _context.Workflows.FindAsync(id);

            return workflow;
        }

        public async Task UpdateAsync(Workflow entity)
        {
            _context.Workflows.Update(entity);
            await _context.SaveChangesAsync();
        }
    }
}