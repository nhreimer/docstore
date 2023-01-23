namespace DocStore.Driver.Configs
{
  public class MainConfig
  {
    /// <summary>
    /// A config file that contains configurations for sources
    /// </summary>
    public string SourcesConfig { get; set; }
    
    /// <summary>
    /// The database configurations
    /// </summary>
    public string BackendConfig { get; set; }

    /// <summary>
    /// The config for errors in parsing
    /// </summary>
    public string ErrorConfig { get; set; }
  }
}