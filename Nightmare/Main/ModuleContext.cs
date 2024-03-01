using System.Runtime.Loader;
using XSAPI;

namespace Nightmare.Main {
  public class ModuleContext : Dictionary<string, ModuleContext.ContextItem> {
    public class ContextItem : AssemblyLoadContext {
      public Module AssemblyModule;
      
      public ContextItem(string assembly_path) : base(true) {
        var module_type = typeof(Module);
        var assembly = LoadFromAssemblyPath(assembly_path);
        Module? module = null;

        foreach(var type in assembly.GetTypes()) {
          if(module_type.IsAssignableFrom(type)) {
            module = (Module?)Activator.CreateInstance(type);
            break;
          }
        }

        AssemblyModule = module!;
      }
      ~ContextItem() {
        Unload();
      }
    }

    public Module LoadFromAssemblyPath(string assembly_name, string assembly_path) {
      var context_item = new ContextItem(assembly_path);

      Add(assembly_name, context_item);

      return context_item.AssemblyModule;
    }
  }
}
