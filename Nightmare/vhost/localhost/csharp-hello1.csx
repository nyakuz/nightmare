using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

public class SharpHello1 {
  public async ValueTask Page(HttpResponse response) {
    await response.WriteAsync("Example 2");
  }
}