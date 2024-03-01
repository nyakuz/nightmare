using Microsoft.AspNetCore.SignalR;
using System.Text.Json;
using XSAPI;

namespace Nightmare.Main {
  public interface IMainHub {
    public Task O(string method);
    public Task P(string method, dynamic arg0);
    public Task Q(string method, dynamic arg0, dynamic arg1);
  }

  public class MainHub: Hub<IMainHub>, IHub {
    public class HubSapiContext: HubSapi {
      public readonly string RequestHostOverride;
      public readonly string RequestPathOverride;
      public readonly MainRouter Router;
      public string VhostName = string.Empty;
      public override string VirtualHostName { get => VhostName; }
      public override string RequestHost { get => RequestHostOverride; }
      public override string RequestPath { get => RequestPathOverride; }

      public HubSapiContext(string request_host, string request_path, MainRouter router) {
        RequestHostOverride = request_host;
        RequestPathOverride = request_path;
        VhostName = router.FindVirtualHost(request_host);
        Router = router;
      }
    }

    public HubSapiContext Sapi { get => (HubSapiContext)Context.Items["SAPI"]!; set => Context.Items["SAPI"] = value; }
    public string VirtualHostName { get => Sapi.VirtualHostName; }
    public string RequestPath { get => Sapi.RequestPath; }
    public dynamic? GetProperty(Type method_type) {
      var type = Context.GetType();

      if(method_type.IsAssignableFrom(type) == true) {
        return Context;
      }

      foreach(var prop in type.GetProperties()) {
        if(method_type.IsAssignableFrom(prop.PropertyType) == true) {
          return prop.GetValue(Context);
        }
      }

      return null;
    }
    public dynamic? GetService(Type method_type) {
      var http_context = Context.GetHttpContext()!;

      var type = http_context.GetType();

      if(method_type.IsAssignableFrom(type) == true) {
        return http_context;
      }

      foreach(var prop in type.GetProperties()) {
        if(method_type.IsAssignableFrom(prop.PropertyType) == true) {
          return prop.GetValue(http_context);
        }
      }

      return http_context.RequestServices.GetService(method_type);
    }

    public Dictionary<string, dynamic> GetAllSecureCookie() {
      var http_context = Context.GetHttpContext()!;
      var secure = (ISecureService)http_context.RequestServices.GetService(typeof(ISecureService))!;

      return secure.Load(http_context.Request.Cookies);
    }
    public dynamic GetSecureCookie(string property_name) {
      var http_context = Context.GetHttpContext()!;
      var secure = (ISecureService)http_context.RequestServices.GetService(typeof(ISecureService))!;

      secure.Get(http_context.Request.Cookies, property_name, out var value);
      return value;
    }
    public void SetSecureCookie(Dictionary<string, dynamic> data) {
      var http_context = Context.GetHttpContext()!;
      var secure = (ISecureService)http_context.RequestServices.GetService(typeof(ISecureService))!;

      secure.Save(http_context.Response.Cookies, data);
    }
    public void DeleteSecureCookie() {
      var http_context = Context.GetHttpContext()!;
      var secure = (ISecureService)http_context.RequestServices.GetService(typeof(ISecureService))!;

      secure.Delete(http_context.Response.Cookies);
    }


    public override async Task OnConnectedAsync() {
      string request_path = "/hub";
      var context = Context.GetHttpContext()!;
      var request = context.Request;
      var router = context.RequestServices.GetRequiredService<MainRouter>();

      if(request.Query.TryGetValue("path", out var value) == true && string.IsNullOrEmpty(value) == false) {
        request_path = "/" + value!;
      }

      Sapi = new HubSapiContext(request.Host.Host, request_path, router);
      var status_code = await router.InvokeHubConnected(this, Sapi, request_path);

      switch(status_code) {
      case 0:
        await base.OnConnectedAsync();
        break;
      default:
        Context.Abort();
        return;
      }
    }
    public override async Task OnDisconnectedAsync(Exception? exception) {
      await Sapi.Router.InvokeHubDisconnected(this, Sapi, Sapi.RequestPathOverride);
      await base.OnDisconnectedAsync(exception);
    }

    public void SendCaller(string method, params object[] arg) {
      switch(arg.Length) {
      case 0:
        Clients.Caller.O(method);
        break;
      case 1:
        Clients.Caller.P(method, arg[0]);
        break;
      case 2:
        Clients.Caller.Q(method, arg[0], arg[1]);
        break;
      }
    }
    public void SendClient(string connection_id, string method, params object[] arg) {
      switch(arg.Length) {
      case 0:
        Clients.Client(connection_id).O(method);
        break;
      case 1:
        Clients.Client(connection_id).P(method, arg[0]);
        break;
      case 2:
        Clients.Client(connection_id).Q(method, arg[0], arg[1]);
        break;
      }
    }
    public Task<int> I(string method) {
      return Sapi.Router.InvokeHubDataReceived(this, Sapi.RequestPathOverride + "/" + method);
    }
    public Task<int> J(string method, JsonElement arg0) {
      return Sapi.Router.InvokeHubDataReceived(this, Sapi.RequestPathOverride + "/" + method, arg0.GetRawText().Trim('"'));
    }
    public Task<int> K(string method, JsonElement arg0, JsonElement arg1) {
      return Sapi.Router.InvokeHubDataReceived(this, Sapi.RequestPathOverride + "/" + method, arg0.GetRawText().Trim('"'), arg1.GetRawText().Trim('"'));
    }
  }
}