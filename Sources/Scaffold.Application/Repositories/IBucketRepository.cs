namespace Scaffold.Application.Repositories
{
    using System;
    using System.Threading.Tasks;
    using Scaffold.Domain.Entities;

    public interface IBucketRepository
    {
        void Add(Bucket bucket);

        Task AddAsync(Bucket bucket);

        Bucket Get(int id);

        Task<Bucket> GetAsync(int id);

        void Remove(Bucket bucket);

        Task RemoveAsync(Bucket bucket);

        void Update(Bucket bucket);

        Task UpdateAsync(Bucket bucket);
    }
}
