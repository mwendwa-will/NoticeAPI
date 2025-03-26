using Microsoft.VisualBasic;
using MySqlConnector;
using System.Data;

namespace NoticeAPI.Services
{
    public class MariaDbConnectionFactory : IDbConnectionFactory
    {
        private readonly string _connectionString;
        public MariaDbConnectionFactory(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("conn");
        }
        public IDbConnection CreateConnection()
        {
            return new MySqlConnection(_connectionString);
        }
    }
}
