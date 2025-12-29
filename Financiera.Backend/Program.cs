using Financiera.Backend.Data;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// 1. Configurar la conexión a PostgreSQL
// Le decimos al sistema que use la cadena de conexión que pusimos en appsettings.json
// 1. Intentar leer la variable de entorno de Render (o del sistema)
// FORZANDO LA CONEXIÓN A NEON (Hardcoded)
var connectionString = "Host=ep-curly-union-adms83ps-pooler.c-2.us-east-1.aws.neon.tech;Database=neondb;Username=neondb_owner;Password=npg_AtR2cSyxlHh9;SSL Mode=Require;Trust Server Certificate=true";
builder.Services.AddDbContext<FinancieraContext>(options =>
    options.UseNpgsql(connectionString));

// 2. Agregar los controladores (los endpoints de la API)
builder.Services.AddControllers();

// 3. Configurar Swagger (Documentación automática de la API)
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// --- AGREGAR ESTO PARA PERMITIR QUE LA WEB HABLE CON EL BACKEND ---
builder.Services.AddCors(options =>
{
    options.AddPolicy("PermitirTodo", policy =>
    {
        policy.AllowAnyOrigin()    // Permite conexión desde cualquier URL (luego lo restringimos)
              .AllowAnyMethod()    // Permite GET, POST, PUT, DELETE
              .AllowAnyHeader();
    });
});
// ------------------------------------------------------------------

var app = builder.Build();

// 4. Configurar el pipeline de peticiones HTTP
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors("PermitirTodo");
app.UseAuthorization();

app.MapControllers();

app.Run();