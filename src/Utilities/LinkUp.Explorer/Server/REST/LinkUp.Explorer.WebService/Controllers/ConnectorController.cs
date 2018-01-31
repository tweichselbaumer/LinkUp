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
    public class ConnectorController : Controller
    {
        public ConnectorController(IConnectorRepository connectorRepository)
        {
            ConnectorRepository = connectorRepository;
        }

        public IConnectorRepository ConnectorRepository
        {
            get; set;
        }
        // GET: api/Connector
        [HttpGet]
        public IEnumerable<string> Get()
        {
            return new string[] { "value1", "value2" };
        }

        // GET: api/Connector/5
        [HttpGet("{id}")]
        public string Get(int id)
        {
            return "value";
        }
        
        // POST: api/Connector
        [HttpPost]
        public void Post([FromBody]string value)
        {
        }
        
        // PUT: api/Connector/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody]string value)
        {
        }

        // DELETE: api/Connector/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }
    }
}
