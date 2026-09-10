USE simulador_combate;

DELIMITER //

-- Registrar una batalla completa junto con sus participantes
DROP PROCEDURE IF EXISTS sp_RegistrarBatalla //
CREATE PROCEDURE sp_RegistrarBatalla(
    IN p_Personaje1_Id INT,
    IN p_Personaje2_Id INT,
    OUT p_BatallaId INT
)
BEGIN
    DECLARE EXIT HANDLER FOR SQLEXCEPTION
    BEGIN
        ROLLBACK;
        RESIGNAL;
    END;

    START TRANSACTION;

    IF NOT EXISTS (SELECT 1 FROM Personajes WHERE Id = p_Personaje1_Id) OR
       NOT EXISTS (SELECT 1 FROM Personajes WHERE Id = p_Personaje2_Id) THEN
        SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'Uno o ambos personajes no existen.';
    END IF;

    INSERT INTO Batallas (Personaje1_Id, Personaje2_Id, Fecha, Finalizada)
    VALUES (p_Personaje1_Id, p_Personaje2_Id, NOW(), FALSE);

    SET p_BatallaId = LAST_INSERT_ID();

    COMMIT;
END //

-- Finalizar una batalla actualizando el ganador de forma atómica
DROP PROCEDURE IF EXISTS sp_FinalizarBatalla //
CREATE PROCEDURE sp_FinalizarBatalla(
    IN p_BatallaId INT,
    IN p_GanadorId INT
)
BEGIN
    DECLARE EXIT HANDLER FOR SQLEXCEPTION
    BEGIN
        ROLLBACK;
        RESIGNAL;
    END;

    START TRANSACTION;

    IF NOT EXISTS (SELECT 1 FROM Batallas WHERE Id = p_BatallaId) THEN
        SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'La batalla no existe.';
    END IF;

    UPDATE Batallas
    SET Finalizada = TRUE,
        Ganador_Id = p_GanadorId
    WHERE Id = p_BatallaId;

    COMMIT;
END //

-- Persistir de forma atómica un turno de combate
DROP PROCEDURE IF EXISTS sp_RegistrarTurno //
CREATE PROCEDURE sp_RegistrarTurno(
    IN p_BatallaId INT,
    IN p_Turno INT,
    IN p_AtacanteId INT,
    IN p_DefensorId INT,
    IN p_DanioRealizado INT,
    IN p_HabilidadUsada VARCHAR(50)
)
BEGIN
    DECLARE EXIT HANDLER FOR SQLEXCEPTION
    BEGIN
        ROLLBACK;
        RESIGNAL;
    END;

    START TRANSACTION;

    INSERT INTO DetalleBatalla (BatallaId, Turno, AtacanteId, DefensorId, DanioRealizado, HabilidadUsada)
    VALUES (p_BatallaId, p_Turno, p_AtacanteId, p_DefensorId, p_DanioRealizado, p_HabilidadUsada);

    COMMIT;
END //

DELIMITER ;