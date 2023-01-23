using System;
using System.Collections.Generic;
using System.IO;

namespace DocStore.Driver.Configs
{
  public class ErrorConfig
  {
    /// <summary>
    /// Dump the contents on failure
    /// </summary>
    public bool DumpOnQueryFailure { get; set; }
    
    /// <summary>
    /// The directory to dump problem children to
    /// </summary>
    public string DumpDirectory { get; set; }
    
    /// <summary>
    /// The query failures that prompt content dumping
    /// </summary>
    public IDictionary< string, bool > DumpOnQueryTypes { get; set; }
  }
}