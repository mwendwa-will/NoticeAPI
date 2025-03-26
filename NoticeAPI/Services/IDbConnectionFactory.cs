using System.Data;

namespace NoticeAPI.Services
{
    public interface IDbConnectionFactory
    {
        IDbConnection CreateConnection();
    }
}
