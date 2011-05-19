using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GroupGiving.Core.Domain;
using Microsoft.WindowsAzure;

namespace GroupGiving.Core.Data.Azure
{
    public class AzureEventRepository : IRepository<GroupGivingEvent>
    {
        private readonly AzureRepository<EventRow> _azureRepository;

        public AzureEventRepository(AzureRepository<EventRow> azureRepository)
        {
            _azureRepository = azureRepository;
        }

        public IEnumerable<GroupGivingEvent> RetrieveAll()
        {
            return (from i in _azureRepository.RetrieveAll() select AutoMapper.Mapper.Map<EventRow, GroupGivingEvent>(i));
        }

        public GroupGivingEvent Retrieve(object id)
        {
            return AutoMapper.Mapper.Map<EventRow, GroupGivingEvent>(_azureRepository.Retrieve(id));
        }

        public void SaveOrUpdate(GroupGivingEvent entity)
        {
            _azureRepository.SaveOrUpdate(AutoMapper.Mapper.Map<GroupGivingEvent,EventRow>(entity));
        }

        public void Delete(object id)
        {
            _azureRepository.Delete(id);
        }
    }
}
