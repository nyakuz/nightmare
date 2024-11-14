using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

public class SharpTestc {
    public async ValueTask Page(HttpResponse response) {
        await Task.Delay(5000);
        await response.WriteAsync("1");
    }
}