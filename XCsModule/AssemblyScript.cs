using System.Reflection;
using System.Runtime.Loader;

namespace XCsModule {
  public class AssemblyScript : AssemblyLoadContext {
    private readonly AssemblyDependencyResolver _resolver;
    public AssemblyScript() : base(true) {
      var app_path = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
      var netcore_path = Path.GetDirectoryName(typeof(GC).Assembly.Location);
      var aspnetcore_path = netcore_path!.Replace("Microsoft.NETCore", "Microsoft.AspNetCore");

      _resolver = new AssemblyDependencyResolver(app_path);
    }

    protected override Assembly? Load(AssemblyName assemblyName) {
      var assemblyPath = _resolver.ResolveAssemblyToPath(assemblyName);
      if(assemblyPath != null) {
        return LoadFromAssemblyPath(assemblyPath);
      }

      return null;
    }

    protected override IntPtr LoadUnmanagedDll(string unmanagedDllName) {

      return IntPtr.Zero;
    }
  }
}
