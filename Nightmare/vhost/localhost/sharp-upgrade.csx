using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Nightmare.Main;

namespace Nightmare.vhost.localhost {
	public class SharpUpgrade {
		public const int BUFFER_SIZE = 4096;

		public async ValueTask Page(HttpContext context, HttpRequest request, HttpResponse response, IFeatureCollection feature, ISecureService session) {
			var upgrade = feature.Get<IHttpUpgradeFeature>()!;

			if(upgrade != null && upgrade.IsUpgradableRequest == true) {
				switch(request.Headers.Upgrade) {
				case "a/1":
					var user_name = session.Load(request.Cookies);

					
					break;
				}
			}
		}
	}
}
