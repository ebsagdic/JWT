using Business.Abstracts;
using JWT.Model;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace JWT.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : CustomBaseController
    {
        private readonly IAuthenticationService _authenticationService;
        private readonly ILogger<AuthController> _logger;

        public AuthController(IAuthenticationService authenticationService, ILogger<AuthController> logger)
        {
            this._authenticationService = authenticationService;
            this._logger = logger;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(UserForLoginDto userForLoginDto)
        {
            var response = await _authenticationService.LoginAsync(userForLoginDto);

            return ActionResultInstance(response);
            //return new ObjectResult(response) {  StatusCode= response.StatusCode};
        }

        [HttpPost("refresh-token")]
        public async Task<IActionResult> CreateTokenByRefreshToken(RefreshTokenDto refreshTokenDto)

        {
            var result = await _authenticationService.CreateTokenByRefreshToken(refreshTokenDto.RefreshToken);

            return ActionResultInstance(result);
        }
        [Authorize]
        [HttpPost("revoke-token")]
        public async Task<IActionResult> RevokeRefreshToken(RefreshTokenDto refreshTokenDto)
        {
            var result = await _authenticationService.RevokeRefreshToken(refreshTokenDto.RefreshToken);

            return ActionResultInstance(result);
        }

    }
}
