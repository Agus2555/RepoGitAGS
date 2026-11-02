USE simulador_combate;

DELIMITER //

-- ─────────────────────────────────────────────────────────────────────────────
-- sp_RegistrarBatalla
-- Complejidad: inserta en Batallas y en ParticipantesBatalla (2 entidades).
-- Valida: existencia de ambos personajes y que no sean el mismo.
-- ─────────────────────────────────────────────────────────────────────────────
DROP PROCEDURE IF EXISTS sp_RegistrarBatalla //
CREATE PROCEDURE sp_RegistrarBatalla(
    IN  p_Personaje1_Id INT,
    IN  p_Personaje2_Id INT,
    OUT p_BatallaId     INT
)
BEGIN
    DECLARE v_Vida1    INT;
    DECLARE v_Recurso1 INT;
    DECLARE v_Vida2    INT;
    DECLARE v_Recurso2 INT;

    DECLARE EXIT HANDLER FOR SQLEXCEPTION
    BEGIN
        ROLLBACK;
        RESIGNAL;
    END;

    START TRANSACTION;

    -- Validar que los personajes son distintos
    IF p_Personaje1_Id = p_Personaje2_Id THEN
        SIGNAL SQLSTATE '45000'
            SET MESSAGE_TEXT = 'Un personaje no puede pelear contra sí mismo.';
    END IF;

    -- Leer estado inicial del personaje 1
    SELECT Vida, RecursoEspecial INTO v_Vida1, v_Recurso1
    FROM Personajes WHERE Id = p_Personaje1_Id;

    IF v_Vida1 IS NULL THEN
        SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'Personaje 1 no existe.';
    END IF;

    -- Leer estado inicial del personaje 2
    SELECT Vida, RecursoEspecial INTO v_Vida2, v_Recurso2
    FROM Personajes WHERE Id = p_Personaje2_Id;

    IF v_Vida2 IS NULL THEN
        SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'Personaje 2 no existe.';
    END IF;

    -- Insertar la batalla
    INSERT INTO Batallas (Personaje1_Id, Personaje2_Id, Fecha, Finalizada)
    VALUES (p_Personaje1_Id, p_Personaje2_Id, NOW(), FALSE);

    SET p_BatallaId = LAST_INSERT_ID();

    -- Insertar las dos filas de participantes con el estado inicial de cada uno
    INSERT INTO ParticipantesBatalla (BatallaId, PersonajeId, VidaInicial, RecursoInicial)
    VALUES
        (p_BatallaId, p_Personaje1_Id, v_Vida1, v_Recurso1),
        (p_BatallaId, p_Personaje2_Id, v_Vida2, v_Recurso2);

    COMMIT;
END //

-- ─────────────────────────────────────────────────────────────────────────────
-- sp_RegistrarTurnoYActualizarDanio
-- Complejidad: INSERT en DetalleBatalla + 2 UPDATE en ParticipantesBatalla.
-- Valida que la batalla esté activa antes de registrar.
-- (Operación multi-entidad → justifica ser SP, no CRUD simple.)
-- ─────────────────────────────────────────────────────────────────────────────
DROP PROCEDURE IF EXISTS sp_RegistrarTurnoYActualizarDanio //
CREATE PROCEDURE sp_RegistrarTurnoYActualizarDanio(
    IN p_BatallaId      INT,
    IN p_Turno          INT,
    IN p_AtacanteId     INT,
    IN p_DefensorId     INT,
    IN p_DanioRealizado INT,
    IN p_Accion         VARCHAR(50)
)
BEGIN
    DECLARE EXIT HANDLER FOR SQLEXCEPTION
    BEGIN
        ROLLBACK;
        RESIGNAL;
    END;

    START TRANSACTION;

    -- Validar que la batalla existe y sigue activa
    IF NOT EXISTS (SELECT 1 FROM Batallas WHERE Id = p_BatallaId AND Finalizada = FALSE) THEN
        SIGNAL SQLSTATE '45000'
            SET MESSAGE_TEXT = 'La batalla no existe o ya ha finalizado.';
    END IF;

    -- Registrar el turno
    INSERT INTO DetalleBatalla (BatallaId, Turno, AtacanteId, DefensorId, DanioRealizado, Accion)
    VALUES (p_BatallaId, p_Turno, p_AtacanteId, p_DefensorId, p_DanioRealizado, p_Accion);

    -- Acumular daño realizado por el atacante
    UPDATE ParticipantesBatalla
    SET DanioRealizado = DanioRealizado + p_DanioRealizado
    WHERE BatallaId = p_BatallaId AND PersonajeId = p_AtacanteId;

    -- Acumular daño recibido por el defensor
    UPDATE ParticipantesBatalla
    SET DanioRecibido = DanioRecibido + p_DanioRealizado
    WHERE BatallaId = p_BatallaId AND PersonajeId = p_DefensorId;

    COMMIT;
END //

-- ─────────────────────────────────────────────────────────────────────────────
-- sp_FinalizarBatalla
-- Complejidad: UPDATE Batallas + UPDATE ParticipantesBatalla (resultado de cada uno).
-- p_GanadorId = NULL indica empate.
-- ─────────────────────────────────────────────────────────────────────────────
DROP PROCEDURE IF EXISTS sp_FinalizarBatalla //
CREATE PROCEDURE sp_FinalizarBatalla(
    IN p_BatallaId  INT,
    IN p_GanadorId  INT   -- NULL para empate
)
BEGIN
    DECLARE EXIT HANDLER FOR SQLEXCEPTION
    BEGIN
        ROLLBACK;
        RESIGNAL;
    END;

    START TRANSACTION;

    -- Validar que la batalla existe y está activa
    IF NOT EXISTS (SELECT 1 FROM Batallas WHERE Id = p_BatallaId AND Finalizada = FALSE) THEN
        SIGNAL SQLSTATE '45000'
            SET MESSAGE_TEXT = 'La batalla no existe o ya está finalizada.';
    END IF;

    -- Cerrar la batalla y registrar al ganador (o NULL en caso de empate)
    UPDATE Batallas
    SET Finalizada = TRUE,
        Ganador_Id = p_GanadorId
    WHERE Id = p_BatallaId;

    -- Marcar el resultado de cada participante
    UPDATE ParticipantesBatalla
    SET Resultado = CASE
            WHEN p_GanadorId IS NULL          THEN 'Empate'
            WHEN PersonajeId = p_GanadorId    THEN 'Victoria'
            ELSE                                   'Derrota'
        END
    WHERE BatallaId = p_BatallaId;

    COMMIT;
END //

DELIMITER ;
