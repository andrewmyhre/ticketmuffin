using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GroupGiving.Core.Domain;

namespace GroupGiving.Core.Data.Fakes
{
    public class FakeRepository<T> : IRepository<T> where T : IDomainObject
    {
        private static List<T> _list = new List<T>();
        public IEnumerable<T> RetrieveAll()
        {
            return _list.AsQueryable();
        }

        public T Retrieve(Func<T, bool> predicate)
        {
            return _list.SingleOrDefault(predicate);
        }

        public void SaveOrUpdate(T entity)
        {
            int index = _list.IndexOf(Retrieve(e=>e.Id==entity.Id));
            if (index > -1)
            {
                _list[index] = entity;
            }
            else
            {
                _list.Add(entity);
                entity.Id = _list.Count.ToString();
            }
        }

        public void Delete(Func<T, bool> predicate)
        {
            _list.Remove(Retrieve(predicate));
        }

        public void Delete(T entity)
        {
            _list.Remove(entity);
        }

        public void CommitUpdates()
        {
            throw new NotImplementedException();
        }

        IEnumerable<T> IRepository<T>.Query(Func<T, bool> predicate)
        {
            throw new NotImplementedException();
        }
    }
}
