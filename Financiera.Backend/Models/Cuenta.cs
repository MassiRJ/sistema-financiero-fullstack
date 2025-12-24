using System;
using System.Collections.Generic;

namespace Financiera.Backend.Models;

public partial class Cuenta
{
    public long CuentaId { get; set; }

    public long ClienteId { get; set; }

    public string NumeroCuenta { get; set; } = null!;

    public string Moneda { get; set; } = null!;

    public decimal Saldo { get; set; }

    public string TipoCuenta { get; set; } = null!;

    public string? Estado { get; set; }

    public DateTime? FechaCreacion { get; set; }

    public virtual Cliente? Cliente { get; set; }

    public virtual ICollection<Transaccione> Transacciones { get; set; } = new List<Transaccione>();
}
