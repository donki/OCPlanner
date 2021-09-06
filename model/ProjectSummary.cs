using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OCPlanner
{
    public class ProjectSummary
    {
        public decimal DailyWorkHours;
        public decimal WeeklyWorkHours;
        public decimal MonthlyWorkHours;
        public int PerceGlobalProgress;
        public int TotalProgress;
        public int TotalHoursPlanned = 0;
        public int TotalTasks = 0;
        public int TotalHoursThisMonth = 0;
        public int TotalTasksThisMonth = 0;
        public int TotalProgressThisMonth = 0;
        public int PerceProgressThisMonth = 0;
        public int TotalHoursNextMonth = 0;
        public int TotalTasksNextMonth = 0;
        public int TotalProgressNextMonth = 0;
        public int PerceProgressNextMonth = 0;
        public int PercePlannedHoursThisMonth = 0;
        public int PercePlannedHoursNextMonth = 0;
        public int PercePlannedRange = 0;
        public List<ProjectSummaryMonth> ProjectSummaryMonths = new List<ProjectSummaryMonth>();


    }
}
