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
        public int event_id { get; set; } // Auto-incrementing ID property
        public string event_name { get; set; }
        public string event_description { get; set; }
        public DateTime event_date { get; set; }
        public List<string> Files { get; set; }
        public int subject_id { get; set; }
        public int creator_id { get; set; }
    }
}
