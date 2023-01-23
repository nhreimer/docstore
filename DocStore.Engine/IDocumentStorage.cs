namespace DocStore.Engine
{
  /// <summary>
  /// Interface for interacting with the DB backend
  /// </summary>
  public interface IDocumentStorage
  {
    bool Insert( StoredDocument document );
    bool Update( StoredDocument document );
    bool Delete( StoredDocument document );
    
    StoredDocument GetDocumentById( string objId );
    ICursor CreateCursor( string sourceName, int bufferSize = 512 );
  }
}