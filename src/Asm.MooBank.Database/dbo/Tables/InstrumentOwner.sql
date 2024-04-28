CREATE TABLE [dbo].[InstrumentOwner]
(
    InstrumentId UNIQUEIDENTIFIER NOT NULL,
    UserId UNIQUEIDENTIFIER NOT NULL,
    GroupId UNIQUEIDENTIFIER NULL,
    CONSTRAINT PK_AccountAccountHolder PRIMARY KEY (InstrumentId, UserId),
    CONSTRAINT FK_AccountAccountHolder_Account FOREIGN KEY (InstrumentId) REFERENCES [Instrument]([Id]),
    CONSTRAINT FK_AccountAccountHolder_AccountHolder FOREIGN KEY (UserId) REFERENCES [User]([Id]),
    CONSTRAINT FK_AccountAccountHolder_AccountGroup FOREIGN KEY (GroupId) REFERENCES [dbo].[Group](Id),
)
