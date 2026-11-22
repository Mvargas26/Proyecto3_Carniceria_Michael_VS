using AccesoDatos_Carniceria.ClienteDatos;
using AccesoDatos_Carniceria.EmpleadoDatos;
using AccesoDatos_Carniceria.FacturaDatos;
using AccesoDatos_Carniceria.ProductoDatos;
using Entidades_Carniceria.Entidades;
using Entidades_Carniceria.Enums;


namespace API_Carniceria.Negocios.FacturaNegocios
{
    public class FacturaNegocios : IFacturaNegocios
    {
        #region variablesinternas
        private readonly IFacturaDatos _facturaDatos;
        private readonly IClienteDatos _clienteDatos;
        private readonly IEmpleadoDatos _empleadoDatos;
        private readonly IProductoDatos _productoDatos;
        private const decimal DESCUENTO_MAXIMO = 25m;
        #endregion

        public FacturaNegocios(IFacturaDatos facturaDatos,IClienteDatos clienteDatos,
            IEmpleadoDatos empleadoDatos,IProductoDatos productoDatos)
        {
            _facturaDatos = facturaDatos;
            _clienteDatos = clienteDatos;
            _empleadoDatos = empleadoDatos;
            _productoDatos = productoDatos;
        }

        public async Task<List<Factura>> ObtenerTodasAsync()
        {
            return await _facturaDatos.ObtenerTodasAsync();
        }//fn ObtenerTodasAsync

        public async Task<Factura?> ObtenerPorIdAsync(Guid id)
        {
            return await _facturaDatos.ObtenerPorIdAsync(id);
        }//fn ObtenerPorIdAsync

        public async Task<Factura?> ObtenerPorNumeroAsync(string numeroFactura)
        {
            return await _facturaDatos.ObtenerPorNumeroAsync(numeroFactura);
        }//fn ObtenerPorNumeroAsync

        public async Task<List<Factura>> ObtenerVentasSemanalesAsync()
        {
            return await _facturaDatos.ObtenerVentasSemanalesAsync();
        }//fn ObtenerVentasSemanalesAsync

        public async Task<List<Factura>> BuscarAsync(string criterio)
        {
            List<Factura> todas = await _facturaDatos.ObtenerTodasAsync();

            if (string.IsNullOrWhiteSpace(criterio))
            {
                return todas;
            }

            string filtro = criterio.Trim().ToLower();
            List<Factura> filtradas = new List<Factura>();

            foreach (Factura item in todas)
            {
                if (item.NumeroFactura.ToLower().Contains(filtro) ||
                    item.IdentificacionCliente.ToLower().Contains(filtro) ||
                    item.NombreCliente.ToLower().Contains(filtro) ||
                    item.Estado.ToString().ToLower().Contains(filtro))
                {
                    filtradas.Add(item);
                }
            }

            return filtradas;
        }// fn BuscarAsync

        public async Task<Factura> CrearAsync(Factura factura)
        {
            await ValidarDatosEncabezadoYDetallesAsync(factura);

            Cliente? cliente = await _clienteDatos.ObtenerPorIdAsync(factura.ClienteId);
            Empleado? empleado = await _empleadoDatos.ObtenerPorIdAsync(factura.EmpleadoId);

            factura.IdentificacionCliente = cliente!.Identificacion;
            factura.NombreCliente = $"{cliente.Nombre} {cliente.PrimerApellido} {cliente.SegundoApellido}".Trim();
            factura.NombreEmpleado = $"{empleado!.Nombre} {empleado.PrimerApellido} {empleado.SegundoApellido}".Trim();
            factura.NumeroFactura = await _facturaDatos.ObtenerSiguienteNumeroFacturaAsync();
            factura.Fecha = DateTime.Now;
            factura.Estado = EstadoFactura.Pendiente;


            if (factura.Id == Guid.Empty)
            {
                factura.Id = Guid.NewGuid();
            }

            await CompletarPreciosYValidarStockAsync(factura.Detalles, factura.Id, null);

            return await _facturaDatos.RegistrarAsync(factura);
        }// fn CrearAsync

        public async Task<Factura> ActualizarAsync(Factura factura)
        {
            if (factura == null)
            {
                throw new ArgumentNullException(nameof(factura));
            }

            Factura? actual = await _facturaDatos.ObtenerPorIdAsync(factura.Id);
            if (actual == null)
            {
                throw new Exception("La factura que desea actualizar no existe.");
            }

            if (actual.Estado == EstadoFactura.Anulada)
            {
                throw new Exception("Una factura en estado Anulada no puede ser modificada.");
            }

            await ValidarDatosEncabezadoYDetallesAsync(factura);

            Dictionary<Guid, decimal> movimientosStock = new Dictionary<Guid, decimal>();

            if (actual.Estado == EstadoFactura.Pagada)
            {
                // Devolver cantidades anteriores y restar las nuevas dentro de la misma transacción
                foreach (DetalleFactura detAnt in actual.Detalles)
                {
                    if (!movimientosStock.ContainsKey(detAnt.ProductoId))
                        movimientosStock[detAnt.ProductoId] = 0m;

                    movimientosStock[detAnt.ProductoId] += detAnt.Cantidad;
                }

                await CompletarPreciosYValidarStockAsync(factura.Detalles, actual.Id, movimientosStock);

                foreach (DetalleFactura detNuevo in factura.Detalles)
                {
                    if (!movimientosStock.ContainsKey(detNuevo.ProductoId))
                        movimientosStock[detNuevo.ProductoId] = 0m;

                    movimientosStock[detNuevo.ProductoId] -= detNuevo.Cantidad;
                }
            }
            else
            {
                await CompletarPreciosYValidarStockAsync(factura.Detalles, actual.Id, null);
            }

            Cliente? cliente = await _clienteDatos.ObtenerPorIdAsync(factura.ClienteId);
            Empleado? empleado = await _empleadoDatos.ObtenerPorIdAsync(factura.EmpleadoId);

            actual.ClienteId = factura.ClienteId;
            actual.IdentificacionCliente = cliente!.Identificacion;
            actual.NombreCliente = $"{cliente.Nombre} {cliente.PrimerApellido} {cliente.SegundoApellido}".Trim();
            actual.EmpleadoId = factura.EmpleadoId;
            actual.NombreEmpleado = $"{empleado!.Nombre} {empleado.PrimerApellido} {empleado.SegundoApellido}".Trim();
            actual.MetodoPago = factura.MetodoPago;
            actual.PorcentajeDescuento = factura.PorcentajeDescuento;
            actual.Detalles = factura.Detalles;

            await _facturaDatos.ActualizarConTransaccionAsync(actual, movimientosStock);
            return actual;
        }// fn ActualizarAsync 

        public async Task<bool> EliminarAsync(Guid id)
        {
            Factura? factura = await _facturaDatos.ObtenerPorIdAsync(id);
            if (factura == null)
            {
                return false;
            }

            if (factura.Estado != EstadoFactura.Pendiente)
            {
                throw new Exception("Únicamente las facturas en estado Pendiente pueden ser eliminadas físicamente.");
            }

            return await _facturaDatos.EliminarAsync(id);
        }//

        public async Task<Factura> PagarAsync(Guid id)
        {
            Factura? factura = await _facturaDatos.ObtenerPorIdAsync(id);
            if (factura == null)
            {
                throw new Exception("La factura no existe.");
            }

            if (factura.Estado == EstadoFactura.Pagada)
            {
                throw new Exception("La factura ya se encuentra pagada.");
            }

            if (factura.Estado == EstadoFactura.Anulada)
            {
                throw new Exception("No se puede pagar una factura que ha sido anulada.");
            }

            Dictionary<Guid, decimal> movimientosStock = new Dictionary<Guid, decimal>();

            foreach (DetalleFactura detalle in factura.Detalles)
            {
                Producto? prod = await _productoDatos.ObtenerPorIdAsync(detalle.ProductoId);
                if (prod == null)
                {
                    throw new Exception($"El producto {detalle.NombreProducto} no existe.");
                }

                if (prod.ExistenciaActual < detalle.Cantidad)
                {
                    throw new Exception($"Stock insuficiente para procesar el pago del producto {prod.NombreProducto}. Stock actual: {prod.ExistenciaActual}.");
                }

                if (!movimientosStock.ContainsKey(detalle.ProductoId))
                    movimientosStock[detalle.ProductoId] = 0m;

                movimientosStock[detalle.ProductoId] -= detalle.Cantidad;
            }

            factura.Estado = EstadoFactura.Pagada;
            await _facturaDatos.ActualizarConTransaccionAsync(factura, movimientosStock);
            return factura;
        }//fn PagarAsync

        public async Task<Factura> AnularAsync(Guid id)
        {
            Factura? factura = await _facturaDatos.ObtenerPorIdAsync(id);
            if (factura == null)
            {
                throw new Exception("La factura no existe.");
            }

            if (factura.Estado == EstadoFactura.Anulada)
            {
                throw new Exception("La factura ya se encuentra anulada.");
            }

            Dictionary<Guid, decimal> movimientosStock = new Dictionary<Guid, decimal>();

            if (factura.Estado == EstadoFactura.Pagada)
            {
                foreach (DetalleFactura detalle in factura.Detalles)
                {
                    if (!movimientosStock.ContainsKey(detalle.ProductoId))
                        movimientosStock[detalle.ProductoId] = 0m;

                    movimientosStock[detalle.ProductoId] += detalle.Cantidad;
                }
            }

            factura.Estado = EstadoFactura.Anulada;
            await _facturaDatos.ActualizarConTransaccionAsync(factura, movimientosStock);
            return factura;
        }// fn AnularAsync

        #region MetodosInternos

        private async Task ValidarDatosEncabezadoYDetallesAsync(Factura factura)
        {
            if (factura == null)
            {
                throw new ArgumentNullException(nameof(factura));
            }

            if (factura.PorcentajeDescuento < 0 || factura.PorcentajeDescuento > DESCUENTO_MAXIMO)
            {
                throw new Exception($"El porcentaje de descuento debe estar entre 0% y {DESCUENTO_MAXIMO}%.");
            }

            Cliente? cliente = await _clienteDatos.ObtenerPorIdAsync(factura.ClienteId);
            if (cliente == null)
            {
                throw new Exception("La persona cliente seleccionada no existe.");
            }

            Empleado? empleado = await _empleadoDatos.ObtenerPorIdAsync(factura.EmpleadoId);
            if (empleado == null)
            {
                throw new Exception("La persona empleada seleccionada no existe.");
            }

            if (factura.Detalles == null || !factura.Detalles.Any())
            {
                throw new Exception("La factura debe contener al menos un producto.");
            }
        }//fn ValidarDatosEncabezadoYDetallesAsync

        private async Task CompletarPreciosYValidarStockAsync( List<DetalleFactura> detalles,
            Guid facturaId, Dictionary<Guid, decimal>? devolucionesPrevias)
        {
            foreach (DetalleFactura detalle in detalles)
            {
                if (detalle.Cantidad <= 0)
                {
                    throw new Exception("La cantidad de cada producto debe ser mayor a cero.");
                }

                Producto? producto = await _productoDatos.ObtenerPorIdAsync(detalle.ProductoId);
                if (producto == null)
                {
                    throw new Exception("Uno de los productos especificados en el detalle no existe.");
                }

                decimal devuelto = (devolucionesPrevias != null && devolucionesPrevias.ContainsKey(detalle.ProductoId))
                    ? devolucionesPrevias[detalle.ProductoId]
                    : 0m;

                decimal stockDisponible = producto.ExistenciaActual + devuelto;

                if (stockDisponible < detalle.Cantidad)
                {
                    throw new Exception($"Stock insuficiente para el producto {producto.NombreProducto}. Stock disponible: {stockDisponible}.");
                }

                detalle.FacturaId = facturaId;
                detalle.CodigoProducto = producto.CodigoProducto;
                detalle.NombreProducto = producto.NombreProducto;
                detalle.UnidadMedida = producto.UnidadMedida;
                detalle.PrecioUnitario = producto.PrecioPorUnidadMedida;
            }
        }// fn CompletarPreciosYValidarStockAsync

        #endregion
    }
}//fn space