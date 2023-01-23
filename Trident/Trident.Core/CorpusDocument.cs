using System.Collections.Generic;

namespace Trident.Core
{
  public class CorpusDocument
  {
    public MetaDataDocument MetaData { get; set; }
    public IList< ContentDocument > Contents { get; set; }
  }
}