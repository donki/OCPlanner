using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OCPlanner
{
    public class OCProject
    {
        public string ProjectName { get; set; }
        public int DailyWorkHours { get; set; }
        public List<string> HolyDays { get; set; }
        public string StartDate { get; set; }
        public string EndDate { get; set; }

        public OCProject()
        {
            ProjectName = "SIN TITULO";
            DailyWorkHours = 4;
            HolyDays = new List<string>();
        }
    }
}
