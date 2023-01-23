using System;

namespace Trident.Core
{
  public class MetaDataDocument
  {
    // `idFeed` CHAR(24) NOT NULL COLLATE 'utf8_general_ci',
    // `url` TINYTEXT NOT NULL COLLATE 'utf8_general_ci',
    // `published` DATE NOT NULL,
    // `title` TEXT NOT NULL COLLATE 'utf8_general_ci',
    // `updated` DATE NOT NULL,
    
    /// <summary>
    /// Was a mongoid from the legacy system. this has been converted into sha1
    /// checksum based on the url
    /// </summary>
    public string IDFeed { get; set; }
    
    /// <summary>
    /// Source URL
    /// </summary>
    public string Url { get; set; }
    
    /// <summary>
    /// Date the article was published (if it can't be found then use the date
    /// that it was obtained)
    /// </summary>
    public DateTime Published { get; set; }
    
    /// <summary>
    /// Title of the article or byline
    /// </summary>
    public string Title { get; set; }
    
    /// <summary>
    /// DateTime the article was last updated. This is largely
    /// useless nowadays, so it should mirror the published date.
    /// </summary>
    public DateTime Updated { get; set; }
  }
}