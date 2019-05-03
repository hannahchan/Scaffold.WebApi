namespace Scaffold.Data.Repositories
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Threading.Tasks;
    using Microsoft.EntityFrameworkCore;
    using Scaffold.Application.Interfaces;
    using Scaffold.Application.Models;
    using Scaffold.Data;
    using Scaffold.Domain.Entities;

    public class BucketRepository : IBucketRepository
    {
        private readonly BucketContext context;

        public BucketRepository(BucketContext context) =>
            this.context = context;

        public void Add(Bucket bucket)
        {
            if (bucket == null)
            {
                throw new ArgumentNullException(nameof(bucket));
            }

            this.context.Set<Bucket>().Add(bucket);
            this.context.SaveChanges();
        }

        public async Task AddAsync(Bucket bucket)
        {
            if (bucket == null)
            {
                throw new ArgumentNullException(nameof(bucket));
            }

            this.context.Set<Bucket>().Add(bucket);
            await this.context.SaveChangesAsync();
        }

        public Bucket Get(int id) =>
            this.context.Set<Bucket>()
                .Where(bucket => bucket.Id == id)
                .Include(bucket => bucket.Items)
                .SingleOrDefault();

        public IList<Bucket> Get(Expression<Func<Bucket, bool>> predicate, int? limit = null, int? offset = null, Ordering<Bucket> ordering = null) =>
            this.BuildQuery(predicate, limit, offset, ordering)
                .Include(bucket => bucket.Items)
                .ToList();

        public async Task<Bucket> GetAsync(int id) =>
            await this.context.Set<Bucket>()
                .Where(bucket => bucket.Id == id)
                .Include(bucket => bucket.Items)
                .SingleOrDefaultAsync();

        public async Task<IList<Bucket>> GetAsync(Expression<Func<Bucket, bool>> predicate, int? limit = null, int? offset = null, Ordering<Bucket> ordering = null) =>
            await this.BuildQuery(predicate, limit, offset, ordering)
                .Include(bucket => bucket.Items)
                .ToListAsync();

        public void Remove(Bucket bucket)
        {
            if (bucket == null)
            {
                throw new ArgumentNullException(nameof(bucket));
            }

            this.context.Set<Bucket>().Remove(bucket);
            this.context.SaveChanges();
        }

        public async Task RemoveAsync(Bucket bucket)
        {
            if (bucket == null)
            {
                throw new ArgumentNullException(nameof(bucket));
            }

            this.context.Set<Bucket>().Remove(bucket);
            await this.context.SaveChangesAsync();
        }

        public void Update(Bucket bucket)
        {
            if (bucket == null)
            {
                throw new ArgumentNullException(nameof(bucket));
            }

            this.context.Set<Bucket>().Update(bucket);
            this.context.SaveChanges();
        }

        public async Task UpdateAsync(Bucket bucket)
        {
            if (bucket == null)
            {
                throw new ArgumentNullException(nameof(bucket));
            }

            this.context.Set<Bucket>().Update(bucket);
            await this.context.SaveChangesAsync();
        }

        private IQueryable<Bucket> BuildQuery(Expression<Func<Bucket, bool>> predicate, int? limit, int? offset, Ordering<Bucket> ordering = null)
        {
            if (predicate == null)
            {
                throw new ArgumentNullException(nameof(predicate));
            }

            IQueryable<Bucket> query = this.context.Set<Bucket>().Where(predicate);

            if (ordering != null && ordering.Count > 0)
            {
                IOrderedQueryable<Bucket> orderedQuery = null;

                foreach (OrderBy orderBy in ordering)
                {
                    if (orderedQuery == null)
                    {
                        if (orderBy.Ascending)
                        {
                            orderedQuery = query.OrderBy(x => x.GetType().GetProperty(orderBy.PropertyName).GetValue(x));
                            continue;
                        }

                        orderedQuery = query.OrderByDescending(x => x.GetType().GetProperty(orderBy.PropertyName).GetValue(x));
                        continue;
                    }

                    if (orderBy.Ascending)
                    {
                        orderedQuery = orderedQuery.ThenBy(x => x.GetType().GetProperty(orderBy.PropertyName).GetValue(x));
                        continue;
                    }

                    orderedQuery = orderedQuery.ThenByDescending(x => x.GetType().GetProperty(orderBy.PropertyName).GetValue(x));
                }

                query = orderedQuery;
            }

            if (offset != null)
            {
                query = query.Skip(offset.GetValueOrDefault());
            }

            if (limit != null)
            {
                query = query.Take(limit.GetValueOrDefault());
            }

            return query;
        }
    }
}
