namespace XSAPI {
  public interface ISecureCookie {
    public Dictionary<string, dynamic> GetAllSecureCookie();
    public dynamic GetSecureCookie(string property_name);
    public void SetSecureCookie(Dictionary<string, dynamic> data);
    public void DeleteSecureCookie();
  }
}
