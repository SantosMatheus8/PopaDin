-- Renomeia tabela Budget para Goal e ajusta colunas para refletir conceito de meta de economia

-- Remove FK antiga
ALTER TABLE Budget DROP CONSTRAINT FK_Budget_User;

-- Renomeia tabela
EXEC sp_rename 'Budget', 'Goal';

-- Renomeia coluna Goal para TargetAmount
EXEC sp_rename 'Goal.Goal', 'TargetAmount', 'COLUMN';

-- Adiciona coluna Deadline (data limite para bater a meta)
ALTER TABLE Goal ADD Deadline DATETIME2 NULL;

-- Recria FK com novo nome
ALTER TABLE Goal ADD CONSTRAINT FK_Goal_User FOREIGN KEY (UserId) REFERENCES [User](Id) ON DELETE CASCADE;
