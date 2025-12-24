using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Financiera.Backend.Data;
using Financiera.Backend.Models;

namespace Financiera.Backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TransaccionesController : ControllerBase
    {
        private readonly FinancieraContext _context;

        public TransaccionesController(FinancieraContext context)
        {
            _context = context;
        }

        // GET: Historial
        [HttpGet("PorCuenta/{cuentaId}")]
        public async Task<ActionResult<IEnumerable<Transaccione>>> GetMovimientos(long cuentaId)
        {
            return await _context.Transacciones
                .Where(t => t.CuentaId == cuentaId)
                .OrderByDescending(t => t.FechaOperacion)
                .ToListAsync();
        }

        // POST: Depósito
        [HttpPost("Deposito")]
        public async Task<IActionResult> RealizarDeposito([FromBody] SolicitudTransaccion req)
        {
            var cuenta = await _context.Cuentas.FindAsync(req.CuentaId);
            if (cuenta == null) return NotFound("Cuenta no encontrada");

            cuenta.Saldo += req.Monto;

            _context.Transacciones.Add(new Transaccione
            {
                CuentaId = req.CuentaId,
                TipoMovimiento = "DEPOSITO",
                Monto = req.Monto,
                SaldoHistorico = cuenta.Saldo,
                UsuarioResponsable = "CAJERO_WEB",
                Descripcion = "Depósito en Ventanilla"
            });

            await _context.SaveChangesAsync();
            return Ok(new { mensaje = "Depósito Exitoso", nuevoSaldo = cuenta.Saldo });
        }

        // POST: Retiro
        [HttpPost("Retiro")]
        public async Task<IActionResult> RealizarRetiro([FromBody] SolicitudTransaccion req)
        {
            var cuenta = await _context.Cuentas.FindAsync(req.CuentaId);
            if (cuenta == null) return NotFound("Cuenta no encontrada");
            if (cuenta.Saldo < req.Monto) return BadRequest("Saldo Insuficiente.");

            cuenta.Saldo -= req.Monto;

            _context.Transacciones.Add(new Transaccione
            {
                CuentaId = req.CuentaId,
                TipoMovimiento = "RETIRO",
                Monto = req.Monto,
                SaldoHistorico = cuenta.Saldo,
                UsuarioResponsable = "CAJERO_WEB",
                Descripcion = "Retiro en Ventanilla"
            });

            await _context.SaveChangesAsync();
            return Ok(new { mensaje = "Retiro Exitoso", nuevoSaldo = cuenta.Saldo });
        }

        // POST: Transferencia (CONVERSIÓN DE MONEDA INCLUIDA)
        [HttpPost("Transferencia")]
        public async Task<IActionResult> RealizarTransferencia([FromBody] SolicitudTransferencia req)
        {
            // 1. Validar Cuenta Origen
            var origen = await _context.Cuentas.FindAsync(req.CuentaOrigenId);
            if (origen == null) return NotFound("Cuenta Origen no existe.");
            if (origen.Saldo < req.Monto) return BadRequest("Saldo insuficiente en cuenta origen.");

            // 2. Validar Cuenta Destino
            var destino = await _context.Cuentas.FirstOrDefaultAsync(c => c.NumeroCuenta == req.NumeroCuentaDestino);
            if (destino == null) return BadRequest("La cuenta destino no existe. Verifique el número.");
            if (destino.CuentaId == origen.CuentaId) return BadRequest("No puedes transferirte a la misma cuenta.");

            // --- LÓGICA DE TIPO DE CAMBIO ---
            decimal montoFinal = req.Monto;
            string operacionCambio = "";

            // CASO A: Origen Dólares -> Destino Soles (El banco COMPRA dólares)
            if (origen.Moneda == "USD" && destino.Moneda == "PEN")
            {
                decimal tipoCambioCompra = 3.70m; 
                montoFinal = req.Monto * tipoCambioCompra;
                operacionCambio = $" (T.C. Compra: {tipoCambioCompra})";
            }
            // CASO B: Origen Soles -> Destino Dólares (El banco VENDE dólares)
            else if (origen.Moneda == "PEN" && destino.Moneda == "USD")
            {
                decimal tipoCambioVenta = 3.80m;
                montoFinal = req.Monto / tipoCambioVenta;
                operacionCambio = $" (T.C. Venta: {tipoCambioVenta})";
            }
            // CASO C: Monedas Iguales o no soportadas
            else if (origen.Moneda != destino.Moneda)
            {
                return BadRequest("Conversión de moneda no soportada.");
            }
            // --------------------------------

            // 3. Ejecutar Movimiento
            origen.Saldo -= req.Monto;
            destino.Saldo += montoFinal;

            // 4. Registrar Historia
            var txSalida = new Transaccione
            {
                CuentaId = origen.CuentaId,
                TipoMovimiento = "TRANSFERENCIA_SALIDA",
                Monto = req.Monto,
                SaldoHistorico = origen.Saldo,
                UsuarioResponsable = "CAJERO_WEB",
                Descripcion = $"Transferencia a {destino.NumeroCuenta}" + operacionCambio
            };

            var txEntrada = new Transaccione
            {
                CuentaId = destino.CuentaId,
                TipoMovimiento = "TRANSFERENCIA_ENTRADA",
                Monto = montoFinal,
                SaldoHistorico = destino.Saldo,
                UsuarioResponsable = "CAJERO_WEB",
                Descripcion = $"Recibido de {origen.NumeroCuenta}" + operacionCambio
            };

            _context.Transacciones.AddRange(txSalida, txEntrada);
            await _context.SaveChangesAsync();

            return Ok(new { mensaje = "Transferencia Exitosa" });
        }
    }

    public class SolicitudTransaccion
    {
        public long CuentaId { get; set; }
        public decimal Monto { get; set; }
    }

    public class SolicitudTransferencia
    {
        public long CuentaOrigenId { get; set; }
        public string NumeroCuentaDestino { get; set; } = string.Empty;
        public decimal Monto { get; set; }
    }
}