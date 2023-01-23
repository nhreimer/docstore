using Trident.Core;

namespace DocStore.Driver.Persistence
{
  /// <summary>
  /// storage interface for content
  /// </summary>
  public interface IContentStorage
  {
    bool Insert( MetaDataDocument metaDataDocument );
    bool Insert( ContentDocument contentDocument );
  }
}