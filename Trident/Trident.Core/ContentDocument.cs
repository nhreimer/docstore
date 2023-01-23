﻿using System;

namespace Trident.Core
{
  public class ContentDocument
  {
    // `ID` INT(10) UNSIGNED NOT NULL AUTO_INCREMENT,
    // `idSource` TINYINT(3) UNSIGNED NOT NULL,
    // `idFeed` CHAR(24) NOT NULL COLLATE 'utf8_general_ci',
    // `CreatedTimeStamp` TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    // `SearchParagraph` TEXT NOT NULL COLLATE 'utf8_general_ci',
    // `RetrieveChecksum` CHAR(64) NOT NULL COLLATE 'utf8_general_ci',
    // `RetrieveParagraph` TEXT NOT NULL COLLATE 'utf8_general_ci',
    
    public int ParagraphId { get; set; } // autogenerated
    public int SourceId { get; set; } // foreign key in Sources
    public string FeedId { get; set; } // foreign key in Master Article, this is now a checksum based on the url
    public DateTime CreatedTimeStamp { get; set; }  // DateTime.Now
    public string SearchParagraph { get; set; }     // cleaned paragraph
    
    public string RetrieveChecksum { get; set; } // checksum of the RetrieveParagraph (Sha256)
    
    public string RetrieveParagraph { get; set; }   // original paragraph
  }
}