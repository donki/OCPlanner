using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OCPlanner
{
    public class GanttLink
    {
        public string id { get; set; }
        public string source { get; set; }
        public string target { get; set; }
        public string type { get; set; }
    }
}
