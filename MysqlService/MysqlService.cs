using Microsoft.Extensions.Configuration;
using MySql.Data.MySqlClient;
using System.Data;

public interface IMysqlService: IAsyncDisposable {
  public ValueTask<MySqlConnection> Connect(string connection_string_key = "");
  public ValueTask<MySqlConnection> Connect(CancellationToken cancellation_token, string connection_string_key = "");
}

public class MysqlService: IMysqlService {
  private readonly MySqlConnection Connection;
  private readonly IConfiguration Config;

  public MysqlService(IConfiguration config) {
    Config = config;
    Connection = new MySqlConnection();
  }

  public ValueTask DisposeAsync() {
    return Connection.DisposeAsync();
  }

  public async ValueTask<MySqlConnection> Connect(string connection_string_key = "") {
    if(Connection.State != ConnectionState.Open) {
      if(connection_string_key == string.Empty) connection_string_key = "MysqlConnection";
      Connection.ConnectionString = Config.GetConnectionString(connection_string_key)!;
      await Connection.OpenAsync();
    }

    return Connection;
  }

  public async ValueTask<MySqlConnection> Connect(CancellationToken cancellation_token, string connection_string_key = "") {
    if(Connection.State != ConnectionState.Open) {
      if(connection_string_key == string.Empty) connection_string_key = "MysqlConnection";
      Connection.ConnectionString = Config.GetConnectionString(connection_string_key)!;
      await Connection.OpenAsync(cancellation_token);
    }

    return Connection;
  }
}