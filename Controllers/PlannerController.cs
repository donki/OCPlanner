using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using RestSharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
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
                Task.Run(() => CallWebHook(tmp));

            }
        }

        public void CallWebHook(String Json)
        {
            var client = new RestClient(project.WebHook);
            var request = new RestRequest(Method.POST);
            request.AddHeader("APIKEY", project.APIKey);
            request.AddHeader("accept", "application/json");
            request.AddParameter("application/json; charset=utf-8", Json, ParameterType.RequestBody);
            request.RequestFormat = DataFormat.Json;

            Object lockObject = new object();
            lock (lockObject)
            {
                IRestResponse response = client.Execute(request);
                if (response.StatusCode == 0)
                {
                    Thread.Sleep(1000);
                    response = client.Execute(request);
                }
                if ((response.StatusCode != System.Net.HttpStatusCode.OK) &&
                    (response.StatusCode != System.Net.HttpStatusCode.Accepted) &&
                    (response.StatusCode != System.Net.HttpStatusCode.Created))
                {
                    if (response.StatusCode != System.Net.HttpStatusCode.NoContent)
                    {
                        //throw new Exception("Response Error: " + response.Content + "\n\rRequest Object:\n\r" + Json + "\n\r StatusCode " + response.StatusCode.ToString());
                    }
                }

            }

        }

        [HttpPost]
        [Route("Project")]
        public ObjectResult UpdateGanttProject(OCProject pganttProject)
        {
            lock (lockSaveProject)
            {
                project.DailyPlannedWorkHours = pganttProject.DailyPlannedWorkHours;
                project.DailyFullWorkHours = pganttProject.DailyFullWorkHours;
                project.ProjectName = pganttProject.ProjectName;
                project.HolyDays = pganttProject.HolyDays;
                project.StartDate = pganttProject.StartDate;
                project.EndDate = pganttProject.EndDate;
                project.WebHook = pganttProject.WebHook;
                project.APIKey = pganttProject.APIKey;
                SaveProject();
            }
            return Ok(JsonConvert.SerializeObject(pganttProject));
        }

        [HttpGet]
        [Route("Project")]
        public ObjectResult GetProject()
        {
            var ganttProject = new OCProject();
            ganttProject.DailyPlannedWorkHours = project.DailyPlannedWorkHours;
            ganttProject.DailyFullWorkHours = project.DailyFullWorkHours;
            if (project.DailyFullWorkHours>0)
            {
                ganttProject.PercePlannedHoursvsFullWorkHours = (int)((project.DailyPlannedWorkHours * 100) / project.DailyFullWorkHours);
            }
            ganttProject.ProjectName = project.ProjectName;
            ganttProject.HolyDays = project.HolyDays;
            ganttProject.StartDate = project.StartDate;
            ganttProject.EndDate = project.EndDate;
            ganttProject.WebHook = project.WebHook;
            if (project.APIKey == null)
            {
                var appSettings = HttpContext.RequestServices.GetRequiredService<IConfiguration>();
                var GenerateSaaSInvoiceAPIKey = appSettings.GetValue<string>("GenerateSaaSInvoiceAPIKey");
                project.APIKey = GenerateSaaSInvoiceAPIKey;
            }
            ganttProject.APIKey = project.APIKey;
            return Ok(JsonConvert.SerializeObject(ganttProject));
        }

        [HttpGet]
        [Route("external/Project")]
        public ObjectResult GetFullProject()
        {
            string APIKey = Request.Query["APIKey"];
            if ((!project.APIKey.Equals(APIKey)))
            {
                return new UnauthorizedObjectResult("");
            }
            return Ok(JsonConvert.SerializeObject(project));
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
                KeepActivitiesFiveYears();

                foreach (OCActivity t in project.Activities)
                {
                    var MonthName = Convert.ToDateTime(t.start_date).ToString("MM/yyyy");
                    var Month = pSummary.ProjectSummaryMonths.Find((x) => x.MonthName.Equals(MonthName));
                    if (t.planned)
                    {
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
                        Month.TotalHoursPlanned += Convert.ToDecimal(t.duration);
                        if ((Convert.ToDateTime(project.StartDate).Date <= Convert.ToDateTime(t.start_date).Date) &&
                            (Convert.ToDateTime(project.EndDate).Date >= Convert.ToDateTime(t.start_date).Date))
                        {
                            MonthPerido.TotalTasks++;
                            MonthPerido.TotalProgress += Convert.ToInt32(Convert.ToDecimal(t.progress.Replace('.', ',')) * 100);
                            MonthPerido.TotalHoursPlanned += Convert.ToDecimal(t.duration);
                        }
                    } else
                    {
                        Month.TotalNonPlannedHours += Convert.ToDecimal(t.duration);
                        if ((Convert.ToDateTime(project.StartDate).Date <= Convert.ToDateTime(t.start_date).Date) &&
                            (Convert.ToDateTime(project.EndDate).Date >= Convert.ToDateTime(t.start_date).Date))
                        {
                            MonthPerido.TotalNonPlannedHours += Convert.ToDecimal(t.duration);
                        }

                    }
                } 

                pSummary.DailyWorkHours = project.DailyPlannedWorkHours;
                pSummary.WeeklyWorkHours = pSummary.DailyWorkHours * 5;
                pSummary.MonthlyWorkHours = pSummary.DailyWorkHours * 20;

                foreach(ProjectSummaryMonth psm in pSummary.ProjectSummaryMonths)
                {
                    var getFreeDays = getFreeDaysBetweenDates(psm.BeginOfMonth, psm.EndOfMonth) + countWeekEndDaysBetweenDates(psm.BeginOfMonth, psm.EndOfMonth);

                    var getWorkDays = (int)(psm.EndOfMonth - psm.BeginOfMonth).TotalDays;


                    psm.TotalWorkHours = (int)((Convert.ToInt32(getWorkDays) - getFreeDays) * project.DailyPlannedWorkHours);

                    if (psm.TotalHoursPlanned > 0)
                    {
                        psm.PerceProgress = (int)(psm.TotalProgress / psm.TotalHoursPlanned);
                    }
                    
                    if (pSummary.MonthlyWorkHours > 0)
                    {
                        psm.PercePlanned = (int)(psm.TotalHoursPlanned * 100 / psm.TotalWorkHours);
                        if (psm.isMonthPeriod)
                        {
                            pSummary.PercePlannedRange = psm.PercePlanned;
                        }
                    }

                    psm.TotalRemainHours = psm.TotalWorkHours - psm.TotalHoursPlanned;
                    psm.PerceNonPlannedHours = (int)((psm.TotalNonPlannedHours * 100) / ((getWorkDays) *(project.DailyFullWorkHours - pSummary.DailyWorkHours)));
                }

                if (pSummary.ProjectSummaryMonths.Count >= 2)
                {
                    pSummary.PercePlannedHoursThisMonth = pSummary.ProjectSummaryMonths[1].PercePlanned;
                }

                if (pSummary.ProjectSummaryMonths.Count >= 3)
                {
                    pSummary.PercePlannedHoursNextMonth = pSummary.ProjectSummaryMonths[2].PercePlanned;
                }



                return Ok(JsonConvert.SerializeObject(pSummary));
            }
        }

        private int countWeekEndDaysBetweenDates(DateTime start, DateTime stop)
        {
            int days = 0;
            while (start <= stop)
            {
                if (start.DayOfWeek == DayOfWeek.Saturday || start.DayOfWeek == DayOfWeek.Sunday)
                {
                    ++days;
                }
                start = start.AddDays(1);
            }
            return days;
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
                KeepActivitiesFiveYears();
                return Ok(JsonConvert.SerializeObject(project.Activities));
            }

        }

        private void KeepActivitiesFiveYears()
        {
            List<OCActivity> toRemove = new List<OCActivity>();

            foreach (OCActivity t in project.Activities)
            {
                var date = Convert.ToDateTime(t.start_date).Date;
                if (date >= new DateTime(DateTime.Today.Year+5,1,1))
                {
                    toRemove.Add(t);
                }
            }

            foreach (OCActivity t in toRemove)
            {
                project.Activities.Remove(t);
            }
            SaveProject();
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

        [HttpPatch("external/Activities/{id}")]
        public ObjectResult UpdateTaskexternal(string id, [FromBody] OCActivity task)
        {
            string APIKey = Request.Query["APIKey"];
            if ((!project.APIKey.Equals(APIKey)))
            {
                return new UnauthorizedObjectResult("");
            }
            return InternalUpdateTask(task);
        }

        [HttpPut("Activities/{id}")]
        public ObjectResult UpdateTask(string id, [FromBody] OCActivity task)
        {
            return InternalUpdateTask(task);

        }

        private ObjectResult InternalUpdateTask(OCActivity task)
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
                        tmp = RecalcDates(tmp);
                        InsertTask(tmp);
                    }
                    else
                    {
                        tmp = RecalcDates(tmp);
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
            destinationtask.planned = sourcetask.planned;
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
            task.duration = task.duration.Replace('.', ',');
            task.end_date = realStartDate.AddMinutes(Convert.ToDouble(task.duration) *60).ToString("yyyy-MM-dd HH:mm");
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

            if (CalcDayOcupation(realStartDate) >= project.DailyFullWorkHours)
            {
                task.start_date = realStartDate.AddDays(1).ToString("yyyy-MM-dd HH:mm");
                realStartDate = CalcRealStartDate(task);
            } 

            task.start_date = realStartDate.ToString("yyyy-MM-dd HH:mm");
            return realStartDate;
        }

        private decimal CalcDayOcupation(DateTime date)
        {
            decimal ocupation = 0;
            foreach(OCActivity activity in project.Activities)
            {
                if (Convert.ToDateTime(activity.start_date).Date == date.Date)
                {
                    ocupation = ocupation + Convert.ToDecimal(activity.duration);
                }
            }

            return ocupation;
        }
        
        private OCActivity SplitByMaxDayHours(OCActivity task)
        {
            if (Convert.ToDecimal(task.duration) > project.DailyPlannedWorkHours)
            {
                var FullDuration = Convert.ToDecimal(task.duration);
                var TasksPerDuration = FullDuration / project.DailyPlannedWorkHours;

                if ((FullDuration % project.DailyPlannedWorkHours) >0)
                {
                    TasksPerDuration++;
                }

                task.start_date = Convert.ToDateTime(task.start_date).Date.AddHours(9).ToString("yyyy-MM-dd HH:mm");
                task.duration = project.DailyFullWorkHours.ToString();
                RecalcDates(task);
                FullDuration = (int)(FullDuration - project.DailyPlannedWorkHours);

                for (int i = 1; i < TasksPerDuration; i++)
                {
                    var tmp = CopyTask(task);
                    tmp.parentid = task.id;
                    tmp.start_date = Convert.ToDateTime(task.start_date).Date.AddDays(i).AddHours(9).ToString("yyyy-MM-dd HH:mm");
                    tmp = RecalcDates(tmp);
                    if ((FullDuration - project.DailyPlannedWorkHours) >0)
                    {
                        tmp.duration = project.DailyPlannedWorkHours.ToString();
                        FullDuration = (int)(FullDuration - project.DailyPlannedWorkHours);

                    } else
                    {
                        tmp.duration = FullDuration.ToString();
                    }

                    tmp.end_date = Convert.ToDateTime(tmp.start_date).AddHours(Convert.ToDouble(tmp.duration)).ToString("yyyy-MM-dd HH:mm");

                    task.idlinked.Add(tmp.id);
                    project.Activities.Add(tmp);
                }

            }
            return task;
        }

    }
}
