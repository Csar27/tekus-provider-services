/*
 * ============================================================================
 * 02_seed_data.sql
 * Tekus Provider Services - technical test
 * ----------------------------------------------------------------------------
 * Initial data set (>= 10 rows per table). All identifiers are fixed so the
 * relationships between tables are stable and reproducible.
 *
 * Demo credentials (every seeded user):
 *     e-mail    : see [Users] below (e.g. admin@tekus.co)
 *     password  : Tekus.2026!
 *
 * The password hash is a PBKDF2-SHA256 hash with a fixed salt, and is byte-for-byte
 * the same value produced by the code seeder (Tekus.Infrastructure.Persistence.
 * Seeding.DevelopmentSeeder.DemoPasswordHash), so a database seeded by either path
 * accepts the same credentials.
 *
 * Usage:
 *     sqlcmd -S (localdb)\MSSQLLocalDB -i db/02_seed_data.sql
 *
 * The script is idempotent: it only inserts rows for tables that are empty.
 * ============================================================================
 */

USE [TekusProviderServices];
GO

/* --------------------------------------------------------------------------
 * Shared demo password hash (PBKDF2-SHA256, 100000 iterations, fixed salt).
 * Password: Tekus.2026!
 * -------------------------------------------------------------------------- */
DECLARE @PasswordHash nvarchar(512) =
    N'PBKDF2-SHA256$100000$MTIzNDU2Nzg5MGFiY2RlZg==$BqJhyRr44ekSR8RNG1C/VuQ0yQENgPuAUED+TUaJVfQ=';
DECLARE @Utc datetime2 = '2026-01-15T10:00:00';

/* ==========================================================================
 * USERS  (10)
 * Role: 1 = Admin, 2 = Analyst
 * ========================================================================== */
IF NOT EXISTS (SELECT 1 FROM [Users])
BEGIN
    INSERT INTO [Users] ([Id], [Email], [FullName], [PasswordHash], [Role], [IsActive], [CreatedAtUtc], [LastLoginAtUtc], [FailedLoginAttempts], [LockedUntilUtc], [Version]) VALUES
        ('10000000-0000-0000-0000-000000000001', 'admin@tekus.co',       'Tekus Administrator', @PasswordHash, 1, 1, @Utc, NULL, 0, NULL, 1),
        ('10000000-0000-0000-0000-000000000002', 'analyst@tekus.co',     'Tekus Analyst',       @PasswordHash, 2, 1, @Utc, NULL, 0, NULL, 1),
        ('10000000-0000-0000-0000-000000000003', 'maria.lopez@tekus.co', 'Maria Lopez',         @PasswordHash, 2, 1, @Utc, NULL, 0, NULL, 1),
        ('10000000-0000-0000-0000-000000000004', 'carlos.ramirez@tekus.co','Carlos Ramirez',    @PasswordHash, 1, 1, @Utc, NULL, 0, NULL, 1),
        ('10000000-0000-0000-0000-000000000005', 'ana.martinez@tekus.co','Ana Martinez',         @PasswordHash, 2, 1, @Utc, NULL, 0, NULL, 1),
        ('10000000-0000-0000-0000-000000000006', 'luis.garcia@tekus.co', 'Luis Garcia',          @PasswordHash, 2, 1, @Utc, NULL, 0, NULL, 1),
        ('10000000-0000-0000-0000-000000000007', 'sofia.torres@tekus.co','Sofia Torres',         @PasswordHash, 1, 1, @Utc, NULL, 0, NULL, 1),
        ('10000000-0000-0000-0000-000000000008', 'diego.fernandez@tekus.co','Diego Fernandez',  @PasswordHash, 2, 1, @Utc, NULL, 0, NULL, 1),
        ('10000000-0000-0000-0000-000000000009', 'laura.morales@tekus.co','Laura Morales',        @PasswordHash, 2, 1, @Utc, NULL, 0, NULL, 1),
        ('10000000-0000-0000-0000-000000000010', 'andres.castro@tekus.co','Andres Castro',       @PasswordHash, 2, 0, @Utc, NULL, 0, NULL, 1);
END;
GO

/* ==========================================================================
 * COUNTRIES  (12)  - local snapshot of the external catalogue
 * ========================================================================== */
DECLARE @Utc datetime2 = '2026-01-15T10:00:00';
IF NOT EXISTS (SELECT 1 FROM [Countries])
BEGIN
    INSERT INTO [Countries] ([Code], [Name], [Region], [CurrencyCode], [Alpha3Code], [LastSyncedAtUtc]) VALUES
        ('CO', 'Colombia',      'Americas', 'COP', 'COL', @Utc),
        ('MX', 'Mexico',        'Americas', 'MXN', 'MEX', @Utc),
        ('PE', 'Peru',          'Americas', 'PEN', 'PER', @Utc),
        ('CL', 'Chile',         'Americas', 'CLP', 'CHL', @Utc),
        ('AR', 'Argentina',     'Americas', 'ARS', 'ARG', @Utc),
        ('BR', 'Brazil',        'Americas', 'BRL', 'BRA', @Utc),
        ('US', 'United States', 'Americas', 'USD', 'USA', @Utc),
        ('ES', 'Spain',         'Europe',   'EUR', 'ESP', @Utc),
        ('PA', 'Panama',        'Americas', 'PAB', 'PAN', @Utc),
        ('EC', 'Ecuador',       'Americas', 'USD', 'ECU', @Utc),
        ('GT', 'Guatemala',     'Americas', 'GTQ', 'GTM', @Utc),
        ('CR', 'Costa Rica',    'Americas', 'CRC', 'CRI', @Utc);
END;
GO

/* ==========================================================================
 * SERVICES  (12)
 * ========================================================================== */
DECLARE @Utc datetime2 = '2026-01-15T10:00:00';
IF NOT EXISTS (SELECT 1 FROM [Services])
BEGIN
    INSERT INTO [Services] ([Id], [Name], [HourlyRateUsd], [Description], [IsActive], [CreatedAtUtc], [UpdatedAtUtc], [Version]) VALUES
        ('30000000-0000-0000-0000-000000000001', N'Descarga espacial de contenidos',      120.50, N'Descarga de contenidos binarios a través de enlaces orbitales.',           1, @Utc, @Utc, 1),
        ('30000000-0000-0000-0000-000000000002', N'Teletransporte de materia',            450.00, N'Transporte de materia a larga distancia mediante portales estables.',     1, @Utc, @Utc, 1),
        ('30000000-0000-0000-0000-000000000003', N'Análisis de datos interplanetarios',    80.75, N'Procesamiento y análisis de datasets provenientes de sondas.',            1, @Utc, @Utc, 1),
        ('30000000-0000-0000-0000-000000000004', N'Soporte de infraestructura orbital',   210.25, N'Mantenimiento continuo de estaciones y plataformas en órbita.',           1, @Utc, @Utc, 1),
        ('30000000-0000-0000-0000-000000000005', N'Consultoría en tecnología cuántica',   300.00, N'Asesoría especializada en computación y redes cuánticas.',                 1, @Utc, @Utc, 1),
        ('30000000-0000-0000-0000-000000000006', N'Desaparición forzada de bytes',         95.40, N'Borrado seguro e irreversible de datos sensibles.',                       1, @Utc, @Utc, 1),
        ('30000000-0000-0000-0000-000000000007', N'Simulación de agujeros de gusano',      512.30, N'Modelado y simulación de pasarelas topológicas.',                         1, @Utc, @Utc, 1),
        ('30000000-0000-0000-0000-000000000008', N'Mantenimiento de estaciones espaciales',175.00, N'Soporte técnico preventivo y correctivo de estaciones.',                  1, @Utc, @Utc, 1),
        ('30000000-0000-0000-0000-000000000009', N'Traducción simultánea alienígena',       60.90, N'Interpretación en tiempo real de lenguajes no terrestres.',               1, @Utc, @Utc, 1),
        ('30000000-0000-0000-0000-000000000010', N'Seguridad perimetral galáctica',       133.75, N'Vigilancia y defensa de perímetros en el sistema solar.',                1, @Utc, @Utc, 1),
        ('30000000-0000-0000-0000-000000000011', N'Transporte de carga lunar',            289.60, N'Logística de carga entre la Tierra y la superficie lunar.',              1, @Utc, @Utc, 1),
        ('30000000-0000-0000-0000-000000000012', N'Renderizado de nebulosas',              45.20, N'Generación de imágenes de alta fidelidad de nebulosas.',                  1, @Utc, @Utc, 1);
END;
GO

/* ==========================================================================
 * SERVICE COUNTRIES  (36)  - availability of each service per country
 * ========================================================================== */
DECLARE @Utc datetime2 = '2026-01-15T10:00:00';
IF NOT EXISTS (SELECT 1 FROM [ServiceCountries])
BEGIN
    INSERT INTO [ServiceCountries] ([Id], [ServiceId], [CountryCode], [AvailableSinceUtc]) VALUES
        ('60000000-0000-0000-0000-000000000001', '30000000-0000-0000-0000-000000000001', 'CO', @Utc),
        ('60000000-0000-0000-0000-000000000002', '30000000-0000-0000-0000-000000000001', 'MX', @Utc),
        ('60000000-0000-0000-0000-000000000003', '30000000-0000-0000-0000-000000000001', 'PE', @Utc),
        ('60000000-0000-0000-0000-000000000004', '30000000-0000-0000-0000-000000000002', 'CO', @Utc),
        ('60000000-0000-0000-0000-000000000005', '30000000-0000-0000-0000-000000000002', 'CL', @Utc),
        ('60000000-0000-0000-0000-000000000006', '30000000-0000-0000-0000-000000000003', 'CO', @Utc),
        ('60000000-0000-0000-0000-000000000007', '30000000-0000-0000-0000-000000000003', 'MX', @Utc),
        ('60000000-0000-0000-0000-000000000008', '30000000-0000-0000-0000-000000000003', 'PE', @Utc),
        ('60000000-0000-0000-0000-000000000009', '30000000-0000-0000-0000-000000000003', 'CL', @Utc),
        ('60000000-0000-0000-0000-000000000010', '30000000-0000-0000-0000-000000000003', 'AR', @Utc),
        ('60000000-0000-0000-0000-000000000011', '30000000-0000-0000-0000-000000000003', 'BR', @Utc),
        ('60000000-0000-0000-0000-000000000012', '30000000-0000-0000-0000-000000000004', 'US', @Utc),
        ('60000000-0000-0000-0000-000000000013', '30000000-0000-0000-0000-000000000004', 'ES', @Utc),
        ('60000000-0000-0000-0000-000000000014', '30000000-0000-0000-0000-000000000005', 'CO', @Utc),
        ('60000000-0000-0000-0000-000000000015', '30000000-0000-0000-0000-000000000005', 'US', @Utc),
        ('60000000-0000-0000-0000-000000000016', '30000000-0000-0000-0000-000000000006', 'CO', @Utc),
        ('60000000-0000-0000-0000-000000000017', '30000000-0000-0000-0000-000000000006', 'PE', @Utc),
        ('60000000-0000-0000-0000-000000000018', '30000000-0000-0000-0000-000000000006', 'EC', @Utc),
        ('60000000-0000-0000-0000-000000000019', '30000000-0000-0000-0000-000000000007', 'US', @Utc),
        ('60000000-0000-0000-0000-000000000020', '30000000-0000-0000-0000-000000000008', 'CO', @Utc),
        ('60000000-0000-0000-0000-000000000021', '30000000-0000-0000-0000-000000000008', 'MX', @Utc),
        ('60000000-0000-0000-0000-000000000022', '30000000-0000-0000-0000-000000000008', 'PA', @Utc),
        ('60000000-0000-0000-0000-000000000023', '30000000-0000-0000-0000-000000000008', 'CR', @Utc),
        ('60000000-0000-0000-0000-000000000024', '30000000-0000-0000-0000-000000000009', 'ES', @Utc),
        ('60000000-0000-0000-0000-000000000025', '30000000-0000-0000-0000-000000000009', 'MX', @Utc),
        ('60000000-0000-0000-0000-000000000026', '30000000-0000-0000-0000-000000000009', 'AR', @Utc),
        ('60000000-0000-0000-0000-000000000027', '30000000-0000-0000-0000-000000000010', 'CO', @Utc),
        ('60000000-0000-0000-0000-000000000028', '30000000-0000-0000-0000-000000000010', 'US', @Utc),
        ('60000000-0000-0000-0000-000000000029', '30000000-0000-0000-0000-000000000010', 'BR', @Utc),
        ('60000000-0000-0000-0000-000000000030', '30000000-0000-0000-0000-000000000011', 'CL', @Utc),
        ('60000000-0000-0000-0000-000000000031', '30000000-0000-0000-0000-000000000011', 'PE', @Utc),
        ('60000000-0000-0000-0000-000000000032', '30000000-0000-0000-0000-000000000011', 'GT', @Utc),
        ('60000000-0000-0000-0000-000000000033', '30000000-0000-0000-0000-000000000012', 'CO', @Utc),
        ('60000000-0000-0000-0000-000000000034', '30000000-0000-0000-0000-000000000012', 'AR', @Utc),
        ('60000000-0000-0000-0000-000000000035', '30000000-0000-0000-0000-000000000012', 'BR', @Utc),
        ('60000000-0000-0000-0000-000000000036', '30000000-0000-0000-0000-000000000012', 'ES', @Utc);
END;
GO

/* ==========================================================================
 * PROVIDERS  (12)
 * ========================================================================== */
DECLARE @Utc datetime2 = '2026-01-15T10:00:00';
IF NOT EXISTS (SELECT 1 FROM [Providers])
BEGIN
    INSERT INTO [Providers] ([Id], [Nit], [Name], [Email], [IsActive], [CreatedAtUtc], [UpdatedAtUtc], [Version]) VALUES
        ('20000000-0000-0000-0000-000000000001', '900123456-1', N'Servicios Integrales Andinos',   'contacto@andinos.example',           1, @Utc, @Utc, 1),
        ('20000000-0000-0000-0000-000000000002', '901234567-2', N'Corporación Espacial Ltda',      'info@corporacionespacial.example',   1, @Utc, @Utc, 1),
        ('20000000-0000-0000-0000-000000000003', '900987654-3', N'Soluciones Cuánticas S.A.',       'ventas@cuanticas.example',           1, @Utc, @Utc, 1),
        ('20000000-0000-0000-0000-000000000004', '830123456-4', N'Tecnología Orbital SAS',          'hola@orbital.example',               1, @Utc, @Utc, 1),
        ('20000000-0000-0000-0000-000000000005', '900555123-5', N'Ingeniería del Vacío S.A.',       'contacto@vacio.example',             1, @Utc, @Utc, 1),
        ('20000000-0000-0000-0000-000000000006', '860111222-6', N'Logística Interplanetaria',        'ops@interplanetaria.example',        1, @Utc, @Utc, 1),
        ('20000000-0000-0000-0000-000000000007', '901444888-7', N'BioSistemas Andinos',              'info@biosistemas.example',           1, @Utc, @Utc, 1),
        ('20000000-0000-0000-0000-000000000008', '900777333-8', N'Energía Fusion Colombia',          'contacto@fusion.example',            1, @Utc, @Utc, 1),
        ('20000000-0000-0000-0000-000000000009', '900222999-9', N'NanoMateriales del Sur',           'ventas@nanomateriales.example',      1, @Utc, @Utc, 1),
        ('20000000-0000-0000-0000-000000000010', '900333111-0', N'DataMining Latinoamérica',         'bigdata@datamining.example',         1, @Utc, @Utc, 1),
        ('20000000-0000-0000-0000-000000000011', '811000555-1', N'AstroConsulting Group',            'contact@astroconsulting.example',    1, @Utc, @Utc, 1),
        ('20000000-0000-0000-0000-000000000012', '20456789012', N'Quantum Bytes Perú',               'info@quantumperu.example',           1, @Utc, @Utc, 1);
END;
GO

/* ==========================================================================
 * PROVIDER CUSTOM FIELDS  (14)
 * ========================================================================== */
DECLARE @Utc datetime2 = '2026-01-15T10:00:00';
IF NOT EXISTS (SELECT 1 FROM [ProviderCustomFields])
BEGIN
    INSERT INTO [ProviderCustomFields] ([Id], [ProviderId], [Key], [Value], [CreatedAtUtc], [UpdatedAtUtc]) VALUES
        ('40000000-0000-0000-0000-000000000001', '20000000-0000-0000-0000-000000000001', N'Número de contacto en marte', N'+57 555 0101',  @Utc, @Utc),
        ('40000000-0000-0000-0000-000000000002', '20000000-0000-0000-0000-000000000001', N'Sector',                        N'Gobierno',      @Utc, @Utc),
        ('40000000-0000-0000-0000-000000000003', '20000000-0000-0000-0000-000000000002', N'Certificación',                 N'ISO-9001',      @Utc, @Utc),
        ('40000000-0000-0000-0000-000000000004', '20000000-0000-0000-0000-000000000002', N'Año de fundación',              N'2015',          @Utc, @Utc),
        ('40000000-0000-0000-0000-000000000005', '20000000-0000-0000-0000-000000000003', N'Número de contacto en marte', N'+57 555 0103',  @Utc, @Utc),
        ('40000000-0000-0000-0000-000000000006', '20000000-0000-0000-0000-000000000004', N'Certificación',                 N'CMMI-3',        @Utc, @Utc),
        ('40000000-0000-0000-0000-000000000007', '20000000-0000-0000-0000-000000000005', N'Sector',                        N'Manufactura',   @Utc, @Utc),
        ('40000000-0000-0000-0000-000000000008', '20000000-0000-0000-0000-000000000006', N'Régimen',                       N'Gran contribuyente', @Utc, @Utc),
        ('40000000-0000-0000-0000-000000000009', '20000000-0000-0000-0000-000000000007', N'Sector',                        N'Salud',         @Utc, @Utc),
        ('40000000-0000-0000-0000-000000000010', '20000000-0000-0000-0000-000000000008', N'Certificación',                 N'ISO-14001',     @Utc, @Utc),
        ('40000000-0000-0000-0000-000000000011', '20000000-0000-0000-0000-000000000009', N'Sector',                        N'Minería',       @Utc, @Utc),
        ('40000000-0000-0000-0000-000000000012', '20000000-0000-0000-0000-000000000010', N'Sector',                        N'Tecnología',    @Utc, @Utc),
        ('40000000-0000-0000-0000-000000000013', '20000000-0000-0000-0000-000000000011', N'Año de fundación',              N'2008',          @Utc, @Utc),
        ('40000000-0000-0000-0000-000000000014', '20000000-0000-0000-0000-000000000012', N'Número de contacto en marte', N'+51 555 0112',  @Utc, @Utc);
END;
GO

/* ==========================================================================
 * PROVIDER SERVICES  (28)  - which provider offers which service
 * ========================================================================== */
DECLARE @Utc datetime2 = '2026-01-15T10:00:00';
IF NOT EXISTS (SELECT 1 FROM [ProviderServices])
BEGIN
    INSERT INTO [ProviderServices] ([Id], [ProviderId], [ServiceId], [AssignedAtUtc]) VALUES
        ('50000000-0000-0000-0000-000000000001', '20000000-0000-0000-0000-000000000001', '30000000-0000-0000-0000-000000000001', @Utc),
        ('50000000-0000-0000-0000-000000000002', '20000000-0000-0000-0000-000000000001', '30000000-0000-0000-0000-000000000003', @Utc),
        ('50000000-0000-0000-0000-000000000003', '20000000-0000-0000-0000-000000000001', '30000000-0000-0000-0000-000000000006', @Utc),
        ('50000000-0000-0000-0000-000000000004', '20000000-0000-0000-0000-000000000002', '30000000-0000-0000-0000-000000000001', @Utc),
        ('50000000-0000-0000-0000-000000000005', '20000000-0000-0000-0000-000000000002', '30000000-0000-0000-0000-000000000002', @Utc),
        ('50000000-0000-0000-0000-000000000006', '20000000-0000-0000-0000-000000000002', '30000000-0000-0000-0000-000000000008', @Utc),
        ('50000000-0000-0000-0000-000000000007', '20000000-0000-0000-0000-000000000003', '30000000-0000-0000-0000-000000000005', @Utc),
        ('50000000-0000-0000-0000-000000000008', '20000000-0000-0000-0000-000000000003', '30000000-0000-0000-0000-000000000010', @Utc),
        ('50000000-0000-0000-0000-000000000009', '20000000-0000-0000-0000-000000000004', '30000000-0000-0000-0000-000000000004', @Utc),
        ('50000000-0000-0000-0000-000000000010', '20000000-0000-0000-0000-000000000004', '30000000-0000-0000-0000-000000000007', @Utc),
        ('50000000-0000-0000-0000-000000000011', '20000000-0000-0000-0000-000000000005', '30000000-0000-0000-0000-000000000002', @Utc),
        ('50000000-0000-0000-0000-000000000012', '20000000-0000-0000-0000-000000000005', '30000000-0000-0000-0000-000000000011', @Utc),
        ('50000000-0000-0000-0000-000000000013', '20000000-0000-0000-0000-000000000006', '30000000-0000-0000-0000-000000000008', @Utc),
        ('50000000-0000-0000-0000-000000000014', '20000000-0000-0000-0000-000000000006', '30000000-0000-0000-0000-000000000011', @Utc),
        ('50000000-0000-0000-0000-000000000015', '20000000-0000-0000-0000-000000000007', '30000000-0000-0000-0000-000000000003', @Utc),
        ('50000000-0000-0000-0000-000000000016', '20000000-0000-0000-0000-000000000007', '30000000-0000-0000-0000-000000000009', @Utc),
        ('50000000-0000-0000-0000-000000000017', '20000000-0000-0000-0000-000000000008', '30000000-0000-0000-0000-000000000007', @Utc),
        ('50000000-0000-0000-0000-000000000018', '20000000-0000-0000-0000-000000000008', '30000000-0000-0000-0000-000000000010', @Utc),
        ('50000000-0000-0000-0000-000000000019', '20000000-0000-0000-0000-000000000009', '30000000-0000-0000-0000-000000000006', @Utc),
        ('50000000-0000-0000-0000-000000000020', '20000000-0000-0000-0000-000000000009', '30000000-0000-0000-0000-000000000012', @Utc),
        ('50000000-0000-0000-0000-000000000021', '20000000-0000-0000-0000-000000000010', '30000000-0000-0000-0000-000000000003', @Utc),
        ('50000000-0000-0000-0000-000000000022', '20000000-0000-0000-0000-000000000010', '30000000-0000-0000-0000-000000000012', @Utc),
        ('50000000-0000-0000-0000-000000000023', '20000000-0000-0000-0000-000000000011', '30000000-0000-0000-0000-000000000004', @Utc),
        ('50000000-0000-0000-0000-000000000024', '20000000-0000-0000-0000-000000000011', '30000000-0000-0000-0000-000000000005', @Utc),
        ('50000000-0000-0000-0000-000000000025', '20000000-0000-0000-0000-000000000011', '30000000-0000-0000-0000-000000000009', @Utc),
        ('50000000-0000-0000-0000-000000000026', '20000000-0000-0000-0000-000000000012', '30000000-0000-0000-0000-000000000001', @Utc),
        ('50000000-0000-0000-0000-000000000027', '20000000-0000-0000-0000-000000000012', '30000000-0000-0000-0000-000000000003', @Utc),
        ('50000000-0000-0000-0000-000000000028', '20000000-0000-0000-0000-000000000012', '30000000-0000-0000-0000-000000000011', @Utc);
END;
GO

/* ==========================================================================
 * REFRESH TOKENS  (12)  - only SHA-256 hashes are stored
 * Demo plaintexts: demo-refresh-token-01 .. demo-refresh-token-12
 * ========================================================================== */
DECLARE @Utc datetime2 = '2026-01-15T10:00:00';
IF NOT EXISTS (SELECT 1 FROM [RefreshTokens])
BEGIN
    INSERT INTO [RefreshTokens] ([Id], [UserId], [TokenHash], [ExpiresAtUtc], [CreatedAtUtc], [CreatedByIp], [RevokedAtUtc], [RevokedReason], [ReplacedByTokenHash]) VALUES
        ('70000000-0000-0000-0000-000000000001', '10000000-0000-0000-0000-000000000001', 'ZnQX4+rIqZ/XpwVq+eSCXihPkQq5hiYvCL3SrDZZwmE=', DATEADD(DAY, 7, @Utc), @Utc, '127.0.0.1', NULL, NULL, NULL),
        ('70000000-0000-0000-0000-000000000002', '10000000-0000-0000-0000-000000000001', 'xmtEhqs2w8rMIGz4LVikCKA0dbPoVwcGmwvRnbiysFk=', DATEADD(DAY, 7, @Utc), @Utc, '127.0.0.1', NULL, NULL, NULL),
        ('70000000-0000-0000-0000-000000000003', '10000000-0000-0000-0000-000000000001', 'k2MzRX8na7KdF8MIrACRJK71vvdYfUCP8hHurN7ucB8=', DATEADD(DAY, 7, @Utc), @Utc, '127.0.0.1', NULL, NULL, NULL),
        ('70000000-0000-0000-0000-000000000004', '10000000-0000-0000-0000-000000000002', '+otCCHeQ0IFfGDsHn9PLV41RPNy0L/mhpQKwbAzvulw=', DATEADD(DAY, 7, @Utc), @Utc, '127.0.0.1', NULL, NULL, NULL),
        ('70000000-0000-0000-0000-000000000005', '10000000-0000-0000-0000-000000000002', 'SxLniM7I9sCQ0W8iRUjmKMNchQY9Yx9Py1Hf9Bl02iY=', DATEADD(DAY, 7, @Utc), @Utc, '127.0.0.1', NULL, NULL, NULL),
        ('70000000-0000-0000-0000-000000000006', '10000000-0000-0000-0000-000000000002', 'Jkv2xnB4KmkHT1M5n32aB0/BRB91yGyqZBz3BspvJT8=', DATEADD(DAY, 7, @Utc), @Utc, '127.0.0.1', NULL, NULL, NULL),
        ('70000000-0000-0000-0000-000000000007', '10000000-0000-0000-0000-000000000003', 'srPytVPZ7EYzZtciI3uDBbj1LJeu6Z0P5t4rTOIo3BA=', DATEADD(DAY, 7, @Utc), @Utc, '127.0.0.1', NULL, NULL, NULL),
        ('70000000-0000-0000-0000-000000000008', '10000000-0000-0000-0000-000000000003', 's6NYy5eiUA1xnVziJovogO1rldqKvEdFRwqfKAda4CE=', DATEADD(DAY, 7, @Utc), @Utc, '127.0.0.1', NULL, NULL, NULL),
        ('70000000-0000-0000-0000-000000000009', '10000000-0000-0000-0000-000000000004', 'T79jO+ZtmyQtnXOZGbSwcVaEz9a7IXgaPU3XXrAct+E=', DATEADD(DAY, 7, @Utc), @Utc, '127.0.0.1', NULL, NULL, NULL),
        ('70000000-0000-0000-0000-000000000010', '10000000-0000-0000-0000-000000000004', 'UF5AeUPYt7lu21yfMa6XhZrxumoIsZWSqN4c3hNoaAI=', DATEADD(DAY, 7, @Utc), @Utc, '127.0.0.1', NULL, NULL, NULL),
        ('70000000-0000-0000-0000-000000000011', '10000000-0000-0000-0000-000000000005', '7hYc8XecSFn/92jQtSXbHHGB75j4yWtFC3dXGT63Vw0=', DATEADD(DAY, 7, @Utc), @Utc, '127.0.0.1', NULL, NULL, NULL),
        ('70000000-0000-0000-0000-000000000012', '10000000-0000-0000-0000-000000000005', 'nffrTvcpi/XvvFUNF9hI4s7+5oUHeEbBS1it14gajTU=', DATEADD(DAY, 7, @Utc), @Utc, '127.0.0.1', NULL, NULL, NULL);
END;
GO

/* ==========================================================================
 * AUDIT LOGS  (12)  - one row per domain event
 * ========================================================================== */
DECLARE @Utc datetime2 = '2026-01-15T10:00:00';
IF NOT EXISTS (SELECT 1 FROM [AuditLogs])
BEGIN
    INSERT INTO [AuditLogs] ([Id], [OccurredAtUtc], [EventType], [AggregateType], [AggregateId], [Summary]) VALUES
        ('80000000-0000-0000-0000-000000000001', @Utc, 'provider.created', 'Provider', '20000000-0000-0000-0000-000000000001', N'Provider ''Servicios Integrales Andinos'' (NIT 900123456-1) was created.'),
        ('80000000-0000-0000-0000-000000000002', @Utc, 'provider.custom_field_changed', 'Provider', '20000000-0000-0000-0000-000000000001', N'Custom field ''Sector'' of provider 20000000-0000-0000-0000-000000000001 was changed.'),
        ('80000000-0000-0000-0000-000000000003', @Utc, 'provider.service_assigned', 'Provider', '20000000-0000-0000-0000-000000000001', N'Service 30000000-0000-0000-0000-000000000001 was assigned to provider 20000000-0000-0000-0000-000000000001.'),
        ('80000000-0000-0000-0000-000000000004', @Utc, 'provider.created', 'Provider', '20000000-0000-0000-0000-000000000002', N'Provider ''Corporación Espacial Ltda'' (NIT 901234567-2) was created.'),
        ('80000000-0000-0000-0000-000000000005', @Utc, 'service.created', 'Service', '30000000-0000-0000-0000-000000000001', N'Service ''Descarga espacial de contenidos'' was created.'),
        ('80000000-0000-0000-0000-000000000006', @Utc, 'service.countries_changed', 'Service', '30000000-0000-0000-0000-000000000001', N'Availability of service 30000000-0000-0000-0000-000000000001 changed for country CO.'),
        ('80000000-0000-0000-0000-000000000007', @Utc, 'service.created', 'Service', '30000000-0000-0000-0000-000000000003', N'Service ''Análisis de datos interplanetarios'' was created.'),
        ('80000000-0000-0000-0000-000000000008', @Utc, 'service.countries_changed', 'Service', '30000000-0000-0000-0000-000000000003', N'Availability of service 30000000-0000-0000-0000-000000000003 changed for country BR.'),
        ('80000000-0000-0000-0000-000000000009', @Utc, 'provider.updated', 'Provider', '20000000-0000-0000-0000-000000000005', N'Provider ''Ingeniería del Vacío S.A.'' (NIT 900555123-5) was updated.'),
        ('80000000-0000-0000-0000-000000000010', @Utc, 'service.updated', 'Service', '30000000-0000-0000-0000-000000000007', N'Service ''Simulación de agujeros de gusano'' was updated.'),
        ('80000000-0000-0000-0000-000000000011', @Utc, 'provider.service_removed', 'Provider', '20000000-0000-0000-0000-000000000011', N'Service 30000000-0000-0000-0000-000000000006 was removed from provider 20000000-0000-0000-0000-000000000011.'),
        ('80000000-0000-0000-0000-000000000012', @Utc, 'provider.created', 'Provider', '20000000-0000-0000-0000-000000000012', N'Provider ''Quantum Bytes Perú'' (NIT 20456789012) was created.');
END;
GO

/* --------------------------------------------------------------------------
 * Row counts (sanity check)
 * -------------------------------------------------------------------------- */
SELECT 'Users'                 AS [Table], COUNT(*) AS [Rows] FROM [Users]
UNION ALL SELECT 'Countries',             COUNT(*) FROM [Countries]
UNION ALL SELECT 'Services',             COUNT(*) FROM [Services]
UNION ALL SELECT 'ServiceCountries',     COUNT(*) FROM [ServiceCountries]
UNION ALL SELECT 'Providers',            COUNT(*) FROM [Providers]
UNION ALL SELECT 'ProviderCustomFields', COUNT(*) FROM [ProviderCustomFields]
UNION ALL SELECT 'ProviderServices',     COUNT(*) FROM [ProviderServices]
UNION ALL SELECT 'RefreshTokens',        COUNT(*) FROM [RefreshTokens]
UNION ALL SELECT 'AuditLogs',            COUNT(*) FROM [AuditLogs];
GO
