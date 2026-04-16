-- Add Amount columns to CompetentPerson and CompetentEquipment tables
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('CompetentPersonRegistrations') AND name = 'Amount')
BEGIN
    ALTER TABLE CompetentPersonRegistrations ADD Amount DECIMAL(18,2) NOT NULL DEFAULT 0;
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('CompetentEquipmentRegistrations') AND name = 'Amount')
BEGIN
    ALTER TABLE CompetentEquipmentRegistrations ADD Amount DECIMAL(18,2) NOT NULL DEFAULT 0;
END
GO
