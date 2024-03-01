using Microsoft.AspNetCore.DataProtection;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;

public interface ISecureService {
  public void SaveFromJsonString(IResponseCookies cookie, string value);
  public string LoadToJsonString(IRequestCookieCollection cookie);
  public void Save<T>(IResponseCookies cookie, T data);
  public void Save(IResponseCookies cookie, Dictionary<string, dynamic> data);
  public Dictionary<string, dynamic> Load(IRequestCookieCollection cookie);
  public bool Get(IRequestCookieCollection cookie, string property_name, out dynamic value);
  public void Delete(IResponseCookies cookie);
}

public class SecureService: ISecureService {
  public const string CookieName = "e";
  public const string Sess = ".sess";
  public const string CSRF = ".anti";
  public const string XSRF = "xsrf";
  public byte[] salt;
  private IDataProtector _protector;

  public SecureService(IConfiguration config, IDataProtectionProvider dataProtectionProvider) {
    var key = config.GetConnectionString("SessionKey")!;
    salt = Convert.FromHexString(key);
    _protector = dataProtectionProvider.CreateProtector("a");
  }

  public void SaveFromJsonString(IResponseCookies cookie, string value) {
    var enc_text = Convert.ToBase64String(_protector.Protect(Encoding.UTF8.GetBytes(value)));
    cookie.Append(CookieName, enc_text);
  }

  public string LoadToJsonString(IRequestCookieCollection cookie) {
    if(cookie.TryGetValue(CookieName, out var e) == true) {
      return Encoding.UTF8.GetString(_protector.Unprotect(Convert.FromBase64String(e)));
    }

    return string.Empty;
  }

  public void Save(IResponseCookies cookie, Dictionary<string, dynamic> data) {
    Save<Dictionary<string, dynamic>>(cookie, data);
  }
  public void Save<T>(IResponseCookies cookie, T data) {
    var option = new CookieOptions() {
      Secure = true,
      HttpOnly = true
    };

    var enc_data = JsonSerializer.SerializeToUtf8Bytes(data, new JsonSerializerOptions() {
      Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
      WriteIndented = true
    });

    var enc_text = Convert.ToBase64String(_protector.Protect(enc_data));
    cookie.Append(CookieName, enc_text, option) ;
  }

  public Dictionary<string, dynamic> Load(IRequestCookieCollection cookie) {
    Dictionary<string, dynamic> pairs = new();
    if(cookie.TryGetValue(CookieName, out var enc_text) == true) { 
      var dec_data = _protector.Unprotect(Convert.FromBase64String(enc_text));
      var reader = new Utf8JsonReader(dec_data);
      var property_name = string.Empty;

      while(reader.Read()) {
        switch(reader.TokenType) {
        case JsonTokenType.PropertyName:
          property_name = reader.GetString()!;
          break;
        case JsonTokenType.String:
          pairs[property_name] = reader.GetString()!;
          break;
        case JsonTokenType.Number:
          if(reader.TryGetInt64(out var i64) == true) {
            pairs[property_name] = i64;
          } else if(reader.TryGetDecimal(out var d) == true) {
            pairs[property_name] = d;
          }
          break;
        }
      }
    }

    return pairs;
  }
  public bool Get(IRequestCookieCollection cookie, string property_name, out dynamic value) {
    if(cookie.TryGetValue(CookieName, out var enc_text) == true) {
      var dec_data = _protector.Unprotect(Convert.FromBase64String(enc_text));
      var reader = new Utf8JsonReader(dec_data);

      while(reader.Read()) {
        switch(reader.TokenType) {
        case JsonTokenType.PropertyName:
          if(reader.GetString() == property_name) {
            while(reader.Read()) {
              switch(reader.TokenType) {
              case JsonTokenType.String:
                var tmp = reader.GetString();

                if(tmp != null) {
                  value = tmp;
                  return true;
                }

                value = string.Empty;
                return false;
              case JsonTokenType.Number:
                if(reader.TryGetInt64(out var i64) == true) {
                  value = i64;
                  return true;
                } else if(reader.TryGetDecimal(out var d) == true) {
                  value = d;
                  return true;
                }

                value = 0;
                return false;
              }
            }
          }
          break;
        }
      }
    }

    value = 0;
    return false;
  }
  public void Delete(IResponseCookies cookie) {
    cookie.Delete(CookieName);
    cookie.Append(CookieName, string.Empty, new CookieOptions() {
      Expires = DateTime.UnixEpoch,
      MaxAge = TimeSpan.Zero
    });
  }
}