-- ============================================
-- FIX PRODUCTS TABLE FOR SIOMS
-- ============================================
PRINT '🔧 FIXING PRODUCTS TABLE...';
PRINT '=================================';
GO

-- 1. Show current table structure
PRINT '1. Current Products table structure:';
SELECT 
    COLUMN_NAME, 
    DATA_TYPE,
    IS_NULLABLE,
    COLUMN_DEFAULT as DefaultValue
FROM INFORMATION_SCHEMA.COLUMNS 
WHERE TABLE_NAME = 'Products'
ORDER BY ORDINAL_POSITION;
GO

-- 2. Check what columns exist
PRINT '';
PRINT '2. Checking existing columns...';
IF EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Products' AND COLUMN_NAME = 'StockQuantity')
    PRINT '   StockQuantity: EXISTS';
ELSE
    PRINT '   StockQuantity: DOES NOT EXIST';

IF EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Products' AND COLUMN_NAME = 'CurrentStock')
    PRINT '   CurrentStock: EXISTS';
ELSE
    PRINT '   CurrentStock: DOES NOT EXIST';

IF EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Products' AND COLUMN_NAME = 'ReorderLevel')
    PRINT '   ReorderLevel: EXISTS';
ELSE
    PRINT '   ReorderLevel: DOES NOT EXIST';
GO

-- 3. CREATE StockQuantity IF IT DOESN'T EXIST
PRINT '';
PRINT '3. Ensuring StockQuantity column exists...';
IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Products' AND COLUMN_NAME = 'StockQuantity')
BEGIN
    PRINT '   Creating StockQuantity column...';
    ALTER TABLE Products ADD StockQuantity INT NOT NULL DEFAULT 0;
    PRINT '   ✓ StockQuantity column created (DEFAULT: 0)';
END
ELSE
BEGIN
    PRINT '   ✓ StockQuantity column already exists';
END
GO

-- 4. MIGRATE DATA from CurrentStock to StockQuantity if both exist
PRINT '';
PRINT '4. Migrating data if needed...';
IF EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Products' AND COLUMN_NAME = 'CurrentStock')
   AND EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Products' AND COLUMN_NAME = 'StockQuantity')
BEGIN
    DECLARE @MigrationSql NVARCHAR(MAX);
    SET @MigrationSql = N'UPDATE Products SET StockQuantity = CurrentStock WHERE CurrentStock > 0 AND StockQuantity = 0';
    EXEC sp_executesql @MigrationSql;
    
    DECLARE @RowsAffected INT = @@ROWCOUNT;
    PRINT '   ✓ Migrated ' + CAST(@RowsAffected AS NVARCHAR(10)) + ' records from CurrentStock to StockQuantity';
END
ELSE
BEGIN
    PRINT '   ✓ No data migration needed';
END
GO

-- 5. CREATE ReorderLevel IF IT DOESN'T EXIST
PRINT '';
PRINT '5. Ensuring ReorderLevel column exists...';
IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Products' AND COLUMN_NAME = 'ReorderLevel')
BEGIN
    PRINT '   Creating ReorderLevel column...';
    ALTER TABLE Products ADD ReorderLevel INT NOT NULL DEFAULT 0;
    PRINT '   ✓ ReorderLevel column created (DEFAULT: 0)';
END
ELSE
BEGIN
    PRINT '   ✓ ReorderLevel column already exists';
END
GO

-- 6. REMOVE CurrentStock column if it exists
PRINT '';
PRINT '6. Removing CurrentStock column...';
IF EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Products' AND COLUMN_NAME = 'CurrentStock')
BEGIN
    -- First check if data is the same
    DECLARE @CheckSql NVARCHAR(MAX);
    SET @CheckSql = N'
        DECLARE @Diff INT;
        SELECT @Diff = COUNT(*) FROM Products WHERE CurrentStock > 0 AND CurrentStock <> ISNULL(StockQuantity, 0);
        IF @Diff = 0 
        BEGIN 
            ALTER TABLE Products DROP COLUMN CurrentStock;
            SELECT ''REMOVED'' as Result;
        END
        ELSE
            SELECT ''KEPT - '' + CAST(@Diff as NVARCHAR(10)) + '' rows differ'' as Result;
    ';
    
    DECLARE @ResultTable TABLE (Result NVARCHAR(100));
    INSERT INTO @ResultTable EXEC sp_executesql @CheckSql;
    
    DECLARE @ResultMsg NVARCHAR(100);
    SELECT @ResultMsg = Result FROM @ResultTable;
    
    PRINT '   Result: ' + @ResultMsg;
END
ELSE
BEGIN
    PRINT '   ✓ CurrentStock column does not exist';
END
GO

-- 7. ADD DEFAULT CONSTRAINTS
PRINT '';
PRINT '7. Adding default constraints...';

-- CreatedDate - Check if has default ANYWHERE (not just named constraint)
IF EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS 
           WHERE TABLE_NAME = 'Products' AND COLUMN_NAME = 'CreatedDate' 
           AND COLUMN_DEFAULT IS NULL)
BEGIN
    -- Only add if no default exists
    ALTER TABLE Products ADD CONSTRAINT DF_Products_CreatedDate DEFAULT GETDATE() FOR CreatedDate;
    PRINT '   ✓ Added default for CreatedDate';
END
ELSE
    PRINT '   ✓ CreatedDate already has default';

-- StockQuantity - Only if column exists AND has no default
IF EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS 
           WHERE TABLE_NAME = 'Products' AND COLUMN_NAME = 'StockQuantity')
BEGIN
    IF EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS 
               WHERE TABLE_NAME = 'Products' AND COLUMN_NAME = 'StockQuantity' 
               AND COLUMN_DEFAULT IS NULL)
    BEGIN
        ALTER TABLE Products ADD CONSTRAINT DF_Products_StockQuantity DEFAULT 0 FOR StockQuantity;
        PRINT '   ✓ Added default for StockQuantity';
    END
    ELSE
        PRINT '   ✓ StockQuantity already has default';
END

-- ReorderLevel - Only if column exists AND has no default
IF EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS 
           WHERE TABLE_NAME = 'Products' AND COLUMN_NAME = 'ReorderLevel')
BEGIN
    IF EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS 
               WHERE TABLE_NAME = 'Products' AND COLUMN_NAME = 'ReorderLevel' 
               AND COLUMN_DEFAULT IS NULL)
    BEGIN
        ALTER TABLE Products ADD CONSTRAINT DF_Products_ReorderLevel DEFAULT 0 FOR ReorderLevel;
        PRINT '   ✓ Added default for ReorderLevel';
    END
    ELSE
        PRINT '   ✓ ReorderLevel already has default';
END
GO

-- 8. SHOW FINAL STRUCTURE
PRINT '';
PRINT '8. FINAL TABLE STRUCTURE:';
SELECT 
    COLUMN_NAME, 
    DATA_TYPE,
    IS_NULLABLE,
    COLUMN_DEFAULT as DefaultValue
FROM INFORMATION_SCHEMA.COLUMNS 
WHERE TABLE_NAME = 'Products'
ORDER BY ORDINAL_POSITION;
GO

-- 9. TEST QUERIES
PRINT '';
PRINT '9. Testing queries...';
BEGIN TRY
    -- Test 1: Basic select
    IF EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Products' AND COLUMN_NAME = 'StockQuantity')
    BEGIN
        PRINT '   Test 1 - Basic StockQuantity query:';
        DECLARE @Test1 NVARCHAR(MAX) = N'SELECT TOP 3 ProductId, Name, StockQuantity FROM Products ORDER BY ProductId';
        EXEC sp_executesql @Test1;
    END
    
    -- Test 2: Complex query
    IF EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Products' AND COLUMN_NAME = 'StockQuantity')
    AND EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Products' AND COLUMN_NAME = 'ReorderLevel')
    BEGIN
        PRINT '';
        PRINT '   Test 2 - Complex query with CASE:';
        DECLARE @Test2 NVARCHAR(MAX) = N'
            SELECT TOP 3 
                ProductId, 
                Name, 
                StockQuantity, 
                ReorderLevel,
                CASE 
                    WHEN StockQuantity <= ReorderLevel THEN ''LOW STOCK''
                    ELSE ''OK''
                END as Status
            FROM Products 
            ORDER BY ProductId';
        EXEC sp_executesql @Test2;
        PRINT '   ✅ All queries executed successfully!';
    END
END TRY
BEGIN CATCH
    PRINT '   ❌ Error: ' + ERROR_MESSAGE();
END CATCH
GO

PRINT '';
PRINT '=======================================';
PRINT '✅ DATABASE FIX COMPLETED SUCCESSFULLY!';
PRINT '=======================================';