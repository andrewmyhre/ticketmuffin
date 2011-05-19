using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GroupGiving.Core.Domain;

namespace GroupGiving.Core.Data.Fakes
{
    public class FakeRepository<T> : IRepository<T> where T : DomainBase
    {
        private static List<T> _list = new List<T>();
        public IEnumerable<T> RetrieveAll()
        {
            return _list.AsQueryable();
        }

        public T Retrieve(object id)
        {
            return _list.Where(i => i.Id == id).FirstOrDefault();
        }

        public void SaveOrUpdate(T entity)
        {
            int index = _list.IndexOf(Retrieve(entity.Id));
            if (index > -1)
            {
                _list[index] = entity;
            }
            else
            {
                _list.Add(entity);
                entity.Id = Guid.NewGuid().ToString();
            }
        }

        public void Delete(object id)
        {
            _list.Remove(Retrieve(id));
        }
    }
}
