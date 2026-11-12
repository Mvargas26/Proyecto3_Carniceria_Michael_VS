using Entidades_Carniceria.Entidades;
using Entidades_Carniceria.Enums;
using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Text;

namespace AccesoDatos_Carniceria.EmpleadoDatos
{
    public class EmpleadoDatos : IEmpleadoDatos
    {
        private readonly ConexionBD _conexionBD;

        public EmpleadoDatos(ConexionBD conexionBD)
        {
            _conexionBD = conexionBD;
        }

        public async Task<List<Empleado>> ObtenerTodosAsync()
        {
            List<Empleado> lista = new List<Empleado>();

            using (SqlConnection conexion = _conexionBD.ObtenerConexion())
            {
                using (SqlCommand comando = new SqlCommand("SP_Empleado_CRUD", conexion))
                {
                    comando.CommandType = CommandType.StoredProcedure;
                    comando.Parameters.AddWithValue("@Operacion", "R");

                    await conexion.OpenAsync();

                    using (SqlDataReader lector = await comando.ExecuteReaderAsync())
                    {
                        while (await lector.ReadAsync())
                        {
                            Empleado empleado = new Empleado
                            {
                                Id = lector.GetGuid(lector.GetOrdinal("Id")),
                                TipoIdentificacion = (TipoIdentificacion)lector.GetInt32(lector.GetOrdinal("TipoIdentificacionId")),
                                Identificacion = lector.GetString(lector.GetOrdinal("Identificacion")),
                                Nombre = lector.GetString(lector.GetOrdinal("Nombre")),
                                PrimerApellido = lector.GetString(lector.GetOrdinal("PrimerApellido")),
                                SegundoApellido = lector.GetString(lector.GetOrdinal("SegundoApellido")),
                                FechaNacimiento = lector.GetDateTime(lector.GetOrdinal("FechaNacimiento")),
                                SalarioMensual = lector.GetDecimal(lector.GetOrdinal("SalarioMensual")),
                                FechaIngreso = lector.GetDateTime(lector.GetOrdinal("FechaIngreso")),
                                AreaTrabajo = (AreaTrabajo)lector.GetInt32(lector.GetOrdinal("AreaTrabajoId")),
                                Puesto = (Puesto)lector.GetInt32(lector.GetOrdinal("PuestoId")),
                                Turno = (Turno)lector.GetInt32(lector.GetOrdinal("TurnoId"))
                            };

                            lista.Add(empleado);
                        }
                    }
                }
            }

            return lista;
        }//fn ObtenerTodosAsync

        public async Task<Empleado?> ObtenerPorIdAsync(Guid id)
        {
            Empleado? empleado = null;

            using (SqlConnection conexion = _conexionBD.ObtenerConexion())
            {
                using (SqlCommand comando = new SqlCommand("SP_Empleado_CRUD", conexion))
                {
                    comando.CommandType = CommandType.StoredProcedure;
                    comando.Parameters.AddWithValue("@Operacion", "R");
                    comando.Parameters.AddWithValue("@Id", id);

                    await conexion.OpenAsync();

                    using (SqlDataReader lector = await comando.ExecuteReaderAsync())
                    {
                        if (await lector.ReadAsync())
                        {
                            empleado = new Empleado
                            {
                                Id = lector.GetGuid(lector.GetOrdinal("Id")),
                                TipoIdentificacion = (TipoIdentificacion)lector.GetInt32(lector.GetOrdinal("TipoIdentificacionId")),
                                Identificacion = lector.GetString(lector.GetOrdinal("Identificacion")),
                                Nombre = lector.GetString(lector.GetOrdinal("Nombre")),
                                PrimerApellido = lector.GetString(lector.GetOrdinal("PrimerApellido")),
                                SegundoApellido = lector.GetString(lector.GetOrdinal("SegundoApellido")),
                                FechaNacimiento = lector.GetDateTime(lector.GetOrdinal("FechaNacimiento")),
                                SalarioMensual = lector.GetDecimal(lector.GetOrdinal("SalarioMensual")),
                                FechaIngreso = lector.GetDateTime(lector.GetOrdinal("FechaIngreso")),
                                AreaTrabajo = (AreaTrabajo)lector.GetInt32(lector.GetOrdinal("AreaTrabajoId")),
                                Puesto = (Puesto)lector.GetInt32(lector.GetOrdinal("PuestoId")),
                                Turno = (Turno)lector.GetInt32(lector.GetOrdinal("TurnoId"))
                            };
                        }
                    }
                }
            }

            return empleado;
        }//fn ObtenerPorIdAsync

        public async Task<Empleado> RegistrarAsync(Empleado empleado)
        {
            using (SqlConnection conexion = _conexionBD.ObtenerConexion())
            {
                using (SqlCommand comando = new SqlCommand("SP_Empleado_CRUD", conexion))
                {
                    comando.CommandType = CommandType.StoredProcedure;
                    comando.Parameters.AddWithValue("@Operacion", "C");

                    if (empleado.Id == Guid.Empty)
                    {
                        comando.Parameters.AddWithValue("@Id", DBNull.Value);
                    }
                    else
                    {
                        comando.Parameters.AddWithValue("@Id", empleado.Id);
                    }

                    comando.Parameters.AddWithValue("@TipoIdentificacionId", (int)empleado.TipoIdentificacion);
                    comando.Parameters.AddWithValue("@Identificacion", empleado.Identificacion);
                    comando.Parameters.AddWithValue("@Nombre", empleado.Nombre);
                    comando.Parameters.AddWithValue("@PrimerApellido", empleado.PrimerApellido);
                    comando.Parameters.AddWithValue("@SegundoApellido", empleado.SegundoApellido);
                    comando.Parameters.AddWithValue("@FechaNacimiento", empleado.FechaNacimiento);
                    comando.Parameters.AddWithValue("@SalarioMensual", empleado.SalarioMensual);
                    comando.Parameters.AddWithValue("@FechaIngreso", empleado.FechaIngreso);
                    comando.Parameters.AddWithValue("@AreaTrabajoId", (int)empleado.AreaTrabajo);
                    comando.Parameters.AddWithValue("@PuestoId", (int)empleado.Puesto);
                    comando.Parameters.AddWithValue("@TurnoId", (int)empleado.Turno);

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
                                empleado.Id = lector.GetGuid(lector.GetOrdinal("Id"));
                            }
                        }
                    }
                }
            }

            return empleado;
        }//fn RegistrarAsync

        public async Task<bool> ActualizarAsync(Empleado empleado)
        {
            using (SqlConnection conexion = _conexionBD.ObtenerConexion())
            {
                using (SqlCommand comando = new SqlCommand("SP_Empleado_CRUD", conexion))
                {
                    comando.CommandType = CommandType.StoredProcedure;
                    comando.Parameters.AddWithValue("@Operacion", "U");
                    comando.Parameters.AddWithValue("@Id", empleado.Id);
                    comando.Parameters.AddWithValue("@TipoIdentificacionId", (int)empleado.TipoIdentificacion);
                    comando.Parameters.AddWithValue("@Identificacion", empleado.Identificacion);
                    comando.Parameters.AddWithValue("@Nombre", empleado.Nombre);
                    comando.Parameters.AddWithValue("@PrimerApellido", empleado.PrimerApellido);
                    comando.Parameters.AddWithValue("@SegundoApellido", empleado.SegundoApellido);
                    comando.Parameters.AddWithValue("@FechaNacimiento", empleado.FechaNacimiento);
                    comando.Parameters.AddWithValue("@SalarioMensual", empleado.SalarioMensual);
                    comando.Parameters.AddWithValue("@FechaIngreso", empleado.FechaIngreso);
                    comando.Parameters.AddWithValue("@AreaTrabajoId", (int)empleado.AreaTrabajo);
                    comando.Parameters.AddWithValue("@PuestoId", (int)empleado.Puesto);
                    comando.Parameters.AddWithValue("@TurnoId", (int)empleado.Turno);

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
                using (SqlCommand comando = new SqlCommand("SP_Empleado_CRUD", conexion))
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
        }
    
    
    }//fn class


}// fn space
