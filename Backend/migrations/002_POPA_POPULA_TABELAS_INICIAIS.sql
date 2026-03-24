SET NOCOUNT ON;

DECLARE @i INT = 1;

-- Senha padrão para todos os usuários de seed: Senha@123
DECLARE @bcryptHash NVARCHAR(MAX) = '$2a$11$4gzZWKmhAGORC0x3FovtjOFm3697x19USHrYVAEVGtN.1z81UDWoK';

WHILE @i <= 40
BEGIN
    INSERT INTO [User] (Name, Email, Password, Balance, CreatedAt, UpdatedAt)
    VALUES (
        CONCAT('User ', @i),
        CONCAT('user', @i, '@example.com'),
        @bcryptHash,
        RAND() * 10000,
        DATEADD(DAY, -@i, GETDATE()),
        GETDATE()
    );

    SET @i = @i + 1;
END;

GO

SET NOCOUNT ON;

DECLARE @i INT = 1;

WHILE @i <= 40
BEGIN
    INSERT INTO Tag (Name, Description, TagType, UserId, CreatedAt, UpdatedAt)
    VALUES (
        CONCAT('Tag ', @i),
        CONCAT('Descrição da tag ', @i),
        (@i % 2),
        CASE WHEN @i % 2 = 0 THEN @i ELSE NULL END,
        DATEADD(DAY, -@i, GETDATE()),
        GETDATE()
    );

    SET @i = @i + 1;
END;

GO

SET NOCOUNT ON;

DECLARE @i INT = 1;

WHILE @i <= 40
BEGIN
    INSERT INTO Budget (Name, Goal, FinishAt, UserId, CreatedAt, UpdatedAt)
    VALUES (
        CONCAT('Budget ', @i),
        500 + (RAND() * 4500),
        DATEADD(DAY, @i, GETDATE()),
        ((@i - 1) % 40) + 1,
        DATEADD(DAY, -@i, GETDATE()),
        GETDATE()
    );

    SET @i = @i + 1;
END;

GO

SET NOCOUNT ON;

DECLARE @i INT = 1;

WHILE @i <= 40
BEGIN
    INSERT INTO Record (Operation, Value, Frequency, UserId, CreatedAt, UpdatedAt)
    VALUES (
        (@i % 2),
        RAND() * 1500,
        (@i % 5),
        ((@i - 1) % 40) + 1,
        DATEADD(DAY, -@i, GETDATE()),
        GETDATE()
    );

    SET @i = @i + 1;
END;

GO

SET NOCOUNT ON;

DECLARE @i INT = 1;

WHILE @i <= 40
BEGIN
    INSERT INTO RecordTag (RecordId, TagId)
    VALUES (
        ((@i - 1) % 40) + 1,
        ((@i * 3 - 1) % 40) + 1
    );

    SET @i = @i + 1;
END;

GO

