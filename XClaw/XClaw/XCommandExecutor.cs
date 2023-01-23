using System.Web;
using System.Text;
using System.Collections.Generic;

using HtmlAgilityPack;

namespace XClaw
{
  public class XCommandExecutor
  {
    private static NLog.Logger _logger = NLog.LogManager.GetCurrentClassLogger();
    
    public XCommandExecutor( HtmlDocument htmlDocument )
    {
      Document = htmlDocument;
    }

    public HtmlDocument Document { get; private set; }
    
    /// <summary>
    /// Executes all group of commands. returns a list of each command result
    /// </summary>
    /// <param name="commands"></param>
    /// <returns>a list of each command result</returns>
    public IList< XCommandResult > ExecuteCommandBlock( 
      IEnumerable< XPathCommand > commands )
    {
      var result = new List< XCommandResult >();
      
      foreach ( var command in commands )
        result.Add( Execute( command ) );

      return result;
    }

    /// <summary>
    /// Executes a single command gets its result
    /// </summary>
    /// <param name="command"></param>
    /// <returns></returns>
    public XCommandResult Execute( XPathCommand command )
    {
      return ExecuteCommand( FindFinalCommand( command ) );
    }

    /// <summary>
    /// Executes a block of commands, aggregating them into a single string result
    /// </summary>
    /// <param name="commands"></param>
    /// <returns>string</returns>
    public string AggregateCommandBlock( IEnumerable< XPathCommand > commands )
    {
      var result = new StringBuilder();

      foreach ( var command in commands )
      {
        var commandResult = Execute( command );
        if ( !string.IsNullOrEmpty( commandResult.SingleResult ) )
          result.Append( commandResult.SingleResult );
        else if ( commandResult.MultiResult != null )
          result.AppendJoin( ' ', commandResult.MultiResult );
      }

      return result.ToString();
    }
    
    /// <summary>
    /// Executes this command, regardless of its success/failure branching options.
    /// Ensure that this runs on the final command in the tree.
    /// </summary>
    /// <param name="command"></param>
    /// <returns></returns>
    private XCommandResult ExecuteCommand( XPathCommand command )
    {
      XCommandResult result = null;

      if ( command != null )
      {
        result = ( command.UseSingleNode )
          ? ExecuteSingleNode( command )
          : ExecuteMultiNode( command );

        // the result failed due to a query problem NOT due to an exception,
        // so we want to indicate that the query failed
        if ( result == null )
        {
          result = new XCommandResult
          {
            CommandExecuted = command,
            Success = false
          };
        }
      }
      else
        _logger.Warn( "command led to null command" );

      return result;
    }

    /// <summary>
    /// Collects data across multiple nodes based on the command
    /// </summary>
    /// <param name="command"></param>
    /// <returns></returns>
    private XCommandResult ExecuteMultiNode( XPathCommand command )
    {
      XCommandResult result = null;
      var nodes = Document.DocumentNode.SelectNodes( command.Query );

      if ( nodes != null )
      {
        result = new XCommandResult
        {
          MultiResult = new List< string >(),
          Success = true
        };

        foreach ( var node in nodes )
        {
          var data = GetValue( node, command.Attribute );
          if ( data != null )
            result.MultiResult.Add( data );
        }
      }

      return result;
    }
   
    /// <summary>
    /// Collects data on a single node based on the command
    /// </summary>
    /// <param name="command"></param>
    /// <returns></returns>
    private XCommandResult ExecuteSingleNode( XPathCommand command )
    {
      XCommandResult result = null;
      var node = Document.DocumentNode.SelectSingleNode( command.Query );
      if ( node != null )
      {
        var value = GetValue( node, command.Attribute );
        result = new XCommandResult
        {
          CommandExecuted = command,
          Success = true,
          SingleResult = value
        };
      }
      return result;
    }

    /// <summary>
    /// Gets either the requested attribute value or the innerText of the html node
    /// </summary>
    /// <param name="htmlNode"></param>
    /// <param name="attribute"></param>
    /// <returns>null if unsuccessful, otherwise string</returns>
    private static string GetValue( HtmlNode htmlNode, string attribute )
    {
      if ( !string.IsNullOrEmpty( attribute ) )
      {
        if ( htmlNode.HasAttributes && htmlNode.Attributes.Contains( attribute ) )
          return HttpUtility.HtmlDecode( htmlNode.Attributes[ attribute ].Value );
        
        _logger.Error( $"attribute `{attribute}` not found. command will result in failure." );
        return null;
      }

      return HttpUtility.HtmlDecode( htmlNode.InnerText );
    }

    
    /// <summary>
    /// Recursively runs the tests, taking the correct path until the final query is reached
    /// </summary>
    /// <param name="command">the initial command</param>
    /// <returns>the final command to be run</returns>
    private XPathCommand FindFinalCommand( XPathCommand command )
    {
      /***
       * there are three options:
       * 1. onSuccess && onFailure NOT NULL
       * 2. onSuccess || onFailure NOT NULL
       * 3. onSuccess && onFailure NULL
       */

      var nextPath = FindCorrectPath( command );

      while ( nextPath != null && nextPath != command )
      {
        command = nextPath;
        nextPath = FindCorrectPath( nextPath );
      }

      return command;
    }

    private XPathCommand FindCorrectPath( XPathCommand command )
    {
      // not a test condition
      if ( command.OnSuccess == null && command.OnFailure == null )
        return command;

      // then one of them isn't null and our current query is a test condition
      var testNode = Document.DocumentNode.SelectSingleNode( command.Query );

      // either OnSuccess or OnFailure might be null
      return testNode == null ? command.OnFailure : command.OnSuccess;
    }
  }
}