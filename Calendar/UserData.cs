using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Calendar;

namespace Calendar
{
    public class UserData
    {
        public int user_id { get; set; }
        public string user_name { get; set; }
        public string google_id { get; set; }
        public string email { get; set; }
        public List<GroupData> groups { get; set; }
    }
}
