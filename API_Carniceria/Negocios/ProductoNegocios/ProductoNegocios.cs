using AccesoDatos_Carniceria.ProductoDatos;
using AccesoDatos_Carniceria.ProveedorDatos;
using API_Carniceria.Repositories.Factura_Repository;
using API_Carniceria.Repositories.Producto_Repository;
using API_Carniceria.Repositories.Proveedor_Repository;
using Entidades_Carniceria.Entidades;

namespace API_Carniceria.Negocios.ProductoNegocios
{
    public class ProductoNegocios : IProductoNegocios
    {
        private readonly IProductoDatos _productoDatos;
        private readonly IProveedorDatos _proveedorDatos;
        private readonly IFacturaRepository _facturaRepository;

        public ProductoNegocios(
            IProductoDatos productoDatos,
            IProveedorDatos proveedorDatos,
            IFacturaRepository facturaRepository)
        {
            _productoDatos = productoDatos;
            _proveedorDatos = proveedorDatos;
            _facturaRepository = facturaRepository;
        }

        public async Task<IReadOnlyCollection<Producto>> ObtenerTodosAsync()
        {
            return await _productoDatos.ObtenerTodosAsync();
        }//fn ObtenerTodosAsync

        public async Task<Producto?> ObtenerPorIdAsync(Guid id)
        {
            return await _productoDatos.ObtenerPorIdAsync(id);
        }//fn ObtenerPorIdAsync

        public async Task<IReadOnlyCollection<Producto>> BuscarAsync(string texto, string criterio)
        {
            List<Producto> todos = await _productoDatos.ObtenerTodosAsync();

            if (string.IsNullOrWhiteSpace(texto))
            {
                return todos;
            }

            string filtro = texto.Trim().ToLower();
            List<Producto> filtrados = new List<Producto>();

            foreach (Producto item in todos)
            {
                bool coincide;

                switch (criterio?.ToLower())
                {
                    case "codigo":
                    case "codigoproducto":
                        coincide = item.CodigoProducto.ToLower().Contains(filtro);
                        break;

                    case "nombre":
                    case "nombreproducto":
                        coincide = item.NombreProducto.ToLower().Contains(filtro);
                        break;

                    case "categoria":
                        coincide = item.Categoria.ToString().ToLower().Contains(filtro);
                        break;

                    default:
                        coincide = item.CodigoProducto.ToLower().Contains(filtro) ||
                                   item.NombreProducto.ToLower().Contains(filtro);
                        break;
                }

                if (coincide)
                {
                    filtrados.Add(item);
                }
            }

            return filtrados;
        }//fn BuscarAsync

        public async Task<Producto> CrearAsync(Producto producto)
        {
            await ValidarProductoAsync(producto);
            return await _productoDatos.RegistrarAsync(producto);
        }//fn CrearAsync

        public async Task<bool> ActualizarAsync(Producto producto)
        {
            Producto? actual = await _productoDatos.ObtenerPorIdAsync(producto.Id);

            if (actual == null)
            {
                return false;
            }

            await ValidarProductoAsync(producto);
            return await _productoDatos.ActualizarAsync(producto);
        }//fn ActualizarAsync

        public async Task<bool> EliminarAsync(Guid id)
        {
            Producto? actual = await _productoDatos.ObtenerPorIdAsync(id);

            if (actual == null)
            {
                return false;
            }

            if (await _facturaRepository.ProductoTieneFacturasAsync(id))
            {
                throw new Exception("No se puede eliminar el producto porque está asociado a una factura.");
            }

            try
            {
                return await _productoDatos.EliminarAsync(id);
            }
            catch (Exception ex)
            {
                if (ex.Message.Contains("REFERENCE") || ex.Message.Contains("FK_"))
                {
                    throw new Exception("No se puede eliminar el producto porque está asociado a una factura.");
                }

                throw;
            }
        }//fn EliminarAsync

        #region MetodosInternos

        private async Task ValidarProductoAsync(Producto producto)
        {
            if (string.IsNullOrWhiteSpace(producto.CodigoProducto))
            {
                throw new Exception("El código del producto es obligatorio.");
            }

            List<Producto> productos = await _productoDatos.ObtenerTodosAsync();
            bool codigoRepetido = false;

            foreach (Producto item in productos)
            {
                if (item.Id != producto.Id &&
                    item.CodigoProducto.Trim().Equals(producto.CodigoProducto.Trim(), StringComparison.OrdinalIgnoreCase))
                {
                    codigoRepetido = true;
                    break;
                }
            }

            if (codigoRepetido)
            {
                throw new Exception("Ya existe un producto registrado con este código.");
            }

            if (string.IsNullOrWhiteSpace(producto.NombreProducto))
            {
                throw new Exception("El nombre del producto es obligatorio.");
            }

            if (producto.PrecioPorUnidadMedida <= 0)
            {
                throw new Exception("El precio por unidad de medida debe ser mayor a cero.");
            }

            if (producto.ExistenciaActual < 0)
            {
                throw new Exception("La existencia actual no puede ser un valor negativo.");
            }

            if (producto.ExistenciaMinima < 0)
            {
                throw new Exception("La existencia mínima no puede ser un valor negativo.");
            }

            if (producto.ProveedorId == Guid.Empty)
            {
                throw new Exception("Debe seleccionar un proveedor principal válido.");
            }

            Proveedor? proveedorExiste = await _proveedorDatos.ObtenerPorIdAsync(producto.ProveedorId);
            if (proveedorExiste == null)
            {
                throw new Exception("El proveedor principal seleccionado no existe en el sistema.");
            }
        }//fn ValidarProductoAsync

        #endregion
    }

}//fn space
