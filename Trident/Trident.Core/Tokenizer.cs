using System;
using System.Text;
using System.Text.RegularExpressions;

namespace Trident.Language
{
  public static class Tokenizer 
  {
    private static readonly Regex _Stripper = new Regex( @"\W+(?<!(?:-))", RegexOptions.Compiled );
    private static readonly string DELIMITER = " ";
    private static readonly String[] _SplitDelimiter = new [] { DELIMITER };
    // private static readonly string[] _EmptyTokenSet = new string[] { };
    // public static readonly string[] _ParagraphDelimiter = new string[] { @" ", @" " };
    
    /// <summary>
    /// Strips down a paragraph to its searchable words.
    /// </summary>
    /// <param name="data"></param>
    /// <returns></returns>
    public static string Tokenize( string data )
    {
      var stripped = _Stripper.Replace( data, DELIMITER ).Trim();
      var result = new StringBuilder();

      foreach ( var word in stripped.Split( _SplitDelimiter, StringSplitOptions.RemoveEmptyEntries ) )
      {
        var standardizedWord = StandardizeToken( word );
        if ( !string.IsNullOrWhiteSpace( standardizedWord ) )
        {
          if ( result.Length > 0 )
            result.Append( ' ' );

          result.Append( standardizedWord );
        }
      }

      return result.ToString();
    }

    /// <summary>
    /// Standardizes a token, if it can be. Words that contain non-alphabetic 
    /// characters are not valid.
    /// </summary>
    /// <param name="token"></param>
    /// <returns>Standardized token if possible, otherwise null</returns>
    private static string StandardizeToken( String token )
    {
      StringBuilder result = null;

      var validationState = true;

      if ( !string.IsNullOrWhiteSpace( token ) )
      {
        result = new StringBuilder( token.Length );
        foreach ( var c in token )
        {
          if ( !Char.IsLetter( c ) )
          {
            validationState = false;
            break;
          }

          result.Append( Char.ToLower( c ) );
        }
      }
      else
        validationState = false;

      return ( result != null && validationState ? result.ToString() : null );
    }

  }
}