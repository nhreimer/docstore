using System.Security.Cryptography;
using DocStore.Driver.Configs;
using DocStore.Driver.Persistence;
using DocStore.Engine;
using Trident.Core;

namespace DocStore.Driver
{
  public class CorpusQueryCreator : DocStoreQueryCreator, IContentQueryable
  {
    private readonly BackendConfig _metadataBackendConfig;
    private readonly BackendConfig _contentBackendConfig;
    
    public CorpusQueryCreator( 
      BackendConfig docStoreBackendConfig,
      BackendConfig metadataBackendConfig,
      BackendConfig contentBackendConfig,
      DDbStringSanitizer sanitizeString ) 
      : base( docStoreBackendConfig, sanitizeString )
    {
      _metadataBackendConfig = metadataBackendConfig;
      _contentBackendConfig = contentBackendConfig;
    }

    public string GetInsertQuery( MetaDataDocument document )
    {
      if ( string.IsNullOrEmpty( document.IDFeed ) )
      {
        var hash = new SHA1Managed();
        hash.Initialize();
        document.IDFeed = HashIdGenerator.GetChecksumString( hash, document.Url );
      }
      
      return string.Format( _metadataBackendConfig.Queries[ "insert" ],
        _metadataBackendConfig.Database,
        _metadataBackendConfig.Table,
        document.IDFeed,
        SanitizeString( document.Url ),
        document.Published.ToString( "yyyy-MM-dd" ),
        SanitizeString( document.Title ),
        document.Updated.ToString( "yyyy-MM-dd" ) );
    }

    public string GetInsertQuery( ContentDocument document )
    {
      if ( string.IsNullOrEmpty( document.RetrieveChecksum ) )
      {
        var hash = new SHA256Managed();
        hash.Initialize();
        document.RetrieveChecksum = HashIdGenerator.GetChecksumString( hash, document.RetrieveParagraph );
      }
      
      return string.Format( _contentBackendConfig.Queries[ "insert" ], 
        _contentBackendConfig.Database,
        _contentBackendConfig.Table,
        document.SourceId, 
        document.FeedId,
        document.SearchParagraph, 
        document.RetrieveChecksum, 
        SanitizeString( document.RetrieveParagraph ) );
    }
  }
}