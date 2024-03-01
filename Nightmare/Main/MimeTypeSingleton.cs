using Microsoft.AspNetCore.StaticFiles;
using Microsoft.Extensions.Options;

namespace Nightmare.Main {
  public class MimeTypeSingleton {
    private readonly IContentTypeProvider _contentTypeProvider;

    public MimeTypeSingleton(IOptions<StaticFileOptions> options) {
      _contentTypeProvider = options.Value.ContentTypeProvider;
      //_contentTypeProvider = new FileExtensionContentTypeProvider();
    }

    public bool TryGetName(string extension, out string? mimeType) {
      return _contentTypeProvider.TryGetContentType(extension, out mimeType);
    }
  }
}
