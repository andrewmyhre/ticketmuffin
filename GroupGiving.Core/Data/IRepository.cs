using System.Collections.Generic;

namespace GroupGiving.Core.Data
{
    public interface IRepository<T>
    {
        IEnumerable<T> RetrieveAll();
        T Retrieve(object id);
        void SaveOrUpdate(T entity);
        void Delete(object id);
    }
}