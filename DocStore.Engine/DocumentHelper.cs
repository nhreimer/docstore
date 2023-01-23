using System;
using System.Security.Cryptography;

namespace DocStore.Engine
{
  public static class DocumentHelper
  {
    public static StoredDocument Create( string data, string type, string name )
    {
      return new StoredDocument
      {
        MetaData = new MetaDocument
        {
          ObjId = HashIdGenerator.GetChecksumString( new SHA1Managed(), name ),
          ObjType = type,
          ObjName = name,
          Updated = DateTime.Now,
          Created = DateTime.Now,
          IsArchived = false
        },
        Data = data
      };
    }
  }
}