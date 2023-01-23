using System;
using System.Globalization;
using XClaw;

namespace DocStore.Driver.Processors.Transforms
{
  internal static class HtmlDateTimeHelper
  {
    private static readonly NLog.Logger _logger = NLog.LogManager.GetCurrentClassLogger();
    
    public static DateTime GetDateTimeFromUnixTimestamp( XCommandResult publishedResult )
    {
      DateTime published = DateTime.MinValue;

      var cmd = publishedResult.CommandExecuted;
      
      if ( !long.TryParse( publishedResult.SingleResult.Trim(), out var unixTimestamp ) )
        _logger.Error( $"unable to convert unix timestamp `{publishedResult.SingleResult}` into integer" );
      else
      {
        DateTimeOffset dateTimeOffset = DateTimeOffset.MinValue;
        
        // is it the millisecond variety?
        if ( cmd.Options[ "unix" ].Equals( "ms", StringComparison.OrdinalIgnoreCase ) )
        {
          try { dateTimeOffset = DateTimeOffset.FromUnixTimeMilliseconds( unixTimestamp ); }
          catch ( ArgumentOutOfRangeException e ) { _logger.Error( e ); }
        }
        // if ms is not indicated then assume seconds
        else
        {
          try { dateTimeOffset = DateTimeOffset.FromUnixTimeSeconds( unixTimestamp ); }
          catch ( ArgumentOutOfRangeException e ) { _logger.Error( e ); }
        }
        
        published = dateTimeOffset.DateTime;
      }

      return published;
    }

    public static DateTime GetDateTimeFromString( XCommandResult publishedResult )
    {
      var cmd = publishedResult.CommandExecuted;
      
      DateTime published; 

      var culture = ( cmd.Options != null && cmd.Options.ContainsKey( "cultureName" ) )
        ? CultureInfo.CreateSpecificCulture( cmd.Options[ "cultureName" ] )
        : CultureInfo.InvariantCulture;

      var format = ( cmd.Options != null && cmd.Options.ContainsKey( "format" ) )
        ? cmd.Options[ "format" ]
        : string.Empty;

      if ( !string.IsNullOrEmpty( format ) )
      {
        if ( !DateTime.TryParseExact(
              publishedResult.SingleResult,
              format,
              culture,
              DateTimeStyles.None,
              out published ) )
        {
          _logger.Error(
            $"failed to parse DateTime `{publishedResult.SingleResult}` using format `{format}` and culture `{culture}`" );
        }
      }
      else
      {
        if ( !DateTime.TryParse(
              publishedResult.SingleResult,
              culture,
              DateTimeStyles.None,
              out published ) )
        {
          _logger.Error(
            $"failed to parse DateTime `{publishedResult.SingleResult}` using no format and culture `{culture.Name}`" );
        }
      }

      return published;
    }
  }
}