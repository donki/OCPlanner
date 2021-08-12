using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OCPlanner
{
    public class GanttProject
    {
        public string ProjectName { get; set; }
        public int DailyWorkHours { get; set; }
        
        public GanttProject()
        {
            ProjectName = "SIN TITULO";
            DailyWorkHours = 4;
        }
    }
}
