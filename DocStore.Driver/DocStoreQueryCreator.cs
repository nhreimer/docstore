using System;
using System.Data.Common;
using DocStore.Driver.Configs;
using DocStore.Engine;

using IConvertible = DocStore.Engine.IConvertible;

namespace DocStore.Driver
{
  /// <summary>
  /// Binds the queries in a config to a queryable type
  /// </summary>
  public class DocStoreQueryCreator : IQueryable
  {
    public delegate string DDbStringSanitizer( string s );

    /// <summary>
    /// Creates a TableCursor based on the config that iterates through
    /// Unarchived documents.
    /// </summary>
    private sealed class __DocStoreTableCursor : TableCursor
    {
      private readonly BackendConfig _backendConfig;
      private readonly string _sourceName;
      
      public __DocStoreTableCursor(
        IConvertible converter,
        DbConnection connection,
        int bufferSize,
        BackendConfig backendConfig, 
        string sourceName )
        : base( converter, connection, bufferSize )
      {
        _backendConfig = backendConfig;
        _sourceName = sourceName;
      }

      protected override string GetNextQuery( int lowerbound, int size )
      {
        return string.Format( 
          _backendConfig.Queries[ "cursor" ],
          _backendConfig.Database,
          _backendConfig.Table,
          _sourceName,
          lowerbound, 
          size );
      }
    }
    
    private readonly BackendConfig _backendConfig;
    
    public DocStoreQueryCreator( BackendConfig backendConfig, 
      DDbStringSanitizer sanitizeString )
    {
      _backendConfig = backendConfig;
      SanitizeString = sanitizeString;
    }

    public DDbStringSanitizer SanitizeString { get; private set; }
    
    public StoredDocument Convert( object[] row )
    {
      return new StoredDocument
      {
        MetaData = new MetaDocument
        {
          ObjId = row[ ( int )DocStoreFields.ObjId ].ToString(),
          ObjType = row[ ( int )DocStoreFields.ObjType ].ToString(),
          ObjName = row[ ( int )DocStoreFields.ObjName ].ToString(),
          Created = DateTime.Parse( row[ ( int )DocStoreFields.ObjCreated ].ToString() ),
          Updated = DateTime.Parse( row[ ( int )DocStoreFields.ObjUpdated ].ToString() ),
          IsArchived = System.Convert.ToBoolean( row[ ( int )DocStoreFields.ObjIsArchived ] )
        },
        Data = row[ ( int )DocStoreFields.Data ].ToString()
      };
    }

    public string GetInsertQuery( StoredDocument doc )
    {
      return string.Format( _backendConfig.Queries[ "insert" ],
        _backendConfig.Database,
        _backendConfig.Table,
        doc.MetaData.ObjId,
        SanitizeString( doc.MetaData.ObjType ),
        SanitizeString( doc.MetaData.ObjName ),
        doc.MetaData.Updated.ToString( "yyyy-MM-dd HH:mm:ss" ),
        doc.MetaData.IsArchived,
        SanitizeString( doc.Data ) );
    }

    public string GetUpdateQuery( StoredDocument doc )
    {
      return string.Format( _backendConfig.Queries[ "update" ],
          _backendConfig.Database,
          _backendConfig.Table,
          doc.MetaData.ObjId,
          SanitizeString( doc.MetaData.ObjType ),
          SanitizeString( doc.MetaData.ObjName ),
          doc.MetaData.Updated.ToString( "yyyy-MM-dd HH:mm:ss" ),
          doc.MetaData.IsArchived,
          SanitizeString( doc.Data )
        );
    }

    public string GetDeleteQuery( StoredDocument doc )
    {
      return string.Format( _backendConfig.Queries[ "delete" ],
        _backendConfig.Database,
        _backendConfig.Table,
        doc.MetaData.ObjId
        );
    }

    public string GetDocumentById( string objId )
    {
      return string.Format( _backendConfig.Queries[ "select" ],
        _backendConfig.Database,
        _backendConfig.Table,
        objId
        );
    }

    public TableCursor CreateCursor( DbConnection connection, 
      string sourceName, 
      int bufferSize = 512 )
    {
      return new __DocStoreTableCursor( 
        this, 
        connection, 
        bufferSize, 
        _backendConfig, 
        sourceName );
    }
  }
}