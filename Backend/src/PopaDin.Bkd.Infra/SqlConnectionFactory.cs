using System.Data;
using Microsoft.Data.SqlClient;
using PopaDin.Bkd.Domain.Interfaces;

namespace PopaDin.Bkd.Infra;

public class SqlConnectionFactory(string connectionString) : IDbConnectionFactory
{
    public IDbConnection CreateConnection()
    {
        return new SqlConnection(connectionString);
    }
}
