using AccesoDatos_Carniceria.ClienteDatos;
using AccesoDatos_Carniceria.FacturaDatos;
using Entidades_Carniceria.Entidades;
using Entidades_Carniceria.Enums;
using System.Text.RegularExpressions;

namespace API_Carniceria.Negocios.ClienteNegocios
{
    public class ClienteNegocios : IClienteNegocios
    {
        private readonly IClienteDatos _clienteDatos;
        private readonly IFacturaDatos _facturaDatos;
        public ClienteNegocios(IClienteDatos clienteDatos, IFacturaDatos facturaDatos)
        {
            _clienteDatos = clienteDatos;
            _facturaDatos = facturaDatos;
        }

        public async Task<List<Cliente>> ObtenerTodosAsync()
        {
            return await _clienteDatos.ObtenerTodosAsync();
        }//fn ObtenerTodosAsync

        public async Task<Cliente?> ObtenerPorIdAsync(Guid id)
        {
            return await _clienteDatos.ObtenerPorIdAsync(id);
        }//fn ObtenerPorIdAsync

        public async Task<Cliente?> BuscarPorIdentificacionAsync(string identificacion)
        {
            if (string.IsNullOrWhiteSpace(identificacion))
            {
                return null;
            }//fn BuscarPorIdentificacionAsync

            string idBuscada = identificacion.Trim();
            List<Cliente> clientes = await _clienteDatos.ObtenerTodosAsync();

            foreach (Cliente item in clientes)
            {
                if (item.Identificacion == idBuscada)
                {
                    return item;
                }
            }

            return null;
        }

        public async Task<Cliente> CrearAsync(Cliente cliente)
        {
            await ValidarClienteAsync(cliente);
            return await _clienteDatos.RegistrarAsync(cliente);
        }//fn CrearAsync

        public async Task<bool> ActualizarAsync(Cliente cliente)
        {
            Cliente? actual = await _clienteDatos.ObtenerPorIdAsync(cliente.Id);

            if (actual == null)
            {
                return false;
            }

            await ValidarClienteAsync(cliente);
            return await _clienteDatos.ActualizarAsync(cliente);
        }//fn ActualizarAsync

        public async Task<bool> EliminarAsync(Guid id)
        {
            Cliente? actual = await _clienteDatos.ObtenerPorIdAsync(id);

            if (actual == null)
            {
                return false;
            }

            bool tieneFacturas = await _facturaDatos.ClienteTieneFacturasAsync(id);
            if (tieneFacturas)
            {
                throw new Exception("No se puede eliminar la persona cliente porque tiene facturas asociadas.");
            }

            try
            {
                return await _clienteDatos.EliminarAsync(id);
            }
            catch (Exception ex)
            {
                if (ex.Message.Contains("REFERENCE") || ex.Message.Contains("FK_"))
                {
                    throw new Exception("No se puede eliminar la persona cliente porque tiene facturas asociadas.");
                }

                throw;
            }
        }//fn EliminarAsync

        #region Metodos Internos
        private async Task ValidarClienteAsync(Cliente cliente)
        {
            if (string.IsNullOrWhiteSpace(cliente.Nombre) || cliente.Nombre.Trim().Length < 3 || cliente.Nombre.Trim().Length > 50)
            {
                throw new Exception("El nombre es obligatorio y debe tener entre 3 y 50 caracteres.");
            }

            if (string.IsNullOrWhiteSpace(cliente.PrimerApellido) || cliente.PrimerApellido.Trim().Length < 3 || cliente.PrimerApellido.Trim().Length > 75)
            {
                throw new Exception("El primer apellido es obligatorio y debe tener entre 3 y 75 caracteres.");
            }

            if (string.IsNullOrWhiteSpace(cliente.SegundoApellido) || cliente.SegundoApellido.Trim().Length < 3 || cliente.SegundoApellido.Trim().Length > 75)
            {
                throw new Exception("El segundo apellido es obligatorio y debe tener entre 3 y 75 caracteres.");
            }

            if (string.IsNullOrWhiteSpace(cliente.Identificacion))
            {
                throw new Exception("El número de identificación es obligatorio.");
            }

            bool formatoValido;

            switch (cliente.TipoIdentificacion)
            {
                case TipoIdentificacion.CedulaNacional:
                    formatoValido = Regex.IsMatch(cliente.Identificacion, @"^\d-\d{4}-\d{4}$");
                    break;
                case TipoIdentificacion.Dimex:
                    formatoValido = Regex.IsMatch(cliente.Identificacion, @"^\d{12}$");
                    break;
                case TipoIdentificacion.CedulaJuridica:
                    formatoValido = Regex.IsMatch(cliente.Identificacion, @"^3-\d{3}-\d{6}$");
                    break;
                case TipoIdentificacion.Pasaporte:
                    formatoValido = Regex.IsMatch(cliente.Identificacion, @"^[a-zA-Z0-9]{6,15}$");
                    break;
                default:
                    formatoValido = false;
                    break;
            }

            if (!formatoValido)
            {
                throw new Exception("El formato de la identificación no es válido para el tipo seleccionado.");
            }

            List<Cliente> clientes = await _clienteDatos.ObtenerTodosAsync();
            foreach (Cliente item in clientes)
            {
                if (item.Identificacion == cliente.Identificacion && item.Id != cliente.Id)
                {
                    throw new Exception("Ya existe una persona cliente registrada con esta identificación.");
                }
            }

            if (cliente.FechaNacimiento > DateTime.Today)
            {
                throw new Exception("La fecha de nacimiento no puede ser una fecha futura.");
            }
        }//fn ValidarClienteAsync

        #endregion


    }//fin class

}//fn space