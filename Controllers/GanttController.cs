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
      if (project != null ) { return; }
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

      if(project == null)
      {
        project = new Project();
        System.IO.File.Create(projectfile).Close();
      }
    }

    private void SaveProject()
    {

      lock(lockSaveProject)
      {
        var tmp = JsonConvert.SerializeObject(project);
        System.IO.File.WriteAllText(projectfile, tmp);

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

      var tmp = project.Tasks.Find(x => x.id == id);
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
      } else
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
          project.Tasks.Remove(tmp);
          project.Tasks.Add(task);
          SaveProject();
        }
      }
      return Ok(JsonConvert.SerializeObject(task));

    }


    [HttpPost]
    [Route("Tasks")]
    public ObjectResult InsertTask([FromBody]GanttTask task)
    {

      lock (lockSaveProject)
      {
        project.Tasks.Add(task);
        SaveProject();
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

      var tmp = project.Links.Find(x => x.id == id);
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

      lock (lockSaveProject)
      {
        project.Links.Add(link);
        SaveProject();
      }
      return Ok(JsonConvert.SerializeObject(link));

    }

    #endregion
  }
}
