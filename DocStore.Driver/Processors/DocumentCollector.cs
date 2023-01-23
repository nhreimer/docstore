using System;
using System.IO;
using System.Collections.Generic;

using DocStore.Engine;
using DocStore.Driver.Configs;
using DocStore.Driver.Parsing;
using DocStore.Driver.Requesters;
using DocStore.Driver.Processors.Transforms;

using Newtonsoft.Json;

namespace DocStore.Driver.Processors
{
  public class DocumentCollector : IProcessable< SourceConfig >
  {
    private static readonly NLog.Logger _logger = NLog.LogManager.GetCurrentClassLogger();
    
    private readonly ITransformable< string, IList< string > > _linkTransformer; 

    private readonly IDocumentStorage _persister;

    private readonly Random _randomWaitInMs;
    
    public DocumentCollector( ITransformable< string, IList< string > > linkTransformer,
                              IDocumentStorage persister )
    {
      _linkTransformer = linkTransformer;
      _persister = persister;

      _randomWaitInMs = new Random();
    }

    public void Process( SourceConfig sourceConfig )
    {
      // create the landing requester
      var landingRequester = CreateRequestable( sourceConfig, "Landing" );
      
      // request the source landing page
      var landingResource = landingRequester.Request( sourceConfig.LandingUrl, 0 );
      if ( !string.IsNullOrEmpty( landingResource ) )
      {
        // transform the landing page's format
        var formattedLanding = 
          TransformUtility.Convert( sourceConfig.DataFormats.Landing, landingResource );
        
        // transform the landing page into a bunch of links
        var linkResources = _linkTransformer.Transform( formattedLanding );

        // create the requester for content
        var contentRequester = CreateRequestable( sourceConfig, "Content" );
        
        // process each link request
        foreach ( var linkResource in linkResources )
          ProcessLinkRequest( contentRequester, sourceConfig, linkResource );
      }
      else
        _logger.Error( $"request to fetch `{sourceConfig.LandingUrl}` failed." );
    }
   
    /// <summary>
    /// Processes a single link request by fetching it and then storing it
    /// </summary>
    /// <param name="requester"></param>
    /// <param name="sourceConfig"></param>
    /// <param name="linkResource"></param>
    private void ProcessLinkRequest( 
      IRequestable requester, 
      SourceConfig sourceConfig, 
      string linkResource )
    {
      // if the document already exists then do not fetch it
      if ( DoesDocumentExist( linkResource ) )
      {
        _logger.Info( $"skipping retrieval. document already in storage. {linkResource}" );
        return;
      }
      
      // process each link
      var linkContents = RetrieveLinkContents( requester, sourceConfig, linkResource );
      if ( !string.IsNullOrEmpty( linkContents ) )
      {
        // convert the format from the source format to the final format
        // so that it can be processed for links
        var formattedContents = 
          TransformUtility.Convert( sourceConfig.DataFormats.Content, linkContents );
            
        // create the StoredDocument now that we've successfully retrieved the data
        var document = CreateStoredDocument( sourceConfig, formattedContents, linkResource );
        
        // persist the StoredDocument 
        if ( _persister.Insert( document ) )
          _logger.Info( $"successfully stored document from link {linkResource}" );
        else
          _logger.Error( $"failed to store document from link {linkResource}" );
      }
      else
        _logger.Error( $"link contents are empty or null for: {linkResource}" );
    }
    
    private string RetrieveLinkContents( 
      IRequestable requester, 
      SourceConfig sourceConfig, 
      string linkResource )
    {
      int attempts = 0;
      
      while ( attempts < 3 )
      {
        // add 5 seconds for each attempt 
        var nextWaitInMs = _randomWaitInMs.Next( 
                                sourceConfig.MinWaitInMs, 
                                sourceConfig.MaxWaitInMs ) + attempts * 5;

        // fetch link
        _logger.Info( 
          $"({attempts}). {sourceConfig.SourceName} waiting {nextWaitInMs}ms to retrieve url {linkResource}" );
        
        var contentHtml = requester.Request( linkResource, nextWaitInMs );

        // store contents in DocStore
        if ( !string.IsNullOrEmpty( contentHtml ) )
          return contentHtml;

        _logger.Error( $"failed to fetch any content from link {linkResource}" );

        // no matter what, increase this
        ++attempts;
      }

      return null;
    }

    private bool DoesDocumentExist( string linkResource )
    {
      // check whether we already have that in the store
      return ( _persister.GetDocumentById( HashIdGenerator.CreateObjId( linkResource ) ) != null );
    }
    
    /// <summary>
    /// Creates a Requestable based on the config
    /// </summary>
    /// <param name="sourceConfig"></param>
    /// <param name="requestType"></param>
    /// <returns>IRequestable</returns>
    private static IRequestable CreateRequestable( 
      SourceConfig sourceConfig, 
      string requestType )
    {
      var requesterConfig = 
        JsonConvert.DeserializeObject< HttpRequesterConfig >( 
          File.ReadAllText( sourceConfig.Requesters[ requestType ] ) );

      return new HttpRequester( requesterConfig );
    }
    
    /// <summary>
    /// Creates a StoredDocument based on certain info
    /// </summary>
    /// <param name="sourceConfig"></param>
    /// <param name="contentHtml"></param>
    /// <param name="link"></param>
    /// <returns></returns>
    private static StoredDocument CreateStoredDocument( 
      SourceConfig sourceConfig, 
      string contentHtml, 
      string link )
    {
      return new StoredDocument
      {
        MetaData = new MetaDocument
        {
          ObjName = link,
          ObjType = sourceConfig.SourceName,
          Updated = DateTime.Now
        },
        Data = contentHtml
      };
    }
  }
}