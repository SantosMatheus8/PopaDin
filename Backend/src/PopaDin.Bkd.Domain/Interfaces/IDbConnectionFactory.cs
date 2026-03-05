using System.Data;

namespace PopaDin.Bkd.Domain.Interfaces;

public interface IDbConnectionFactory
{
    IDbConnection CreateConnection();
}
