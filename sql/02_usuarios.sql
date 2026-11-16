-- ─────────────────────────────────────────────────────────────────────────────
-- 02_usuarios.sql
-- Crea los dos usuarios de la API. Se ejecuta con root:
--   mysql -u root -p < sql/02_usuarios.sql
--   (en PowerShell:  cmd /c "mysql -u root -p < sql\02_usuarios.sql")
--
-- ⚠ Las contraseñas de este archivo son DE EJEMPLO.
--   Reemplazalas antes de ejecutar (ver README) y usá las mismas
--   en las variables de entorno de la API.
-- ─────────────────────────────────────────────────────────────────────────────

-- administrador: todos los permisos, pero SOLO sobre la base combate_api.
-- La API lo usa para todas las escrituras (POST, PUT y DELETE).
CREATE USER IF NOT EXISTS 'administrador'@'localhost' IDENTIFIED BY 'Admin_Cambiar_123';
GRANT ALL PRIVILEGES ON combate_api.* TO 'administrador'@'localhost';

-- invitado: únicamente SELECT. La API lo usa para todas las consultas (GET).
-- Al darlo sobre combate_api.* (a nivel base de datos y no tabla por tabla)
-- vale para las tablas que existen hoy y también para las que se creen después.
CREATE USER IF NOT EXISTS 'invitado'@'localhost' IDENTIFIED BY 'Invitado_Cambiar_123';
GRANT SELECT ON combate_api.* TO 'invitado'@'localhost';

FLUSH PRIVILEGES;
