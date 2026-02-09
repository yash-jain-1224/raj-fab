-- Add AmendmentCount column to FactoryMapApprovals
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'FactoryMapApprovals' AND COLUMN_NAME = 'AmendmentCount')
BEGIN
    ALTER TABLE FactoryMapApprovals ADD AmendmentCount INT NOT NULL DEFAULT 0;
END

-- Add AmendmentCount column to LicenseRenewals  
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'LicenseRenewals' AND COLUMN_NAME = 'AmendmentCount')
BEGIN
    ALTER TABLE LicenseRenewals ADD AmendmentCount INT NOT NULL DEFAULT 0;
END

-- Add AmendmentCount column to FactoryRegistrations
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'FactoryRegistrations' AND COLUMN_NAME = 'AmendmentCount')
BEGIN
    ALTER TABLE FactoryRegistrations ADD AmendmentCount INT NOT NULL DEFAULT 0;
END
