using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OCPlanner
{
    public class OCProject
    {
        public string ProjectName { get; set; }
        public decimal DailyPlannedWorkHours { get; set; }
        public decimal DailyFullWorkHours { get; set; }
        public int PercePlannedHoursvsFullWorkHours { get; set; }
        public List<string> HolyDays { get; set; }
        public string StartDate { get; set; }
        public string EndDate { get; set; }
        public string WebHook { get; set; }
        public string APIKey { get; set; }

        public OCProject()
        {
            ProjectName = "SIN TITULO";
            DailyPlannedWorkHours = 4;
            DailyFullWorkHours = 8;
            HolyDays = new List<string>();
        }
    }
}
