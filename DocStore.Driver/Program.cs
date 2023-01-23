using System;
using System.IO;
using System.Threading.Tasks;
using System.Collections.Generic;

using DocStore.Driver.Configs;
using DocStore.Driver.Persistence;
using DocStore.Driver.Processors;
using DocStore.Driver.Processors.Transforms;

using Newtonsoft.Json;

namespace DocStore.Driver
{
  internal static class Program
  {
    private static NLog.Logger _logger = NLog.LogManager.GetCurrentClassLogger();

    private const string MAIN_CONFIG = 
      @"D:\projects\_myprojects\DocStore\DocStore.Driver\Configs\MainConfig\MainConfig.json";

    private static void RunSource(
      MainConfig mainConfig,
      SourceConfig source,
      bool runCollection, 
      bool runParser
      )
    {
      _logger.Info( $"starting source {source.SourceName}" );
      
      if ( source.Skip )
      {
        _logger.Info( $"skipping source {source.SourceName}" );
        return;
      }

      // Run the collector for this source
      if ( runCollection )
      {
        var documentCollector =
          new DocumentCollector( 
            TransformUtility.CreateLinkTransform( mainConfig, source ),
            DbFactory.CreateSqlPersister( mainConfig )
          );

        documentCollector.Process( source );
      }

      // Run the parser for this source
      if ( runParser )
      {
        var persister = DbFactory.CreateSqlPersister( mainConfig );
        
        var documentParser =
          new DocumentParser( 
            TransformUtility.CreateContentTransform( mainConfig, source ),
            persister,  // doc storage
            persister   // content storage
          );

        documentParser.Process( source );
      }
      
      _logger.Info( $"ending source {source.SourceName}" );
    }
    
    private static void Run(
      MainConfig mainConfig,
      IList< SourceConfig > sources,
      bool runCollection, 
      bool runParser,
      bool runParallel )
    {
      if ( runParallel )
      {
        _logger.Info( $"using multiple threads" );
        Parallel.ForEach(
          sources,
          new ParallelOptions { MaxDegreeOfParallelism = 8 },
          source => { RunSource( mainConfig, source, runCollection, runParser ); }
        );
      }
      else
      {
        _logger.Info( $"using single thread" );
        foreach ( var source in sources )
          RunSource( mainConfig, source, runCollection, runParser );
      }
    }

    /// <summary>
    /// Loads a single source in order to be tested or run independently
    /// </summary>
    /// <param name="mainConfig"></param>
    /// <param name="sourceName"></param>
    /// <returns></returns>
    private static SourceConfig LoadSourceConfig(
      MainConfig mainConfig, 
      string sourceName )
    {
      var sources = 
        JsonConvert.DeserializeObject< List< SourceConfig > >( File.ReadAllText( mainConfig.SourcesConfig ) );

      if ( sources != null )
      {
        foreach ( var source in sources )
        {
          if ( sourceName.Equals( source.SourceName, StringComparison.OrdinalIgnoreCase ) )
            return source;
        }
      }

      return null;
    }

    private static bool HasProtocol( string url )
    {
      return url.IndexOf( "http:", StringComparison.Ordinal ) > -1 ||
             url.IndexOf( "https:", StringComparison.Ordinal ) > -1;
    }
    
    /***
     * Configurations:
     *  MainConfig      <-- pointer to other configs
     *    SourceConfig  <-- all the sources and their info, e.g., SourceId, LandingPage, Name
     *      Schema      <-- specific parser information for a source
     *      Requester   <-- specific requesters to use for collecting new content
     *    BackendConfig <-- db queries
     *    ErrorConfig   <-- content dumping for query-related errors
     */
    private static void Main( string[] args )
    {
      var mainConfig = 
        JsonConvert.DeserializeObject< MainConfig >( File.ReadAllText( MAIN_CONFIG ) );
      
      if ( mainConfig != null )
      {
        var sources = 
          JsonConvert.DeserializeObject< List< SourceConfig > >( File.ReadAllText( mainConfig.SourcesConfig ) );
        
        if ( sources != null )
          Run( mainConfig, sources, true, false, false );

        // run an individual source
        // var source = LoadSourceConfig( mainConfig, "wprost" );
        // if ( source != null )
        //   RunSource( mainConfig, source, false, true );
      }
     
      _logger.Info( "done" );
    }
  }
}