using Microsoft.CodeAnalysis;
using System.Collections.Immutable;

namespace XCsModule {
  public class AssemblyResolver: MetadataReferenceResolver {
    public override bool Equals(object? other) {
      return false;
    }

    public override int GetHashCode() {
      return 0;
    }

    public override bool ResolveMissingAssemblies => base.ResolveMissingAssemblies;

    public override ImmutableArray<PortableExecutableReference> ResolveReference(string reference, string? baseFilePath, MetadataReferenceProperties properties) {
      return default;
    }

    public override PortableExecutableReference? ResolveMissingAssembly(MetadataReference definition, AssemblyIdentity referenceIdentity) {
      return base.ResolveMissingAssembly(definition, referenceIdentity);
    }
  }
}
