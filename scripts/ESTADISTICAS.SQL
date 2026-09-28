USE simulador_combate;

DELIMITER //

-- ─────────────────────────────────────────────────────────────────────────────
-- CRITERIO GENERAL
-- Las consultas simples (listar batallas de un período, buscar por id) van en
-- los repositorios con Dapper. Estos SP se justifican porque requieren
-- agregaciones, relaciones entre varias tablas o lógica adicional.
-- Todos usan START TRANSACTION READ ONLY ... COMMIT como exige la consigna.
-- ─────────────────────────────────────────────────────────────────────────────

-- ── 1. Resumen de batallas por período y tipo de personaje ──────────────────
DROP PROCEDURE IF EXISTS sp_ResumenBatallasPorPeriodo //
CREATE PROCEDURE sp_ResumenBatallasPorPeriodo(
    IN p_FechaInicio DATETIME,
    IN p_FechaFin    DATETIME
)
BEGIN
    DECLARE EXIT HANDLER FOR SQLEXCEPTION
    BEGIN
        ROLLBACK;
        RESIGNAL;
    END;

    START TRANSACTION READ ONLY;

    SELECT
        p.TipoClase,
        COUNT(DISTINCT b.Id)                                               AS TotalBatallas,
        SUM(CASE WHEN pb.Resultado = 'Victoria' THEN 1 ELSE 0 END)        AS Victorias,
        SUM(CASE WHEN pb.Resultado = 'Derrota'  THEN 1 ELSE 0 END)        AS Derrotas,
        SUM(CASE WHEN pb.Resultado = 'Empate'   THEN 1 ELSE 0 END)        AS Empates
    FROM Personajes p
    INNER JOIN ParticipantesBatalla pb ON p.Id = pb.PersonajeId
    INNER JOIN Batallas b              ON pb.BatallaId = b.Id
    WHERE b.Finalizada = TRUE
      AND b.Fecha BETWEEN p_FechaInicio AND p_FechaFin
    GROUP BY p.TipoClase
    ORDER BY TotalBatallas DESC;

    COMMIT;
END //

-- ── 2. Victorias y derrotas agrupadas por tipo de personaje y período ───────
DROP PROCEDURE IF EXISTS sp_VictoriasPorTipoYPeriodo //
CREATE PROCEDURE sp_VictoriasPorTipoYPeriodo(
    IN p_FechaInicio DATETIME,
    IN p_FechaFin    DATETIME
)
BEGIN
    DECLARE EXIT HANDLER FOR SQLEXCEPTION
    BEGIN
        ROLLBACK;
        RESIGNAL;
    END;

    START TRANSACTION READ ONLY;

    SELECT
        p.TipoClase,
        SUM(CASE WHEN pb.Resultado = 'Victoria' THEN 1 ELSE 0 END)        AS Victorias,
        SUM(CASE WHEN pb.Resultado = 'Derrota'  THEN 1 ELSE 0 END)        AS Derrotas,
        SUM(CASE WHEN pb.Resultado = 'Empate'   THEN 1 ELSE 0 END)        AS Empates,
        COUNT(*)                                                            AS TotalBatallas
    FROM Personajes p
    INNER JOIN ParticipantesBatalla pb ON p.Id = pb.PersonajeId
    INNER JOIN Batallas b              ON pb.BatallaId = b.Id
    WHERE b.Finalizada = TRUE
      AND b.Fecha BETWEEN p_FechaInicio AND p_FechaFin
    GROUP BY p.TipoClase
    ORDER BY Victorias DESC;

    COMMIT;
END //

-- ── 3. Ranking de personajes según victorias y porcentaje de efectividad ────
DROP PROCEDURE IF EXISTS sp_RankingPersonajes //
CREATE PROCEDURE sp_RankingPersonajes()
BEGIN
    DECLARE EXIT HANDLER FOR SQLEXCEPTION
    BEGIN
        ROLLBACK;
        RESIGNAL;
    END;

    START TRANSACTION READ ONLY;

    SELECT
        p.Id,
        p.Nombre,
        p.TipoClase,
        COUNT(pb.BatallaId)                                                AS TotalBatallas,
        SUM(CASE WHEN pb.Resultado = 'Victoria' THEN 1 ELSE 0 END)        AS Victorias,
        SUM(CASE WHEN pb.Resultado = 'Derrota'  THEN 1 ELSE 0 END)        AS Derrotas,
        ROUND(
            CASE
                WHEN COUNT(pb.BatallaId) > 0
                THEN SUM(CASE WHEN pb.Resultado = 'Victoria' THEN 1 ELSE 0 END) * 100.0
                     / COUNT(pb.BatallaId)
                ELSE 0
            END, 2
        )                                                                   AS PorcentajeEfectividad
    FROM Personajes p
    LEFT JOIN ParticipantesBatalla pb ON p.Id = pb.PersonajeId
    GROUP BY p.Id, p.Nombre, p.TipoClase
    ORDER BY PorcentajeEfectividad DESC, Victorias DESC;

    COMMIT;
END //

-- ── 4. Daño promedio por clase de personaje y tipo de acción ────────────────
DROP PROCEDURE IF EXISTS sp_DanioPromedioPorClaseYAccion //
CREATE PROCEDURE sp_DanioPromedioPorClaseYAccion()
BEGIN
    DECLARE EXIT HANDLER FOR SQLEXCEPTION
    BEGIN
        ROLLBACK;
        RESIGNAL;
    END;

    START TRANSACTION READ ONLY;

    SELECT
        p.TipoClase,
        db.Accion,
        ROUND(AVG(db.DanioRealizado), 2)    AS DanioPromedio,
        COUNT(*)                             AS VecesUsada
    FROM DetalleBatalla db
    INNER JOIN Personajes p ON db.AtacanteId = p.Id
    WHERE db.DanioRealizado > 0
    GROUP BY p.TipoClase, db.Accion
    ORDER BY p.TipoClase, DanioPromedio DESC;

    COMMIT;
END //

-- ── 5. Daño total realizado y recibido por personaje en un período ───────────
-- Bug corregido respecto al script original:
-- el producto cartesiano (doble LEFT JOIN a DetalleBatalla) inflaba las sumas.
-- La solución usa ParticipantesBatalla, que ya acumula los totales por batalla.
DROP PROCEDURE IF EXISTS sp_DanioPorPersonajeEnPeriodo //
CREATE PROCEDURE sp_DanioPorPersonajeEnPeriodo(
    IN p_FechaInicio DATETIME,
    IN p_FechaFin    DATETIME
)
BEGIN
    DECLARE EXIT HANDLER FOR SQLEXCEPTION
    BEGIN
        ROLLBACK;
        RESIGNAL;
    END;

    START TRANSACTION READ ONLY;

    -- LEFT JOIN para incluir personajes sin batallas en el período (con 0).
    -- El filtro de fecha va en la condición del JOIN (no en WHERE),
    -- para que los personajes sin batallas en el período aparezcan con 0.
    SELECT
        p.Id,
        p.Nombre,
        p.TipoClase,
        COALESCE(SUM(pb.DanioRealizado), 0)  AS DanioTotalRealizado,
        COALESCE(SUM(pb.DanioRecibido),  0)  AS DanioTotalRecibido
    FROM Personajes p
    LEFT JOIN ParticipantesBatalla pb ON p.Id = pb.PersonajeId
    LEFT JOIN Batallas b              ON pb.BatallaId = b.Id
        AND b.Finalizada = TRUE
        AND b.Fecha BETWEEN p_FechaInicio AND p_FechaFin
    GROUP BY p.Id, p.Nombre, p.TipoClase
    ORDER BY DanioTotalRealizado DESC;

    COMMIT;
END //

DELIMITER ;
