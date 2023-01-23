namespace DocStore.Driver.Requesters
{
  /// <summary>
  /// Requests a resource 
  /// </summary>
  public interface IRequestable
  {
    public string Request( string resourceLocation, int waitInMilliseconds );
  }
}