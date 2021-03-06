﻿CREATE TABLE [dbo].[BadgeCriterias] (
    [ID]      UNIQUEIDENTIFIER NOT NULL,
    [Details] NVARCHAR (140)   NOT NULL,
    [BadgeID] UNIQUEIDENTIFIER NOT NULL,
    PRIMARY KEY CLUSTERED ([ID] ASC),
    CONSTRAINT [FK53B941FBD448E5D8] FOREIGN KEY ([BadgeID]) REFERENCES [dbo].[Badges] ([ID])
);

