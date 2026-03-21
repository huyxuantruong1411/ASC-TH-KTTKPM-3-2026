using ASC.Models.BaseTypes;
using System;
using System.Threading.Tasks;

namespace ASC.DataAccess.Interfaces
{
    public interface IUnitOfWork : IDisposable
    {
        // Factory pattern để lấy Repository tương ứng với Entity
        IRepository<T> Repository<T>() where T : BaseEntity;

        public int CommitTransaction();
    }
}