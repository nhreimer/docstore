using System;
using System.Collections.Generic;
using System.Text;
using DocStore.Driver.Configs;
using DocStore.Driver.Parsing;

using HtmlAgilityPack;

using Trident.Core;
using XClaw;

namespace DocStore.Driver.Processors.Transforms
{
  /// <summary>
  /// Transforms contents from a landing page into fetchable links using XClaw
  /// </summary>
  public class HtmlLinkTransformer : ITransformable< string, IList< string > >
  {
    private static NLog.Logger _logger = NLog.LogManager.GetCurrentClassLogger();
    
    private const string LANDING_CMDS = "landing";
    
    private readonly SourceConfig _sourceConfig;
    private readonly IQueryErrorLogger _queryErrorLogger;
    
    public HtmlLinkTransformer( SourceConfig sourceConfig, 
                                IQueryErrorLogger queryErrorLogger )
    {
      _sourceConfig = sourceConfig;
      _queryErrorLogger = queryErrorLogger;
    }
    
    /// <summary>
    /// Takes in content and finds the links
    /// </summary>
    /// <param name="data">Raw contents that has the links in it</param>
    /// <returns>A list with links, otherwise an empty list</returns>
    public IList< string > Transform( string data )
    {
      if ( !string.IsNullOrEmpty( data ) )
      {
        // scrape links from landing page
        var htmlDocument = TextUtility.LoadAndClean( data );
        var links = ParseLinks( htmlDocument );
        
        // save the html content for debugging
        if ( links.Count == 0 )
          _queryErrorLogger.DumpContents( LANDING_CMDS, _sourceConfig.SourceName, data );

        return links;
      }

      return new List< string >();
    }
    
    /// <summary>
    /// Runs XClaw on the html document in order to extract links
    /// from the document
    /// </summary>
    /// <param name="htmlDocument"></param>
    /// <returns></returns>
    private IList< string > ParseLinks( HtmlDocument htmlDocument )
    {
      var commandExecutor = new XCommandExecutor( htmlDocument );
      
      var commands = 
        XClawHelper.DeserializeFromFile( _sourceConfig.XCmdConfigFile );
      
      // execute all the commands in the array "landing" in the config file
      var xresults = 
        commandExecutor.ExecuteCommandBlock( commands[ LANDING_CMDS ] );
      
      // add the UrlPrefix to every link and then return the results
      // the replace stmt is there b/c of // being prefixed by certain servers, e.g., gosc

      return XClawHelper.ReduceResults( xresults, CreateUrl );
    }

    /// <summary>
    /// Prevents the protocol from appearing in multiple places and prevents
    /// slashes from piling up
    /// </summary>
    /// <param name="url"></param>
    /// <returns></returns>
    private string CreateUrl( string url )
    {
      string testurl;

      if ( HasProtocol( _sourceConfig.UrlPrefix ) )
      {
        if ( HasProtocol( url ) )
          testurl = $"{url}{_sourceConfig.UrlPostfix}";
        else
          testurl = $"{_sourceConfig.UrlPrefix}{url}{_sourceConfig.UrlPostfix}";
      }
      else if ( HasProtocol( url ) )
      {
        testurl = $"{url}{_sourceConfig.UrlPostfix}";
      }
      else
      {
        _logger.Error( $"url is missing protocol. please check SourceConfig UrlPrefix: `{url}`" );
        return string.Empty;
      }

      var splitUrl = testurl.Split( '/', StringSplitOptions.RemoveEmptyEntries );

      var builder = new System.Text.StringBuilder( splitUrl[ 0 ] );
      builder.Append( @"//" );

      for ( int i = 1; i < splitUrl.Length; ++i )
      {
        // don't add / to the end. uniformly set it so the checksums on the links 
        // don't get fucked up and cause the same article to be stored twice
        if ( i == splitUrl.Length - 1 )
          builder.Append( splitUrl[ i ] );
        else
          builder.Append( $"{splitUrl[ i ]}/" );
      }

      return builder.ToString();
    }
    
    private static bool HasProtocol( string url )
    {
      return url.IndexOf( "http:", StringComparison.Ordinal ) > -1 ||
             url.IndexOf( "https:", StringComparison.Ordinal ) > -1;
    }
  }
}