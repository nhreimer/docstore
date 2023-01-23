using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using DocStore.Driver.Configs;
using DocStore.Driver.Parsing;
using DocStore.Engine;

using Trident.Core;
using Trident.Language;
using XClaw;

namespace DocStore.Driver.Processors.Transforms
{
  using XClawConfig = 
    IDictionary< 
      string, 
      IList< XPathCommand > >;
  
  public class HtmlContentTransformer : ITransformable< StoredDocument, CorpusDocument >
  {
    private static readonly NLog.Logger _logger = NLog.LogManager.GetCurrentClassLogger();
    
    private const string TITLE_CMDS = "title";
    private const string PUBLISHED_CMDS = "published";
    private const string PARAGRAPH_CMDS = "paragraphs";

    private readonly SourceConfig _sourceConfig;
    private readonly IQueryErrorLogger _queryErrorLogger;
    private readonly XClawConfig _cmdConfig;
    
    public HtmlContentTransformer( 
      SourceConfig sourceConfig,
      IQueryErrorLogger queryErrorLogger )
    {
      _sourceConfig = sourceConfig;
      _queryErrorLogger = queryErrorLogger;
      
      // load the commands
      _cmdConfig = XClawHelper.DeserializeFromFile( _sourceConfig.XCmdConfigFile );
    }
    
    public CorpusDocument Transform( StoredDocument storedDocument )
    {
      var result = new CorpusDocument
      {
        Contents = new List< ContentDocument >() 
      };

      // load the XClaw commands
      var cmdExecutor = 
        new XCommandExecutor( TextUtility.LoadAndClean( storedDocument.Data ) );

      // run the commands
      result.MetaData = ParseMetaData( storedDocument.MetaData.ObjName, cmdExecutor );
      var rawContentsList = ParseContent( cmdExecutor );
      
      // convert the valid paragraphs to ContentDocuments
      foreach ( var rawContents in rawContentsList )
      {
        if ( !string.IsNullOrEmpty( rawContents ) )
          result.Contents.Add( CreateContentDocument( result.MetaData, rawContents ) );
      }

      return result;
    }

    private MetaDataDocument ParseMetaData( string url, XCommandExecutor cmdExecutor )
    {
      var title = 
        TextUtility.CleanText( cmdExecutor.AggregateCommandBlock( _cmdConfig[ TITLE_CMDS ] ) );

      if ( string.IsNullOrEmpty( title ) )
      {
        _logger.Warn( $"article title not found for {url}" );
        _queryErrorLogger.DumpContents( 
          TITLE_CMDS, 
          _sourceConfig.SourceName, 
          cmdExecutor.Document.DocumentNode.InnerHtml );
      }

      // there can only be one date query (although nesting is allowed).
      // use xpath concatenate to glue parts together 
      var published = ParsePublicationDate( url, _cmdConfig, cmdExecutor );

      return new MetaDataDocument
      {
        Url = url,
        IDFeed = HashIdGenerator.CreateObjId( url ),
        Title = title,
        Published = published
      };
    }
    
    private IList< string > ParseContent( XCommandExecutor cmdExecutor )
    {
      // basic string cleaning performed
      var xresults = 
        cmdExecutor.ExecuteCommandBlock( _cmdConfig[ PARAGRAPH_CMDS ] );

      // log any errors
      foreach ( var xresult in xresults )
      {
        if ( !xresult.Success )
        {
          _queryErrorLogger.DumpContents( 
            PARAGRAPH_CMDS, 
            _sourceConfig.SourceName, 
            cmdExecutor.Document.DocumentNode.InnerHtml );
        }
      }
      
      return XClawHelper.ReduceResults( 
        xresults, 
        text =>
        {
           var result = TextUtility.CleanText( text );
           // result = TextUtility.RemoveEmbeddedHtmlTags( result );
           return result;
        } );
    }

    private ContentDocument CreateContentDocument(
      MetaDataDocument metaDataDocument,
      string paragraph )
    {
      // don't allow any embedded html crap in there
      // var htmlFreeParagraph = TextUtility.RemoveEmbeddedHtmlTags( paragraph );
      
      return new ContentDocument
      {
        FeedId = metaDataDocument.IDFeed,
        SourceId = _sourceConfig.SourceId,
        RetrieveParagraph = paragraph,
        SearchParagraph = Tokenizer.Tokenize( paragraph ),
        RetrieveChecksum = HashIdGenerator.GetChecksumString( new SHA256Managed(), paragraph )
      };
    }
    
    private DateTime ParsePublicationDate(
      string url,
      XClawConfig cmdConfig, 
      XCommandExecutor cmdExecutor )
    {
      var cmd = cmdConfig[ PUBLISHED_CMDS ][ 0 ];
      var publishedResult = cmdExecutor.Execute( cmd );
      
      DateTime published = default;
      if ( publishedResult.Success )
      {
        var publishedDate = 
          TextUtility.CleanText( publishedResult.SingleResult );
        
        if ( !string.IsNullOrEmpty( publishedDate ) )
        {
          // check for unix timestamp first, which some webpages use
          if ( cmd.Options != null && cmd.Options.ContainsKey( "unix" ) )
            published = HtmlDateTimeHelper.GetDateTimeFromUnixTimestamp( publishedResult );
          
          // non-unix timestamps use string formatting and culture info
          else
            published = HtmlDateTimeHelper.GetDateTimeFromString( publishedResult );
          
          // published can still be an error and we need the url info to trace back to the source
          if ( published.Equals( DateTime.MinValue ) )
            _logger.Error( $"Date parsing failed for url `{url}`" );
        }
        else
          _logger.Error( $"XPath query successful but no data for date found for url `{url}`" );
      }
      // dump the contents after the failure
      else
      {
        _queryErrorLogger.DumpContents( 
          PUBLISHED_CMDS, 
          _sourceConfig.SourceName,
          cmdExecutor.Document.DocumentNode.InnerHtml );
      }

      return published;
    }
  }
}