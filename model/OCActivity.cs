using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OCPlanner
{
    public class OCActivity
    {
        public string title { get; set; }
        public string duration { get; set; }
        public string start_date { get; set; }
        public string activitytype { get; set; }
        public string? id { get; set; }
        public string? description { get; set; }
        public string? progress { get; set; }
        public string? end_date { get; set; }
        public string? url { get; set; }
        public List<string>? tags { get; set; }
        public string period { get; set; }
        public List<string> idlinked { get; set; }
        public string parentid { get; set; }
        public string externalid { get; set; }
        public Boolean planned { get; set; }

        public OCActivity()
        {
            tags = new List<string>();
            idlinked = new List<string>();
            planned = true;
            progress = "0";
        }

    }
}
