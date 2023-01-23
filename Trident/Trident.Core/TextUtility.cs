using System;
using System.Net;
using System.Text.RegularExpressions;
using HtmlAgilityPack;

namespace Trident.Core
{
  public static class TextConverter< T >
  {
    public delegate bool DTryParse( out T obj, string str );
  
    public static T Convert( string str, DTryParse tryParse )
    {
      T obj = default;
      
      if ( !string.IsNullOrEmpty( str ) )
        tryParse( out obj, str );

      return obj;
    }
  }
  
  public static class TextUtility
  {
    private static readonly Regex[] _Regices = { new Regex( @"<.*?>", RegexOptions.Compiled ),
      new Regex( @"\s+", RegexOptions.Compiled ) };
    
    public static HtmlDocument LoadAndClean( string html )
    {
      var htmlDocument = new HtmlDocument();
      htmlDocument.LoadHtml( html );
      return RemoveScripts( htmlDocument );
    }
    
    private static HtmlDocument RemoveScripts( HtmlDocument htmlDocument )
    {
      // HtmlAgilityPack sometimes treats scripts as innertext 
      // so removing all the scripts is a workaround
      // e.g. http://www.newsweek.pl/polska/general-jerzy-gut-odchodzi-z-wojska-kolejna-dymisja-w-dowodztwie-armii,artykuly,406916,1.html
      var scripts = htmlDocument.DocumentNode.SelectNodes( "//script" );
      
      if ( scripts != null )
      {
        foreach ( var script in scripts )
          script.Remove();
      }

      return htmlDocument;
    }

    // public static string RemoveEmbeddedHtmlTags( string text )
    // {
    //   // remove any embedded html tags
    //   var htmlDocument = LoadAndClean( text );
    //   return htmlDocument.DocumentNode.InnerText;
    // }
    
    public static string CleanText( string text )
    {
      if ( string.IsNullOrEmpty( text ) )
        return text;
      
      text = text.Replace( "\r", string.Empty )
                 .Replace( "\n", string.Empty )
                 .Replace( "\t", string.Empty ).Trim();

      //some cutsie crap Gazeta.Ru likes to throw in to emphasize words in a headerline
      //fulltext = fulltext.Replace( "»", String.Empty );
      //fulltext = fulltext.Replace( "«", String.Empty );

      text = _Regices[ 0 ].Replace( text, string.Empty );
      text = _Regices[ 1 ].Replace( text, " " );
      
      // decode any html entities 
      text = WebUtility.HtmlDecode( text );

      return text;
    }
  }
}