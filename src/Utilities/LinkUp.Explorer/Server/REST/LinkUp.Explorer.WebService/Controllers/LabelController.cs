using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LinkUp.Explorer.WebService.DataContract;
using LinkUp.Explorer.WebService.Repositories;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace LinkUp.Explorer.WebService.Controllers
{
    [Produces("application/json")]
    [Route("api/[controller]")]
    public class LabelController : Controller
    {
        public LabelController(ILabelRepository labelRepository)
        {
            LabelRepository = labelRepository;
        }

        public ILabelRepository LabelRepository
        {
            get; set;
        }
        // GET: api/Label
        [HttpGet]
        public IEnumerable<string> Get()
        {
            return new string[] { "value1", "value2" };
        }

        // GET: api/Label/5
        [HttpGet("{*name}")]
        public Label Get(string name)
        {
            return LabelRepository.GetLabel(name);
        }

        // POST: api/Label
        [HttpPost]
        public void Post([FromBody]string value)
        {
        }

        // PUT: api/Label/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody]string value)
        {
        }

        // DELETE: api/Label/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }
    }
}
