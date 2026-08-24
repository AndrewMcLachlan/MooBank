/*
    Demo account backfill, part 1 of 6. Run first: parts 2, 3 and 6 tag the rows they write with
    the tags created here.

    Creates the tags the purpose-driven reports classify by, and points the Mortgage, Super and
    Savings accounts at them. Principal vs Interest, Super Contributions, Super Returns and Savings
    Interest all read AccountTagPurpose, so they render nothing until these rows exist regardless of
    what transactions an account holds.

    Idempotent: an existing tag is reused and an existing purpose is left alone.

    Run by hand against production. Not referenced by the .sqlproj.
*/

SET NOCOUNT ON;
SET XACT_ABORT ON;

DECLARE @FamilyId UNIQUEIDENTIFIER = 'B0DDD93D-827F-4716-B4E2-D1922FAF7E27';

BEGIN TRAN;

IF NOT EXISTS (SELECT 1 FROM dbo.Family WHERE Id = @FamilyId)
    THROW 50000, 'Demo family not found. Check @FamilyId.', 1;

/*
    An instrument belongs to a family through its owner, so resolving one by name means joining out
    to the owning user. Names are resolved rather than hard-coded as ids so the scripts survive a
    database restore. A name matching more than once is left unresolved and reported, rather than
    picked between.
*/
DECLARE @Accounts TABLE ([Key] VARCHAR(20) NOT NULL PRIMARY KEY, [Name] NVARCHAR(50) NOT NULL, InstrumentId UNIQUEIDENTIFIER NULL);

INSERT INTO @Accounts ([Key], [Name])
VALUES ('Mortgage', 'Mortgage'), ('Super', 'Super'), ('Savings', 'Savings Account');

UPDATE a
SET InstrumentId = f.Id
FROM @Accounts a
INNER JOIN (
    SELECT i.[Name], MIN(i.Id) AS Id, COUNT(*) AS Matches
    FROM dbo.Instrument i
    WHERE EXISTS (
        SELECT 1
        FROM dbo.InstrumentOwner io
        INNER JOIN dbo.[User] u ON u.Id = io.UserId
        WHERE io.InstrumentId = i.Id AND u.FamilyId = @FamilyId)
    GROUP BY i.[Name]
) f ON f.[Name] = a.[Name] AND f.Matches = 1;

IF EXISTS (SELECT 1 FROM @Accounts WHERE InstrumentId IS NULL)
BEGIN
    DECLARE @Missing NVARCHAR(400) = (SELECT STRING_AGG([Name], ', ') FROM @Accounts WHERE InstrumentId IS NULL);
    DECLARE @MissingMessage NVARCHAR(600) = CONCAT('Demo accounts not found, or matched more than once: ', @Missing);
    THROW 50000, @MissingMessage, 1;
END

/*
    A tag is invisible to the reports without a TagSettings row: GetMonthlyTotalsForTag builds its
    eligible-tag set by joining Tag to TagSettings, so an unconfigured tag drops out of the join and
    the series comes back empty.
*/
DECLARE @TagNames TABLE ([Name] NVARCHAR(50) NOT NULL PRIMARY KEY);

INSERT INTO @TagNames ([Name])
VALUES ('Mortgage Interest'), ('Employer Contribution'), ('Personal Contribution'), ('Interest');

INSERT INTO dbo.Tag ([Name], FamilyId)
SELECT n.[Name], @FamilyId
FROM @TagNames n
WHERE NOT EXISTS (
    SELECT 1 FROM dbo.Tag t
    WHERE t.[Name] = n.[Name] AND t.FamilyId = @FamilyId AND t.Deleted = 0);

INSERT INTO dbo.TagSettings (TagId)
SELECT t.Id
FROM dbo.Tag t
INNER JOIN @TagNames n ON n.[Name] = t.[Name]
WHERE t.FamilyId = @FamilyId AND t.Deleted = 0
  AND NOT EXISTS (SELECT 1 FROM dbo.TagSettings s WHERE s.TagId = t.Id);

/*
    AccountTagPurpose is keyed on (InstrumentId, Purpose) and hangs off LogicalAccount, so an
    account that is not a logical account cannot carry one.
*/
DECLARE @Purposes TABLE (InstrumentId UNIQUEIDENTIFIER NOT NULL, Purpose TINYINT NOT NULL, TagId INT NOT NULL, PRIMARY KEY (InstrumentId, Purpose));

INSERT INTO @Purposes (InstrumentId, Purpose, TagId)
SELECT a.InstrumentId, p.Purpose, t.Id
FROM (VALUES
    ('Mortgage', CAST(4 AS TINYINT), N'Mortgage Interest'),      -- TagPurpose.MortgageInterest
    ('Super',    CAST(2 AS TINYINT), N'Employer Contribution'),  -- TagPurpose.EmployerContribution
    ('Super',    CAST(3 AS TINYINT), N'Personal Contribution'),  -- TagPurpose.PersonalContribution
    ('Savings',  CAST(1 AS TINYINT), N'Interest')                -- TagPurpose.Interest
) p ([Key], Purpose, TagName)
INNER JOIN @Accounts a ON a.[Key] = p.[Key]
INNER JOIN dbo.Tag t ON t.[Name] = p.TagName AND t.FamilyId = @FamilyId AND t.Deleted = 0;

IF EXISTS (SELECT 1 FROM @Purposes p WHERE NOT EXISTS (SELECT 1 FROM dbo.LogicalAccount la WHERE la.InstrumentId = p.InstrumentId))
    THROW 50000, 'A demo account targeted for a tag purpose is not a logical account.', 1;

INSERT INTO dbo.AccountTagPurpose (InstrumentId, Purpose, TagId)
SELECT p.InstrumentId, p.Purpose, p.TagId
FROM @Purposes p
WHERE NOT EXISTS (
    SELECT 1 FROM dbo.AccountTagPurpose e
    WHERE e.InstrumentId = p.InstrumentId AND e.Purpose = p.Purpose);

COMMIT;

SELECT i.[Name] AS Account, atp.Purpose, t.[Name] AS Tag
FROM dbo.AccountTagPurpose atp
INNER JOIN dbo.Instrument i ON i.Id = atp.InstrumentId
INNER JOIN dbo.Tag t ON t.Id = atp.TagId
WHERE atp.InstrumentId IN (SELECT InstrumentId FROM @Accounts)
ORDER BY i.[Name], atp.Purpose;
