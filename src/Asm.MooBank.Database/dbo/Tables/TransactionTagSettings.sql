CREATE TABLE [dbo].[TransactionTagSettings]
(
    TransactionTagId INT NOT NULL,
    ApplySmoothing BIT NOT NULL CONSTRAINT DF_TransactionTagSettings_ApplySettings DEFAULT(0),
    ExcludeFromReporting BIT NOT NULL CONSTRAINT DF_TransactionTagSettings_ExcludeFromReporting DEFAULT(0),
    CONSTRAINT PK_TransactionTagSettings PRIMARY KEY CLUSTERED (TransactionTagId),
    CONSTRAINT FK_TransactionTag_TransactionTagSettings FOREIGN KEY (TransactionTagId) REFERENCES TransactionTag(TransactionTagId)
)