-- The minimum an account-based pension must pay out each year, as a share of the balance, by the
-- age reached in that year.
--
-- Legislated (ATO Schedule 7) rather than an assumption, so a projection that ignores it shows a
-- balance the law would not let you keep. Held as data because the rates have been changed before:
-- they were halved for several years from 2019-20.
--
-- One row per band, keyed by the age it starts at. The rate applies from MinAge up to the next
-- band's MinAge.
CREATE TABLE [dbo].[MinimumDrawdownRate]
(
    [MinAge] TINYINT NOT NULL,
    [Rate] DECIMAL(6,4) NOT NULL,
    CONSTRAINT [PK_MinimumDrawdownRate] PRIMARY KEY CLUSTERED ([MinAge]),
    CONSTRAINT [CK_MinimumDrawdownRate_MinAge] CHECK ([MinAge] BETWEEN 0 AND 120),
    CONSTRAINT [CK_MinimumDrawdownRate_Rate] CHECK ([Rate] BETWEEN 0 AND 1)
)
GO
