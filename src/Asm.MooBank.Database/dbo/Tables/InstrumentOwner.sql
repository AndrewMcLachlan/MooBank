CREATE TABLE [dbo].[InstrumentOwner]
(
    InstrumentId UNIQUEIDENTIFIER NOT NULL,
    UserId UNIQUEIDENTIFIER NOT NULL,
    GroupId UNIQUEIDENTIFIER NULL,
    CONSTRAINT PK_InstrumentOwner PRIMARY KEY (InstrumentId, UserId),
    CONSTRAINT FK_InstrumentOwner_Account FOREIGN KEY (InstrumentId) REFERENCES [Instrument]([Id]),
    CONSTRAINT FK_InstrumentOwner_AccountHolder FOREIGN KEY (UserId) REFERENCES [User]([Id]),
    CONSTRAINT FK_InstrumentOwner_AccountGroup FOREIGN KEY (GroupId) REFERENCES [dbo].[Group](Id),
)
