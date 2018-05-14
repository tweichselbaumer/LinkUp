using LinkUp.Explorer.WebService.DataContract;
using LinkUp.Explorer.WebService.Repositories;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;

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

        // DELETE: api/Connector/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }

        // GET: api/Connector
        [HttpGet]
        public IEnumerable<Connector> Get()
        {
            return ConnectorRepository.GetAll();
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
    }
}