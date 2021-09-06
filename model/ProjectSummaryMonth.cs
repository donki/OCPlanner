using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OCPlanner
{
    public class ProjectSummaryMonth
    {
        public string MonthName;
        public int TotalWorkHours;
        public decimal TotalHoursPlanned;
        public decimal TotalRemainHours;
        public int TotalTasks;
        public int TotalProgress;
        public int PerceProgress;
        public int PercePlanned;
        public DateTime BeginOfMonth;
        public DateTime EndOfMonth;
        public Boolean isMonthPeriod = false;
        public decimal TotalNonPlannedHours = 0;
        public int PerceNonPlannedHours = 0;
    }
}
