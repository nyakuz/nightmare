using Microsoft.AspNetCore.Mvc;

namespace Nightmare.Controllers {
  [Route("api/[controller]", Name = "test")]
  [ApiController]
  public class TestController: ControllerBase {
    public async Task<IActionResult> Index(IMysqlService mysql) {
      using var conn = await mysql.Connect();
      using var com = conn.CreateCommand();

      com.CommandText = "SELECT now()";
      using var reader = com.ExecuteReader();
      await reader.ReadAsync();

      return Ok(reader.GetDateTime(0));
    }
  }
}
