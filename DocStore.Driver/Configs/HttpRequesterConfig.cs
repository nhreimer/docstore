using System.Collections.Generic;

namespace DocStore.Driver.Configs
{
  public class HttpRequesterConfig
  {
    public string Verb { get; set; }
    public IDictionary< string, string > Headers { get; set; }
    public string Body { get; set; }
    public string Format { get; set; }
  }
}