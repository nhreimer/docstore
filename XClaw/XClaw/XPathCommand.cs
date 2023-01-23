using System.Collections.Generic;

namespace XClaw
{
  public class XPathCommand
  {
    public string Query { get; set; }         // the actual xpath query
    public string Attribute { get; set; }     // do we need to access the attribute's value?
    public bool UseSingleNode { get; set; }   // attempt to return a list or a single item
    // public string ResultType { get; set; }    // uses a callback T Convert( string s )
    
    // public string Format { get; set; }  // used primarily for DateTime formats
    
    /// <summary>
    /// used for any custom options that you want to pass in.
    /// this is particularly useful for providing DateTime formats
    /// </summary>
    public IDictionary< string, string > Options { get; set; }

    // if the following are null then we won't bother using this as a test
    // these can be nested
    public XPathCommand OnSuccess { get; set; } // if the query above exists then this gets called
    
    public XPathCommand OnFailure { get; set; } // if the query above fails then this gets called

    public override string ToString()
    {
      return Attribute != null ? $"{Query}, {Attribute}" : Query;
    }
  }
}