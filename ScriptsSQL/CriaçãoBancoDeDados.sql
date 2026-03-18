use DB_TechHive
go

-- ============================================
--  BANCO DE DADOS ESTRUTURA DE USUÁRIOS
--  TechHive Solutions - Gestão de Usuários
-- ============================================

-- (Opcional) Criar banco de dados
-- CREATE DATABASE DB_TechHive;
-- GO
-- USE DB_TechHive;
-- GO

-- ============================================
--  CRIAÇÃO DA TABELA USERS
-- ============================================

CREATE TABLE dbo.Users (
    UserId INT IDENTITY(1,1) PRIMARY KEY,
    FullName NVARCHAR(150) NOT NULL,
    Email NVARCHAR(150) NOT NULL UNIQUE,
    Department NVARCHAR(100) NOT NULL,
    Role NVARCHAR(100) NOT NULL,
    IsActive BIT NOT NULL DEFAULT 1,
    CreatedAt DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
    UpdatedAt DATETIME2 NULL
);
GO

-- ============================================
--  INSERTS INICIAIS DE USUÁRIOS FICTÍCIOS
-- ============================================

INSERT INTO dbo.Users (FullName, Email, Department, Role, IsActive)
VALUES
('Ana Beatriz Ramos', 'ana.ramos@techhive.com', 'RH', 'Analista de Recursos Humanos', 1),
('Carlos Eduardo Matos', 'carlos.matos@techhive.com', 'TI', 'Administrador de Sistemas', 1),
('Juliana Martins Ferreira', 'juliana.ferreira@techhive.com', 'TI', 'Desenvolvedora Back-end', 1),
('Ricardo Alves Nunes', 'ricardo.nunes@techhive.com', 'Financeiro', 'Analista Financeiro', 1),
('Fernanda Souza Lima', 'fernanda.lima@techhive.com', 'RH', 'Coordenadora de RH', 1),
('Marcos Vinícius Duarte', 'marcos.duarte@techhive.com', 'TI', 'Suporte Técnico', 1),
('Patrícia Gomes Moreira', 'patricia.moreira@techhive.com', 'Marketing', 'Analista de Marketing', 0);
GO

-- ============================================
-- CONSULTA DE TESTE
-- ============================================
SELECT * FROM dbo.Users;
GO