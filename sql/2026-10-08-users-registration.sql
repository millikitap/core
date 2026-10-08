-- Регистрация: место учёбы/работы и дата согласия с политикой.
-- Выполнить на боевой базе TimesMachineArchiveMillikitap до выкладки новой сборки.
IF COL_LENGTH('dbo.Users', 'PlaceOfStudyWork') IS NULL
BEGIN
    ALTER TABLE dbo.Users ADD PlaceOfStudyWork nvarchar(300) NULL;
END
GO
IF COL_LENGTH('dbo.Users', 'PrivacyAcceptedAt') IS NULL
BEGIN
    ALTER TABLE dbo.Users ADD PrivacyAcceptedAt datetime NULL;
END
GO
