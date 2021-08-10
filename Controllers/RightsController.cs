using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Remote_Management
{
    [Route("api/[controller]")]
    [ApiController]
    public class RightsController : ControllerBase
    {
        
        [HttpGet]
        [Route("GetRights/{UserName}")]
        public ObjectResult Get(string UserName)
        {


            var List = JsonConvert.DeserializeObject<List<string>>(System.IO.File.ReadAllText("permisions.json"));

            return new OkObjectResult(List.IndexOf(UserName)>=0);
            
        }
    }
}
