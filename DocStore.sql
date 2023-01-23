CREATE TABLE `objstore` (
	`objId` CHAR(40) NOT NULL COMMENT 'hash of name' COLLATE 'utf8_general_ci',
	`type` VARCHAR(50) NOT NULL COMMENT 'object type' COLLATE 'utf8_general_ci',
	`name` VARCHAR(255) NOT NULL COMMENT 'unique resource name' COLLATE 'utf8_general_ci',
	`created` TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP COMMENT 'insertion timestamp',
	`updated` TIMESTAMP NOT NULL DEFAULT '0000-00-00 00:00:00' COMMENT 'last updated',
	`isArchived` TINYINT(1) NOT NULL COMMENT 'indicates whether it\'s in a final state',
	`data` MEDIUMTEXT NOT NULL COMMENT 'serialized binary object' COLLATE 'utf8_general_ci',
	PRIMARY KEY (`objId`) USING BTREE,
	INDEX `objstore_type_idx` (`type`) USING BTREE
)
COMMENT='key/value object store. modeled after filesystem.'
COLLATE='utf8_general_ci'
ENGINE=InnoDB
ROW_FORMAT=COMPRESSED
;
