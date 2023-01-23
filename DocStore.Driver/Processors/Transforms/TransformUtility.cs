using System;
using System.IO;
using System.Collections.Generic;

using DocStore.Driver.Configs;
using DocStore.Driver.Parsing;
using DocStore.Engine;
using Newtonsoft.Json;
using Trident.Core;

namespace DocStore.Driver.Processors.Transforms
{
  internal static class TransformUtility
  {
    public static string Convert( string sourceType, string sourceData )
    {
      var transform = CreateFormatTransform( sourceType );
      return transform?.Transform( sourceData );
    }

    public static ITransformable< string, string > CreateFormatTransform( string sourceType )
    {
      if ( sourceType.Equals( "html", StringComparison.OrdinalIgnoreCase ) )
        return new XmlToXmlTransformer();

      if ( sourceType.Equals( "json", StringComparison.OrdinalIgnoreCase ) )
        return new JsonToXmlTransformer();

      return null;
    }

    public static ITransformable< string, IList< string > > CreateLinkTransform(
      MainConfig mainConfig, 
      SourceConfig sourceConfig )
    {
      var queryErrorConfig = 
        JsonConvert.DeserializeObject< ErrorConfig >( File.ReadAllText( mainConfig.ErrorConfig ) );
      
      return new HtmlLinkTransformer( sourceConfig, 
        new QueryErrorLogger( queryErrorConfig ) );
    }
    
    public static ITransformable< StoredDocument, CorpusDocument > CreateContentTransform(
      MainConfig mainConfig, 
      SourceConfig sourceConfig )
    {
      var queryErrorConfig = 
        JsonConvert.DeserializeObject< ErrorConfig >( File.ReadAllText( mainConfig.ErrorConfig ) );
      
      return new HtmlContentTransformer( sourceConfig, 
        new QueryErrorLogger( queryErrorConfig ) );
    }
  }
}