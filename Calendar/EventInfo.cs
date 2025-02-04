using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Calendar;

namespace Calendar
{
    public class EventInfo
    {
        public int Id { get; set; } // Auto-incrementing ID property
        public string Name { get; set; }
        public string Description { get; set; }                      
        public int Date { get; set; }
        public int SubjectId { get; set; }
        public int GroupId { get; set; }

        public DateTime DueDateDatetime()
        {
            DateTime unixEpoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            DateTime date = unixEpoch.AddDays(Date);
            Console.WriteLine($"{Date} => {date.ToString()}");
            return date;
        }
    }
}
