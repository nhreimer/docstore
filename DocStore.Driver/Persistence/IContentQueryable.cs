using DocStore.Engine;
using Trident.Core;

namespace DocStore.Driver.Persistence
{
  /// <summary>
  /// Adds support for additional documents beyond the document store
  /// </summary>
  public interface IContentQueryable : IQueryable
  {
    string GetInsertQuery( MetaDataDocument document );
    string GetInsertQuery( ContentDocument document );
  }
}