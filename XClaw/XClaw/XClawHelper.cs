using System.IO;
using System.Collections.Generic;

using Newtonsoft.Json;

namespace XClaw
{
  using XClawConfig = IDictionary< string, IList< XPathCommand > >;
  
  public static class XClawHelper 
  {
    private static NLog.Logger _logger = NLog.LogManager.GetCurrentClassLogger();

    public delegate string DPreprocessor( string inText );

    /// <summary>
    /// 
    /// </summary>
    /// <param name="json">The JSON as text</param>
    /// <returns></returns>
    public static XClawConfig DeserializeCommands( string json )
    {
      return JsonConvert.DeserializeObject< XClawConfig >( json );
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="jsonFileName">JSON file</param>
    /// <returns></returns>
    public static XClawConfig DeserializeFromFile( string jsonFileName )
    {
      var json = File.ReadAllText( jsonFileName );
      return DeserializeCommands( json );
    }

    /// <summary>
    /// Reduces command results down to a list of strings
    /// </summary>
    /// <param name="commandResults"></param>
    /// <returns></returns>
    public static IList< string > ReduceResults(
      IEnumerable< XCommandResult > commandResults )
    {
      return ReduceResults( commandResults, RunEmptyCallback );
    }
    
    /// <summary>
    /// Reduces command results down to a list of strings
    /// </summary>
    /// <param name="commandResults"></param>
    /// <param name="preprocess">A callback to preprocess text prior to adding to list</param>
    /// <returns></returns>
    public static IList< string > ReduceResults( 
      IEnumerable< XCommandResult > commandResults,
      DPreprocessor preprocess )
    {
      var result = new List< string >();

      foreach ( var commandResult in commandResults )
      {
        if ( commandResult.Success )
        {
          if ( !string.IsNullOrEmpty( commandResult.SingleResult ) )
            result.Add( preprocess( commandResult.SingleResult ) );
          else if ( commandResult.MultiResult != null )
          {
            foreach ( var subresult in commandResult.MultiResult )
              result.Add( preprocess( subresult ) );
          }
        }
        else
          _logger.Warn( $"query failed to produce results: {commandResult.CommandExecuted.Query}" );
      }
      
      return result;
    }

    private static string RunEmptyCallback( string inText )
    {
      return inText;
    }
  }
}