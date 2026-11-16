<h1 align="center">E.T. Nº12 D.E. 1º "Libertador Gral. José de San Martín"</h1>

<p align="center">
  <img src="https://et12.edu.ar/imgs/et12.svg">
</p>

# Combate por API
**Software 2.4**

| Atributo | Detalle |
| :--- | :--- |
| **Espacio Curricular** | Software 2.4 (Troncal) |
| **Plan** | Combate por API |
| **Ubicación Temporal** | 4.º Bimestre |
| **Duración** | 1 Bimestre |

---

## 📋 Sinopsis

Transformación del proyecto **Código en combate** (3er bimestre) en una **Web API con .NET 10 y Minimal API**. La API permite administrar personajes, combates y turnos y consultar resultados e historiales. Cada tipo de personaje (**Guerrero, Mago, Arquero y Asesino**) actúa con sus propias reglas mediante **polimorfismo**, sin `if` ni `switch` por clase.

Los datos se guardan en **MySQL** (base `combate_api`) con **Dapper**. Hay dos usuarios de base de datos: un **administrador**, que hace todas las escrituras, y un **invitado** de solo lectura, que hace todos los GET. Los endpoints se documentan y prueban desde **Swagger**, y las reglas de combate se verifican con **pruebas unitarias** que no necesitan MySQL.

Documentación de las etapas:
- [docs/punto-de-partida.md](docs/punto-de-partida.md): modelo inicial, recorrido de solicitudes, hipótesis y dudas.
- [docs/indagacion.md](docs/indagacion.md): pruebas de concepto, respuestas, DTO, dependencias, Swagger y corrección de hipótesis.
- [docs/reflexion-final.md](docs/reflexion-final.md): comparación con el punto de partida, aprendizajes y mejoras.

---

## 🗂️ Estructura

```text
RepoGitAGS/
├── src/
│   ├── Proyecto.sln
│   ├── Aplicacion/           # Servicios, interfaces, DTOs y excepciones (casos de uso)
│   │   ├── Servicios/        # ServicioPersonajes, ServicioCombates, ServicioTurnos, Mapeo
│   │   ├── Interfaces/       # IServicioPersonajes, IServicioCombates, IServicioTurnos
│   │   ├── Dtos/             # Contratos JSON de entrada y salida
│   │   └── Excepciones/      # NoEncontrado (404), DatosInvalidos (400), Conflicto (409)
│   ├── Persistencia/
│   │   ├── Entidades/        # Personaje (abstracta), Guerrero, Mago, Arquero, Asesino, Combate, Turno
│   │   ├── Repositorios/     # PersonajeRepository, CombateRepository, TurnoRepository (Dapper)
│   │   ├── IDbConnectionFactory.cs
│   │   └── MySqlConnectionFactory.cs
│   ├── API/                  # Program.cs: inyección de dependencias, Swagger y las 15 rutas
│   └── Tests/                # PersonajesTests.cs, CombateTests.cs (xUnit, sin MySQL)
├── sql/
│   ├── 01_estructura.sql     # Base combate_api y tablas personajes, combates, turnos
│   ├── 02_usuarios.sql       # Usuarios administrador e invitado
│   └── 03_datos_muestra.sql  # Datos de ejemplo
├── backup/
│   └── combate_api.sql       # Exportación de la base (estructura + datos, sin credenciales)
├── exploracion/              # Pruebas de concepto de la etapa de Indagación
└── docs/
```

**Capas:** `API` → `Aplicacion` → `Persistencia`. Las reglas del combate están en las entidades (`Combate`, `Personaje` y subclases), los repositorios solo leen y guardan, y los endpoints solo reciben y devuelven DTOs.

---

## ⚙️ Requisitos

- **.NET SDK 10**: `dotnet --version` tiene que mostrar 10.x.
- **MySQL Server 8.0 o superior**, con el cliente `mysql` y `mysqldump` en el PATH.
- Para el último paso, un navegador con Swagger.

---

## 🛢️ Base de datos

### 0. Reemplazar las contraseñas de ejemplo

`sql/02_usuarios.sql` trae contraseñas **de ejemplo** (`Admin_Cambiar_123` e `Invitado_Cambiar_123`). Antes de ejecutarlo, **reemplazalas por las tuyas** y usá las mismas en las variables de entorno de la API (ver más abajo). Las contraseñas reales nunca se suben al repositorio.

### 1. Orden de ejecución

Se ejecutan **en este orden**, desde la carpeta raíz del repositorio:

| # | Script | Usuario | Qué hace |
|---|---|---|---|
| 1 | `sql/01_estructura.sql` | root | Crea la base `combate_api` y las tablas `personajes`, `combates` y `turnos` (borra las tablas si existían). |
| 2 | `sql/02_usuarios.sql` | root | Crea `administrador` (todos los permisos sobre `combate_api`) e `invitado` (solo `SELECT` sobre `combate_api.*`). |
| 3 | `sql/03_datos_muestra.sql` | administrador | Vacía las tablas y carga 5 personajes, 3 combates y 5 turnos. |

**Linux / macOS / Git Bash:**

```bash
mysql -u root -p < sql/01_estructura.sql
mysql -u root -p < sql/02_usuarios.sql
mysql -u administrador -p < sql/03_datos_muestra.sql
```

**Windows PowerShell** (PowerShell no tiene el operador `<`, entonces se usa `cmd /c`):

```powershell
cmd /c "mysql -u root -p < sql\01_estructura.sql"
cmd /c "mysql -u root -p < sql\02_usuarios.sql"
cmd /c "mysql -u administrador -p < sql\03_datos_muestra.sql"
```

> Si `mysql` no está en el PATH de Windows, la ruta suele ser `C:\Program Files\MySQL\MySQL Server <versión>\bin\mysql.exe`. También se pueden abrir los scripts en MySQL Workbench y ejecutarlos uno por uno.

### 2. Tablas

| Tabla | Qué guarda |
|---|---|
| `personajes` | `Id`, `Nombre`, `Tipo`. Las estadísticas (vida, ataque, defensa y recurso) salen de la clase de cada tipo en C#. |
| `combates` | Los dos participantes y el **estado actual** del combate: `VidaP1`, `VidaP2`, `RecursoP1`, `RecursoP2`, `Estado` (`EnCurso` / `Finalizado`) y `GanadorId`. |
| `turnos` | Historial: `Numero`, `AtacanteId`, `DefensorId`, `Accion`, `Danio`, `Nota` (editable) y la vida y el recurso de ambos **antes** del turno (`VidaP1Antes`, `VidaP2Antes`, `RecursoP1Antes`, `RecursoP2Antes`), que se usan para deshacer el último. `UNIQUE (CombateId, Numero)`. |

Las claves foráneas no tienen `ON DELETE CASCADE`, así que un combate con turnos o un personaje con combates no se puede borrar ni siquiera desde SQL.

### 3. Datos de muestra

| Id | Personaje | Tipo | Para probar |
|---|---|---|---|
| 1 | Thorin | Guerrero | DELETE → 409 (tiene combates) |
| 2 | Gandalf | Mago | `GET /api/personajes/2/combates` (está en 2 combates) |
| 3 | Legolas | Arquero | |
| 4 | Erebus | Asesino | |
| 5 | Conan | Guerrero | DELETE → 204 (no tiene combates) |

| Id | Combate | Estado | Para probar |
|---|---|---|---|
| 1 | Thorin vs Gandalf | EnCurso, 2 turnos, le toca a Thorin | POST turno, PUT nota, DELETE del último turno, 409 al borrar el turno 1 |
| 2 | Legolas vs Erebus | EnCurso, sin turnos | PUT y DELETE de combate |
| 3 | Erebus vs Gandalf | Finalizado, ganó Erebus | POST turno → 400 (combate finalizado) |

### 4. Verificar que invitado puede leer y no puede escribir

```bash
mysql -u invitado -p combate_api -e "SELECT Id, Nombre, Tipo FROM personajes;"
# → muestra los personajes

mysql -u invitado -p combate_api -e "INSERT INTO personajes (Nombre, Tipo) VALUES ('Hacker', 'Mago');"
# → ERROR 1142 (42000): INSERT command denied to user 'invitado'@'localhost' for table 'personajes'

mysql -u invitado -p combate_api -e "UPDATE personajes SET Nombre = 'X' WHERE Id = 1;"
# → ERROR 1142 (42000): UPDATE command denied to user 'invitado'@'localhost' for table 'personajes'

mysql -u invitado -p combate_api -e "DELETE FROM turnos;"
# → ERROR 1142 (42000): DELETE command denied to user 'invitado'@'localhost' for table 'turnos'

mysql -u invitado -p -e "SHOW GRANTS;"
# → GRANT SELECT ON `combate_api`.* TO `invitado`@`localhost`
```

Así salió la prueba con los usuarios del proyecto. Como el permiso está dado sobre `combate_api.*`, vale también para las tablas que se creen más adelante.

### 5. Backup: exportar y restaurar

**Exportar** (estructura + datos, **sin usuarios ni contraseñas**: se exporta solo la base `combate_api`, no la base `mysql` donde viven los usuarios):

```bash
mysqldump -u administrador -p --databases combate_api --no-tablespaces --set-gtid-purged=OFF --skip-dump-date --result-file=backup/combate_api.sql
```

- `--databases` incluye el `CREATE DATABASE` y el `USE`, así el archivo se restaura solo.
- `--no-tablespaces` hace falta porque administrador no tiene el permiso global `PROCESS`.
- `--set-gtid-purged=OFF` evita las líneas de replicación (`GTID_PURGED`, `SQL_LOG_BIN`), que piden permisos de superusuario y harían fallar la restauración en otro servidor.

**Restaurar:**

```bash
mysql -u administrador -p -e "DROP DATABASE IF EXISTS combate_api;"
mysql -u administrador -p < backup/combate_api.sql
# PowerShell: cmd /c "mysql -u administrador -p < backup\combate_api.sql"
```

Los permisos de `administrador` e `invitado` no se pierden al borrar y restaurar la base, porque MySQL los guarda aparte. En una instalación nueva hay que correr primero `sql/02_usuarios.sql` con root y después restaurar el backup.

**Verificar la restauración:**

```bash
mysql -u invitado -p combate_api -e "SELECT (SELECT COUNT(*) FROM personajes) AS personajes, (SELECT COUNT(*) FROM combates) AS combates, (SELECT COUNT(*) FROM turnos) AS turnos;"
```

---

## 🔐 Variables de entorno

La API **no tiene contraseñas en el código ni en `appsettings.json`**. Lee las dos cadenas de conexión de variables de entorno. ASP.NET Core convierte el `__` (doble guion bajo) en `:`, así que `ConnectionStrings__Invitado` es `ConnectionStrings:Invitado`.

| Variable | Usuario | Se usa en |
|---|---|---|
| `ConnectionStrings__Invitado` | invitado | Todos los **GET** (todas las consultas SELECT) |
| `ConnectionStrings__Administrador` | administrador | Todos los **POST, PUT y DELETE** (INSERT, UPDATE y DELETE) |

**Linux / macOS / Git Bash:**

```bash
export ConnectionStrings__Invitado="Server=localhost;Database=combate_api;Uid=invitado;Pwd=TU_CLAVE_INVITADO;"
export ConnectionStrings__Administrador="Server=localhost;Database=combate_api;Uid=administrador;Pwd=TU_CLAVE_ADMIN;"
```

**Windows PowerShell:**

```powershell
$env:ConnectionStrings__Invitado = "Server=localhost;Database=combate_api;Uid=invitado;Pwd=TU_CLAVE_INVITADO;"
$env:ConnectionStrings__Administrador = "Server=localhost;Database=combate_api;Uid=administrador;Pwd=TU_CLAVE_ADMIN;"
```

Las variables duran mientras esté abierta esa terminal, así que la API hay que ejecutarla en la misma. Si falta alguna, la API no arranca y muestra: *"Faltan las variables de entorno ConnectionStrings__Invitado y/o ConnectionStrings__Administrador"*.

> **En Linux**, si la API da `Access denied` con usuarios `@'localhost'`, probá con `Server=127.0.0.1`, o creá los usuarios también como `'usuario'@'127.0.0.1'`.

---

## ▶️ Compilar, probar y ejecutar

```bash
# Compilar la solución
dotnet build src/Proyecto.sln

# Pruebas unitarias (NO necesitan MySQL)
dotnet test src/Proyecto.sln

# Iniciar la API (en la misma terminal donde se definieron las variables de entorno)
dotnet run --project src/API
```

La API queda en **http://localhost:5080** y **Swagger** en **http://localhost:5080/swagger**. Con `dotnet run` se usa el perfil de `Properties/launchSettings.json`, que abre el navegador en Swagger.

Desde Swagger, cada ruta tiene **Try it out → Execute**, y se ven el cuerpo que recibe y los códigos que puede devolver.

### Pruebas automatizadas

El proyecto `Tests` tiene **116 pruebas** (xUnit) sobre el dominio, sin MySQL:

- **`PersonajesTests.cs`**:
  - El daño de `Atacar` y el efecto de `UsarHabilidad` de cada clase, con y sin recurso.
  - Daño mínimo 1 y vida nunca negativa.
  - Un **personaje derrotado no puede actuar**.
  - Polimorfismo: la misma llamada da daños distintos según la clase.
  - La fábrica y las estadísticas por defecto, y la validación de nombre y tipo.
- **`CombateTests.cs`**:
  - **Turnos alternados**, acción fuera de turno, acción inválida.
  - **Finalización del combate** con ganador, y que un combate finalizado no acepte más turnos.
  - **Cambiar la nota no altera el daño** ni las vidas.
  - **Borrar el último turno restaura el estado previo**: vidas, recursos, estado, ganador y a quién le toca.
  - No se puede deshacer un turno que no sea el último.
  - Cambio de participantes antes y después del primer turno.

---

## 📡 Endpoints (las 15 rutas)

Todas reciben y devuelven JSON. Los errores tienen siempre el formato `{ "mensaje": "..." }`.

| Código | Significado |
|---|---|
| **200** | OK, con datos |
| **201** | Recurso creado: devuelve el recurso y la cabecera `Location` |
| **204** | Hecho, sin cuerpo |
| **400** | Datos inválidos, JSON roto, acción fuera de turno o combate finalizado |
| **404** | El recurso no existe |
| **409** | La operación rompe una restricción |

Los ejemplos son respuestas reales de la API con los datos de muestra.

### Personajes

#### 1. `GET /api/personajes` → 200
Lista de personajes.
```json
[
  { "id": 1, "nombre": "Thorin", "tipo": "Guerrero", "vidaMaxima": 120, "ataque": 20, "defensa": 8, "nombreRecurso": "Furia", "recursoInicial": 0, "habilidad": "Golpe Devastador" },
  { "id": 2, "nombre": "Gandalf", "tipo": "Mago", "vidaMaxima": 80, "ataque": 14, "defensa": 4, "nombreRecurso": "Maná", "recursoInicial": 40, "habilidad": "Bola de Fuego" }
]
```

#### 2. `GET /api/personajes/{id}` → 200 / 404
`GET /api/personajes/1` devuelve 200:
```json
{ "id": 1, "nombre": "Thorin", "tipo": "Guerrero", "vidaMaxima": 120, "ataque": 20, "defensa": 8, "nombreRecurso": "Furia", "recursoInicial": 0, "habilidad": "Golpe Devastador" }
```
`GET /api/personajes/99` devuelve 404:
```json
{ "mensaje": "No existe el personaje con Id 99." }
```

#### 3. `GET /api/personajes/{id}/combates` → 200 / 404
- `GET /api/personajes/2/combates` devuelve 200 con los combates 1 y 3.
- `GET /api/personajes/5/combates` devuelve 200 con `[]`, porque no participó.
- `GET /api/personajes/99/combates` devuelve 404.

#### 4. `POST /api/personajes` → 201 / 400
Recibe solo **nombre y tipo** (`Guerrero`, `Mago`, `Arquero` o `Asesino`, sin distinguir mayúsculas).
```json
{ "nombre": "Aragorn", "tipo": "Guerrero" }
```
Devuelve 201 con `Location: /api/personajes/6`:
```json
{ "id": 6, "nombre": "Aragorn", "tipo": "Guerrero", "vidaMaxima": 120, "ataque": 20, "defensa": 8, "nombreRecurso": "Furia", "recursoInicial": 0, "habilidad": "Golpe Devastador" }
```
Casos de 400:
- `{ "nombre": "X", "tipo": "Paladin" }` devuelve `"Tipo de personaje inválido: 'Paladin'. Usá Guerrero, Mago, Arquero o Asesino."`
- `{ "nombre": "", "tipo": "Mago" }` devuelve `"El nombre no puede estar vacío."`
- Un JSON roto devuelve `"El cuerpo de la solicitud no es un JSON válido."`

#### 5. `PUT /api/personajes/{id}` → 200 / 400 / 404
Modifica **solo el nombre**.
```json
{ "nombre": "Aragorn II" }
```
- Devuelve 200 con el personaje actualizado.
- Con `{ "nombre": "" }` devuelve 400.
- Con un Id inexistente devuelve 404.

#### 6. `DELETE /api/personajes/{id}` → 204 / 404 / 409
- `DELETE /api/personajes/5` (Conan) devuelve **204**, sin cuerpo.
- `DELETE /api/personajes/1` (Thorin) devuelve **409** `"No se puede eliminar: el personaje participó en al menos un combate."`
- `DELETE /api/personajes/99` devuelve 404.

### Combates

#### 7. `GET /api/combates` → 200
Lista con Id, participantes (con su vida y su recurso en ese combate), estado, cantidad de turnos y ganador. Si no hay combates devuelve `[]`.
```json
[
  {
    "id": 1,
    "participante1": { "id": 1, "nombre": "Thorin", "tipo": "Guerrero", "vida": 100, "vidaMaxima": 120, "nombreRecurso": "Furia", "recurso": 10 },
    "participante2": { "id": 2, "nombre": "Gandalf", "tipo": "Mago", "vida": 64, "vidaMaxima": 80, "nombreRecurso": "Maná", "recurso": 30 },
    "estado": "EnCurso", "cantidadTurnos": 2, "ganadorId": null
  }
]
```

#### 8. `POST /api/combates` → 201 / 400 / 404
```json
{ "personaje1Id": 5, "personaje2Id": 3 }
```
Devuelve 201 con `Location: /api/combates/4`. Cada participante arranca con la vida y el recurso inicial de su clase:
```json
{
  "id": 4, "estado": "EnCurso",
  "participante1": { "id": 5, "nombre": "Conan", "tipo": "Guerrero", "vida": 120, "vidaMaxima": 120, "nombreRecurso": "Furia", "recurso": 0 },
  "participante2": { "id": 3, "nombre": "Legolas", "tipo": "Arquero", "vida": 100, "vidaMaxima": 100, "nombreRecurso": "Flechas", "recurso": 10 },
  "siguienteAtacanteId": 5, "ganadorId": null, "resultado": "En curso: le toca a Conan", "turnos": []
}
```
- `{ "personaje1Id": 5, "personaje2Id": 5 }` devuelve 400 `"Un combate necesita dos personajes distintos."`
- `{ "personaje1Id": 5, "personaje2Id": 99 }` devuelve 404 `"No existe el personaje con Id 99."`

#### 9. `GET /api/combates/{id}` → 200 / 404
Devuelve el combate, los participantes, los turnos y el resultado. `GET /api/combates/1` da:
```json
{
  "id": 1, "estado": "EnCurso",
  "participante1": { "id": 1, "nombre": "Thorin", "tipo": "Guerrero", "vida": 100, "vidaMaxima": 120, "nombreRecurso": "Furia", "recurso": 10 },
  "participante2": { "id": 2, "nombre": "Gandalf", "tipo": "Mago", "vida": 64, "vidaMaxima": 80, "nombreRecurso": "Maná", "recurso": 30 },
  "siguienteAtacanteId": 1, "ganadorId": null, "resultado": "En curso: le toca a Thorin",
  "turnos": [
    { "id": 1, "numero": 1, "atacanteId": 1, "defensorId": 2, "accion": "atacar", "danio": 16, "nota": "Primer golpe de Thorin" },
    { "id": 2, "numero": 2, "atacanteId": 2, "defensorId": 1, "accion": "habilidad", "danio": 20, "nota": null }
  ]
}
```
En un combate finalizado, `GET /api/combates/3` da `"estado": "Finalizado"`, `"ganadorId": 4` y `"resultado": "Ganó Erebus"`. Con `GET /api/combates/99` devuelve 404.

#### 10. `PUT /api/combates/{id}` → 200 / 400 / 404 / 409
Cambia los dos participantes **solo antes del primer turno**.
```json
{ "personaje1Id": 5, "personaje2Id": 4 }
```
- `PUT /api/combates/2` devuelve 200: el combate queda Conan vs Erebus, con vidas y recursos iniciales.
- `PUT /api/combates/1` devuelve **409** `"No se pueden cambiar los participantes: el combate ya tiene turnos."`
- Con los mismos Id devuelve 400, y con un combate o personaje inexistente, 404.

#### 11. `DELETE /api/combates/{id}` → 204 / 404 / 409
- Un combate sin turnos (el 2, o uno recién creado) devuelve **204**.
- `DELETE /api/combates/1` devuelve **409** `"No se puede eliminar: el combate ya tiene turnos."`
- `DELETE /api/combates/99` devuelve 404.

### Turnos

#### 12. `GET /api/combates/{id}/turnos` → 200 / 404
Devuelve los turnos **en orden de ejecución**.
- `GET /api/combates/1/turnos` devuelve 200 con 2 turnos.
- `GET /api/combates/2/turnos` devuelve 200 con `[]`.
- `GET /api/combates/99/turnos` devuelve 404.

#### 13. `POST /api/combates/{id}/turnos` → 200 / 400 / 404
Recibe `atacanteId` y `accion` (`"atacar"` o `"habilidad"`). El defensor es siempre el otro participante.
```json
{ "atacanteId": 1, "accion": "atacar" }
```
200: el turno y el estado actualizado del combate.
```json
{
  "turno": { "id": 6, "numero": 3, "atacanteId": 1, "defensorId": 2, "accion": "atacar", "danio": 16, "nota": null },
  "estado": "EnCurso",
  "participante1": { "id": 1, "nombre": "Thorin", "tipo": "Guerrero", "vida": 100, "vidaMaxima": 120, "nombreRecurso": "Furia", "recurso": 20 },
  "participante2": { "id": 2, "nombre": "Gandalf", "tipo": "Mago", "vida": 48, "vidaMaxima": 80, "nombreRecurso": "Maná", "recurso": 30 },
  "ganadorId": null,
  "resultado": "En curso: le toca a Gandalf"
}
```
Si el turno deja al defensor en 0, vuelve `"estado": "Finalizado"` con el ganador, por ejemplo `"ganadorId": 5` y `"resultado": "Ganó Conan"`.

| Caso | Solicitud | Respuesta |
|---|---|---|
| Fuera de turno | combate 1, `{ "atacanteId": 2, "accion": "atacar" }` | 400 `"Acción fuera de turno: le toca a Thorin (Id 1)."` |
| Acción inválida | combate 1, `{ "atacanteId": 1, "accion": "volar" }` | 400 `"Acción inválida: usá 'atacar' o 'habilidad'."` |
| Combate finalizado | combate 3, `{ "atacanteId": 2, "accion": "atacar" }` | 400 `"El combate ya finalizó, no se pueden jugar más turnos."` |
| Atacante que no está en el combate | combate 1, `{ "atacanteId": 3, ... }` | 404 `"El atacante con Id 3 no existe en este combate."` |
| Combate inexistente | `POST /api/combates/99/turnos` | 404 |

#### 14. `PUT /api/combates/{id}/turnos/{turnoId}` → 200 / 400 / 404
Modifica **solo la nota** (hasta 200 caracteres; puede ser `""`).
```json
{ "nota": "Nota editada" }
```
`PUT /api/combates/1/turnos/1` devuelve 200. El daño sigue siendo 16:
```json
{ "id": 1, "numero": 1, "atacanteId": 1, "defensorId": 2, "accion": "atacar", "danio": 16, "nota": "Nota editada" }
```
- Con `{ "nota": null }` o más de 200 caracteres devuelve 400.
- Si el turno no existe o no pertenece a ese combate, devuelve 404.

#### 15. `DELETE /api/combates/{id}/turnos/{turnoId}` → 204 / 404 / 409
Borra **solo el último turno** y restaura vidas, recursos, estado y ganador con los datos guardados antes de ese turno.
- Después del POST del punto 13, `DELETE /api/combates/1/turnos/6` devuelve **204**, y `GET /api/combates/1` vuelve a mostrar Thorin 100 (furia 10), Gandalf 64 (maná 30) y `"siguienteAtacanteId": 1`.
- `DELETE /api/combates/1/turnos/1` devuelve **409** `"Solo se puede borrar el último turno del combate."`
- Si el turno o el combate no existen, devuelve 404.

---

## 🧪 Cómo comprobar los casos 409

Con los datos de muestra recién cargados (`sql/03_datos_muestra.sql`):

| Restricción | Cómo provocarla | Respuesta |
|---|---|---|
| Borrar un personaje que participó en un combate | `DELETE /api/personajes/1` | 409 |
| Cambiar participantes de un combate con turnos | `PUT /api/combates/1` con `{ "personaje1Id": 5, "personaje2Id": 4 }` | 409 |
| Borrar un combate con turnos | `DELETE /api/combates/1` | 409 |
| Borrar un turno que no es el último | `DELETE /api/combates/1/turnos/1` | 409 |

Y los mismos casos **sin** conflicto, para comparar:
- `DELETE /api/personajes/5` → 204
- `PUT /api/combates/2` → 200
- `DELETE /api/combates/2` → 204
- `DELETE /api/combates/1/turnos/2` → 204

## 🧪 Cómo comprobar que la nota no altera el daño y que deshacer restaura

1. `GET /api/combates/1/turnos`: el turno 1 tiene `"danio": 16`.
2. `PUT /api/combates/1/turnos/1` con `{ "nota": "Cambio de nota" }`: la respuesta sigue con `"danio": 16`.
3. `GET /api/combates/1`: las vidas siguen igual (Thorin 100, Gandalf 64).
4. `POST /api/combates/1/turnos` con `{ "atacanteId": 1, "accion": "atacar" }`: Gandalf queda en 48.
5. `DELETE /api/combates/1/turnos/{id del turno nuevo}`: devuelve 204.
6. `GET /api/combates/1`: Gandalf vuelve a 64, la furia de Thorin a 10 y le toca otra vez a Thorin.

Esto también está automatizado en `CombateTests.cs` (`CambiarNota_NoAlteraElDanioNiLasVidas`, `DeshacerUltimoTurno_RestauraVidasRecursosYOrden` y `DeshacerTurno_QueTerminoElCombate_VuelveAEnCursoSinGanador`).

---

## 🧬 Polimorfismo: cómo se elige el comportamiento de cada personaje

`Personaje` es abstracta y declara `abstract int Atacar(Personaje objetivo)` y `abstract int UsarHabilidad(Personaje objetivo)`. Cada subclase las implementa con sus propias reglas:

| Clase | Recurso inicial | Atacar | Habilidad |
|---|---|---|---|
| Guerrero (120/20/8) | Furia 0 | Con ≥20 de furia pega +15 y gasta 20; si no, suma 10 de furia | **Golpe Devastador**: ×1.5, gasta 20 de furia |
| Mago (80/14/4) | Maná 40 | Proyectil normal, sin costo | **Bola de Fuego**: ×2, gasta 10 de maná |
| Arquero (100/16/6) | Flechas 10 | Con flechas +10 y gasta 1; sin flechas la mitad | **Lluvia de Flechas**: ×2, gasta 3 flechas |
| Asesino (90/15/5) | Energía 30 | Con ≥15 de energía golpe ×3 y gasta 15; si no, normal y suma 5 | **Puñalada Letal**: ×3, gasta 15 de energía |

En el turno, `Combate.EjecutarTurno` hace un `switch` **sobre la acción** que eligió el cliente, nunca sobre la clase del personaje:

```csharp
int danio = accionElegida switch
{
    "atacar"    => atacante.Atacar(defensor),
    "habilidad" => atacante.UsarHabilidad(defensor),
    _           => throw new ArgumentException("Acción inválida: usá 'atacar' o 'habilidad'.")
};
```

C# ejecuta el `override` de la clase real del objeto. El único lugar que conoce las clases concretas es `FabricaPersonajes`, y solo se usa para **crear** o **reconstruir** el objeto a partir del texto del tipo.

---

## 🔬 Exploración (etapa de Indagación)

Pruebas de concepto independientes del proyecto final, sin MySQL:

```bash
dotnet run --project exploracion                 # Act. 1 y 2: GET fijo, polimorfismo Guerrero/Mago, DTO en memoria
dotnet run --project exploracion/configuracion   # Act. 3: inyección de dependencias + repositorio en memoria + Swagger
```

Las dos quedan en `http://localhost:5000/swagger`. Se corren de a una, porque usan el mismo puerto.
