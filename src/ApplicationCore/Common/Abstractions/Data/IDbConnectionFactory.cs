using System.Data;

namespace ApplicationCore.Common.Abstractions.Data
{
    public interface IDbConnectionFactory
    {
        /// <summary>
        /// Este metodo crea una instancia de SqlConection en base al nombre de la cadena de conexion.
        /// </summary>
        /// <param name="connectionName">Nombre de la cadena de conexión</param>
        /// <returns></returns>
        IDbConnection CreateConnection(string connectionName);
    }
}
