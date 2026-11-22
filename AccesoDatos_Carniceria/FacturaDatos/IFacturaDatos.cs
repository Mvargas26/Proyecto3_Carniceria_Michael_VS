using Entidades_Carniceria.Entidades;
using System;
using System.Collections.Generic;
using System.Text;

namespace AccesoDatos_Carniceria.FacturaDatos
{
    public interface IFacturaDatos
    {
        Task<List<Factura>> ObtenerTodasAsync();
        Task<Factura?> ObtenerPorIdAsync(Guid id);
        Task<Factura?> ObtenerPorNumeroAsync(string numeroFactura);
        Task<string> ObtenerSiguienteNumeroFacturaAsync();
        Task<Factura> RegistrarAsync(Factura factura);
        Task<bool> ActualizarConTransaccionAsync(Factura factura, Dictionary<Guid, decimal> movimientosStock);
        Task<bool> EliminarAsync(Guid id);
        Task<List<Factura>> ObtenerVentasSemanalesAsync();
        Task<bool> ClienteTieneFacturasAsync(Guid clienteId);
        Task<bool> EmpleadoTieneFacturasAsync(Guid empleadoId);
        Task<bool> ProductoTieneFacturasAsync(Guid productoId);
    }//fn class
}//fn space
