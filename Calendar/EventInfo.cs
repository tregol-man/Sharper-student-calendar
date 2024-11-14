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
        public DateTime DueDate { get; set; }
        public int SubjectId { get; set; }
        public int GroupId { get; set; }
    }
}
