using Covid.Common.Mapper;
using Covid.Repository.Facades;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;

namespace Covid.Api.Controllers.V1
{
    [Route("api/[controller]")]
    [ApiController]
    public class ValuesController : CovidControllerBase
    {
        public ValuesController(
            ICovidRepositoryFacade repositoryFacade,
            IMapper mapper)
            : base(repositoryFacade, mapper)
        {

        }

        // GET api/values
        [HttpGet]
        public ActionResult<IEnumerable<string>> Get()
        {
            return new string[] { "v1.1", "v1.2" };
        }

        // GET api/values/5
        [HttpGet("{id}")]
        public ActionResult<string> Get(int id)
        {
            return "value";
        }

        // POST api/values
        [HttpPost]
        public void Post([FromBody] string value)
        {
        }

        // PUT api/values/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody] string value)
        {
        }

        // DELETE api/values/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }
    }
}
