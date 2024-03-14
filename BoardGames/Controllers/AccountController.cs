using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.IdentityModel.Tokens;
using MyBGList.Controllers;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace BoardGamesAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AccountController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<DomainsController> _logger;
        private readonly IConfiguration _configuration;
        private readonly UserManager<ApiUser> _userManager;
        private readonly SignInManager<ApiUser> _singinManager;

        public AccountController(ApplicationDbContext context,
            ILogger<DomainsController> logger,
            IConfiguration configuration,
            UserManager<ApiUser> userManager,
            SignInManager<ApiUser> singinManager)
        {
            _context = context;
            _logger = logger;
            _configuration = configuration;
            _userManager = userManager;
            _singinManager = singinManager;
        }

        [HttpPost("Register")]
        [ResponseCache(CacheProfileName = "NoCache")]
        public async Task<IActionResult> Register(RegisterDTO input)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var newUser = new ApiUser();
                    newUser.UserName = input.UserName;
                    newUser.Email = input.Email;
                    var result = await _userManager.CreateAsync(newUser, input.Password);

                    if (result.Succeeded)
                    {
                        _logger.LogInformation("User {userName} {email} has been created",
                            newUser.UserName, newUser.Email);
                        return StatusCode(201, $"User '{newUser.UserName}' has been created.");
                    }
                    else
                    {
                        throw new Exception($"Error: {string.Join(", ", result.Errors.Select(e => e.Description))}");
                    }
                }
                else
                {
                    var details = new ValidationProblemDetails();
                    details.Type = "https://tools.ietf.org/html/rfc7231#section-6.5.1";
                    details.Status = StatusCodes.Status400BadRequest;
                    return new BadRequestObjectResult(details);
                }
            }
            catch (Exception e)
            {
                var exceptionDetails = new ProblemDetails();
                exceptionDetails.Detail = e.Message;
                exceptionDetails.Status = StatusCodes.Status500InternalServerError;
                exceptionDetails.Type = "https://tools.ietf.org/html/rfc7231#section-6.6.1";

                return StatusCode(StatusCodes.Status500InternalServerError, exceptionDetails);
            }
        }

        [HttpPost("Login")]
        [ResponseCache(CacheProfileName = "NoCache")]
        public async Task<IActionResult> Login(LoginDTO input)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var user = await _userManager.FindByNameAsync(input.UserName);
                    if (user is null || !await _userManager.CheckPasswordAsync(user, input.Password))
                    {
                        throw new Exception("Invalid login attempt.");
                    }
                    else
                    {
                        var signingCredentials = new SigningCredentials(
                            new SymmetricSecurityKey(Encoding.UTF8.GetBytes(
                                _configuration["JWT:SigningKey"])),
                            SecurityAlgorithms.HmacSha256);

                        var claims = new List<Claim>();
                        claims.Add(new Claim(ClaimTypes.Name, user.UserName));
                        claims.AddRange((await _userManager.GetRolesAsync(user))
                            .Select(r => new Claim(ClaimTypes.Role, r)));

                        var jwtObject = new JwtSecurityToken(
                            issuer: _configuration["JWT:Issuer"],
                            audience: _configuration["JWT:Audience"],
                            claims: claims,
                            expires: DateTime.Now.AddSeconds(300),
                            signingCredentials: signingCredentials);

                        var jwtString = new JwtSecurityTokenHandler().WriteToken(jwtObject);

                        return StatusCode(StatusCodes.Status200OK, jwtString);
                    }
                }
                else
                {
                    var details = new ValidationProblemDetails(ModelState);
                    details.Type = "https://tools.ietf.org/html/rfc7231#section-6.5.1";
                    details.Status = StatusCodes.Status400BadRequest;
                    return new BadRequestObjectResult(details);
                }
            }
            catch(Exception e)
            {
                var exceptionDetails = new ProblemDetails();
                exceptionDetails.Detail = e.Message;
                exceptionDetails.Status = StatusCodes.Status401Unauthorized;
                exceptionDetails.Type = "https://tools.ietf.org/html/rfc7231#section-6.6.1";
                return StatusCode(StatusCodes.Status401Unauthorized, exceptionDetails);
            }
        }
    }
}
