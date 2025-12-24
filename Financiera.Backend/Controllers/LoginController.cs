using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Financiera.Backend.Data;
using Financiera.Backend.Models;

namespace Financiera.Backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LoginController : ControllerBase
    {
        private readonly FinancieraContext _context;

        public LoginController(FinancieraContext context)
        {
            _context = context;
        }

        [HttpPost]
        public async Task<IActionResult> IniciarSesion([FromBody] LoginRequest req)
        {
            // 1. Buscar si el usuario existe y la clave coincide
            var usuario = await _context.Usuarios
                .FirstOrDefaultAsync(u => u.NombreUsuario == req.Usuario && u.Clave == req.Clave);

            if (usuario == null)
            {
                return Unauthorized(new { mensaje = "Usuario o clave incorrectos" });
            }

            // 2. Si existe, devolver ÉXITO y sus datos (menos la clave)
            return Ok(new 
            { 
                mensaje = "Bienvenido", 
                usuario = usuario.NombreCompleto, 
                rol = usuario.Rol 
            });
        }
    }

    public class LoginRequest
    {
        public string Usuario { get; set; } = "";
        public string Clave { get; set; } = "";
    }
}