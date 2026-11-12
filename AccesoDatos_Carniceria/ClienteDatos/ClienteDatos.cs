using Microsoft.Data.SqlClient;
using System.Data;
using Entidades_Carniceria.Entidades;
using Entidades_Carniceria.Enums;


namespace AccesoDatos_Carniceria.ClienteDatos
{
    public class ClienteDatos : IClienteDatos
    {
        private readonly ConexionBD _conexionBD;

        public ClienteDatos(ConexionBD conexionBD)
        {
            _conexionBD = conexionBD;
        }

        public async Task<List<Cliente>> ObtenerTodosAsync()
        {
            List<Cliente> lista = new List<Cliente>();

            using (SqlConnection conexion = _conexionBD.ObtenerConexion())
            {
                using (SqlCommand comando = new SqlCommand("SP_Cliente_CRUD", conexion))
                {
                    comando.CommandType = CommandType.StoredProcedure;
                    comando.Parameters.AddWithValue("@Operacion", "R");

                    await conexion.OpenAsync();

                    using (SqlDataReader lector = await comando.ExecuteReaderAsync())
                    {
                        while (await lector.ReadAsync())
                        {
                            Cliente cliente = new Cliente
                            {
                                Id = lector.GetGuid(lector.GetOrdinal("Id")),
                                TipoIdentificacion = (TipoIdentificacion)lector.GetInt32(lector.GetOrdinal("TipoIdentificacionId")),
                                Identificacion = lector.GetString(lector.GetOrdinal("Identificacion")),
                                Nombre = lector.GetString(lector.GetOrdinal("Nombre")),
                                PrimerApellido = lector.GetString(lector.GetOrdinal("PrimerApellido")),
                                SegundoApellido = lector.GetString(lector.GetOrdinal("SegundoApellido")),
                                FechaNacimiento = lector.GetDateTime(lector.GetOrdinal("FechaNacimiento"))
                            };

                            lista.Add(cliente);
                        }
                    }
                }
            }

            return lista;
        }//fn

        public async Task<Cliente?> ObtenerPorIdAsync(Guid id)
        {
            Cliente? cliente = null;

            using (SqlConnection conexion = _conexionBD.ObtenerConexion())
            {
                using (SqlCommand comando = new SqlCommand("SP_Cliente_CRUD", conexion))
                {
                    comando.CommandType = CommandType.StoredProcedure;
                    comando.Parameters.AddWithValue("@Operacion", "R");
                    comando.Parameters.AddWithValue("@Id", id);

                    await conexion.OpenAsync();

                    using (SqlDataReader lector = await comando.ExecuteReaderAsync())
                    {
                        if (await lector.ReadAsync())
                        {
                            cliente = new Cliente
                            {
                                Id = lector.GetGuid(lector.GetOrdinal("Id")),
                                TipoIdentificacion = (TipoIdentificacion)lector.GetInt32(lector.GetOrdinal("TipoIdentificacionId")),
                                Identificacion = lector.GetString(lector.GetOrdinal("Identificacion")),
                                Nombre = lector.GetString(lector.GetOrdinal("Nombre")),
                                PrimerApellido = lector.GetString(lector.GetOrdinal("PrimerApellido")),
                                SegundoApellido = lector.GetString(lector.GetOrdinal("SegundoApellido")),
                                FechaNacimiento = lector.GetDateTime(lector.GetOrdinal("FechaNacimiento"))
                            };
                        }
                    }
                }
            }

            return cliente;
        }//fn

        public async Task<Cliente> RegistrarAsync(Cliente cliente)
        {
            using (SqlConnection conexion = _conexionBD.ObtenerConexion())
            {
                using (SqlCommand comando = new SqlCommand("SP_Cliente_CRUD", conexion))
                {
                    comando.CommandType = CommandType.StoredProcedure;
                    comando.Parameters.AddWithValue("@Operacion", "C");

                    if (cliente.Id == Guid.Empty)
                    {
                        comando.Parameters.AddWithValue("@Id", DBNull.Value);
                    }
                    else
                    {
                        comando.Parameters.AddWithValue("@Id", cliente.Id);
                    }

                    comando.Parameters.AddWithValue("@TipoIdentificacionId", (int)cliente.TipoIdentificacion);
                    comando.Parameters.AddWithValue("@Identificacion", cliente.Identificacion);
                    comando.Parameters.AddWithValue("@Nombre", cliente.Nombre);
                    comando.Parameters.AddWithValue("@PrimerApellido", cliente.PrimerApellido);
                    comando.Parameters.AddWithValue("@SegundoApellido", cliente.SegundoApellido);
                    comando.Parameters.AddWithValue("@FechaNacimiento", cliente.FechaNacimiento);

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
                                cliente.Id = lector.GetGuid(lector.GetOrdinal("Id"));
                            }
                        }
                    }
                }
            }

            return cliente;
        }//fn

        public async Task<bool> ActualizarAsync(Cliente cliente)
        {
            using (SqlConnection conexion = _conexionBD.ObtenerConexion())
            {
                using (SqlCommand comando = new SqlCommand("SP_Cliente_CRUD", conexion))
                {
                    comando.CommandType = CommandType.StoredProcedure;
                    comando.Parameters.AddWithValue("@Operacion", "U");
                    comando.Parameters.AddWithValue("@Id", cliente.Id);
                    comando.Parameters.AddWithValue("@TipoIdentificacionId", (int)cliente.TipoIdentificacion);
                    comando.Parameters.AddWithValue("@Identificacion", cliente.Identificacion);
                    comando.Parameters.AddWithValue("@Nombre", cliente.Nombre);
                    comando.Parameters.AddWithValue("@PrimerApellido", cliente.PrimerApellido);
                    comando.Parameters.AddWithValue("@SegundoApellido", cliente.SegundoApellido);
                    comando.Parameters.AddWithValue("@FechaNacimiento", cliente.FechaNacimiento);

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
        }//fn

        public async Task<bool> EliminarAsync(Guid id)
        {
            using (SqlConnection conexion = _conexionBD.ObtenerConexion())
            {
                using (SqlCommand comando = new SqlCommand("SP_Cliente_CRUD", conexion))
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
        }//fn

    }//fin class
}//fn space
