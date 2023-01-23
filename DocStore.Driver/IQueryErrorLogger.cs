namespace DocStore.Driver
{
  public interface IQueryErrorLogger
  {
    void DumpContents( string errorType, string sourceName, string contents );
  }
}