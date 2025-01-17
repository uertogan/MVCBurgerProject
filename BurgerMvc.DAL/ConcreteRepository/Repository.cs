﻿using BurgerMvc.DAL.AbstractRepository;
using BurgerMvc.DAL.Data;
using BurgerMvc.DAL.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace BurgerMvc.DAL.ConcreteRepository
{
    public class Repository<T> : IRepository<T> where T : BaseEntity
    {
        private readonly AppDbContext _context;
        private readonly DbSet<T> _entities;

        public Repository(AppDbContext context)
        {
            _context = context;
            _entities = _context.Set<T>();
        }

        public async Task AddAsync(T entity)
        {
            await _entities.AddAsync(entity);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var entity = await GetByIdAsync(id);
            if (entity != null)
            {
                entity.IsDeleted = true;
                entity.DeletedDate = DateTime.Now;
                _entities.Update(entity);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<IEnumerable<T>> GetAllAsync()
        {
            return await _entities.Where(x => !x.IsDeleted).ToListAsync();
        }

        public async Task<T> GetByIdAsync(int id)
        {
            var a = await _entities.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id);
            return a;
        }

        public async Task AddRangeAsync(IEnumerable<T> entity)
        {
            await _entities.AddRangeAsync(entity);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(T entity)
        {
            entity.ModifiedDate = DateTime.Now;
            _entities.Update(entity);
            await _context.SaveChangesAsync();
        }
    }
}
