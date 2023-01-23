using System.Data.Common;

namespace DocStore.Engine
{
  public class SqlPersister : IDocumentStorage
  {
    private static NLog.Logger _logger = NLog.LogManager.GetCurrentClassLogger();
    
    private readonly DbConnection _connection;
    private readonly IQueryable _querier;

    public SqlPersister(
      DbConnection connection,
      IQueryable querier )
    {
      _connection = connection;
      _querier = querier;
    }

    public bool Insert( StoredDocument document )
    {
      document.MetaData.ObjId = HashIdGenerator.CreateObjId( document.MetaData.ObjName );
      return ExecuteStatement( _querier.GetInsertQuery( document ) );
    }

    public bool Update( StoredDocument document )
    {
      document.MetaData.ObjId = HashIdGenerator.CreateObjId( document.MetaData.ObjName );
      return ExecuteStatement( _querier.GetUpdateQuery( document ) );
    }

    public bool Delete( StoredDocument document )
    {
      return ExecuteStatement( _querier.GetDeleteQuery( document ) );
    }

    public StoredDocument GetDocumentById( string objId )
    {
      var query = _querier.GetDocumentById( objId );
      
      try
      {
        using var cmd = _connection.CreateCommand();
        cmd.CommandText = query;
        using var reader = cmd.ExecuteReader();
        if ( reader.HasRows && reader.Read() )
        {
          var fields = new object[ reader.FieldCount ];
          reader.GetValues( fields );
          return _querier.Convert( fields );
        }

        _logger.Debug( $"unable to find record for query: {cmd.CommandText}" );
        return null;
      }
      catch ( DbException e )
      {
        _logger.Error( e );
        _logger.Debug( query );
        return null;
      }
    }

    public ICursor CreateCursor( string sourceName, int bufferSize = 512 )
    {
      return _querier.CreateCursor( _connection, sourceName, bufferSize );
    }
    
    protected bool ExecuteStatement( string query )
    {
      try
      {
        using var cmd = _connection.CreateCommand();
        cmd.CommandText = query;
        return cmd.ExecuteNonQuery() > 0;
      }
      catch ( DbException e )
      {
        _logger.Error( e );
        // output the query but don't flood the log file with full documents!
        if ( query.Length <= 255 )
          _logger.Debug( query );
        else
          _logger.Debug( $"[truncated]: {query.Substring( 0, 255 )}" );
        
        return false;
      }
    }
  }
}