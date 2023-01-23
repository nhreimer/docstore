namespace DocStore.Driver.Parsing
{
  public interface ITransformable< in TParams, out TResult >
  {
    TResult Transform( TParams data );
  }
}