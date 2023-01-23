using System.Collections.Generic;

namespace DocStore.Driver.Configs
{
  public class BackendConfig
  {
    public string Connection { get; set; }
    public string Database { get; set; }
    public string Table { get; set; }
    
    public IDictionary< string, string > Queries { get; set; }
  }
}