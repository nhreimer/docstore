using System.Collections.Generic;
using System.IO;
using DocStore.Driver.Configs;
using MySql.Data.MySqlClient;
using Newtonsoft.Json;

namespace DocStore.Driver.Persistence
{
  /// <summary>
  /// A simple factory for creating the Document Persister
  /// </summary>
  internal static class DbFactory
  {
    public static DocumentsSqlPersister CreateSqlPersister( MainConfig mainConfig )
    {
      var backend = 
        JsonConvert.DeserializeObject< IDictionary< string, BackendConfig > >( 
          File.ReadAllText( mainConfig.BackendConfig ) );
      
      // TODO: at some point, this might need to be refactored 
      // TODO: so that each connection to each database can be used
      var connection = new MySqlConnection( backend[ "docstore" ].Connection );
      connection.Open();

      var queryCreator = new CorpusQueryCreator(
        backend[ "docstore" ],
        backend[ "articles" ],
        backend[ "paragraphs" ],
        MySqlHelper.EscapeString
      );

      return new DocumentsSqlPersister( connection, queryCreator );
    }
  }
}