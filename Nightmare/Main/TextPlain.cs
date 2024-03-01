using Microsoft.AspNetCore.Mvc.Formatters;

namespace Nightmare.Main {
  public class TextPlain: InputFormatter {
    public const string Name = "text/plain";
    public TextPlain() {
      SupportedMediaTypes.Add(Name);
    }
    public override async Task<InputFormatterResult> ReadRequestBodyAsync(InputFormatterContext context) {
      try {
        using(var reader = new StreamReader(context.HttpContext.Request.Body)) {
          var text = await reader.ReadToEndAsync();
          return await InputFormatterResult.SuccessAsync(text);
        }
      } catch {
        return await InputFormatterResult.FailureAsync();
      }
    }
  }
}
