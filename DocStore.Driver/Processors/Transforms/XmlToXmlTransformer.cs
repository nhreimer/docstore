using DocStore.Driver.Parsing;

namespace DocStore.Driver.Processors.Transforms
{
  /// <summary>
  /// Acts as a passthrough
  /// </summary>
  public class XmlToXmlTransformer : ITransformable< string, string >
  {
    public string Transform( string data )
    {
      return data;
    }
  }
}