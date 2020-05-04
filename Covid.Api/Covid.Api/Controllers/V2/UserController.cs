using Covid.Common.Mapper;
using Covid.Repository.Facades;
using Covid.Web.Model.Users;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;
using Dom = Covid.Web.Model.Users;
using Repo = Covid.Repository.Model.Users;

namespace Covid.Api.Controllers.V2
{
    /// <summary>
    /// Instantiates an instance of the User controller
    /// </summary>
    //[Route("api/{version:apiVersion}/users")]
    [Route("api/users")]
    [ApiController]
    public class UserController : CovidControllerBase
    {
        public UserController(
            ICovidRepositoryFacade repositoryFacade,
            IMapper mapper)
            : base(repositoryFacade, mapper)
        {

        }

        [HttpGet]
        [ProducesDefaultResponseType]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<User>>> GetUsersAsync()
        {
            var repoUsers = await _repositoryFacade.Users.GetUsersAsync();
            // Test
            var users = _mapper.MapEnumerable<Repo.User, Dom.User>(repoUsers);

            return Ok(users);
        }

        [HttpGet("{id:int}", Name = nameof(GetUserByIdAsync))]
        [ProducesDefaultResponseType]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<User>> GetUserByIdAsync(int id)
        {
            var repoUser = await _repositoryFacade.Users.GetUserByIdAsync(id);
            if (repoUser == null)
                return NotFound();

            return Ok(_mapper.Map<Repo.User, Dom.User>(repoUser));
        }

        [HttpPost("create")]
        [ProducesDefaultResponseType]
        [ProducesResponseType(StatusCodes.Status201Created)]
        public async Task<IActionResult> CreateUserAsync([FromBody] Dom.CreateUser user)
        {
            var repoUser = _mapper.Map<Dom.CreateUser, Repo.User>(user);

            var result = await _repositoryFacade.Users.CreateUserAsync(repoUser);
            if (!result.Success)
                return InternalServerError();

            return new CreatedAtRouteResult(nameof(GetUserByIdAsync),
                new { id = result.Value },
                result.Value);
        }

        [HttpPut("{id}")]
        [ProducesDefaultResponseType]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> UpdateUserAsync(int id, [FromBody] Dom.User user)
        {
            var repoUser = await _repositoryFacade.Users.GetUserByIdAsync(id);
            if (repoUser == null)
                return NotFound();

            _mapper.Map<Dom.User, Repo.User>(user, repoUser);

            var success = await _repositoryFacade.Users.UpdateUserAsync(id, repoUser);
            if (!success)
                return InternalServerError();

            return Ok();
        }

        [HttpDelete("{id}")]
        [ProducesDefaultResponseType]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> DeleteUserAsync(int id)
        {
            var repoUser = await _repositoryFacade.Users.GetUserByIdAsync(id);
            if (repoUser == null)
                return NotFound();

            var success = await _repositoryFacade.Users.DeleteUserAsync(id);
            if (!success)
                return InternalServerError();

            return Ok();
        }
    }
}
