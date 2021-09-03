using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OCPlanner
{
    public class Project
    {
        public string ProjectName;
        public int DailyPlannedWorkHours = 4;
        public int DailyFullWorkHours = 8;
        public List<OCActivity> Activities = new List<OCActivity>();
        public List<string> HolyDays = new List<string>();
        public string StartDate { get; set; }
        public string EndDate { get; set; }
    }
}
