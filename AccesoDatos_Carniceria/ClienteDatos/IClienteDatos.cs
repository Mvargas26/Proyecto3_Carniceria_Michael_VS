using Entidades_Carniceria.Entidades;
using System;
using System.Collections.Generic;
using System.Text;

namespace AccesoDatos_Carniceria.ClienteDatos
{
    public interface IClienteDatos
    {
        Task<List<Cliente>> ObtenerTodosAsync();
        Task<Cliente?> ObtenerPorIdAsync(Guid id);
        Task<Cliente> RegistrarAsync(Cliente cliente);
        Task<bool> ActualizarAsync(Cliente cliente);
        Task<bool> EliminarAsync(Guid id);

    }//fn interface
}//fn space
