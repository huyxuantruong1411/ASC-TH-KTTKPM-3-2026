using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ASC.DataAccess.Interfaces;
using ASC.Model.BaseTypes;
using ASC.Models.BaseTypes;
using Microsoft.EntityFrameworkCore;

namespace ASC.DataAccess
{
    public class Repository<T> : IRepository<T> where T : BaseEntity, new()
    {
        private DbContext dbContext;

        public Repository(DbContext _dbContext)
        {
            this.dbContext = _dbContext;
        }

        public async Task<T> AddAsync(T entity)
        {
            var entityToInsert = entity as BaseEntity;
            if (entityToInsert != null)
            {
                entityToInsert.CreatedDate = DateTime.UtcNow;
                entityToInsert.UpdatedDate = DateTime.UtcNow;
            }

            await dbContext.Set<T>().AddAsync(entity);
            return entity;
        }

        public void Update(T entity)
        {
            var entityToUpdate = entity as BaseEntity;
            if (entityToUpdate != null)
            {
                entityToUpdate.UpdatedDate = DateTime.UtcNow;
            }

            dbContext.Set<T>().Update(entity);
        }

        public void Delete(T entity)
        {
            var entityToDelete = entity as BaseEntity;
            if (entityToDelete != null)
            {
                entityToDelete.UpdatedDate = DateTime.UtcNow;
                entityToDelete.IsDeleted = true;
            }

            dbContext.Set<T>().Remove(entity);
        }

        public async Task<T> FindAsync(string partitionKey, string rowKey)
        {
            var result = await dbContext.Set<T>().FindAsync(partitionKey, rowKey);
            return result!;
        }

        public async Task<IEnumerable<T>> FindAllByPartitionKeyAsync(string partitionkey)
        {
            var result = await dbContext.Set<T>()
                                  .Where(t => t.PartitionKey == partitionkey)
                                  .ToListAsync();
            return result;
        }

        public async Task<IEnumerable<T>> FindAllAsync()
        {
            var result = await dbContext.Set<T>().ToListAsync();
            return result;
        }
    }
}