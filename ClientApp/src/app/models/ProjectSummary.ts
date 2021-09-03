import { ProjectSummaryMonth } from "./ProjecSummaryMonth";

export class ProjectSummary {
    DailyWorkHours = 0;
    PerceGlobalProgress = 0;
    TotalHoursPlanned = 0;
    TotalProgress = 0;    
    TotalTasks = 0;
    TotalTasksThisMonth = 0;
    TotalHoursThisMonth = 0;
    TotalProgressThisMonth = 0; 
    PerceProgressThisMonth = 0;  
    TotalHoursNextMonth = 0;
    TotalTasksNextMonth = 0;
    TotalProgressNextMonth = 0;
    PerceProgressNextMonth = 0;
    WeeklyWorkHours = 0;
    MonthlyWorkHours = 0;
    PercePlannedHoursThisMonth = 0;
    PercePlannedHoursNextMonth = 0;
    PercePlannedRange = 0;
    ProjectSummaryMonths : ProjectSummaryMonth[] = [];
}