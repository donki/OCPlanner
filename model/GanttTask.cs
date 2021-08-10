using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OCPlanner
{
  public class GanttTask
  {
    public string id { get; set; }
    public string text { get; set; }
    public string duration { get; set; }
    public string progress { get; set; }
    public string parent { get; set; }
    public string start_date { get; set; }
    public string end_date { get; set; }
  }
}
