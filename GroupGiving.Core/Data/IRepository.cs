using System;
using System.Collections.Generic;
using System.Linq;
using GroupGiving.Core.Domain;

namespace GroupGiving.Core.Data
{
    public interface IRepository<T> where T : IDomainObject
    {
        IEnumerable<T> RetrieveAll();
        T Retrieve(Func<T, bool> predicate);
        void SaveOrUpdate(T entity);
        void Delete(Func<T, bool> predicate);
        void Delete(T entity);
        void CommitUpdates();
        IEnumerable<T> Query(Func<T, bool> predicate);
    }
}