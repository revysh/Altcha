using Ixnas.AltchaNet;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;

namespace AltchaSampleWebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AltchaController : ControllerBase
    {
        private readonly AltchaService _altchaService;
        private readonly IMemoryCache _cache;
        public AltchaController(AltchaService altchaService, IMemoryCache cache)
        {
            _altchaService = altchaService;
            _cache = cache;
        }
        // In a real app, move this to appsettings.json
        private static readonly string AltchaSecret = "your-very-secure-random-secret-key";

        [HttpGet("challenge")]
        public IActionResult GetChallenge()
        {
            string ipAddress = Request.HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
            _cache.TryGetValue(ipAddress, out int failCount);

            var complexity = failCount switch
            {
                0 => new AltchaComplexity(50000, 100000),
                1 => new AltchaComplexity(200000, 300000),
                2 => new AltchaComplexity(500000, 800000),
                _ => new AltchaComplexity(1000000, 3000000)
            };
            var overrides = new AltchaGenerateChallengeOverrides { 
                Complexity = complexity,
                Expiry = AltchaExpiry.FromSeconds(10)
            };
                
            var challenge = _altchaService.Generate(overrides);
            return Ok(challenge);
        }

        [HttpPost("submit")]
        public async Task<IActionResult> SubmitForm([FromForm] string altcha)
        {
            string ipAddress = Request.HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
            var result = await _altchaService.Validate(altcha);
            result.IsValid = false;
            if (!result.IsValid)
            {
                // Increment failure count in cache for 10 minutes
                _cache.TryGetValue(ipAddress, out int failCount);
                _cache.Set(ipAddress, failCount + 1, TimeSpan.FromMinutes(10));

                return BadRequest("Invalid captcha.");
            }
            // Reset failures on success
            _cache.Remove(ipAddress);
            return Ok("Success!");
        }
    }
}
