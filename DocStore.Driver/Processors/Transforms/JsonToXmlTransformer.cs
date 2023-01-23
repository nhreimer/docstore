using System.Xml;
using System.Xml.Linq;
using System.Runtime.Serialization.Json;

using DocStore.Driver.Parsing;

namespace DocStore.Driver.Processors.Transforms
{
  public class JsonToXmlTransformer : ITransformable< string, string >
  {
    public string Transform( string data )
    {
      /***
       * NOTES:
       *  using XDocument instead because its JSON to XML converter actually works.
       *  Newtonsoft kept producing weird results that weren't reliably XPathable.
       *
       *  If you do decide to switch then know that the conversion engines create
       *  wildly different results between the two.
       */
      return XDocument.Load( JsonReaderWriterFactory.CreateJsonReader(
        System.Text.Encoding.UTF8.GetBytes( data ), 
        new XmlDictionaryReaderQuotas() ) ).ToString();
    }
  }
}