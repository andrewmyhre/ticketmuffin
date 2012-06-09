using System;
using System.Collections.Generic;
using System.Linq;

namespace TicketMuffin.Service
{
    public static class MsmqAsyncListenerHelper
    {
        private static HashSet<DailyProcessedMessageCount> _dailyProcessedMessageCountList = new HashSet<DailyProcessedMessageCount>();
        private static object _syncLock = new object();

        public static void AddOneToProcessedMessageCount()
        {
            lock (_syncLock)
            {
                var dayCount = _dailyProcessedMessageCountList.Select(dc => dc).Where(d => d.Day == DateTime.Now.DayOfWeek);

                var hourCountList = dayCount.Select(hl => hl.ProcessedMessageCountList).FirstOrDefault();

                hourCountList.Single(h => h.HourOfDay == DateTime.Now.Hour).MessageCount++;
            }
        }

        public static int GetMessageCount(DayOfWeek day, int hour)
        {
            lock (_syncLock)
            {
                var dayCount = _dailyProcessedMessageCountList.Select(dc => dc).Where(d => d.Day == day);

                var hourCountList = dayCount.Select(dl => dl.ProcessedMessageCountList).FirstOrDefault();

                return hourCountList.Single(h => h.HourOfDay == hour).MessageCount;
            }
        }

        public static void SetupMessageProcessedCountList()
        {
            HashSet<ProcessedMessageCount> processedMessageCountList = new HashSet<ProcessedMessageCount>();
            processedMessageCountList.Add(new ProcessedMessageCount { HourLabel = "12 - 1 am", HourOfDay = 0 });
            processedMessageCountList.Add(new ProcessedMessageCount { HourLabel = "1 - 2 am", HourOfDay = 1 });
            processedMessageCountList.Add(new ProcessedMessageCount { HourLabel = "2 - 3 am", HourOfDay = 2 });
            processedMessageCountList.Add(new ProcessedMessageCount { HourLabel = "3 - 4 am", HourOfDay = 3 });
            processedMessageCountList.Add(new ProcessedMessageCount { HourLabel = "4 - 5 am", HourOfDay = 4 });
            processedMessageCountList.Add(new ProcessedMessageCount { HourLabel = "5 - 6 am", HourOfDay = 5 });
            processedMessageCountList.Add(new ProcessedMessageCount { HourLabel = "6 - 7 am", HourOfDay = 6 });
            processedMessageCountList.Add(new ProcessedMessageCount { HourLabel = "7 - 8 am", HourOfDay = 7 });
            processedMessageCountList.Add(new ProcessedMessageCount { HourLabel = "8 - 9 am", HourOfDay = 8 });
            processedMessageCountList.Add(new ProcessedMessageCount { HourLabel = "9 - 10 am", HourOfDay = 9 });
            processedMessageCountList.Add(new ProcessedMessageCount { HourLabel = "10 - 11 am", HourOfDay = 10 });
            processedMessageCountList.Add(new ProcessedMessageCount { HourLabel = "11 - 12 am", HourOfDay = 11 });
            processedMessageCountList.Add(new ProcessedMessageCount { HourLabel = "12 - 1 pm", HourOfDay = 12 });
            processedMessageCountList.Add(new ProcessedMessageCount { HourLabel = "1 - 2 pm", HourOfDay = 13 });
            processedMessageCountList.Add(new ProcessedMessageCount { HourLabel = "2 - 3 pm", HourOfDay = 14 });
            processedMessageCountList.Add(new ProcessedMessageCount { HourLabel = "3 - 4 pm", HourOfDay = 15 });
            processedMessageCountList.Add(new ProcessedMessageCount { HourLabel = "4 - 5 pm", HourOfDay = 16 });
            processedMessageCountList.Add(new ProcessedMessageCount { HourLabel = "5 - 6 pm", HourOfDay = 17 });
            processedMessageCountList.Add(new ProcessedMessageCount { HourLabel = "6 - 7 pm", HourOfDay = 18 });
            processedMessageCountList.Add(new ProcessedMessageCount { HourLabel = "7 - 8 pm", HourOfDay = 19 });
            processedMessageCountList.Add(new ProcessedMessageCount { HourLabel = "8 - 9 pm", HourOfDay = 20 });
            processedMessageCountList.Add(new ProcessedMessageCount { HourLabel = "9 - 10 pm", HourOfDay = 21 });
            processedMessageCountList.Add(new ProcessedMessageCount { HourLabel = "10 - 11 pm", HourOfDay = 22 });
            processedMessageCountList.Add(new ProcessedMessageCount { HourLabel = "11 - 12 pm", HourOfDay = 23 });


            _dailyProcessedMessageCountList.Add(new DailyProcessedMessageCount { Day = DayOfWeek.Monday, ProcessedMessageCountList = processedMessageCountList });
            _dailyProcessedMessageCountList.Add(new DailyProcessedMessageCount { Day = DayOfWeek.Tuesday, ProcessedMessageCountList = processedMessageCountList });
            _dailyProcessedMessageCountList.Add(new DailyProcessedMessageCount { Day = DayOfWeek.Wednesday, ProcessedMessageCountList = processedMessageCountList });
            _dailyProcessedMessageCountList.Add(new DailyProcessedMessageCount { Day = DayOfWeek.Thursday, ProcessedMessageCountList = processedMessageCountList });
            _dailyProcessedMessageCountList.Add(new DailyProcessedMessageCount { Day = DayOfWeek.Friday, ProcessedMessageCountList = processedMessageCountList });
            _dailyProcessedMessageCountList.Add(new DailyProcessedMessageCount { Day = DayOfWeek.Saturday, ProcessedMessageCountList = processedMessageCountList });
            _dailyProcessedMessageCountList.Add(new DailyProcessedMessageCount { Day = DayOfWeek.Sunday, ProcessedMessageCountList = processedMessageCountList });
        }
    }
}