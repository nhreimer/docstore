using System;
using System.IO;

using DocStore.Driver.Configs;

namespace DocStore.Driver
{
  /// <summary>
  /// Dumps the content that the 
  /// </summary>
  public class QueryErrorLogger : IQueryErrorLogger
  {
    private static NLog.Logger _logger = NLog.LogManager.GetCurrentClassLogger();
    
    private readonly ErrorConfig _errorConfig;
    
    public QueryErrorLogger( ErrorConfig errorConfig )
    {
      _errorConfig = errorConfig;
    }

    /// <summary>
    /// Will dump the contents to a file based on the Error Config.
    /// The errorType is checked against the config.
    /// The DumpOnQueryFailure must also be true
    /// </summary>
    /// <param name="errorType"></param>
    /// <param name="sourceName"></param>
    /// <param name="contents"></param>
    public void DumpContents( string errorType, string sourceName, string contents )
    {
      // Checks the following
      // 1. DumpOnQueryFailure must be true
      // 2. DumpOnQueryTypes must be defined and must be true for the errorType
      if ( _errorConfig.DumpOnQueryFailure &&
           _errorConfig.DumpOnQueryTypes != null &&
           _errorConfig.DumpOnQueryTypes.ContainsKey( errorType ) &&
           _errorConfig.DumpOnQueryTypes[ errorType ] )
      {
        var filename = CreateDumpFilename( errorType, sourceName );
        if ( filename != null )
        {
          _logger.Info( $"Query error in parsing. Filename: `{filename}`" );
          File.WriteAllText( filename, contents );
        }
      }
    }

    private string CreateDumpFilename( string errorType, string sourceName )
    {
      if ( string.IsNullOrEmpty( _errorConfig.DumpDirectory ) )
      {
        _logger.Error( $"cannot dump contents for `{errorType}` because DumpDirectory is empty." );
        return null;
      }

      if ( !Directory.Exists( _errorConfig.DumpDirectory ) )
      {
        _logger.Error( $"cannot dump contents for `{errorType}` because DumpDirectory doesn't exist" );
        return null;
      }

      var sourceNameId = sourceName.GetHashCode();

      var fullFileName =
        Path.Combine( 
          _errorConfig.DumpDirectory, 
          $"{sourceName}_{errorType}_{DateTime.Now:yyyy-MM-dd}_{sourceNameId}.txt" );

      if ( File.Exists( fullFileName ) )
      {
        _logger.Warn( $"cannot dump file contents for ${sourceNameId} because it already exists." );
        return null;
      }

      return fullFileName;
    }
  }
}