using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Text;

namespace AccesoDatos_Carniceria
{
    public class ConexionBD
    {
        private readonly string _cadenaConexion;

        public ConexionBD(IConfiguration configuracion)
        {
            string cadena = configuracion.GetConnectionString("ConexionSQL");
            if (cadena != null)
            {
                _cadenaConexion = cadena;
            }
            else
            {
                _cadenaConexion = string.Empty;
            }
        }

        public SqlConnection ObtenerConexion()
        {
            return new SqlConnection(_cadenaConexion);
        }
    }//fn class
}//fn space
