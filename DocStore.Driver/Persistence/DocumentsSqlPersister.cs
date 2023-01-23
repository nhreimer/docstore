using System.Data.Common;
using DocStore.Engine;
using Trident.Core;

namespace DocStore.Driver.Persistence
{
  /// <summary>
  /// Adds support for documents beyond the Document Store
  /// </summary>
  public class DocumentsSqlPersister : SqlPersister, IContentStorage
  {
    private readonly IContentQueryable _contentQuerier;

    public DocumentsSqlPersister( DbConnection connection, IContentQueryable querier )
      : base( connection, querier )
    {
      _contentQuerier = querier;
    }

    public bool Insert( MetaDataDocument metaDataDocument )
    {
      var query = _contentQuerier.GetInsertQuery( metaDataDocument );
      return ExecuteStatement( query );
    }

    public bool Insert( ContentDocument contentDocument )
    {
      var query = _contentQuerier.GetInsertQuery( contentDocument );
      return ExecuteStatement( query );
    }
  }
}