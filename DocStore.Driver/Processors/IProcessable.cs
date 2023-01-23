namespace DocStore.Driver.Parsing
{
  public interface IProcessable< in TParam >
  {
    void Process( TParam data );
  }
}