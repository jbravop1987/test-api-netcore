using Microsoft.AspNetCore.Mvc;
using test_api.Services;
using test_api.DTOs;

namespace test_api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TokenController : ControllerBase
    {
        private readonly IJwtService _jwtService;

        public TokenController(IJwtService jwtService)
        {
            _jwtService = jwtService;
        }

        /// <summary>
        /// Genera un JWT token válido para acceder a los endpoints protegidos
        /// No requiere autenticación
        /// </summary>
        [HttpPost("generate")]
        [ProducesResponseType(typeof(TokenResponseDto), StatusCodes.Status200OK)]
        public IActionResult GenerateToken()
        {
            try
            {
                var token = _jwtService.GenerateToken();
                var expiresAt = DateTime.UtcNow.AddMinutes(60);

                return Ok(new TokenResponseDto
                {
                    Success = true,
                    Message = "Token generado exitosamente",
                    Token = token,
                    ExpiresAt = expiresAt
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new TokenResponseDto
                {
                    Success = false,
                    Message = $"Error generando token: {ex.Message}"
                });
            }
        }

        /// <summary>
        /// Obtiene información del JWT (para debugging)
        /// </summary>
        [HttpPost("info")]
        [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
        public IActionResult GetTokenInfo([FromBody] string token)
        {
            if (string.IsNullOrWhiteSpace(token))
            {
                return BadRequest(new { error = "Token requerido" });
            }

            var principal = _jwtService.GetPrincipalFromToken(token);
            
            if (principal == null)
            {
                return BadRequest(new { error = "Token inválido o expirado" });
            }

            var claims = principal.Claims.Select(c => new { c.Type, c.Value }).ToList();

            return Ok(new
            {
                success = true,
                message = "Token válido",
                claims = claims
            });
        }
    }
}
