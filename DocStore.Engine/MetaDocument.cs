using System;

namespace DocStore.Engine
{
  /// <summary>
  /// The meta data for the obj store
  /// </summary>
  public class MetaDocument
  {
    public string ObjId { get; set; }
    public string ObjType { get; set; }
    public string ObjName { get; set; }
    public bool IsArchived { get; set; }
    public DateTime Created { get; set; }
    public DateTime Updated { get; set; }
  }
}