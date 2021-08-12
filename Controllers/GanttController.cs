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
    public class GanttController : ControllerBase
    {

        private static Project project;
        private static string appPath;
        private static string projectfile;

        public static object lockSaveProject = new object();

        public GanttController()
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
        public ObjectResult UpdateGanttProject(GanttProject pganttProject)
        {
            lock (lockSaveProject)
            {
                project.DailyWorkHours = pganttProject.DailyWorkHours;
                project.ProjectName = pganttProject.ProjectName;
                SaveProject();
            }
            return Ok(JsonConvert.SerializeObject(pganttProject));
        }

        [HttpGet]
        [Route("Project")]
        public ObjectResult GetGanttProject()
        {
            var ganttProject = new GanttProject();
            ganttProject.DailyWorkHours = project.DailyWorkHours;
            ganttProject.ProjectName = project.ProjectName;

            return Ok(JsonConvert.SerializeObject(ganttProject));
        }

        [HttpGet]
        [Route("ProjectSummary")]
        public ObjectResult GetProjectSymmary()
        {
            var pSummary = new ProjectSummary();

            lock (lockSaveProject)
            {
                var date = DateTime.Now;
                var firstDayOfMonth = new DateTime(date.Year, date.Month, 1);
                var lastDayOfMonth = firstDayOfMonth.AddMonths(1).AddDays(-1);
                var firstDayOfNextMonth = new DateTime(date.Year, date.Month+1, 1);
                var lastDayOfNextMonth = firstDayOfMonth.AddMonths(2).AddDays(-1);

                foreach (GanttTask t in project.Tasks)
                {
                    pSummary.TotalHoursPlanned += Convert.ToInt32(t.duration);
                    pSummary.TotalTasks++;
                    pSummary.TotalProgress += Convert.ToInt32(Convert.ToDecimal(t.progress.Replace('.',','))*100);
                    t.start_date = t.start_date.Replace("00", "").Trim();

                    if ((firstDayOfMonth < Convert.ToDateTime(t.start_date)) &&
                        (lastDayOfMonth > Convert.ToDateTime(t.start_date)))
                    {
                        pSummary.TotalTasksThisMonth++;
                        pSummary.TotalProgressThisMonth += Convert.ToInt32(Convert.ToDecimal(t.progress.Replace('.', ',')) * 100);
                        pSummary.TotalHoursThisMonth += Convert.ToInt32(t.duration);
                    }

                    if ((firstDayOfNextMonth < Convert.ToDateTime(t.start_date)) &&
                        (lastDayOfNextMonth > Convert.ToDateTime(t.start_date)))
                    {
                        pSummary.TotalTasksNextMonth++;
                        pSummary.TotalProgressNextMonth += Convert.ToInt32(Convert.ToDecimal(t.progress.Replace('.', ',')) * 100);
                        pSummary.TotalHoursNextMonth += Convert.ToInt32(t.duration);
                    }
                }
                pSummary.DailyWorkHours = project.DailyWorkHours;
                pSummary.WeeklyWorkHours = pSummary.DailyWorkHours * 5;
                pSummary.MonthlyWorkHours = pSummary.DailyWorkHours * 20;
                pSummary.PerceGlobalProgress = (pSummary.TotalProgress) / pSummary.TotalTasks; 
                if (pSummary.TotalTasksThisMonth>0)
                {
                    pSummary.PerceProgressThisMonth = (pSummary.TotalProgressThisMonth) / pSummary.TotalTasksThisMonth;
                }
                if (pSummary.TotalTasksNextMonth > 0)
                {
                    pSummary.PerceProgressNextMonth = (pSummary.TotalProgressNextMonth) / pSummary.TotalTasksNextMonth;
                }
                if (pSummary.MonthlyWorkHours > 0)
                {
                    pSummary.PercePlannedHoursThisMonth = (pSummary.TotalHoursThisMonth * 100) / pSummary.MonthlyWorkHours;
                }

                if (pSummary.MonthlyWorkHours > 0)
                {
                    pSummary.PercePlannedHoursNextMonth = (pSummary.TotalHoursNextMonth * 100) / pSummary.MonthlyWorkHours;
                }


                return Ok(JsonConvert.SerializeObject(pSummary));
            }
        }


        #region TASKS
        [HttpGet]
        [Route("Tasks")]
        public ObjectResult GetTasks()
        {

            lock (lockSaveProject)
            {
                return Ok(JsonConvert.SerializeObject(project.Tasks));
            }

        }


        [HttpGet]
        [Route("Tasks/{id}")]
        public ObjectResult GetTaskbytId(string id)
        {

            var tmp = project.Tasks.Find(x => x.id.Equals(id));
            if (tmp != null)
            {
                return Ok(JsonConvert.SerializeObject(id));
            }
            else
            {
                return new NotFoundObjectResult(id);
            }



        }

        [HttpDelete("Tasks/{id}")]
        public ObjectResult RemoveTask(string id)
        {

            var tmp = project.Tasks.Find(x => x.id.Equals(id));
            if (tmp != null)
            {
                lock (lockSaveProject)
                {
                    project.Tasks.Remove(tmp);
                    SaveProject();
                }
                return Ok(JsonConvert.SerializeObject(id));
            }
            else
            {
                return new NotFoundObjectResult(id);
            }


        }


        [HttpPut("Tasks/{id}")]
        public ObjectResult UpdateTask(string id, [FromBody] GanttTask task)
        {

            var tmp = project.Tasks.Find(x => x.id.Equals(task.id));
            if (tmp != null)
            {
                lock (lockSaveProject)
                {
                    tmp.progress = task.progress;
                    tmp.start_date = task.start_date;
                    tmp.text = task.text;
                    tmp.parent = task.parent;
                    tmp.end_date = task.end_date;
                    tmp.duration = task.duration;
                    SaveProject();
                }
            }
            return Ok(JsonConvert.SerializeObject(task));

        }


        [HttpPost]
        [Route("Tasks")]
        public ObjectResult InsertTask([FromBody] GanttTask task)
        {

            var tmp = project.Tasks.Find(x => x.id.Equals(task.id));
            if (tmp == null)
            {
                lock (lockSaveProject)
                {
                    project.Tasks.Add(task);
                    SaveProject();
                }
            }
            return Ok(JsonConvert.SerializeObject(task));

        }

        #endregion

        #region LINKS
        [HttpGet]
        [Route("Links")]
        public ObjectResult GetLinks()
        {

            lock (lockSaveProject)
            {
                return Ok(JsonConvert.SerializeObject(project.Links));
            }

        }

        [HttpGet]
        [Route("Links/{id}")]
        public ObjectResult GetLinkbytId(string id)
        {

            var tmp = project.Links.Find(x => x.id.Equals(id));
            if (tmp != null)
            {
                return Ok(JsonConvert.SerializeObject(id));
            }
            else
            {
                return new NotFoundObjectResult(id);
            }



        }

        [HttpDelete("Links/{id}")]
        public ObjectResult RemoveLink(string id)
        {

            var tmp = project.Links.Find(x => x.id.Equals(id));
            if (tmp != null)
            {
                lock (lockSaveProject)
                {
                    project.Links.Remove(tmp);
                    SaveProject();
                }
                return Ok(JsonConvert.SerializeObject(id));
            }
            else
            {
                return new NotFoundObjectResult(id);
            }


        }


        [HttpPut("Links/{id}")]
        public ObjectResult UpdateLink(string id, [FromBody] GanttLink link)
        {

            var tmp = project.Links.Find(x => x.id.Equals(link.id));
            if (tmp != null)
            {
                lock (lockSaveProject)
                {
                    project.Links.Remove(tmp);
                    project.Links.Add(link);
                    SaveProject();
                }
            }
            return Ok(JsonConvert.SerializeObject(link));

        }


        [HttpPost]
        [Route("Links")]
        public ObjectResult InsertLinks([FromBody] GanttLink link)
        {

            var tmp = project.Links.Find(x => x.id.Equals(link.id));
            if (tmp == null)
            {
                lock (lockSaveProject)
                {
                    project.Links.Add(link);
                    SaveProject();
                }
            }
            return Ok(JsonConvert.SerializeObject(link));

        }

        #endregion
    }
}
