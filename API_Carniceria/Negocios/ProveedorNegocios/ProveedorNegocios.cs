using AccesoDatos_Carniceria.ProveedorDatos;
using API_Carniceria.Repositories.Producto_Repository;
using API_Carniceria.Repositories.Proveedor_Repository;
using Entidades_Carniceria.Entidades;
using Entidades_Carniceria.Enums;
using System.Text.RegularExpressions;

namespace API_Carniceria.Services.Proveedor_Service
{
    public class ProveedorNegocios : IProveedorNegocios
    {
        private readonly IProveedorDatos _proveedorDatos;
        private readonly IProductoRepository _productoRepository;

        public ProveedorNegocios(IProveedorDatos proveedorDatos, IProductoRepository productoRepository)
        {
            _proveedorDatos = proveedorDatos;
            _productoRepository = productoRepository;
        }

        public async Task<IReadOnlyCollection<Proveedor>> ObtenerTodosAsync()
        {
            return await _proveedorDatos.ObtenerTodosAsync();
        }//fin ObtenerTodosAsync

        public async Task<Proveedor?> ObtenerPorIdAsync(Guid id)
        {
            return await _proveedorDatos.ObtenerPorIdAsync(id);
        }//fin ObtenerPorIdAsync

        public async Task<IReadOnlyCollection<Proveedor>> BuscarAsync(string texto, string criterio)
        {
            List<Proveedor> todos = await _proveedorDatos.ObtenerTodosAsync();

            if (string.IsNullOrWhiteSpace(texto))
            {
                return todos;
            }

            string filtro = texto.Trim().ToLower();
            List<Proveedor> filtrados = new List<Proveedor>();

            foreach (Proveedor item in todos)
            {
                bool coincide;

                switch (criterio?.ToLower())
                {
                    case "identificacion":
                        coincide = item.Identificacion.ToLower().Contains(filtro);
                        break;

                    case "razonsocial":
                        coincide = item.RazonSocial.ToLower().Contains(filtro);
                        break;

                    case "contacto":
                    case "nombrecontacto":
                        coincide = item.NombreContacto.ToLower().Contains(filtro);
                        break;

                    default:
                        coincide = item.Identificacion.ToLower().Contains(filtro) ||
                                   item.RazonSocial.ToLower().Contains(filtro) ||
                                   item.NombreContacto.ToLower().Contains(filtro);
                        break;
                }

                if (coincide)
                {
                    filtrados.Add(item);
                }
            }

            return filtrados;

        }//fin BuscarAsync

        public async Task<Proveedor> CrearAsync(Proveedor proveedor)
        {
            await ValidarProveedorAsync(proveedor);
            return await _proveedorDatos.RegistrarAsync(proveedor);
        }//fin CrearAsync

        public async Task<bool> ActualizarAsync(Proveedor proveedor)
        {
            Proveedor? actual = await _proveedorDatos.ObtenerPorIdAsync(proveedor.Id);

            if (actual == null)
            {
                return false;
            }

            await ValidarProveedorAsync(proveedor);
            return await _proveedorDatos.ActualizarAsync(proveedor);
        }//fin ActualizarAsync

        public async Task<bool> EliminarAsync(Guid id)
        {
            Proveedor? actual = await _proveedorDatos.ObtenerPorIdAsync(id);

            if (actual == null)
            {
                return false;
            }

            var productos = await _productoRepository.ObtenerTodosAsync();
            bool tieneProductosAsociados = false;

            foreach (var prod in productos)
            {
                if (prod.ProveedorId == id)
                {
                    tieneProductosAsociados = true;
                    break;
                }
            }

            if (tieneProductosAsociados)
            {
                throw new Exception("No es posible eliminar el proveedor porque se encuentra asociado como proveedor principal de uno o más productos.");
            }

            try
            {
                return await _proveedorDatos.EliminarAsync(id);
            }
            catch (Exception ex)
            {
                if (ex.Message.Contains("REFERENCE") || ex.Message.Contains("FK_"))
                {
                    throw new Exception("No es posible eliminar el proveedor porque se encuentra asociado como proveedor principal de uno o más productos.");
                }

                throw;
            }
        }//

        #region MetodosInternos

        private async Task ValidarProveedorAsync(Proveedor proveedor)
        {
            if (string.IsNullOrWhiteSpace(proveedor.Identificacion))
            {
                throw new Exception("El número de identificación es obligatorio.");
            }

            bool formatoValido;

            switch (proveedor.TipoIdentificacion)
            {
                case TipoIdentificacion.CedulaNacional:
                    formatoValido = Regex.IsMatch(proveedor.Identificacion, @"^\d-\d{4}-\d{4}$");
                    break;
                case TipoIdentificacion.CedulaJuridica:
                    formatoValido = Regex.IsMatch(proveedor.Identificacion, @"^3-\d{3}-\d{6}$");
                    break;
                case TipoIdentificacion.Dimex:
                    formatoValido = Regex.IsMatch(proveedor.Identificacion, @"^\d{12}$");
                    break;
                default:
                    formatoValido = false;
                    break;
            }

            if (!formatoValido)
            {
                throw new Exception("El formato de la identificación no es válido para el tipo seleccionado.");
            }

            List<Proveedor> proveedores = await _proveedorDatos.ObtenerTodosAsync();
            bool identificacionRepetida = false;

            foreach (Proveedor existente in proveedores)
            {
                bool mismaIdentificacion = existente.Identificacion == proveedor.Identificacion;
                bool esOtroProveedor = existente.Id != proveedor.Id;

                if (mismaIdentificacion && esOtroProveedor)
                {
                    identificacionRepetida = true;
                    break;
                }
            }

            if (identificacionRepetida)
            {
                throw new Exception("Ya existe un proveedor registrado con esta identificación.");
            }

            if (!Regex.IsMatch(proveedor.Telefono, @"^\d{8}$"))
            {
                throw new Exception("El teléfono debe contener exactamente 8 dígitos numéricos.");
            }
        }//fin ValidarProveedorAsync

        #endregion

    }//fin class

}// fn space
