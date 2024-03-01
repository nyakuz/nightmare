using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

public class SharpQuery {
	public async ValueTask Page(HttpResponse response, IMysqlService mysql) {
		using var conn = await mysql.Connect();
		using var com = conn.CreateCommand();

		com.CommandText = "SELECT now()";
		using var reader = com.ExecuteReader();
		await reader.ReadAsync();

		await response.WriteAsync($"NOW: {reader.GetDateTime(0)}");
	}
}