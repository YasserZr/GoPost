CREATE TABLE Departement (
    departementId INT IDENTITY(1,1) PRIMARY KEY,
    libelle NVARCHAR(50) NOT NULL,
    descriptions NVARCHAR(50) NOT NULL
    
);

CREATE TABLE Employee (
    employeeId INT IDENTITY(1,1) PRIMARY KEY,
    prenom NVARCHAR(50) NOT NULL,
    nom NVARCHAR(50) NOT NULL,
    CONSTRAINT FK_Department FOREIGN KEY (departementId)
        REFERENCES Departement(departementId)
        ON DELETE CASCADE
    
);