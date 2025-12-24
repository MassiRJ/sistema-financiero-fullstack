using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Financiera.Backend.Data;
using Financiera.Backend.Models;

namespace Financiera.Backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ClientesController : ControllerBase
    {
        private readonly FinancieraContext _context;

        public ClientesController(FinancieraContext context)
        {
            _context = context;
        }

        // GET: api/Clientes
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Cliente>>> GetClientes()
        {
            return await _context.Clientes.ToListAsync();
        }

        // POST: api/Clientes
        [HttpPost]
        public async Task<ActionResult<Cliente>> PostCliente(Cliente cliente)
        {
            _context.Clientes.Add(cliente);
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                if (ClienteExists(cliente.ClienteId))
                {
                    return Conflict();
                }
                else
                {
                    throw;
                }
            }

            return CreatedAtAction("GetClientes", new { id = cliente.ClienteId }, cliente);
        }

        private bool ClienteExists(long id)
        {
            return _context.Clientes.Any(e => e.ClienteId == id);
        }
    }
}