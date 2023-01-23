using System.Data.Common;
using System.Collections.Generic;

namespace DocStore.Engine
{
  public abstract class TableCursor : ICursor
  {
    private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

    private readonly IConvertible _converter;
    private readonly DbConnection _connection;
    private readonly int _bufferSize;
    
    private int _multiplier;
    private int _cursor;
    
    private IList< StoredDocument > _buffer;

    public TableCursor( IConvertible converter,
                        DbConnection connection, 
                        int bufferSize )
    {
      _converter = converter;
      _connection = connection;
      _bufferSize = bufferSize;
      
      // do NOT fill the buffer in the constructor
    }
    
    public uint Count { get; private set; } 
    
    public StoredDocument Next()
    {
      // this is the initial buffer fill
      if ( _buffer == null )
      {
        _buffer = new List< StoredDocument >();
        FillBuffer();
      }

      // this indicates that the source is exhausted
      if ( _buffer.Count == 0 )
        return default;
      
      // keep retrieving documents until our buffer is exhausted
      if ( _cursor >= _buffer.Count )
      {
        FillBuffer();
        _cursor = 1;
      }
      else
        ++_cursor;

      return _buffer.Count == 0 ? null : _buffer[ _cursor - 1 ];
    }

    protected abstract string GetNextQuery( int lowerbound, int size );

    private void FillBuffer()
    {
      _buffer.Clear();
      var lowerbound = _multiplier * _bufferSize;

      logger.Info( $"Buffer: {lowerbound} - {lowerbound + _bufferSize}" );
      
      using var cmd = _connection.CreateCommand();
      cmd.CommandText = GetNextQuery( lowerbound, _bufferSize );

      using var results = cmd.ExecuteReader();
      while ( results.Read() )
      {
        var fields = new object[ results.FieldCount ];
        results.GetValues( fields );

        _buffer.Add( _converter.Convert( fields ) );
        
        ++Count;
      }
      
      ++_multiplier;
    }
  }
}