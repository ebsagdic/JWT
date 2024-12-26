using Business.Abstracts;
using JWT.Model;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace JWT.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : CustomBaseController
    {
        private readonly IUserService _userService;
        private readonly ILogger<UserController> _logger;

        public UserController(IUserService usertService, ILogger<UserController> logger)
        {
            this._userService = usertService;
            this._logger = logger;
        }

        [Authorize]
        [HttpGet("GetUserInfoFromLogined")]
        public async Task<IActionResult> GetUserInfoFromLogined()
        {
            var response = await _userService.GetUserByNameAsync(HttpContext.User.Identity.Name);
            //return new ObjectResult(response);
            return ActionResultInstance(response);

        }

        [HttpGet("{userName}", Name = "GetUserLogin")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetUserLogin(string userName)
        {
            var response = await _userService.GetUserByNameAsync(userName);
            //return new ObjectResult(response);
            return ActionResultInstance(response);

        }

        [HttpPost("register")]
        public async Task<IActionResult> Register(UserForRegisterDto userForRegisterDto)
        {
            var response = await _userService.CreateUserAsync(userForRegisterDto);
            return ActionResultInstance(response);
        }


        [HttpGet("roles")]
        public IActionResult GetAllRoles()
        {
            var response = _userService.GetAllRoles();
            return ActionResultInstance(response);
        }
    }
}
