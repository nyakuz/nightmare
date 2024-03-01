using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using System.Reflection;
using System.Reflection.PortableExecutable;

namespace XCsModule {
  public class ScriptBuilder {
    public readonly CSharpCompilationOptions Option;
    public readonly List<PortableExecutableReference> Reference = new();

    public static CSharpCompilation Create(ScriptContext script_context, CSharpCompilation compilation, SyntaxTree syntax_tree) {
      var model = compilation.GetSemanticModel(syntax_tree);
      var reference = new List<PortableExecutableReference>();

      foreach(var node in syntax_tree.GetRoot().DescendantNodes().OfType<IdentifierNameSyntax>()) {
        var symbol = model.GetSymbolInfo(node).Symbol;
        if(symbol == null) {
          var typename = node.ToString();

          if(script_context.TryGetValue(typename, out var item) == true) {
            reference.Add(item.Metadata.GetReference());
          }
        }
      }

      return compilation.AddReferences(reference);
    }


    public ScriptBuilder(CSharpCompilationOptions option, IEnumerable<MetadataReference> reference, params string[] reference_path) {
      Option = option;

      foreach(var metadata in reference) {
        Reference.Add((PortableExecutableReference)metadata);
      }

      for(var i = 0; i < reference_path.Length; i++) {
        foreach(var file in new DirectoryInfo(reference_path[i]).EnumerateFiles("*.dll")) {
          Reference.Add(MetadataReference.CreateFromFile(file.FullName));
        }
      }
    }

    public IEnumerable<MetadataReference> EnumReferences(ScriptContext script_context) {
      foreach(var metadata in Reference) {
        yield return metadata;
      }

      foreach(var (_, script_reference) in script_context.AsEnumerable()) {
        yield return script_reference.Metadata.GetReference();
      }
    }

    public bool Build(ScriptContext script_context, string script_filepath, out AssemblyScript? script, out AssemblyMetadata? metadata, out Assembly? assembly) {
      using var fs = File.OpenRead(script_filepath);

      var context = new AssemblyScript();
      var filename = Path.GetFileNameWithoutExtension(script_filepath);
      var syntax_tree = CSharpSyntaxTree.ParseText(SourceText.From(fs), path: script_filepath);

      var compilation = CSharpCompilation.Create(
        filename,
        new[] { syntax_tree },
        EnumReferences(script_context),
        Option
      );

      using var ms = new MemoryStream();
      var result = Create(script_context, compilation, syntax_tree).Emit(ms);

      if(result.Success == true) {
        script = context;

        ms.Position = 0;
        assembly = context.LoadFromStream(ms);

        ms.Position = 0;
        metadata = AssemblyMetadata.CreateFromStream(ms, PEStreamOptions.LeaveOpen | PEStreamOptions.PrefetchMetadata);

        return true;
      } else {
        script = null;
        assembly = null;
        metadata = null;

        foreach(var msg in result.Diagnostics) {
          Console.Error.WriteLine("{0} {1} {2}", script_filepath, msg.Id, msg.GetMessage());
        }
      }

      return false;
    }
  }
}
