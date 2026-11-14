using Entidades_Carniceria.Entidades;
using Entidades_Carniceria.Enums;
using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Text;

namespace AccesoDatos_Carniceria.ProveedorDatos
{
    public class ProveedorDatos : IProveedorDatos
    {
        private readonly ConexionBD _conexionBD;

        public ProveedorDatos(ConexionBD conexionBD)
        {
            _conexionBD = conexionBD;
        }

        public async Task<List<Proveedor>> ObtenerTodosAsync()
        {
            List<Proveedor> lista = new List<Proveedor>();

            using (SqlConnection conexion = _conexionBD.ObtenerConexion())
            {
                using (SqlCommand comando = new SqlCommand("SP_Proveedor_CRUD", conexion))
                {
                    comando.CommandType = CommandType.StoredProcedure;
                    comando.Parameters.AddWithValue("@Operacion", "R");

                    await conexion.OpenAsync();

                    using (SqlDataReader lector = await comando.ExecuteReaderAsync())
                    {
                        while (await lector.ReadAsync())
                        {
                            Proveedor proveedor = new Proveedor
                            {
                                Id = lector.GetGuid(lector.GetOrdinal("Id")),
                                TipoIdentificacion = (TipoIdentificacion)lector.GetInt32(lector.GetOrdinal("TipoIdentificacionId")),
                                Identificacion = lector.GetString(lector.GetOrdinal("Identificacion")),
                                RazonSocial = lector.GetString(lector.GetOrdinal("RazonSocial")),
                                NombreContacto = lector.GetString(lector.GetOrdinal("NombreContacto")),
                                Telefono = lector.GetString(lector.GetOrdinal("Telefono")),
                                CorreoElectronico = lector.GetString(lector.GetOrdinal("CorreoElectronico")),
                                TipoSuministro = (TipoSuministro)lector.GetInt32(lector.GetOrdinal("TipoSuministroId")),
                                FrecuenciaEntrega = (FrecuenciaEntrega)lector.GetInt32(lector.GetOrdinal("FrecuenciaEntregaId"))
                            };

                            lista.Add(proveedor);
                        }
                    }
                }
            }

            return lista;
        }//fin ObtenerTodosAsync

        public async Task<Proveedor?> ObtenerPorIdAsync(Guid id)
        {
            Proveedor? proveedor = null;

            using (SqlConnection conexion = _conexionBD.ObtenerConexion())
            {
                using (SqlCommand comando = new SqlCommand("SP_Proveedor_CRUD", conexion))
                {
                    comando.CommandType = CommandType.StoredProcedure;
                    comando.Parameters.AddWithValue("@Operacion", "R");
                    comando.Parameters.AddWithValue("@Id", id);

                    await conexion.OpenAsync();

                    using (SqlDataReader lector = await comando.ExecuteReaderAsync())
                    {
                        if (await lector.ReadAsync())
                        {
                            proveedor = new Proveedor
                            {
                                Id = lector.GetGuid(lector.GetOrdinal("Id")),
                                TipoIdentificacion = (TipoIdentificacion)lector.GetInt32(lector.GetOrdinal("TipoIdentificacionId")),
                                Identificacion = lector.GetString(lector.GetOrdinal("Identificacion")),
                                RazonSocial = lector.GetString(lector.GetOrdinal("RazonSocial")),
                                NombreContacto = lector.GetString(lector.GetOrdinal("NombreContacto")),
                                Telefono = lector.GetString(lector.GetOrdinal("Telefono")),
                                CorreoElectronico = lector.GetString(lector.GetOrdinal("CorreoElectronico")),
                                TipoSuministro = (TipoSuministro)lector.GetInt32(lector.GetOrdinal("TipoSuministroId")),
                                FrecuenciaEntrega = (FrecuenciaEntrega)lector.GetInt32(lector.GetOrdinal("FrecuenciaEntregaId"))
                            };
                        }
                    }
                }
            }

            return proveedor;
        }//fin ObtenerPorIdAsync

        public async Task<Proveedor> RegistrarAsync(Proveedor proveedor)
        {
            using (SqlConnection conexion = _conexionBD.ObtenerConexion())
            {
                using (SqlCommand comando = new SqlCommand("SP_Proveedor_CRUD", conexion))
                {
                    comando.CommandType = CommandType.StoredProcedure;
                    comando.Parameters.AddWithValue("@Operacion", "C");

                    if (proveedor.Id == Guid.Empty)
                    {
                        comando.Parameters.AddWithValue("@Id", DBNull.Value);
                    }
                    else
                    {
                        comando.Parameters.AddWithValue("@Id", proveedor.Id);
                    }

                    comando.Parameters.AddWithValue("@TipoIdentificacionId", (int)proveedor.TipoIdentificacion);
                    comando.Parameters.AddWithValue("@Identificacion", proveedor.Identificacion);
                    comando.Parameters.AddWithValue("@RazonSocial", proveedor.RazonSocial);
                    comando.Parameters.AddWithValue("@NombreContacto", proveedor.NombreContacto);
                    comando.Parameters.AddWithValue("@Telefono", proveedor.Telefono);
                    comando.Parameters.AddWithValue("@CorreoElectronico", proveedor.CorreoElectronico);
                    comando.Parameters.AddWithValue("@TipoSuministroId", (int)proveedor.TipoSuministro);
                    comando.Parameters.AddWithValue("@FrecuenciaEntregaId", (int)proveedor.FrecuenciaEntrega);

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
                                proveedor.Id = lector.GetGuid(lector.GetOrdinal("Id"));
                            }
                        }
                    }
                }
            }

            return proveedor;
        }//fn 

        public async Task<bool> ActualizarAsync(Proveedor proveedor)
        {
            using (SqlConnection conexion = _conexionBD.ObtenerConexion())
            {
                using (SqlCommand comando = new SqlCommand("SP_Proveedor_CRUD", conexion))
                {
                    comando.CommandType = CommandType.StoredProcedure;
                    comando.Parameters.AddWithValue("@Operacion", "U");
                    comando.Parameters.AddWithValue("@Id", proveedor.Id);
                    comando.Parameters.AddWithValue("@TipoIdentificacionId", (int)proveedor.TipoIdentificacion);
                    comando.Parameters.AddWithValue("@Identificacion", proveedor.Identificacion);
                    comando.Parameters.AddWithValue("@RazonSocial", proveedor.RazonSocial);
                    comando.Parameters.AddWithValue("@NombreContacto", proveedor.NombreContacto);
                    comando.Parameters.AddWithValue("@Telefono", proveedor.Telefono);
                    comando.Parameters.AddWithValue("@CorreoElectronico", proveedor.CorreoElectronico);
                    comando.Parameters.AddWithValue("@TipoSuministroId", (int)proveedor.TipoSuministro);
                    comando.Parameters.AddWithValue("@FrecuenciaEntregaId", (int)proveedor.FrecuenciaEntrega);

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
        }//fin ActualizarAsync

        public async Task<bool> EliminarAsync(Guid id)
        {
            using (SqlConnection conexion = _conexionBD.ObtenerConexion())
            {
                using (SqlCommand comando = new SqlCommand("SP_Proveedor_CRUD", conexion))
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
        }//fin EliminarAsync
    
    }
}//fin space
