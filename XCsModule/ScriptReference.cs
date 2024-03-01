using Microsoft.CodeAnalysis;
using System.Reflection;
using XSAPI;

namespace XCsModule {
  public class ScriptReference {
    public enum ArgumentType {
      None,
      Sbyte, Byte, Short, Ushort,
      Int, Uint, Long, Ulong,
      Float, Double, Decimal,
      String
    }
    public enum FromType {
      None, Query, Route, Form, Body, Header
    }
    public enum ReturnType {
      None,
      Void,
      TaskVoid,
      ValueTaskVoid,
      Int,
      TaskInt,
      ValueTaskInt
    }

    public struct MethodParameter {
      public readonly MethodInfo? Method;
      public readonly ParameterInfo[]? Parameters;
      public readonly ReturnType Return;
      public readonly ArgumentType[] Arguments;
      public readonly string[] Names;
      public readonly FromType[] Froms;
      public MethodParameter(MethodInfo method) {
        var arguments = new List<ArgumentType>();
        var names = new List<string>();
        var froms = new List<FromType>();

        Method = method;
        Parameters = method.GetParameters();

        for(var i = 0; i < Parameters.Length; i++) {
          var pi = Parameters[i];

          switch(pi.ParameterType) {
          case Type t when t == typeof(sbyte):
            arguments.Add(ArgumentType.Sbyte);
            break;
          case Type t when t == typeof(byte):
            arguments.Add(ArgumentType.Byte);
            break;
          case Type t when t == typeof(short):
            arguments.Add(ArgumentType.Short);
            break;
          case Type t when t == typeof(ushort):
            arguments.Add(ArgumentType.Ushort);
            break;
          case Type t when t == typeof(int):
            arguments.Add(ArgumentType.Int);
            break;
          case Type t when t == typeof(uint):
            arguments.Add(ArgumentType.Uint);
            break;
          case Type t when t == typeof(long):
            arguments.Add(ArgumentType.Long);
            break;
          case Type t when t == typeof(ulong):
            arguments.Add(ArgumentType.Ulong);
            break;
          case Type t when t == typeof(float):
            arguments.Add(ArgumentType.Float);
            break;
          case Type t when t == typeof(double):
            arguments.Add(ArgumentType.Double);
            break;
          case Type t when t == typeof(decimal):
            arguments.Add(ArgumentType.Decimal);
            break;
          case Type t when t == typeof(string):
            arguments.Add(ArgumentType.String);
            break;
          default:
            if(arguments.Count != 0) {
              arguments.Add(ArgumentType.None);
            }
            break;
          }

          if(arguments.Count > 0) {
            var name = pi.Name;
            var from = FromType.None;

            foreach(var ca in pi.CustomAttributes) {
              switch(ca.AttributeType.Name) {
              case "FromQueryAttribute":
                from = FromType.Query;
                break;
              case "FromRouteAttribute":
                from = FromType.Route;
                break;
              case "FromFormAttribute":
                from = FromType.Form;
                break;
              case "FromBodyAttribute":
                from = FromType.Body;
                break;
              case "FromHeaderAttribute":
                from = FromType.Header;
                break;
              }

              foreach(var na in ca.NamedArguments) {
                switch(na.MemberName) {
                case "Name":
                  name = (string?)na.TypedValue.Value;
                  break;
                }
              }
            }

            names.Add(name ?? string.Empty);
            froms.Add(from);
          }
        }

        Arguments = arguments.ToArray();
        Names = names.ToArray();
        Froms = froms.ToArray();

        if(typeof(void).IsAssignableFrom(method.ReturnType) == true) {
          Return = ReturnType.Void;
        } else if(typeof(Task).IsAssignableFrom(method.ReturnType) == true) {
          Return = ReturnType.TaskVoid;
        } else if(typeof(ValueTask).IsAssignableFrom(method.ReturnType) == true) {
          Return = ReturnType.ValueTaskVoid;
        } else if(typeof(int).IsAssignableFrom(method.ReturnType) == true) {
          Return = ReturnType.Int;
        } else if(typeof(Task<int>).IsAssignableFrom(method.ReturnType) == true) {
          Return = ReturnType.TaskInt;
        } else if(typeof(ValueTask<int>).IsAssignableFrom(method.ReturnType) == true) {
          Return = ReturnType.ValueTaskInt;
        } else {
          Return = ReturnType.None;
        }
      }
    }

    public Type T;
    public readonly Assembly Assembly;
    public readonly AssemblyScript Script;
    public readonly AssemblyMetadata Metadata;
    public readonly MethodParameter MethodPage;
    public readonly MethodParameter MethodOnHubConnected;
    public readonly MethodParameter MethodOnHubDisconnected;
    public readonly MethodParameter MethodOnHubDataReceived;

    public ScriptReference(string typename, AssemblyScript script, AssemblyMetadata metadata, Assembly assembly) {
      Script = script;
      Metadata = metadata;
      Assembly = assembly;

      var type = assembly.GetType(typename);
      if(type == null) {
        foreach(var tmp in assembly.GetTypes()) {
          if(tmp.IsNested == true) continue;
          type = tmp;
        }
      }

      T = type!;

      foreach(var method in type!.GetMethods()) {
        switch(method.Name) {
        case "Page": MethodPage = new(method); break;
        case "OnHubConnected": MethodOnHubConnected = new(method); break;
        case "OnHubDisconnected": MethodOnHubDisconnected = new(method); break;
        case "OnHubDataReceived": MethodOnHubDataReceived = new(method); break;
        }
      }
    }
    ~ScriptReference() {
      Metadata.Dispose();
      Script.Unload();
    }

    private async ValueTask<dynamic?[]> FindParaneters(MethodParameter method_info, PageSapi sapi) {
      var arguments = new dynamic?[method_info.Parameters!.Length];
      var i = 0;

      for(; i < method_info.Parameters.Length - method_info.Names.Length; i++) {
        var parameter = method_info.Parameters[i];
        var type = parameter.ParameterType;
        dynamic? tmp = type.IsAssignableFrom(sapi.GetType()) == true ? sapi : null;

        if(tmp == null) tmp = sapi.GetProperty(type);
        if(tmp == null) tmp = sapi.GetService(type);
        
        arguments[i] = tmp;
      }

      for(var j = 0; j < method_info.Names.Length; i++, j++) {
        switch(method_info.Froms[j]) {
        case FromType.None:
        case FromType.Query:
          break;
        case FromType.Form:
          continue;
        case FromType.Body:
          arguments[i] = await sapi.ReadBodyTextPlain();
          continue;
        }

        var value = sapi.GetQuery(method_info.Names[j]);

        switch(method_info.Arguments[j]) {
        case ArgumentType.Sbyte:
          if(sbyte.TryParse(value, out sbyte sbyte_value) == true) {
            arguments[i] = sbyte_value;
            continue;
          }
          break;
        case ArgumentType.Byte:
          if(byte.TryParse(value, out byte byte_value) == true) {
            arguments[i] = byte_value;
            continue;
          }
          break;
        case ArgumentType.Ushort:
          if(ushort.TryParse(value, out ushort ushort_value) == true) {
            arguments[i] = ushort_value;
            continue;
          }
          break;
        case ArgumentType.Short:
          if(short.TryParse(value, out short short_value) == true) {
            arguments[i] = short_value;
            continue;
          }
          break;
        case ArgumentType.Uint:
          if(uint.TryParse(value, out uint uint_value) == true) {
            arguments[i] = uint_value;
            continue;
          }
          break;
        case ArgumentType.Int:
          if(int.TryParse(value, out int int_value) == true) {
            arguments[i] = int_value;
            continue;
          }
          break;
        case ArgumentType.Ulong:
          if(ulong.TryParse(value, out ulong ulong_value) == true) {
            arguments[i] = ulong_value;
            continue;
          }
          break;
        case ArgumentType.Long:
          if(long.TryParse(value, out long long_value) == true) {
            arguments[i] = long_value;
            continue;
          }
          break;
        case ArgumentType.Float:
          if(float.TryParse(value, out float float_value) == true) {
            arguments[i] = float_value;
            continue;
          }
          break;
        case ArgumentType.Double:
          if(double.TryParse(value, out double double_value) == true) {
            arguments[i] = double_value;
            continue;
          }
          break;
        case ArgumentType.Decimal:
          if(decimal.TryParse(value, out decimal decimal_value) == true) {
            arguments[i] = decimal_value;
            continue;
          }
          break;
        }

        arguments[i] = value;
      }

      return arguments;
    }

    private dynamic?[] FindHubParaneters(MethodParameter method_info, IHub hub) {
      var arguments = new dynamic?[method_info.Parameters!.Length];

      for(var i = 0; i < method_info.Parameters.Length; i++) {
        var type = method_info.Parameters[i].ParameterType;
        dynamic? tmp = type.IsAssignableFrom(hub.GetType()) == true ? hub : null;

        if(tmp == null) tmp = hub.GetProperty(type);
        if(tmp == null) tmp = hub.GetService(type);

        arguments[i] = tmp;
      }

      return arguments;
    }

    private dynamic?[] FindHubParaneters(MethodParameter method_info, IHub hub, params dynamic[] args) {
      var arguments = new dynamic?[method_info.Parameters!.Length];
      var i = 0;

      for(; i < method_info.Parameters.Length - args.Length; i++) {
        var type = method_info.Parameters[i].ParameterType;
        dynamic? tmp = type.IsAssignableFrom(hub.GetType()) == true ? hub : null;

        if(tmp == null) tmp = hub.GetProperty(type);
        if(tmp == null) tmp = hub.GetService(type);

        arguments[i] = tmp;
      }

      for(var j = 0; j < args.Length; i++, j++) {
        switch(method_info.Arguments[j]) {
        case ArgumentType.Sbyte:
          if(sbyte.TryParse(args[j], out sbyte sbyte_value) == true) {
            arguments[i] = sbyte_value;
            continue;
          }
          break;
        case ArgumentType.Byte:
          if(byte.TryParse(args[j], out byte byte_value) == true) {
            arguments[i] = byte_value;
            continue;
          }
          break;
        case ArgumentType.Ushort:
          if(ushort.TryParse(args[j], out ushort ushort_value) == true) {
            arguments[i] = ushort_value;
            continue;
          }
          break;
        case ArgumentType.Short:
          if(short.TryParse(args[j], out short short_value) == true) {
            arguments[i] = short_value;
            continue;
          }
          break;
        case ArgumentType.Uint:
          if(uint.TryParse(args[j], out uint uint_value) == true) {
            arguments[i] = uint_value;
            continue;
          }
          break;
        case ArgumentType.Int:
          if(int.TryParse(args[j], out int int_value) == true) {
            arguments[i] = int_value;
            continue;
          }
          break;
        case ArgumentType.Ulong:
          if(ulong.TryParse(args[j], out ulong ulong_value) == true) {
            arguments[i] = ulong_value;
            continue;
          }
          break;
        case ArgumentType.Long:
          if(long.TryParse(args[j], out long long_value) == true) {
            arguments[i] = long_value;
            continue;
          }
          break;
        case ArgumentType.Float:
          if(float.TryParse(args[j], out float float_value) == true) {
            arguments[i] = float_value;
            continue;
          }
          break;
        case ArgumentType.Double:
          if(double.TryParse(args[j], out double double_value) == true) {
            arguments[i] = double_value;
            continue;
          }
          break;
        case ArgumentType.Decimal:
          if(decimal.TryParse(args[j], out decimal decimal_value) == true) {
            arguments[i] = decimal_value;
            continue;
          }
          break;
        }

        arguments[i] = args[j];
      }

      return arguments;
    }

    private async ValueTask<int> InvokeScript(MethodParameter method_info, object? obj, params dynamic?[] arguments) {
      int result = 0;

      switch(method_info.Return) {
      case ReturnType.Void:
        method_info.Method!.Invoke(obj, arguments);
        break;
      case ReturnType.TaskVoid:
        await (Task)method_info.Method!.Invoke(obj, arguments)!;
        break;
      case ReturnType.ValueTaskVoid:
        await (ValueTask)method_info.Method!.Invoke(obj, arguments)!;
        break;
      case ReturnType.Int:
        result = (int)method_info.Method!.Invoke(obj, arguments)!;
        break;
      case ReturnType.TaskInt:
        result = await (Task<int>)method_info.Method!.Invoke(obj, arguments)!;
        break;
      case ReturnType.ValueTaskInt:
        result = await (ValueTask<int>)method_info.Method!.Invoke(obj, arguments)!;
        break;
      }

      return result;
    }

    public async ValueTask<int> InvokePage(PageSapi sapi) {
      try {
        var arguments = await FindParaneters(MethodPage, sapi);
        var obj = Activator.CreateInstance(T);
        return await InvokeScript(MethodPage, obj, arguments);
      } catch {
        return 500;
      }
    }

    public async ValueTask<int> InvokeHubConnected(IHub hub) {
      try {
        var arguments = FindHubParaneters(MethodOnHubConnected, hub);
        var obj = Activator.CreateInstance(T);
        return await InvokeScript(MethodOnHubConnected, obj, arguments);
      } catch {
        return 500;
      }
    }

    public async ValueTask<int> InvokeHubDisconnected(IHub hub) {
      try {
        var arguments = FindHubParaneters(MethodOnHubDisconnected, hub);
        var obj = Activator.CreateInstance(T);
        return await InvokeScript(MethodOnHubDisconnected, obj, arguments);
      } catch {
        return 500;
      }
    }

    public async ValueTask<int> InvokeHubDataReceived(IHub hub, params dynamic[] args) {
      try {
        var arguments = FindHubParaneters(MethodOnHubDataReceived, hub, args);
        var obj = Activator.CreateInstance(T);
        return await InvokeScript(MethodOnHubDataReceived, obj, arguments);
      } catch {
        return 500;
      }
    }
  }
}
