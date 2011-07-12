using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GroupGiving.Core.Domain;
using Raven.Client;

namespace GroupGiving.Core.Data
{
    public class RavenDBRepositoryBase<T> : IRepository<T> where T : IDomainObject
    {
        protected readonly IDocumentSession _session;

        public RavenDBRepositoryBase(IDocumentSession session)
        {
            _session = session;
        }

        public IEnumerable<T> RetrieveAll()
        {
            return _session.Query<T>();
        }

        public T Retrieve(Func<T, bool> predicate)
        {
            return _session.Query<T>().SingleOrDefault(predicate);
        }

        public void SaveOrUpdate(T entity)
        {
            _session.Store(entity);
        }

        public void Delete(Func<T, bool> predicate)
        {
            _session.Delete(Retrieve(predicate));
        }

        public void Delete(T entity)
        {
            _session.Delete(entity);
        }

        public void CommitUpdates()
        {
            _session.SaveChanges();
        }

        public IEnumerable<T> Query(Func<T, bool> predicate)
        {
            return _session.Query<T>().Where(predicate);
        }
    }
}
