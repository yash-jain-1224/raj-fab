-- Add PDF URL columns to CompetentPersonRegistrations
IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'CompetentPersonRegistrations' AND COLUMN_NAME = 'ApplicationPDFUrl')
    ALTER TABLE CompetentPersonRegistrations ADD ApplicationPDFUrl NVARCHAR(500) NULL;

IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'CompetentPersonRegistrations' AND COLUMN_NAME = 'CertificateUrl')
    ALTER TABLE CompetentPersonRegistrations ADD CertificateUrl NVARCHAR(500) NULL;

IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'CompetentPersonRegistrations' AND COLUMN_NAME = 'ObjectionLetterUrl')
    ALTER TABLE CompetentPersonRegistrations ADD ObjectionLetterUrl NVARCHAR(500) NULL;

-- Add PDF URL columns to CompetentEquipmentRegistrations
IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'CompetentEquipmentRegistrations' AND COLUMN_NAME = 'ApplicationPDFUrl')
    ALTER TABLE CompetentEquipmentRegistrations ADD ApplicationPDFUrl NVARCHAR(500) NULL;

IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'CompetentEquipmentRegistrations' AND COLUMN_NAME = 'CertificateUrl')
    ALTER TABLE CompetentEquipmentRegistrations ADD CertificateUrl NVARCHAR(500) NULL;

IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'CompetentEquipmentRegistrations' AND COLUMN_NAME = 'ObjectionLetterUrl')
    ALTER TABLE CompetentEquipmentRegistrations ADD ObjectionLetterUrl NVARCHAR(500) NULL;
