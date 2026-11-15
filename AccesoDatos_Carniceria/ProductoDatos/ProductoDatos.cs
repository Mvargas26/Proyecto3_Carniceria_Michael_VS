using Entidades_Carniceria.Entidades;
using Entidades_Carniceria.Enums;
using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Text;

namespace AccesoDatos_Carniceria.ProductoDatos
{
    public class ProductoDatos : IProductoDatos
    {
        private readonly ConexionBD _conexionBD;

        public ProductoDatos(ConexionBD conexionBD)
        {
            _conexionBD = conexionBD;
        }

        public async Task<List<Producto>> ObtenerTodosAsync()
        {
            List<Producto> lista = new List<Producto>();

            using (SqlConnection conexion = _conexionBD.ObtenerConexion())
            {
                using (SqlCommand comando = new SqlCommand("SP_Producto_CRUD", conexion))
                {
                    comando.CommandType = CommandType.StoredProcedure;
                    comando.Parameters.AddWithValue("@Operacion", "R");

                    await conexion.OpenAsync();

                    using (SqlDataReader lector = await comando.ExecuteReaderAsync())
                    {
                        while (await lector.ReadAsync())
                        {
                            Producto producto = new Producto
                            {
                                Id = lector.GetGuid(lector.GetOrdinal("Id")),
                                CodigoProducto = lector.GetString(lector.GetOrdinal("CodigoProducto")),
                                NombreProducto = lector.GetString(lector.GetOrdinal("NombreProducto")),
                                Categoria = (Categoria)lector.GetInt32(lector.GetOrdinal("CategoriaId")),
                                UnidadMedida = (UnidadMedida)lector.GetInt32(lector.GetOrdinal("UnidadMedidaId")),
                                PrecioPorUnidadMedida = lector.GetDecimal(lector.GetOrdinal("PrecioPorUnidadMedida")),
                                ExistenciaActual = lector.GetDecimal(lector.GetOrdinal("ExistenciaActual")),
                                ExistenciaMinima = lector.GetDecimal(lector.GetOrdinal("ExistenciaMinima")),
                                ProveedorId = lector.GetGuid(lector.GetOrdinal("ProveedorId"))
                            };

                            lista.Add(producto);
                        }
                    }
                }
            }

            return lista;
        }//fn ObtenerTodosAsync

        public async Task<Producto?> ObtenerPorIdAsync(Guid id)
        {
            Producto? producto = null;

            using (SqlConnection conexion = _conexionBD.ObtenerConexion())
            {
                using (SqlCommand comando = new SqlCommand("SP_Producto_CRUD", conexion))
                {
                    comando.CommandType = CommandType.StoredProcedure;
                    comando.Parameters.AddWithValue("@Operacion", "R");
                    comando.Parameters.AddWithValue("@Id", id);

                    await conexion.OpenAsync();

                    using (SqlDataReader lector = await comando.ExecuteReaderAsync())
                    {
                        if (await lector.ReadAsync())
                        {
                            producto = new Producto
                            {
                                Id = lector.GetGuid(lector.GetOrdinal("Id")),
                                CodigoProducto = lector.GetString(lector.GetOrdinal("CodigoProducto")),
                                NombreProducto = lector.GetString(lector.GetOrdinal("NombreProducto")),
                                Categoria = (Categoria)lector.GetInt32(lector.GetOrdinal("CategoriaId")),
                                UnidadMedida = (UnidadMedida)lector.GetInt32(lector.GetOrdinal("UnidadMedidaId")),
                                PrecioPorUnidadMedida = lector.GetDecimal(lector.GetOrdinal("PrecioPorUnidadMedida")),
                                ExistenciaActual = lector.GetDecimal(lector.GetOrdinal("ExistenciaActual")),
                                ExistenciaMinima = lector.GetDecimal(lector.GetOrdinal("ExistenciaMinima")),
                                ProveedorId = lector.GetGuid(lector.GetOrdinal("ProveedorId"))
                            };
                        }
                    }
                }
            }

            return producto;
        }//fn ObtenerPorIdAsync

        public async Task<Producto> RegistrarAsync(Producto producto)
        {
            using (SqlConnection conexion = _conexionBD.ObtenerConexion())
            {
                using (SqlCommand comando = new SqlCommand("SP_Producto_CRUD", conexion))
                {
                    comando.CommandType = CommandType.StoredProcedure;
                    comando.Parameters.AddWithValue("@Operacion", "C");

                    if (producto.Id == Guid.Empty)
                    {
                        comando.Parameters.AddWithValue("@Id", DBNull.Value);
                    }
                    else
                    {
                        comando.Parameters.AddWithValue("@Id", producto.Id);
                    }

                    comando.Parameters.AddWithValue("@CodigoProducto", producto.CodigoProducto);
                    comando.Parameters.AddWithValue("@NombreProducto", producto.NombreProducto);
                    comando.Parameters.AddWithValue("@CategoriaId", (int)producto.Categoria);
                    comando.Parameters.AddWithValue("@UnidadMedidaId", (int)producto.UnidadMedida);
                    comando.Parameters.AddWithValue("@PrecioPorUnidadMedida", producto.PrecioPorUnidadMedida);
                    comando.Parameters.AddWithValue("@ExistenciaActual", producto.ExistenciaActual);
                    comando.Parameters.AddWithValue("@ExistenciaMinima", producto.ExistenciaMinima);
                    comando.Parameters.AddWithValue("@ProveedorId", producto.ProveedorId);

                    await conexion.OpenAsync();

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

                            if (!lector.IsDBNull(lector.GetOrdinal("Id")))
                            {
                                producto.Id = lector.GetGuid(lector.GetOrdinal("Id"));
                            }
                        }
                    }
                }
            }

            return producto;
        }//fn RegistrarAsync

        public async Task<bool> ActualizarAsync(Producto producto)
        {
            using (SqlConnection conexion = _conexionBD.ObtenerConexion())
            {
                using (SqlCommand comando = new SqlCommand("SP_Producto_CRUD", conexion))
                {
                    comando.CommandType = CommandType.StoredProcedure;
                    comando.Parameters.AddWithValue("@Operacion", "U");
                    comando.Parameters.AddWithValue("@Id", producto.Id);
                    comando.Parameters.AddWithValue("@CodigoProducto", producto.CodigoProducto);
                    comando.Parameters.AddWithValue("@NombreProducto", producto.NombreProducto);
                    comando.Parameters.AddWithValue("@CategoriaId", (int)producto.Categoria);
                    comando.Parameters.AddWithValue("@UnidadMedidaId", (int)producto.UnidadMedida);
                    comando.Parameters.AddWithValue("@PrecioPorUnidadMedida", producto.PrecioPorUnidadMedida);
                    comando.Parameters.AddWithValue("@ExistenciaActual", producto.ExistenciaActual);
                    comando.Parameters.AddWithValue("@ExistenciaMinima", producto.ExistenciaMinima);
                    comando.Parameters.AddWithValue("@ProveedorId", producto.ProveedorId);

                    await conexion.OpenAsync();

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
            }

            return true;
        }//fn ActualizarAsync

        public async Task<bool> EliminarAsync(Guid id)
        {
            using (SqlConnection conexion = _conexionBD.ObtenerConexion())
            {
                using (SqlCommand comando = new SqlCommand("SP_Producto_CRUD", conexion))
                {
                    comando.CommandType = CommandType.StoredProcedure;
                    comando.Parameters.AddWithValue("@Operacion", "D");
                    comando.Parameters.AddWithValue("@Id", id);

                    await conexion.OpenAsync();

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
            }

            return true;
        }//fn EliminarAsync

    }//fn class
}//fn space
