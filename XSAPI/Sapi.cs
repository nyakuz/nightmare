namespace XSAPI {
  public abstract class Sapi {
    public enum SapiType {
      None, Page, Link
    }

    public SapiType Type { get; set; }
    public string VhostDirectory = string.Empty;
    public string VhostFilePath = string.Empty;
    public string VhostFullPath = string.Empty;

    public abstract string VirtualHostName { get; }
    public abstract string RequestHost { get; }
    public abstract string RequestPath { get; }

    public void GetVhostPath(out string vhost_directory, out string vhost_filepath, out string vhost_fullpath) {
      vhost_directory = VhostDirectory;
      vhost_filepath = VhostFilePath;
      vhost_fullpath = VhostFullPath;
    }
    public void GetVhostPath(out int vhost_filename_index, out string vhost_fullpath) {
      vhost_filename_index = VhostDirectory.Length;
      vhost_fullpath = VhostFullPath;
    }
    public void SetVhostPath(string vhost_directory, string vhost_filepath, string vhost_fullpath) {
      VhostDirectory = vhost_directory;
      VhostFilePath = vhost_filepath;
      VhostFullPath = vhost_fullpath;
    }
  }
}