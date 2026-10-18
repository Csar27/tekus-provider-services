/*
 * ============================================================================
 * 00_create_database.sql
 * Tekus Provider Services - technical test
 * ----------------------------------------------------------------------------
 * Creates the database (and the login, if needed) that the rest of the scripts
 * expect. Safe to run more than once.
 *
 * Usage:
 *     sqlcmd -S (localdb)\MSSQLLocalDB -i db/00_create_database.sql
 * ============================================================================
 */

IF DB_ID(N'TekusProviderServices') IS NULL
BEGIN
    CREATE DATABASE [TekusProviderServices];
END;
GO

-- Report the size once the database exists (handy sanity check).
USE [TekusProviderServices];
SELECT
    DB_NAME()                                AS [Database],
    CAST(SUM(size) * 8.0 / 1024 AS decimal(18,2)) AS [SizeMb]
FROM sys.database_files
GROUP BY type_desc;
GO
