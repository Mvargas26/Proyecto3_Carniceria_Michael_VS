using Entidades_Carniceria.Entidades;
using System;
using System.Collections.Generic;
using System.Text;

namespace AccesoDatos_Carniceria.EmpleadoDatos
{
    public interface IEmpleadoDatos
    {
        Task<List<Empleado>> ObtenerTodosAsync();
        Task<Empleado?> ObtenerPorIdAsync(Guid id);
        Task<Empleado> RegistrarAsync(Empleado empleado);
        Task<bool> ActualizarAsync(Empleado empleado);
        Task<bool> EliminarAsync(Guid id);
    }//fn interface
}//fn space
