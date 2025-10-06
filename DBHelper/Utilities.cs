using System.Data;
using COMMON;
using MySql.Data.MySqlClient;

namespace DBHelper;

public class Utilities
{
    public static IDbConnection GetOpenConnection()
    {
        var connectionString = QarSingleton.GetInstance().GetConnectionString();
        IDbConnection connection = new MySqlConnection(connectionString);
        connection.Open();
        return connection;
    }
}