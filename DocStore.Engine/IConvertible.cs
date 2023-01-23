namespace DocStore.Engine
{
  public interface IConvertible
  {
    StoredDocument Convert( object[] row );
  }
}