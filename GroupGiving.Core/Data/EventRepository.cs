using System;
using System.Collections.Generic;
using System.Data.Services.Client;
using System.Globalization;
using System.Linq;
using System.Text;
using GroupGiving.Core.Data.Azure;
using GroupGiving.Core.Domain;
using Microsoft.WindowsAzure;
using Microsoft.WindowsAzure.StorageClient;

namespace GroupGiving.Core.Data
{
    public class EventRepository
    {
        private readonly AzureRepository<EventRow> _context;
        private readonly CloudStorageAccount account;

        public EventRepository(AzureRepository<EventRow> context)
        {
            AutoMapper.Mapper.CreateMap<EventRow, GroupGivingEvent>();
            AutoMapper.Mapper.CreateMap<GroupGivingEvent, EventRow>();
            _context = context;
        }
        /*
        public void Save(GroupGivingEvent groupGivingEvent)
        {
            if (groupGivingEvent.Id != Guid.Empty) throw new InvalidOperationException("Cannot call Save on a persisted entity (Id != Guid.Empty)");

            groupGivingEvent.Id = Guid.NewGuid();

            EventRow eventRow = AutoMapper.Mapper.Map<GroupGivingEvent,EventRow>(groupGivingEvent);

            _context.AddObject(_context.Table, eventRow);
            _context.SaveChanges();
        }

        public void Update(GroupGivingEvent groupGivingEvent)
        {
            EventRow eventRow = RetrieveRow(groupGivingEvent.Id);
            eventRow.Name = groupGivingEvent.Name;

            _context.UpdateObject(eventRow);
            _context.SaveChanges();

            
        }

        public GroupGivingEvent Retrieve(Guid id)
        {
            var row = RetrieveRow(id);
            return AutoMapper.Mapper.Map<EventRow, GroupGivingEvent>(row);
        }

        private EventRow RetrieveRow(Guid id)
        {
            var query = (from expense in _context.All
                         where expense.Id == id
                         select expense).AsTableServiceQuery();

            return query.Execute().SingleOrDefault();
        }

        public static string EncodePartitionAndRowKey(string key)
        {
            if (key == null)
            {
                return null;
            }

            return Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(key));
        }

        public static string DecodePartitionAndRowKey(string encodedKey)
        {
            if (encodedKey == null)
            {
                return null;
            }

            return System.Text.Encoding.UTF8.GetString(Convert.FromBase64String(encodedKey));
        }
        */
    }
}
