CREATE TABLE [dbo].[TagSettings]
(
    TagId INT NOT NULL,
    ApplySmoothing BIT NOT NULL CONSTRAINT DF_TagSettings_ApplySettings DEFAULT(0),
    ExcludeFromReporting BIT NOT NULL CONSTRAINT DF_Settings_ExcludeFromReporting DEFAULT(0),
    CONSTRAINT PK_TransactionTagSettings PRIMARY KEY CLUSTERED (TagId),
    CONSTRAINT FK_Tag_TagSettings FOREIGN KEY (TagId) REFERENCES [Tag]([Id])
)