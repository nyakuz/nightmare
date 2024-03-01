using Nightmare.Main;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.AspNetCore.DataProtection;


AppDomain.CurrentDomain.UnhandledException += (object sender, UnhandledExceptionEventArgs args) => {
  var exception = (Exception)args.ExceptionObject;
  Console.WriteLine($"Unhandled exception occurred: {exception.Message}");
};

var builder = WebApplication.CreateBuilder(new WebApplicationOptions() {
  Args = args,
  ContentRootPath = Directory.GetCurrentDirectory(),
  WebRootPath = Directory.GetCurrentDirectory() + "/wwwroot"
});
Directory.SetCurrentDirectory(Directory.GetCurrentDirectory() + "/vhost/null");


var appsettings = builder.Configuration.AddJsonFile("/app/kestrel.json", false, true).Build();
appsettings.Reload();
builder.WebHost.ConfigureKestrel((context, options) => {
  options.AddServerHeader = appsettings.GetValue("Kestrel:AddServerHeader",false);
  options.Limits.MaxRequestBodySize = appsettings.GetValue("Kestrel:Limits:MaxRequestBodySize", 104857600); //100mb
  
  context.Configuration["Kestrel:Endpoints:Http:Url"] = appsettings["Kestrel:Endpoints:Http:Url"];
  context.Configuration["Kestrel:Endpoints:Https:Url"] = appsettings["Kestrel:Endpoints:Https:Url"];
});

builder.Services.AddSignalR();
builder.Services.AddHttpContextAccessor();
// builder.Services.AddResponseCaching();
builder.Services.AddResponseCompression(options => {
  options.EnableForHttps = true;
  options.Providers.Add<BrotliCompressionProvider>();
  options.Providers.Add<GzipCompressionProvider>();
  options.MimeTypes = ResponseCompressionDefaults.MimeTypes.Concat(new[] {
    "application/octet-stream"
  });
});
builder.Services.Configure<GzipCompressionProviderOptions>(options => {
  options.Level = System.IO.Compression.CompressionLevel.Fastest;
  /* IMPORTANT: The following code block has a significant OOM memory issue.
    options.Level = System.IO.Compression.CompressionLevel.Optimal;
    options.Level = System.IO.Compression.CompressionLevel.SmallestSize;
  */
});
builder.Services.Configure<BrotliCompressionProviderOptions>(options => {
  options.Level = System.IO.Compression.CompressionLevel.Fastest;
  /* IMPORTANT: The following code block has a significant OOM memory issue.
    options.Level = System.IO.Compression.CompressionLevel.Optimal;
    options.Level = System.IO.Compression.CompressionLevel.SmallestSize;
  */
});
builder.Services.Configure<StaticFileOptions>(options => {
  options.ServeUnknownFileTypes = true;
  options.ContentTypeProvider = new FileExtensionContentTypeProvider();
  options.DefaultContentType = "text/plain";
});

builder.Services.AddSession(options => {
  options.Cookie.Name = SecureService.Sess;
  options.IdleTimeout = TimeSpan.FromSeconds(10);
  options.Cookie.HttpOnly = true;
  options.Cookie.IsEssential = true;
});
builder.Services.AddDataProtection().UseCryptographicAlgorithms(new());
builder.Services.AddAntiforgery(options => {
  options.Cookie.Name = SecureService.CSRF;
  options.HeaderName = SecureService.XSRF;
});
builder.Services.AddControllers(options => {
  options.InputFormatters.Add(new TextPlain());
});
builder.Services.AddSingleton<MimeTypeSingleton>();
builder.Services.AddSingleton(new MainRouter(builder.Services, builder.Environment.ContentRootPath));
builder.Services.AddScoped<ISecureService, SecureService>();

builder.Services.AddControllersWithViews();
#if DEBUG
builder.Services.AddRazorPages().AddRazorRuntimeCompilation();
#else
  builder.Services.AddRazorPages();
#endif


var app = builder.Build();

// app.UseResponseCaching();
app.UseResponseCompression();
app.UseSession();
app.UseStaticFiles();
app.UseMiddleware<MainMiddleware>();


// The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
#if DEBUG
app.UseDeveloperExceptionPage();
#else
  app.UseExceptionHandler("/Error");
  app.UseHsts();
#endif


app.UseRouting();
app.MapControllers();
app.MapRazorPages();
app.MapHub<MainHub>("/~", options => {
  options.Transports =
    Microsoft.AspNetCore.Http.Connections.HttpTransportType.ServerSentEvents |
    Microsoft.AspNetCore.Http.Connections.HttpTransportType.LongPolling;
});


app.Run();