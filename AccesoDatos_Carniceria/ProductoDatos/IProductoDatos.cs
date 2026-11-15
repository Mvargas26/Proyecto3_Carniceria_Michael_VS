using Entidades_Carniceria.Entidades;
using System;
using System.Collections.Generic;
using System.Text;

namespace AccesoDatos_Carniceria.ProductoDatos
{
    public interface IProductoDatos
    {
        Task<List<Producto>> ObtenerTodosAsync();
        Task<Producto?> ObtenerPorIdAsync(Guid id);
        Task<Producto> RegistrarAsync(Producto producto);
        Task<bool> ActualizarAsync(Producto producto);
        Task<bool> EliminarAsync(Guid id);
    }
}//fn space
