using System;
using System.Collections.Generic;

namespace Financiera.Backend.Models;

public partial class Transaccione
{
    public long TransaccionId { get; set; }

    public long CuentaId { get; set; }

    public string TipoMovimiento { get; set; } = null!;

    public decimal Monto { get; set; }

    public decimal SaldoHistorico { get; set; }

    public string? Descripcion { get; set; }

    public string? UsuarioResponsable { get; set; }

    public string? Canal { get; set; }

    public DateTime? FechaOperacion { get; set; }

    public virtual Cuenta Cuenta { get; set; } = null!;
}
