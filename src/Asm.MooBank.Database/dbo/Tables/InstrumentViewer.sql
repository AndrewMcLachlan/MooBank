CREATE TABLE [dbo].[InstrumentViewer]
(
    InstrumentId UNIQUEIDENTIFIER NOT NULL,
    UserId UNIQUEIDENTIFIER NOT NULL,
    GroupId UNIQUEIDENTIFIER NOT NULL,
    CONSTRAINT PK_InstrumentViewer PRIMARY KEY (InstrumentId, UserId),
    CONSTRAINT FK_InstrumentViewer_Account FOREIGN KEY (InstrumentId) REFERENCES [Instrument]([Id]),
    CONSTRAINT FK_InstrumentViewer_User FOREIGN KEY (UserId) REFERENCES [User]([Id]),
    CONSTRAINT FK_InstrumentViewer_Group FOREIGN KEY (GroupId) REFERENCES [dbo].[Group](Id),
)
