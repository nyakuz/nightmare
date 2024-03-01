using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

public class SharpHello {
	public Task Page(HttpContext context) {
		return context.Response.WriteAsync("Hello World!");
	}
}