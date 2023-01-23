using System.Collections.Generic;

namespace DocStore.Driver.Configs
{

  /// <summary>
  /// Configuration for a source
  /// </summary>
  public class SourceConfig
  {
    /// <summary>
    /// File of the XPath Query JSON Config 
    /// </summary>
    public string XCmdConfigFile { get; set; }
    
    /// <summary>
    /// As provided by the database
    /// </summary>
    public int SourceId { get; set; }
    
    /// <summary>
    /// Name of the source, e.g., Super Express
    /// </summary>
    public string SourceName { get; set; }
    
    /// <summary>
    /// Site of the landing page
    /// </summary>
    public string LandingUrl { get; set; }
    
    /// <summary>
    /// Any prefix required for the Urls to be properly formatted, e.g., https:// or
    /// https://mywebsite
    /// </summary>
    public string UrlPrefix { get; set; }
    
    /// <summary>
    /// Any postfix, which is mostly used for xform elements, e.g., ?include=default
    /// </summary>
    public string UrlPostfix { get; set; }
    
    /// <summary>
    /// Indicates whether or not to skip this source
    /// </summary>
    public bool Skip { get; set; }
    
    /// <summary>
    /// The minimum wait between fetches in milliseconds
    /// </summary>
    public int MinWaitInMs { get; set; }
    
    /// <summary>
    /// The maximum wait between fetches in milliseconds
    /// </summary>
    public int MaxWaitInMs { get; set; }
    
    /// <summary>
    /// Defines the type of data to expect
    /// </summary>
    public DataFormatConfig DataFormats { get; set; }
    
    /// <summary>
    /// Defines the locations of the requester config required for a
    /// certain type of content, e.g., landing
    /// </summary>
    public IDictionary< string, string > Requesters { get; set; }
  }
}