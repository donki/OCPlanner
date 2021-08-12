using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OCPlanner
{
    public class Project
    {
        public string ProjectName;
        public int DailyWorkHours = 4;
        public List<GanttTask> Tasks = new List<GanttTask>();
        public List<GanttLink> Links = new List<GanttLink>();
    }
}
