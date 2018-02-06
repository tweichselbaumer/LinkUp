using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LinkUp.Explorer.WebService.Repositories;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace LinkUp.Explorer.WebService.Controllers
{
    [Produces("application/json")]
    [Route("api/[controller]")]
    public class NodeController : Controller
    {
        public NodeController(INodeRepository nodeRepository)
        {
            NodeRepository = nodeRepository;
        }

        public INodeRepository NodeRepository
        {
            get; set;
        }

        // GET: api/Node
        [HttpGet]
        public DataContract.Node Get()
        {
            return NodeRepository.GetAll();
        }

        // GET: api/Node/5
        [HttpGet("{id}")]
        public string Get(int id)
        {
            return "value";
        }

        // POST: api/Node
        [HttpPost]
        public void Post([FromBody]string value)
        {
        }

        // PUT: api/Node/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody]string value)
        {
        }

        // DELETE: api/ApiWithActions/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }
    }
}
