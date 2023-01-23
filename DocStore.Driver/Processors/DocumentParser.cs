using DocStore.Engine;
using DocStore.Driver.Configs;
using DocStore.Driver.Parsing;
using DocStore.Driver.Persistence;

using Trident.Core;

namespace DocStore.Driver.Processors
{
  public class DocumentParser : IProcessable< SourceConfig >
  {
    private static NLog.Logger _logger = NLog.LogManager.GetCurrentClassLogger();

    private readonly ITransformable< StoredDocument, CorpusDocument > _parser;
    private readonly IContentStorage _contentStore;
    private readonly IDocumentStorage _docStore;
    
    public DocumentParser(
      ITransformable< StoredDocument, CorpusDocument > parser,
      IDocumentStorage docStore,
      IContentStorage contentStore )
    {
      _parser = parser;
      _docStore = docStore;
      _contentStore = contentStore;
    }
    
    public void Process( SourceConfig sourceConfig )
    {
      // get a document cursor, which is essentially a source in a sink/source paradigm
      var cursor = _docStore.CreateCursor( sourceConfig.SourceName );
      
      while ( cursor.Next() is { } storedDocument )
      {
        // transform the StoredDocument into a CorpusDocument
        var corpusDocument = _parser.Transform( storedDocument );

        // save the Metadata Document
        var markAsArchived = SaveMetaDataDocument( corpusDocument );
        
        // save all the ContentDocuments
        var documentsSaved = SaveContentDocuments( corpusDocument );
        _logger.Info( $"Paragraphs saved {documentsSaved}/{corpusDocument.Contents.Count}" );
        
        // mark the document as Archived
        if ( markAsArchived )
          ArchiveStoredDocument( storedDocument );
      }
    }

    private int SaveContentDocuments( CorpusDocument corpusDocument )
    {
      int counter = 0;
      
      foreach ( var contentDocument in corpusDocument.Contents )
      {
        if ( !_contentStore.Insert( contentDocument ) )
          _logger.Error( $"failed to persist ContentDocument: {corpusDocument.MetaData.Url}. {contentDocument.RetrieveParagraph}" );
        else
        {
          ++counter;
          _logger.Info( $"successfully inserted new ContentDocument: {contentDocument.FeedId}" );
        }
      }

      return counter;
    }
    
    private void ArchiveStoredDocument( StoredDocument document )
    {
      document.MetaData.IsArchived = true;
      if ( !_docStore.Update( document ) )
        _logger.Error( $"failed to update stored document status for {document.MetaData.ObjId}" );
      else
        _logger.Info( $"successfully updated stored document {document.MetaData.ObjId}" );
    }

    private bool SaveMetaDataDocument( CorpusDocument corpusDocument )
    {
      // save the Metadata Document
      var result = _contentStore.Insert( corpusDocument.MetaData );
      if ( !result )
        _logger.Error( $"failed to save metadata. paragraphs will fail to save if metadata document does not exist" );
      else
        _logger.Info( $"successfully saved MetaData Document {corpusDocument.MetaData.Url}" );

      return result;
    }
  }
}