using System;
using System.Security.Cryptography;

namespace DocStore.Engine
{
  public class HashIdGenerator 
  {
    public static string CreateObjId( string input )
    {
      using var hash = new SHA1Managed();
      hash.Initialize();
      return GetChecksumString( hash, input );
    }
    
    public static byte[] GetChecksum( HashAlgorithm hash, string input )
    {
      hash.Initialize();
      var inputByteArray = System.Text.Encoding.UTF8.GetBytes( input );
      return hash.ComputeHash( inputByteArray );
    }

    public static string GetChecksumString( HashAlgorithm hash, string input )
    {
      hash.Initialize();
      var checksum = GetChecksum( hash, input );
      return BitConverter.ToString( checksum ).Replace( "-", string.Empty ).ToLowerInvariant();
    }
  }
}