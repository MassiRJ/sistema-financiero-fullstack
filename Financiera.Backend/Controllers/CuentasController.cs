using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Financiera.Backend.Data;
using Financiera.Backend.Models;

namespace Financiera.Backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CuentasController : ControllerBase
    {
        private readonly FinancieraContext _context;

        public CuentasController(FinancieraContext context)
        {
            _context = context;
        }

        // GET: api/Cuentas
        // Trae TODAS las cuentas del banco
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Cuenta>>> GetCuentas()
        {
            return await _context.Cuentas.Include(c => c.Cliente).ToListAsync();
        }

        // GET: api/Cuentas/PorCliente/5
        // Trae solo las cuentas de un cliente específico
        [HttpGet("PorCliente/{clienteId}")]
        public async Task<ActionResult<IEnumerable<Cuenta>>> GetCuentasPorCliente(long clienteId)
        {
            var cuentas = await _context.Cuentas
                .Where(c => c.ClienteId == clienteId)
                .ToListAsync();

            if (cuentas == null || !cuentas.Any())
            {
                return NotFound("Este cliente no tiene cuentas.");
            }

            return cuentas;
        }

        // POST: api/Cuentas
        // Abre una nueva cuenta para un cliente
        [HttpPost]
        public async Task<ActionResult<Cuenta>> PostCuenta(Cuenta cuenta)
        {
            // 1. Validar que el cliente exista
            var clienteExiste = await _context.Clientes.AnyAsync(c => c.ClienteId == cuenta.ClienteId);
            if (!clienteExiste)
            {
                return BadRequest("El Cliente especificado no existe.");
            }

            // 2. Validar que el número de cuenta no esté repetido
            var cuentaExiste = await _context.Cuentas.AnyAsync(c => c.NumeroCuenta == cuenta.NumeroCuenta);
            if (cuentaExiste)
            {
                return BadRequest("El número de cuenta ya existe. Use otro.");
            }

            // 3. Configurar valores por defecto
            cuenta.FechaCreacion = DateTime.UtcNow;
            if (cuenta.Saldo < 0) return BadRequest("El saldo inicial no puede ser negativo.");

            _context.Cuentas.Add(cuenta);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetCuentas", new { id = cuenta.CuentaId }, cuenta);
        }
    }
}