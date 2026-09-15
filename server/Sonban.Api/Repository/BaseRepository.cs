using System.Data;
using System.Data.SqlClient;
using Sonban.Api.Classes;

namespace Sonban.Api.Repository
{
    public abstract class BaseRepository {

        private readonly string connectionString;
        private SqlConnection connection;

        protected BaseRepository(ISettingsProvider settingsProvider) {
            connectionString = settingsProvider.GetValue<string>("MainDb");
        }

        protected IDbConnection GetConnection() {
            if (connection == null)
                connection = new SqlConnection(connectionString);
            return connection;
        }
    }
}
