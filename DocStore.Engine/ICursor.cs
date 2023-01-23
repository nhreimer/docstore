namespace DocStore.Engine
{
  public interface ICursor
  {
    /// <summary>
    /// Numbers of records processed so far. This is NOT a count of the
    /// total number of records
    /// </summary>
    uint Count { get; }
    
    /// <summary>
    /// Gets the next document available
    /// </summary>
    /// <returns></returns>
    StoredDocument Next();
  }
}