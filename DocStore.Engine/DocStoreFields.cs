namespace DocStore.Engine
{
  public enum DocStoreFields
  {
    // `objId` CHAR(40) NOT NULL COLLATE 'utf8_general_ci',
    // `type` VARCHAR(50) NOT NULL COLLATE 'utf8_general_ci',
    // `name` VARCHAR(255) NOT NULL COLLATE 'utf8_general_ci',
    // `created` TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
    // `updated` TIMESTAMP NOT NULL DEFAULT '0000-00-00 00:00:00',
    // `isArchived` TINYINT(4) NOT NULL,
    // `data` MEDIUMBLOB NOT NULL,
    
    ObjId = 0,
    ObjType = 1,
    ObjName = 2,
    ObjCreated = 3,
    ObjUpdated = 4,
    ObjIsArchived = 5,
    Data = 6
  }
}