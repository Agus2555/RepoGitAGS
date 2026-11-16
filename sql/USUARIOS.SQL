-- Usuario Administrador: Control total sobre el servidor
CREATE USER IF NOT EXISTS 'admin_combate'@'localhost' IDENTIFIED BY 'AdminPass123!';
GRANT ALL PRIVILEGES ON *.* TO 'admin_combate'@'localhost' WITH GRANT OPTION;

-- Usuario Desarrollo: Acceso exclusivo a la base de datos del proyecto
CREATE USER IF NOT EXISTS '5to_agbd'@'localhost' IDENTIFIED BY 'DevPass123!';
GRANT SELECT, INSERT, UPDATE, DELETE, EXECUTE ON simulador_combate.* TO '5to_agbd'@'localhost';

FLUSH PRIVILEGES;