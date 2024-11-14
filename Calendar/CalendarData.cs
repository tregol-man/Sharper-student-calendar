using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Calendar;

namespace Calendar
{
    public class CalendarData
    {
        public List<EventInfo> Events { get; set; }
        public List<SubjectData> Subjects { get; set; }
        public List<GroupData> Groups { get; set; }
    }
}
