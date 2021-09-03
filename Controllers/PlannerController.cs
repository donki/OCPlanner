using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace OCPlanner.Controllers
{
    [ApiController]
    [Produces("application/json")]
    [Route("api/[controller]")]
    public class PlannerController : ControllerBase
    {

        private static Project project;
        private static string appPath;
        private static string projectfile;

        public static object lockSaveProject = new object();

        public PlannerController()
        {
            if (project != null) { return; }
            appPath = Path.GetDirectoryName(Assembly.GetAssembly(typeof(Program)).Location) + "\\db";
            projectfile = appPath + "\\project.json";

            if (!Directory.Exists(appPath))
            {
                Directory.CreateDirectory(appPath);
            }

            if (System.IO.File.Exists(projectfile))
            {
                project = JsonConvert.DeserializeObject<Project>(System.IO.File.ReadAllText(projectfile));
            }

            if (project == null)
            {
                project = new Project();
                System.IO.File.Create(projectfile).Close();
            }
        }

        private void SaveProject()
        {

            lock (lockSaveProject)
            {
                var tmp = JsonConvert.SerializeObject(project);
                System.IO.File.WriteAllText(projectfile, tmp);

            }
        }

        [HttpPost]
        [Route("Project")]
        public ObjectResult UpdateGanttProject(OCProject pganttProject)
        {
            lock (lockSaveProject)
            {
                project.DailyWorkHours = pganttProject.DailyWorkHours;
                project.ProjectName = pganttProject.ProjectName;
                project.HolyDays = pganttProject.HolyDays;
                project.StartDate = pganttProject.StartDate;
                project.EndDate = pganttProject.EndDate;
                SaveProject();
            }
            return Ok(JsonConvert.SerializeObject(pganttProject));
        }

        [HttpGet]
        [Route("Project")]
        public ObjectResult GetProject()
        {
            var ganttProject = new OCProject();
            ganttProject.DailyWorkHours = project.DailyWorkHours;
            ganttProject.ProjectName = project.ProjectName;
            ganttProject.HolyDays = project.HolyDays;
            ganttProject.StartDate = project.StartDate;
            ganttProject.EndDate = project.EndDate;
            return Ok(JsonConvert.SerializeObject(ganttProject));
        }

        [HttpGet]
        [Route("ProjectSummary")]
        public ObjectResult GetProjectSymmary()
        {
            var pSummary = new ProjectSummary();

            lock (lockSaveProject)
            {
                pSummary.ProjectSummaryMonths.Clear();

                var MonthPerido = new ProjectSummaryMonth();
                MonthPerido.MonthName = Convert.ToDateTime(project.StartDate).ToString("dd/MM/yyyy") + " - " + Convert.ToDateTime(project.EndDate).ToString("dd/MM/yyyy");
                MonthPerido.BeginOfMonth = Convert.ToDateTime(project.StartDate);
                MonthPerido.EndOfMonth = Convert.ToDateTime(project.EndDate);
                MonthPerido.isMonthPeriod = true;
                pSummary.ProjectSummaryMonths.Add(MonthPerido);
                foreach (OCActivity t in project.Activities)
                {
                    var MonthName = Convert.ToDateTime(t.start_date).ToString("MM/yyyy");
                    var Month = pSummary.ProjectSummaryMonths.Find((x) => x.MonthName.Equals(MonthName));
                    if (Month == null)
                    {
                        Month = new ProjectSummaryMonth();
                        Month.MonthName = MonthName;
                        pSummary.ProjectSummaryMonths.Add(Month);
                        Month.BeginOfMonth = new DateTime(Convert.ToDateTime(t.start_date).Date.Year, Convert.ToDateTime(t.start_date).Date.Month, 1);
                        Month.EndOfMonth = new DateTime(Convert.ToDateTime(t.start_date).Date.Year, Convert.ToDateTime(t.start_date).Date.Month, 1).AddMonths(1).AddDays(-1);
                    }

                    Month.TotalTasks++;
                    Month.TotalProgress += Convert.ToInt32(Convert.ToDecimal(t.progress.Replace('.', ',')) * 100);
                    Month.TotalHoursPlanned += Convert.ToInt32(t.duration);
                    if ((Convert.ToDateTime(project.StartDate).Date <= Convert.ToDateTime(t.start_date).Date) &&
                        (Convert.ToDateTime(project.EndDate).Date >= Convert.ToDateTime(t.start_date).Date))
                    {
                        MonthPerido.TotalTasks++;
                        MonthPerido.TotalProgress += Convert.ToInt32(Convert.ToDecimal(t.progress.Replace('.', ',')) * 100);
                        MonthPerido.TotalHoursPlanned += Convert.ToInt32(t.duration);
                    }
                }

                pSummary.DailyWorkHours = project.DailyWorkHours;
                pSummary.WeeklyWorkHours = pSummary.DailyWorkHours * 5;
                pSummary.MonthlyWorkHours = pSummary.DailyWorkHours * 20;

                foreach(ProjectSummaryMonth psm in pSummary.ProjectSummaryMonths)
                {
                    var getFreeDays = getFreeDaysBetweenDates(psm.BeginOfMonth, psm.EndOfMonth);

                    psm.TotalWorkHours = pSummary.MonthlyWorkHours - (getFreeDays * project.DailyWorkHours);

                    if (psm.TotalHoursPlanned > 0)
                    {
                        psm.PerceProgress = (psm.TotalProgress) / psm.TotalHoursPlanned;
                    }
                    
                    if (pSummary.MonthlyWorkHours > 0)
                    {
                        psm.PercePlanned = (psm.TotalHoursPlanned * 100) / (pSummary.MonthlyWorkHours - (getFreeDays * project.DailyWorkHours));
                    }

                    psm.TotalRemainHours = psm.TotalWorkHours - psm.TotalHoursPlanned;
                }

                if (pSummary.ProjectSummaryMonths.Count >= 1)
                {
                    pSummary.PercePlannedHoursThisMonth = pSummary.ProjectSummaryMonths[0].PercePlanned;
                }

                if (pSummary.ProjectSummaryMonths.Count >= 2)
                {
                    pSummary.PercePlannedHoursNextMonth = pSummary.ProjectSummaryMonths[1].PercePlanned;
                }

                pSummary.PercePlannedRange = MonthPerido.PercePlanned;

                return Ok(JsonConvert.SerializeObject(pSummary));
            }
        }

        private int getFreeDaysBetweenDates(DateTime startDate, DateTime endDate)
        {
            int freedays = 0;
            foreach(string sdate in project.HolyDays)
            {
                if ((startDate.Date <= Convert.ToDateTime(sdate).Date) &&
                     (endDate.Date >= Convert.ToDateTime(sdate).Date))
                    freedays++;
            }

            return freedays;

        }


        [HttpGet]
        [Route("Activities")]
        public ObjectResult GetTasks()
        {

            lock (lockSaveProject)
            {
                return Ok(JsonConvert.SerializeObject(project.Activities));
            }

        }


        [HttpGet]
        [Route("Activities/{id}")]
        public ObjectResult GetTaskbytId(string id)
        {

            var tmp = project.Activities.Find(x => x.id.Equals(id));

            if (tmp != null)
            {
                return Ok(JsonConvert.SerializeObject(tmp));
            }
            else
            {
                return new NotFoundObjectResult(id);
            }



        }

        [HttpDelete("Activities/{id}")]
        public ObjectResult RemoveTask(string id)
        {

            var tmp = project.Activities.Find(x => x.id.Equals(id));
            if (tmp != null)
            {
                lock (lockSaveProject)
                {
                    RemoveTask(tmp);
                }
                return Ok(JsonConvert.SerializeObject(id));
            }
            else
            {
                return new NotFoundObjectResult(id);
            }


        }

        private void RemoveTask(OCActivity task)
        {
            project.Activities.Remove(task);
            foreach(string t in task.idlinked)
            {
                var tmp = project.Activities.Find(x => x.id.Equals(t));
                project.Activities.Remove(tmp);
            }
            SaveProject();
        }


        [HttpPut("Activities/{id}")]
        public ObjectResult UpdateTask(string id, [FromBody] OCActivity task)
        {

            var tmp = project.Activities.Find(x => x.id.Equals(task.id));
            if (tmp != null)
            {
                lock (lockSaveProject)
                {
                    CopyTask(task, tmp, true, true);
                    if (tmp.idlinked.Count >= 1)
                    {
                        RemoveTask(tmp);
                        InsertTask(tmp);
                    }
                    SaveProject();
                }
            }
            return Ok(JsonConvert.SerializeObject(task));

        }

        private OCActivity CopyTask(OCActivity sourcetask, OCActivity destinationtask, bool CopyGuid = false, bool CopyIdLinked = false)
        {
            if (destinationtask == null)
            {
                destinationtask = new OCActivity();
            }

            if (CopyGuid)
            {
                destinationtask.id = sourcetask.id;

            }
            else
            {
                destinationtask.id = Guid.NewGuid().ToString();
            }

            destinationtask.progress = sourcetask.progress;
            destinationtask.start_date = sourcetask.start_date;
            destinationtask.title = sourcetask.title;
            destinationtask.end_date = sourcetask.end_date;
            destinationtask.duration = sourcetask.duration;
            destinationtask.activitytype = sourcetask.activitytype;
            destinationtask.period = sourcetask.period;
            if (CopyIdLinked)
            {
                destinationtask.idlinked = sourcetask.idlinked;

            }
            destinationtask.url = sourcetask.url;
            return destinationtask;
        }
        private OCActivity CopyTask(OCActivity task, bool CopyGuid = false, bool CopyIdLinked = false)
        {
            return CopyTask(task, null, CopyGuid, CopyIdLinked);
        }

        [HttpPost]
        [Route("Activities")]
        public ObjectResult InsertTask([FromBody] OCActivity task)
        {

            var tmp = project.Activities.Find(x => x.id.Equals(task.id));
            if (tmp == null)
            {
                lock (lockSaveProject)
                {
                    task.id = Guid.NewGuid().ToString();
                    task = RecalcDates(task);
                    if (task.period.Equals("NoPeriod"))
                    {
                        task = SplitByMaxDayHours(task);

                    } else
                    {
                        PeriodTask(task);
                    }
                    if (task.idlinked.Count>0)
                    {
                        if (!task.title.Contains("(Principal)"))
                        {
                            task.title = task.title + " (Principal)";
                        }
                    }
                    project.Activities.Add(task);
                    SaveProject();
                }
            }
            return Ok(JsonConvert.SerializeObject(task));

        }

        private void PeriodTask(OCActivity task)
        {
            int max = 365*5;

            if (task.period.Equals("Weekly"))
            {
                max = (365 * 5) / 7;
            }
            else if (task.period.Equals("BiWeekly"))
            {
                max = (365 * 5) / 14;
            }
            else if (task.period.Equals("ThreeWeekly"))
            {
                max = (365 * 5) / 21;
            }
            else if (task.period.Equals("Monthly"))
            {
                max = (365 * 5) / 12;
            }

            for (int i = 1; i <= max; i++)
            {
                var tmp = CopyTask(task, false, false);
                task.idlinked.Add(tmp.id);
                if (task.period.Equals("Dayly"))
                {
                    tmp.start_date = Convert.ToDateTime(tmp.start_date).AddDays(1 * i).ToString("yyyy-MM-dd HH:mm");
                    tmp = RecalcDates(tmp);

                }
                if (task.period.Equals("Weekly"))
                {
                    tmp.start_date = Convert.ToDateTime(tmp.start_date).AddDays(7 * i).ToString("yyyy-MM-dd HH:mm");
                    tmp = RecalcDates(tmp);

                }
                else if (task.period.Equals("BiWeekly"))
                {
                    tmp.start_date = Convert.ToDateTime(tmp.start_date).AddDays(14 * i).ToString("yyyy-MM-dd HH:mm");
                    tmp = RecalcDates(tmp);

                }
                else if (task.period.Equals("ThreeWeekly"))
                {
                    tmp.start_date = Convert.ToDateTime(tmp.start_date).AddDays(21 * i).ToString("yyyy-MM-dd HH:mm");
                    tmp = RecalcDates(tmp);

                }
                else if (task.period.Equals("Monthly"))
                {
                    tmp.start_date = Convert.ToDateTime(tmp.start_date).AddMonths(1 * i).ToString("yyyy-MM-dd HH:mm");
                    tmp = RecalcDates(tmp);
                }

                project.Activities.Add(tmp);
            }
        }

        private OCActivity RecalcDates(OCActivity task)
        {
            var realStartDate = CalcRealStartDate(task);
            task.end_date = realStartDate.AddHours(Convert.ToInt64(task.duration)).ToString("yyyy-MM-dd HH:mm");
            return task;
        }

        private DateTime CalcRealStartDate(OCActivity task)
        {
            var realStartDate = Convert.ToDateTime(task.start_date);
            if (realStartDate.DayOfWeek == DayOfWeek.Saturday) 
            {
                realStartDate = realStartDate.AddDays(2);

            } else if (realStartDate.DayOfWeek == DayOfWeek.Sunday)
            {

                realStartDate =  realStartDate.AddDays(1);
            }

            foreach(string sdate in project.HolyDays)
            {
                if(realStartDate.Date.ToString("dd/MM/yyyy").Equals(sdate))
                {
                    realStartDate = realStartDate.AddDays(1);
                    task.start_date = realStartDate.ToString("yyyy-MM-dd HH:mm");
                    realStartDate = CalcRealStartDate(task);
                }
            }

            if (CalcDayOcupation(realStartDate) >= project.DailyWorkHours)
            {
                task.start_date = realStartDate.AddDays(1).ToString("yyyy-MM-dd HH:mm");
                realStartDate = CalcRealStartDate(task);
            } 

            task.start_date = realStartDate.ToString("yyyy-MM-dd HH:mm");
            return realStartDate;
        }

        private int CalcDayOcupation(DateTime date)
        {
            int ocupation = 0;
            foreach(OCActivity activity in project.Activities)
            {
                if (Convert.ToDateTime(activity.start_date).Date == date.Date)
                {
                    ocupation = ocupation + Convert.ToInt16(activity.duration);
                }
            }

            return ocupation;
        }
        
        private OCActivity SplitByMaxDayHours(OCActivity task)
        {
            if (Convert.ToInt16(task.duration) > project.DailyWorkHours)
            {
                var FullDuration = Convert.ToInt32(task.duration);
                var TasksPerDuration = FullDuration / project.DailyWorkHours;

                if ((FullDuration % project.DailyWorkHours)>0)
                {
                    TasksPerDuration++;
                }

                task.start_date = Convert.ToDateTime(task.start_date).Date.AddHours(9).ToString("yyyy-MM-dd HH:mm");
                task.duration = project.DailyWorkHours.ToString();
                RecalcDates(task);
                FullDuration = FullDuration - project.DailyWorkHours;

                for (int i = 1; i < TasksPerDuration; i++)
                {
                    var tmp = CopyTask(task);
                    tmp.start_date = Convert.ToDateTime(task.start_date).Date.AddDays(i).AddHours(9).ToString("yyyy-MM-dd HH:mm");
                    tmp = RecalcDates(tmp);
                    if ((FullDuration - project.DailyWorkHours) >0)
                    {
                        tmp.duration = project.DailyWorkHours.ToString();
                        FullDuration = FullDuration - project.DailyWorkHours;

                    } else
                    {
                        tmp.duration = FullDuration.ToString();
                    }

                    tmp.end_date = Convert.ToDateTime(tmp.start_date).AddHours(Convert.ToInt16(tmp.duration)).ToString("yyyy-MM-dd HH:mm");

                    task.idlinked.Add(tmp.id);
                    project.Activities.Add(tmp);
                }

            }
            return task;
        }

    }
}
