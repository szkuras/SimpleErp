using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;

namespace SimpleErp.Repository;

public class Repository<TEntity> : IRepository<TEntity> where TEntity : class
{
    protected readonly AppDbContext dbContext;

    public Repository(AppDbContext dbContext)
    {
        this.dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
    }

    public async Task<IEnumerable<TEntity>> GetAllAsync()
    {
        return await dbContext.Set<TEntity>().ToListAsync();
    }

    public async Task<TEntity> GetByIdAsync(Guid id)
    {
        return await dbContext.Set<TEntity>().FindAsync(id);
    }

    public  IQueryable<TEntity> Get(Expression<Func<TEntity, bool>> filter)
    {
        return dbContext.Set<TEntity>().Where(filter);
    }

    public async Task<TEntity> AddAsync(TEntity entity)
    {
        var addedEntity = await dbContext.Set<TEntity>().AddAsync(entity);
        await dbContext.SaveChangesAsync();
        return addedEntity.Entity;
    }

    public async Task<TEntity> UpdateAsync(TEntity entity)
    {
        var updatedEntity = dbContext.Set<TEntity>().Update(entity);
        await dbContext.SaveChangesAsync();
        return updatedEntity.Entity;
    }

    public async Task DeleteAsync(Guid id)
    {
        var entityToDelete = await GetByIdAsync(id);
        if (entityToDelete != null)
        {
            dbContext.Set<TEntity>().Remove(entityToDelete);
            await dbContext.SaveChangesAsync();
        }
    }
}

