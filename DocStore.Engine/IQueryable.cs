using System.Data.Common;

namespace DocStore.Engine
{
  public interface IQueryable : IConvertible
  {
    string GetInsertQuery( StoredDocument document );
    string GetUpdateQuery( StoredDocument document );
    string GetDeleteQuery( StoredDocument document );

    string GetDocumentById( string objId );

    TableCursor CreateCursor( DbConnection connection, string sourceName, int bufferSize = 512 );
  }
}