using Entidades_Carniceria.Entidades;
using Entidades_Carniceria.Enums;
using Microsoft.Data.SqlClient;
using System.Data;

namespace AccesoDatos_Carniceria.FacturaDatos
{
    /*
     * Guia de Operaciones del Sp - SP_Factura_CRUD:
     *   R   = Read: Consulta los encabezados de facturas (todas, por Id o por NumeroFactura).
     *   RD  = Read Detalles: Consulta las líneas de detalle asociadas a una factura.
     *   SEQ = Sequence: Calcula el siguiente consecutivo disponible (ej. FAC-0003).
     *   C   = Create: Inserta el encabezado de una nueva factura.
     *   CD  = Create Detalle: Inserta una línea de detalle de factura.
     *   U   = Update: Actualiza los datos y totales del encabezado de la factura.
     *   DD  = Delete Detalle: Elimina los detalles anteriores de una factura antes de reinsertarlos.
     *   STK = Stock: Ajusta ExistenciaActual en Productos (+ suma devoluciones, - resta ventas).
     *   D   = Delete: Elimina físicamente una factura y sus detalles (solo estado Pendiente).
     */
    public class FacturaDatos : IFacturaDatos
    {
        private readonly ConexionBD _conexionBD;

        public FacturaDatos(ConexionBD conexionBD)
        {
            _conexionBD = conexionBD;
        }

        public async Task<List<Factura>> ObtenerTodasAsync()
        {
            List<Factura> facturas = new List<Factura>();

            using (SqlConnection conexion = _conexionBD.ObtenerConexion())
            {
                await conexion.OpenAsync();

                using (SqlCommand comando = new SqlCommand("SP_Factura_CRUD", conexion))
                {
                    comando.CommandType = CommandType.StoredProcedure;
                    comando.Parameters.AddWithValue("@Operacion", "R");

                    using (SqlDataReader lector = await comando.ExecuteReaderAsync())
                    {
                        while (await lector.ReadAsync())
                        {
                            facturas.Add(MapearEncabezado(lector));
                        }
                    }
                }

                foreach (Factura factura in facturas)
                {
                    factura.Detalles = await ObtenerDetallesInternoAsync(conexion, factura.Id);
                }
            }

            return facturas;
        }// fn ObtenerTodasAsync

        public async Task<Factura?> ObtenerPorIdAsync(Guid id)
        {
            Factura? factura = null;

            using (SqlConnection conexion = _conexionBD.ObtenerConexion())
            {
                await conexion.OpenAsync();

                using (SqlCommand comando = new SqlCommand("SP_Factura_CRUD", conexion))
                {
                    comando.CommandType = CommandType.StoredProcedure;
                    comando.Parameters.AddWithValue("@Operacion", "R");
                    comando.Parameters.AddWithValue("@Id", id);

                    using (SqlDataReader lector = await comando.ExecuteReaderAsync())
                    {
                        if (await lector.ReadAsync())
                        {
                            factura = MapearEncabezado(lector);
                        }
                    }
                }

                if (factura != null)
                {
                    factura.Detalles = await ObtenerDetallesInternoAsync(conexion, factura.Id);
                }
            }

            return factura;
        }// fn ObtenerPorIdAsync

        public async Task<Factura?> ObtenerPorNumeroAsync(string numeroFactura)
        {
            Factura? factura = null;

            using (SqlConnection conexion = _conexionBD.ObtenerConexion())
            {
                await conexion.OpenAsync();

                using (SqlCommand comando = new SqlCommand("SP_Factura_CRUD", conexion))
                {
                    comando.CommandType = CommandType.StoredProcedure;
                    comando.Parameters.AddWithValue("@Operacion", "R");
                    comando.Parameters.AddWithValue("@NumeroFactura", numeroFactura);

                    using (SqlDataReader lector = await comando.ExecuteReaderAsync())
                    {
                        if (await lector.ReadAsync())
                        {
                            factura = MapearEncabezado(lector);
                        }
                    }
                }

                if (factura != null)
                {
                    factura.Detalles = await ObtenerDetallesInternoAsync(conexion, factura.Id);
                }
            }

            return factura;
        }// fn ObtenerPorNumeroAsync

        public async Task<string> ObtenerSiguienteNumeroFacturaAsync()
        {
            string siguiente = "FAC-0001";

            using (SqlConnection conexion = _conexionBD.ObtenerConexion())
            {
                using (SqlCommand comando = new SqlCommand("SP_Factura_CRUD", conexion))
                {
                    comando.CommandType = CommandType.StoredProcedure;
                    comando.Parameters.AddWithValue("@Operacion", "SEQ");

                    await conexion.OpenAsync();
                    object? resultado = await comando.ExecuteScalarAsync();

                    if (resultado != null && resultado != DBNull.Value)
                    {
                        siguiente = resultado.ToString()!;
                    }
                }
            }

            return siguiente;
        }//

        public async Task<Factura> RegistrarAsync(Factura factura)
        {
            using (SqlConnection conexion = _conexionBD.ObtenerConexion())
            {
                await conexion.OpenAsync();
                using (SqlTransaction transaccion = conexion.BeginTransaction())
                {
                    try
                    {
                        using (SqlCommand cmdEnc = new SqlCommand("SP_Factura_CRUD", conexion, transaccion))
                        {
                            cmdEnc.CommandType = CommandType.StoredProcedure;
                            cmdEnc.Parameters.AddWithValue("@Operacion", "C");
                            cmdEnc.Parameters.AddWithValue("@Id", factura.Id == Guid.Empty ? DBNull.Value : factura.Id);
                            cmdEnc.Parameters.AddWithValue("@NumeroFactura", factura.NumeroFactura);
                            cmdEnc.Parameters.AddWithValue("@Fecha", factura.Fecha);
                            cmdEnc.Parameters.AddWithValue("@ClienteId", factura.ClienteId);
                            cmdEnc.Parameters.AddWithValue("@EmpleadoId", factura.EmpleadoId);
                            cmdEnc.Parameters.AddWithValue("@MetodoPagoId", (int)factura.MetodoPago);
                            cmdEnc.Parameters.AddWithValue("@EstadoFacturaId", (int)factura.Estado);
                            cmdEnc.Parameters.AddWithValue("@PorcentajeDescuento", factura.PorcentajeDescuento);
                            cmdEnc.Parameters.AddWithValue("@Subtotal", factura.Subtotal);
                            cmdEnc.Parameters.AddWithValue("@MontoDescuento", factura.MontoDescuento);
                            cmdEnc.Parameters.AddWithValue("@Impuesto", factura.Impuesto);
                            cmdEnc.Parameters.AddWithValue("@Total", factura.Total);

                            using (SqlDataReader lector = await cmdEnc.ExecuteReaderAsync())
                            {
                                if (await lector.ReadAsync())
                                {
                                    int exito = lector.GetInt32(lector.GetOrdinal("Exito"));
                                    string mensaje = lector.GetString(lector.GetOrdinal("Mensaje"));

                                    if (exito == 0)
                                    {
                                        throw new Exception(mensaje);
                                    }

                                    if (!lector.IsDBNull(lector.GetOrdinal("Id")))
                                    {
                                        factura.Id = lector.GetGuid(lector.GetOrdinal("Id"));
                                    }
                                }
                            }
                        }

                        foreach (DetalleFactura det in factura.Detalles)
                        {
                            det.FacturaId = factura.Id;

                            using (SqlCommand cmdDet = new SqlCommand("SP_Factura_CRUD", conexion, transaccion))
                            {
                                cmdDet.CommandType = CommandType.StoredProcedure;
                                cmdDet.Parameters.AddWithValue("@Operacion", "CD");
                                cmdDet.Parameters.AddWithValue("@Id", det.Id == Guid.Empty ? DBNull.Value : det.Id);
                                cmdDet.Parameters.AddWithValue("@FacturaId", factura.Id);
                                cmdDet.Parameters.AddWithValue("@ProductoId", det.ProductoId);
                                cmdDet.Parameters.AddWithValue("@Cantidad", det.Cantidad);
                                cmdDet.Parameters.AddWithValue("@PrecioUnitario", det.PrecioUnitario);
                                cmdDet.Parameters.AddWithValue("@Subtotal", det.Subtotal);

                                await cmdDet.ExecuteNonQueryAsync();
                            }
                        }

                        await transaccion.CommitAsync();
                        return factura;
                    }
                    catch
                    {
                        await transaccion.RollbackAsync();
                        throw;
                    }
                }
            }
        }// fn RegistrarAsync

        public async Task<bool> ActualizarConTransaccionAsync(Factura factura, Dictionary<Guid, decimal> movimientosStock)
        {
            using (SqlConnection conexion = _conexionBD.ObtenerConexion())
            {
                await conexion.OpenAsync();
                using (SqlTransaction transaccion = conexion.BeginTransaction())
                {
                    try
                    {
                        // 1. Actualizar encabezado y estado
                        using (SqlCommand cmdEnc = new SqlCommand("SP_Factura_CRUD", conexion, transaccion))
                        {
                            cmdEnc.CommandType = CommandType.StoredProcedure;
                            cmdEnc.Parameters.AddWithValue("@Operacion", "U");
                            cmdEnc.Parameters.AddWithValue("@Id", factura.Id);
                            cmdEnc.Parameters.AddWithValue("@ClienteId", factura.ClienteId);
                            cmdEnc.Parameters.AddWithValue("@EmpleadoId", factura.EmpleadoId);
                            cmdEnc.Parameters.AddWithValue("@MetodoPagoId", (int)factura.MetodoPago);
                            cmdEnc.Parameters.AddWithValue("@EstadoFacturaId", (int)factura.Estado);
                            cmdEnc.Parameters.AddWithValue("@PorcentajeDescuento", factura.PorcentajeDescuento);
                            cmdEnc.Parameters.AddWithValue("@Subtotal", factura.Subtotal);
                            cmdEnc.Parameters.AddWithValue("@MontoDescuento", factura.MontoDescuento);
                            cmdEnc.Parameters.AddWithValue("@Impuesto", factura.Impuesto);
                            cmdEnc.Parameters.AddWithValue("@Total", factura.Total);

                            await cmdEnc.ExecuteNonQueryAsync();
                        }

                        // 2. Reemplazar líneas de detalle
                        using (SqlCommand cmdDelDet = new SqlCommand("SP_Factura_CRUD", conexion, transaccion))
                        {
                            cmdDelDet.CommandType = CommandType.StoredProcedure;
                            cmdDelDet.Parameters.AddWithValue("@Operacion", "DD");
                            cmdDelDet.Parameters.AddWithValue("@Id", factura.Id);

                            await cmdDelDet.ExecuteNonQueryAsync();
                        }

                        foreach (DetalleFactura det in factura.Detalles)
                        {
                            using (SqlCommand cmdDet = new SqlCommand("SP_Factura_CRUD", conexion, transaccion))
                            {
                                cmdDet.CommandType = CommandType.StoredProcedure;
                                cmdDet.Parameters.AddWithValue("@Operacion", "CD");
                                cmdDet.Parameters.AddWithValue("@Id", det.Id);
                                cmdDet.Parameters.AddWithValue("@FacturaId", factura.Id);
                                cmdDet.Parameters.AddWithValue("@ProductoId", det.ProductoId);
                                cmdDet.Parameters.AddWithValue("@Cantidad", det.Cantidad);
                                cmdDet.Parameters.AddWithValue("@PrecioUnitario", det.PrecioUnitario);
                                cmdDet.Parameters.AddWithValue("@Subtotal", det.Subtotal);

                                await cmdDet.ExecuteNonQueryAsync();
                            }
                        }

                        // 3. Aplicar ajustes de inventario si corresponde (pago, anulación o edición de pagada)
                        foreach (KeyValuePair<Guid, decimal> mov in movimientosStock)
                        {
                            if (mov.Value == 0) continue;

                            using (SqlCommand cmdStk = new SqlCommand("SP_Factura_CRUD", conexion, transaccion))
                            {
                                cmdStk.CommandType = CommandType.StoredProcedure;
                                cmdStk.Parameters.AddWithValue("@Operacion", "STK");
                                cmdStk.Parameters.AddWithValue("@ProductoId", mov.Key);
                                cmdStk.Parameters.AddWithValue("@Cantidad", mov.Value);

                                await cmdStk.ExecuteNonQueryAsync();
                            }
                        }

                        await transaccion.CommitAsync();
                        return true;
                    }
                    catch
                    {
                        await transaccion.RollbackAsync();
                        throw;
                    }
                }
            }
        }// fn ActualizarConTransaccionAsync

        public async Task<bool> EliminarAsync(Guid id)
        {
            using (SqlConnection conexion = _conexionBD.ObtenerConexion())
            {
                await conexion.OpenAsync();
                using (SqlTransaction transaccion = conexion.BeginTransaction())
                {
                    try
                    {
                        using (SqlCommand comando = new SqlCommand("SP_Factura_CRUD", conexion, transaccion))
                        {
                            comando.CommandType = CommandType.StoredProcedure;
                            comando.Parameters.AddWithValue("@Operacion", "D");
                            comando.Parameters.AddWithValue("@Id", id);

                            using (SqlDataReader lector = await comando.ExecuteReaderAsync())
                            {
                                if (await lector.ReadAsync())
                                {
                                    int exito = lector.GetInt32(lector.GetOrdinal("Exito"));
                                    string mensaje = lector.GetString(lector.GetOrdinal("Mensaje"));

                                    if (exito == 0)
                                    {
                                        throw new Exception(mensaje);
                                    }
                                }
                            }
                        }

                        await transaccion.CommitAsync();
                        return true;
                    }
                    catch
                    {
                        await transaccion.RollbackAsync();
                        throw;
                    }
                }
            }
        }// fn EliminarAsync

        public async Task<List<Factura>> ObtenerVentasSemanalesAsync()
        {
            List<Factura> ventas = new List<Factura>();

            using (SqlConnection conexion = _conexionBD.ObtenerConexion())
            {
                await conexion.OpenAsync();

                using (SqlCommand comando = new SqlCommand("SP_Reporte_VentasSemanales", conexion))
                {
                    comando.CommandType = CommandType.StoredProcedure;
                    comando.Parameters.AddWithValue("@EstadoPagadaId", (int)EstadoFactura.Pagada);

                    using (SqlDataReader lector = await comando.ExecuteReaderAsync())
                    {
                        while (await lector.ReadAsync())
                        {
                            ventas.Add(MapearEncabezado(lector));
                        }
                    }
                }
            }

            return ventas;
        }// fn ObtenerVentasSemanalesAsync


        #region MetodosInternos
        public async Task<bool> ClienteTieneFacturasAsync(Guid clienteId)
        {
            List<Factura> todas = await ObtenerTodasAsync();
            return todas.Any(f => f.ClienteId == clienteId);
        } // fn ClienteTieneFacturasAsync

        public async Task<bool> EmpleadoTieneFacturasAsync(Guid empleadoId)
        {
            List<Factura> todas = await ObtenerTodasAsync();
            return todas.Any(f => f.EmpleadoId == empleadoId);
        }// fn EmpleadoTieneFacturasAsync

        public async Task<bool> ProductoTieneFacturasAsync(Guid productoId)
        {
            List<Factura> todas = await ObtenerTodasAsync();
            return todas.Any(f => f.Detalles.Any(d => d.ProductoId == productoId));
        } // fn ProductoTieneFacturasAsync
        private static Factura MapearEncabezado(SqlDataReader lector)
        {
            return new Factura
            {
                Id = lector.GetGuid(lector.GetOrdinal("Id")),
                NumeroFactura = lector.GetString(lector.GetOrdinal("NumeroFactura")),
                Fecha = lector.GetDateTime(lector.GetOrdinal("Fecha")),
                ClienteId = lector.GetGuid(lector.GetOrdinal("ClienteId")),
                IdentificacionCliente = lector.GetString(lector.GetOrdinal("IdentificacionCliente")),
                NombreCliente = lector.GetString(lector.GetOrdinal("NombreCliente")),
                EmpleadoId = lector.GetGuid(lector.GetOrdinal("EmpleadoId")),
                NombreEmpleado = lector.GetString(lector.GetOrdinal("NombreEmpleado")),
                MetodoPago = (MetodoPago)lector.GetInt32(lector.GetOrdinal("MetodoPagoId")),
                Estado = (EstadoFactura)lector.GetInt32(lector.GetOrdinal("EstadoFacturaId")),
                PorcentajeDescuento = lector.GetDecimal(lector.GetOrdinal("PorcentajeDescuento"))
            };
        }// fn MapearEncabezado

        private static async Task<List<DetalleFactura>> ObtenerDetallesInternoAsync(SqlConnection conexion, Guid facturaId)
        {
            List<DetalleFactura> detalles = new List<DetalleFactura>();

            using (SqlCommand comando = new SqlCommand("SP_Factura_CRUD", conexion))
            {
                comando.CommandType = CommandType.StoredProcedure;
                comando.Parameters.AddWithValue("@Operacion", "RD");
                comando.Parameters.AddWithValue("@Id", facturaId);

                using (SqlDataReader lector = await comando.ExecuteReaderAsync())
                {
                    while (await lector.ReadAsync())
                    {
                        detalles.Add(new DetalleFactura
                        {
                            Id = lector.GetGuid(lector.GetOrdinal("Id")),
                            FacturaId = lector.GetGuid(lector.GetOrdinal("FacturaId")),
                            ProductoId = lector.GetGuid(lector.GetOrdinal("ProductoId")),
                            CodigoProducto = lector.GetString(lector.GetOrdinal("CodigoProducto")),
                            NombreProducto = lector.GetString(lector.GetOrdinal("NombreProducto")),
                            UnidadMedida = (UnidadMedida)lector.GetInt32(lector.GetOrdinal("UnidadMedidaId")),
                            Cantidad = lector.GetDecimal(lector.GetOrdinal("Cantidad")),
                            PrecioUnitario = lector.GetDecimal(lector.GetOrdinal("PrecioUnitario"))
                        });
                    }
                }
            }

            return detalles;
        }// fn ObtenerDetallesInternoAsync

        #endregion
    }
}
