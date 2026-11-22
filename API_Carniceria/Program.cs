using AccesoDatos_Carniceria;
using AccesoDatos_Carniceria.ClienteDatos;
using AccesoDatos_Carniceria.EmpleadoDatos;
using AccesoDatos_Carniceria.FacturaDatos;
using AccesoDatos_Carniceria.ProductoDatos;
using AccesoDatos_Carniceria.ProveedorDatos;
using API_Carniceria.Negocios.ClienteNegocios;
using API_Carniceria.Negocios.EmpleadoNegocios;
using API_Carniceria.Negocios.FacturaNegocios;
using API_Carniceria.Negocios.ProductoNegocios;
using API_Carniceria.Negocios.ProveedorNegocios;



var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

// Registramos Acceso a Datos (SQL Server)
builder.Services.AddSingleton<ConexionBD>();
builder.Services.AddScoped<IClienteDatos,ClienteDatos>();
builder.Services.AddScoped<IEmpleadoDatos, EmpleadoDatos>();
builder.Services.AddScoped<IProveedorDatos, ProveedorDatos>();
builder.Services.AddScoped<IProductoDatos, ProductoDatos>();
builder.Services.AddScoped<IFacturaDatos, FacturaDatos>();

// Registramos la capa de Negocios
builder.Services.AddScoped<IProveedorNegocios, ProveedorNegocios>();
builder.Services.AddScoped<IProductoNegocios, ProductoNegocios>();
builder.Services.AddScoped<IClienteNegocios, ClienteNegocios>();
builder.Services.AddScoped<IEmpleadoNegocios, EmpleadoNegocios>();
builder.Services.AddScoped<IFacturaNegocios, FacturaNegocios>();


var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseAuthorization();

app.MapControllers();

app.Run();
