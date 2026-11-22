using AccesoDatos_Carniceria.EmpleadoDatos;
using Entidades_Carniceria.Entidades;
using Entidades_Carniceria.Enums;
using AccesoDatos_Carniceria.FacturaDatos;
using System.Text.RegularExpressions;

namespace API_Carniceria.Negocios.EmpleadoNegocios
{
    public class EmpleadoNegocios : IEmpleadoNegocios
    {
        private readonly IEmpleadoDatos _empleadoDatos;
        private readonly IFacturaDatos _facturaDatos;
        public EmpleadoNegocios(IEmpleadoDatos empleadoDatos, IFacturaDatos facturaDatos)
        {
            _empleadoDatos = empleadoDatos;
            _facturaDatos = facturaDatos;
        }

        public async Task<List<Empleado>> ObtenerTodosAsync()
        {
            return await _empleadoDatos.ObtenerTodosAsync();
        }//fn ObtenerTodosAsync

        public async Task<Empleado?> ObtenerPorIdAsync(Guid id)
        {
            return await _empleadoDatos.ObtenerPorIdAsync(id);
        }//fn ObtenerPorIdAsync

        public async Task<Empleado?> BuscarPorIdentificacionAsync(string identificacion)
        {
            if (string.IsNullOrWhiteSpace(identificacion))
            {
                return null;
            }

            string idBuscada = identificacion.Trim();
            List<Empleado> empleados = await _empleadoDatos.ObtenerTodosAsync();

            foreach (Empleado item in empleados)
            {
                if (item.Identificacion == idBuscada)
                {
                    return item;
                }
            }

            return null;
        }//fn BuscarPorIdentificacionAsync

        public async Task<Empleado> CrearAsync(Empleado empleado)
        {
            await ValidarEmpleadoAsync(empleado);
            return await _empleadoDatos.RegistrarAsync(empleado);
        }

        public async Task<bool> ActualizarAsync(Empleado empleado)
        {
            Empleado? actual = await _empleadoDatos.ObtenerPorIdAsync(empleado.Id);

            if (actual == null)
            {
                return false;
            }

            await ValidarEmpleadoAsync(empleado);
            return await _empleadoDatos.ActualizarAsync(empleado);
        }//fn ActualizarAsync

        public async Task<bool> EliminarAsync(Guid id)
        {
            Empleado? actual = await _empleadoDatos.ObtenerPorIdAsync(id);

            if (actual == null)
            {
                return false;
            }

            bool tieneFacturas = await _facturaDatos.EmpleadoTieneFacturasAsync(id);
            if (tieneFacturas)
            {
                throw new Exception("No se puede eliminar la persona empleada porque tiene facturas asociadas.");
            }

            try
            {
                return await _empleadoDatos.EliminarAsync(id);
            }
            catch (Exception ex)
            {
                if (ex.Message.Contains("REFERENCE") || ex.Message.Contains("FK_"))
                {
                    throw new Exception("No se puede eliminar la persona empleada porque tiene facturas asociadas.");
                }

                throw;
            }
        }//fn EliminarAsync

        #region MetodosInternos

        private async Task ValidarEmpleadoAsync(Empleado empleado)
        {
            if (string.IsNullOrWhiteSpace(empleado.Nombre) || empleado.Nombre.Trim().Length < 3 || empleado.Nombre.Trim().Length > 50)
            {
                throw new Exception("El nombre es obligatorio y debe tener entre 3 y 50 caracteres.");
            }

            if (string.IsNullOrWhiteSpace(empleado.PrimerApellido) || empleado.PrimerApellido.Trim().Length < 3 || empleado.PrimerApellido.Trim().Length > 75)
            {
                throw new Exception("El primer apellido es obligatorio y debe tener entre 3 y 75 caracteres.");
            }

            if (string.IsNullOrWhiteSpace(empleado.SegundoApellido) || empleado.SegundoApellido.Trim().Length < 3 || empleado.SegundoApellido.Trim().Length > 75)
            {
                throw new Exception("El segundo apellido es obligatorio y debe tener entre 3 y 75 caracteres.");
            }

            if (string.IsNullOrWhiteSpace(empleado.Identificacion))
            {
                throw new Exception("El número de identificación es obligatorio.");
            }

            bool formatoValido;

            switch (empleado.TipoIdentificacion)
            {
                case TipoIdentificacion.CedulaNacional:
                    formatoValido = Regex.IsMatch(empleado.Identificacion, @"^\d-\d{4}-\d{4}$");
                    break;
                case TipoIdentificacion.Dimex:
                    formatoValido = Regex.IsMatch(empleado.Identificacion, @"^\d{12}$");
                    break;
                case TipoIdentificacion.Pasaporte:
                    formatoValido = Regex.IsMatch(empleado.Identificacion, @"^[a-zA-Z0-9]{6,15}$");
                    break;
                default:
                    formatoValido = false;
                    break;
            }

            if (!formatoValido)
            {
                throw new Exception("El formato de la identificación no es válido para el tipo seleccionado.");
            }

            List<Empleado> empleados = await _empleadoDatos.ObtenerTodosAsync();
            foreach (Empleado item in empleados)
            {
                if (item.Identificacion == empleado.Identificacion && item.Id != empleado.Id)
                {
                    throw new Exception("Ya existe una persona empleada registrada con esta identificación.");
                }
            }

            if (empleado.FechaNacimiento > DateTime.Today)
            {
                throw new Exception("La fecha de nacimiento no puede ser una fecha futura.");
            }

            if (empleado.FechaNacimiento > DateTime.Today.AddYears(-18))
            {
                throw new Exception("La persona empleada debe ser mayor de edad (al menos 18 años).");
            }

            if (empleado.SalarioMensual <= 0)
            {
                throw new Exception("El salario mensual debe ser un valor mayor que cero.");
            }

            if (empleado.FechaIngreso > DateTime.Today)
            {
                throw new Exception("La fecha de ingreso no puede ser una fecha futura.");
            }

            ValidarRelacionAreaPuesto(empleado.AreaTrabajo, empleado.Puesto);
        }//fn ValidarEmpleadoAsync

        private void ValidarRelacionAreaPuesto(AreaTrabajo area, Puesto puesto)
        {
            if (!PuestoPerteneceAArea(area, puesto))
            {
                throw new Exception("El puesto seleccionado no corresponde al área de trabajo asignada.");
            }
        }//fn ValidarRelacionAreaPuesto

        private static bool PuestoPerteneceAArea(AreaTrabajo area, Puesto puesto)
        {
            return area switch
            {
                AreaTrabajo.Administracion => puesto is Puesto.Administrador or Puesto.AsistenteAdministrativo,
                AreaTrabajo.VentasYCaja => puesto is Puesto.Cajero or Puesto.Vendedor or Puesto.ServicioAlCliente,
                AreaTrabajo.CorteYPreparacion => puesto is Puesto.Carnicero or Puesto.Despostador or Puesto.Empacador,
                AreaTrabajo.BodegaEInventario => puesto is Puesto.Bodeguero or Puesto.AuxiliarDeInventario,
                AreaTrabajo.Distribucion => puesto is Puesto.Conductor or Puesto.AyudanteDeDistribucion,
                AreaTrabajo.Limpieza => puesto is Puesto.PersonalDeLimpieza,
                _ => false
            };
        }//fn PuestoPerteneceAArea

        #endregion
    }

}//fn spcae
