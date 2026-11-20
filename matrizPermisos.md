# Matriz de permisos: base `combate_api`

La API usa **dos usuarios de MySQL** con permisos distintos, siguiendo el principio de **mínimo privilegio**: cada uno tiene solo lo que necesita. Aunque un GET tuviera un error, el invitado no puede modificar nada.

Los usuarios se crean con `sql/02_usuarios.sql`.

---

## 1. Usuarios

| Usuario | Host | Permiso otorgado | Para qué lo usa la API | Variable de entorno |
|---|---|---|---|---|
| `administrador` | `localhost` | `GRANT ALL PRIVILEGES ON combate_api.*` | Todas las **escrituras** (POST, PUT y DELETE) | `ConnectionStrings__Administrador` |
| `invitado` | `localhost` | `GRANT SELECT ON combate_api.*` | Todas las **lecturas** (GET y las búsquedas previas a una escritura) | `ConnectionStrings__Invitado` |

---

## 2. Qué puede hacer cada uno dentro de `combate_api`

| Acción | Instrucción SQL | administrador | invitado |
|---|---|:---:|:---:|
| Consultar datos | `SELECT` | ✅ | ✅ |
| Crear registros | `INSERT` | ✅ | ❌ `ERROR 1142` |
| Modificar registros | `UPDATE` | ✅ | ❌ `ERROR 1142` |
| Borrar registros | `DELETE` | ✅ | ❌ `ERROR 1142` |
| Crear, modificar y borrar tablas | `CREATE TABLE`, `ALTER`, `DROP TABLE` | ✅ | ❌ `ERROR 1142` |
| Crear índices | `CREATE INDEX` | ✅ | ❌ `ERROR 1142` |
| Crear vistas | `CREATE VIEW` | ✅ | ❌ `ERROR 1142` |
| Crear y ejecutar procedimientos almacenados | `CREATE PROCEDURE`, `CALL` | ✅ | ❌ `ERROR 1370` |
| Ejecutar funciones almacenadas | `EXECUTE` | ✅ | ❌ `ERROR 1370` |
| Bloquear tablas | `LOCK TABLES` | ✅ | ❌ `ERROR 1044` |
| Borrar la base entera (lo usa la restauración del backup) | `DROP DATABASE combate_api` | ✅ | ❌ |

> **Funciones y triggers:** el administrador tiene el permiso, pero en MySQL 8 con el log binario activado (viene así por defecto) crearlos también pide `SUPER` o `log_bin_trust_function_creators = 1` (`ERROR 1419`). La API no usa ni funciones ni triggers.

---

## 3. Permisos por tabla

**C** = `INSERT` · **R** = `SELECT` · **U** = `UPDATE` · **D** = `DELETE`

| Tabla | administrador | invitado |
|---|:---:|:---:|
| `personajes` | C R U D | R |
| `combates` | C R U D | R |
| `turnos` | C R U D | R |

Como los permisos están dados sobre `combate_api.*`, valen igual para las tablas que se agreguen más adelante.

---

## 4. Fuera de `combate_api`: ninguno de los dos puede

| Acción | administrador | invitado |
|---|:---:|:---:|
| Dar permisos a otros usuarios (`GRANT`) | ❌ `ERROR 1044` | ❌ `ERROR 1044` |
| Crear usuarios (`CREATE USER`) | ❌ `ERROR 1227` | ❌ `ERROR 1227` |
| Crear otra base (`CREATE DATABASE`) | ❌ `ERROR 1044` | ❌ `ERROR 1044` |
| Leer otras bases (por ejemplo `mysql.user`) | ❌ `ERROR 1142` | ❌ `ERROR 1142` |

Estas tareas (crear la base, los usuarios y los permisos) solo las hace `root`, con `sql/01_estructura.sql` y `sql/02_usuarios.sql`.

---

## 5. Qué usuario usa cada ruta de la API

En las rutas que escriben, primero se busca el recurso con **invitado** (por ejemplo para devolver 404 si no existe) y después se escribe con **administrador**.

| # | Ruta | invitado (lee) | administrador (escribe) |
|---|---|---|---|
| 1 | `GET /api/personajes` | `SELECT` personajes | — |
| 2 | `GET /api/personajes/{id}` | `SELECT` personajes | — |
| 3 | `GET /api/personajes/{id}/combates` | `SELECT` personajes, combates, turnos | — |
| 4 | `POST /api/personajes` | — | `INSERT` personajes |
| 5 | `PUT /api/personajes/{id}` | `SELECT` personajes | `UPDATE` personajes |
| 6 | `DELETE /api/personajes/{id}` | `SELECT` personajes, combates | `DELETE` personajes |
| 7 | `GET /api/combates` | `SELECT` combates, personajes, turnos | — |
| 8 | `POST /api/combates` | `SELECT` personajes | `INSERT` combates |
| 9 | `GET /api/combates/{id}` | `SELECT` combates, personajes, turnos | — |
| 10 | `PUT /api/combates/{id}` | `SELECT` combates, personajes, turnos | `UPDATE` combates |
| 11 | `DELETE /api/combates/{id}` | `SELECT` combates, personajes, turnos | `DELETE` combates |
| 12 | `GET /api/combates/{id}/turnos` | `SELECT` combates, personajes, turnos | — |
| 13 | `POST /api/combates/{id}/turnos` | `SELECT` combates, personajes, turnos | `INSERT` turnos + `UPDATE` combates (**una transacción**) |
| 14 | `PUT /api/combates/{id}/turnos/{turnoId}` | `SELECT` combates, personajes, turnos | `UPDATE` turnos |
| 15 | `DELETE /api/combates/{id}/turnos/{turnoId}` | `SELECT` combates, personajes, turnos | `DELETE` turnos + `UPDATE` combates (**una transacción**) |

---

## 6. Cómo se comprobó

Permisos que quedan después de correr `sql/02_usuarios.sql`:

```sql
SHOW GRANTS FOR 'administrador'@'localhost';
-- GRANT USAGE ON *.* TO `administrador`@`localhost`
-- GRANT ALL PRIVILEGES ON `combate_api`.* TO `administrador`@`localhost`

SHOW GRANTS FOR 'invitado'@'localhost';
-- GRANT USAGE ON *.* TO `invitado`@`localhost`
-- GRANT SELECT ON `combate_api`.* TO `invitado`@`localhost`
```

`USAGE` significa "puede conectarse, pero sin permisos globales".

Prueba de que el invitado lee pero no escribe:

```bash
mysql -u invitado -p combate_api -e "SELECT Id, Nombre, Tipo FROM personajes;"
# → muestra los personajes

mysql -u invitado -p combate_api -e "INSERT INTO personajes (Nombre, Tipo) VALUES ('Hacker', 'Mago');"
# → ERROR 1142 (42000): INSERT command denied to user 'invitado'@'localhost' for table 'personajes'
```

### Códigos de error

| Código | Significado |
|---|---|
| `1142` | No tiene permiso para esa instrucción sobre esa tabla |
| `1044` | No tiene acceso a esa base (o a esa acción sobre la base) |
| `1227` | Le falta un permiso global (por ejemplo `CREATE USER`) |
| `1370` | No tiene permiso `EXECUTE` sobre el procedimiento o la función |
| `1419` | Con el log binario activado, crear funciones o triggers pide `SUPER` |
