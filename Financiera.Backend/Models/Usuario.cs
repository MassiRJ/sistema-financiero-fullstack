using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema; // Necesario para [Table] y [Column]

namespace Financiera.Backend.Models;

[Table("usuarios", Schema = "financiera")]
public partial class Usuario
{
    // Mapeamos "UsuarioId" de C# a "usuario_id" de la Base de Datos
    [Column("usuario_id")] 
    public int UsuarioId { get; set; }

    [Column("nombre_usuario")]
    public string NombreUsuario { get; set; } = null!;

    [Column("clave")]
    public string Clave { get; set; } = null!;

    [Column("nombre_completo")]
    public string NombreCompleto { get; set; } = null!;

    [Column("rol")]
    public string? Rol { get; set; }
}