using System.Collections.Generic;

namespace XClaw
{
  
  public class XCommandResult
  {
    /// <summary>
    /// The command used to create this result
    /// </summary>
    public XPathCommand CommandExecuted { get; set; }
    
    /// <summary>
    /// JSON: "useSingleNode: true"
    ///   DEFAULT:  false
    /// 
    /// If a single result was requested AND the query was successful then this
    /// will contain the result
    /// </summary>
    public string SingleResult { get; set; }
    
    /// <summary>
    /// JSON: "useSingleNode: false"
    ///   DEFAULT:  false
    ///
    /// [DEFAULT]
    /// Returns a list of all the matches from the XPath query, except in cases
    /// where the query matches none
    /// </summary>
    public IList< string > MultiResult { get; set; }
    
    /// <summary>
    /// Indicates whether the query result produced any failures.
    /// </summary>
    public bool Success { get; set; }
  }

}