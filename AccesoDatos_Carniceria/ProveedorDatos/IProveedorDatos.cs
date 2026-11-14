using Entidades_Carniceria.Entidades;
using System;
using System.Collections.Generic;
using System.Text;

namespace AccesoDatos_Carniceria.ProveedorDatos
{
    public interface IProveedorDatos
    {
        Task<List<Proveedor>> ObtenerTodosAsync();
        Task<Proveedor?> ObtenerPorIdAsync(Guid id);
        Task<Proveedor> RegistrarAsync(Proveedor proveedor);
        Task<bool> ActualizarAsync(Proveedor proveedor);
        Task<bool> EliminarAsync(Guid id);
    
    
    }//fin class
}//fn space
