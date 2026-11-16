# Indagación

En esta etapa investigué cómo se conectan el modelo orientado a objetos, Minimal API y MySQL, haciendo pruebas chicas en la carpeta `exploracion/` antes de armar el proyecto final. Al final de cada parte anoto qué cambió respecto a lo que había pensado en [punto-de-partida.md](punto-de-partida.md).

---

# Actividad 1: Probar antes de construir

## Prueba de concepto

El archivo está en [exploracion/Program.cs](../exploracion/Program.cs). Se ejecuta con:

```bash
dotnet run --project exploracion
```

Y se abre en el navegador `http://localhost:5000/swagger`.

**Cómo se registra una ruta en Minimal API:** con `app.MapGet`, `app.MapPost`, etc., pasando la ruta y una función que devuelve el resultado:

```csharp
app.MapGet("/api/prueba", () => Results.Ok(new PersonajeFijo(1, "Thorin", "Guerrero")))
   .WithSummary("Devuelve un personaje fijo")
   .Produces<PersonajeFijo>(200);
```

- `Results.Ok(...)` devuelve el objeto en JSON con código 200.
- `.Produces<PersonajeFijo>(200)` le avisa a Swagger qué devuelve y con qué código, así aparece documentado.
- `.WithSummary(...)` es la descripción que se ve en Swagger.

Para que aparezca en Swagger hacen falta 4 líneas en `Program.cs`, que explico más abajo en la Actividad 3. Lo probé desde Swagger con "Try it out" → "Execute" y devuelve:

```json
{ "id": 1, "nombre": "Thorin", "tipo": "Guerrero" }
```

## Guerrero y Mago frente a la misma acción

En la ruta `GET /api/prueba/polimorfismo` hay una lista de `PersonajePrueba` (clase abstracta) con un Guerrero y un Mago, y a los dos se les llama `Atacar()`:

```csharp
List<PersonajePrueba> personajes = [new GuerreroPrueba("Thorin"), new MagoPrueba("Gandalf")];
var respuestas = personajes.Select(p => new RespuestaAccion(p.Nombre, p.GetType().Name, p.Atacar()));
```

La respuesta es:

```json
[
  { "nombre": "Thorin",  "clase": "GuerreroPrueba", "resultado": "Thorin golpea con la espada: 20 de daño y junta 10 de furia." },
  { "nombre": "Gandalf", "clase": "MagoPrueba",     "resultado": "Gandalf lanza un proyectil mágico: 14 de daño sin gastar maná." }
]
```

La misma llamada `p.Atacar()` hace cosas distintas según el objeto real, y en ningún lado hay un `if (p is Guerrero)`. Eso es polimorfismo: la clase base dice **qué** se puede hacer (`abstract string Atacar()`) y cada subclase decide **cómo** lo hace (`override`). En el proyecto final pasa lo mismo con `atacante.Atacar(defensor)` y `atacante.UsarHabilidad(defensor)`.

## Preguntas

### ¿Qué diferencia hay entre devolver 200, 201, 400 y 404?

La diferencia que hay entre devolver 200, 201, 400 y 404 es en lo que significa cada una para la API, en específico si la petición falló o fue exitosa, y de qué lado fue el problema, que puede ser del cliente o del servidor.

- **200 (OK):** la solicitud se procesó con éxito y a la vez el servidor devuelve los datos pedidos. Por ejemplo en un GET o un PUT.
- **201 (Created):** la solicitud tuvo éxito y como resultado se creó un nuevo recurso. Por ejemplo en peticiones POST, registrando un usuario nuevo. Además, en la respuesta va la ruta del recurso nuevo (cabecera `Location`), por ejemplo `/api/personajes/6`.
- **400 (Bad Request):** error del cliente. La petición está mal formada (faltan parámetros obligatorios, el JSON está roto o los datos no pasan la validación) y el servidor no puede entenderla ni procesarla.
- **404 (Not Found):** error del cliente. La petición está bien construida, pero el recurso específico al que se intenta acceder no existe en la ruta indicada (por ejemplo, buscar un ID de usuario que ya fue borrado o no fue creado).

Investigando también encontré otros dos que se usan en el proyecto:

- **204 (No Content):** salió bien pero no hay nada para devolver. Es lo que devuelven los DELETE.
- **409 (Conflict):** los datos están bien y el recurso existe, pero la operación choca con una regla. Por ejemplo, borrar un personaje que ya peleó en un combate, o borrar un turno que no es el último.

### ¿Qué parte de una solicitud corresponde a la ruta y cuál al cuerpo?

La **ruta** es la dirección (la URL) y sirve para decir **a qué recurso** le estás hablando. El **cuerpo** (body) es el JSON que se manda y sirve para mandar **los datos** de lo que querés crear o cambiar.

Por ejemplo:

```http
PUT /api/combates/1/turnos/2
Content-Type: application/json

{ "nota": "Gran golpe" }
```

- `1` y `2` van en la ruta, porque identifican **cuál** combate y **cuál** turno.
- `{ "nota": "Gran golpe" }` va en el cuerpo, porque es **el dato nuevo**.

Así, los Id que identifican algo van en la ruta y lo que se crea o se modifica va en el cuerpo. Los GET y los DELETE no llevan cuerpo, solo ruta. En Minimal API se ve en la firma: `(int id, NotaDto dto, ...)` hace que `id` salga de la ruta (porque está en `{id}`) y `dto` salga del cuerpo JSON.

### ¿Qué permisos necesita una conexión que solo consulta datos?

Solamente **SELECT**. Si una conexión solo lee, no tiene que poder hacer INSERT, UPDATE, DELETE, ni crear o borrar tablas. Esto es el principio de mínimo privilegio: si alguien consigue esa contraseña o hay un error en el código, igual no puede romper ni cambiar nada.

En el proyecto el usuario `invitado` tiene:

```sql
GRANT SELECT ON combate_api.* TO 'invitado'@'localhost';
```

Al darlo sobre `combate_api.*`, o sea a nivel base de datos y no tabla por tabla, vale para todas las tablas que existen ahora y también para las que se creen después. Si invitado intenta un INSERT, MySQL le contesta `ERROR 1142 (42000): INSERT command denied`.

Otra cosa que encontré es que **para llamar a una función o un procedimiento almacenado MySQL pide el permiso EXECUTE, no alcanza con SELECT**. Esto importa para lo que me había sugerido el profe (ver abajo).

### ¿Qué datos debe incluir un backup para restaurar el proyecto?

- La **estructura**: el `CREATE DATABASE` y los `CREATE TABLE` con sus claves primarias, foráneas, `UNIQUE` y `CHECK`.
- Los **datos**: los `INSERT` de personajes, combates y turnos, en un orden que respete las claves foráneas.

Lo que **no** tiene que tener son las credenciales, o sea los usuarios y las contraseñas. Esos se crean aparte con `sql/02_usuarios.sql`, cada uno con su contraseña. Por eso el backup se hace solo de la base `combate_api` (`mysqldump --databases combate_api`), que no incluye la base `mysql` donde están los usuarios.

## Correcciones a las hipótesis iniciales

| Lo que pensaba al principio | Lo que encontré |
|---|---|
| El POST de combate simula toda la pelea de una. | En una API cada solicitud tiene que ser corta y el cliente decide cada acción. Entonces `POST /api/combates` solo **crea** el combate y cada turno es un `POST /api/combates/{id}/turnos` aparte. Para eso el estado del combate (vida y recurso de cada uno) tiene que quedar guardado en la base entre una solicitud y otra. |
| El cliente manda vida, ataque y defensa al crear un personaje. | La consigna dice que se manda **solo nombre y tipo**. Las estadísticas son reglas de cada clase, así que las pone el constructor de cada clase (por ejemplo `Guerrero(string nombre)` arranca con 120 de vida). |
| Con un usuario de base de datos alcanza. | Hacen falta dos: **invitado** (solo SELECT) para todos los GET y **administrador** para todas las escrituras. |
| El Id del recurso puede ir en el cuerpo. | Los Id que identifican el recurso van en la ruta y el cuerpo lleva los datos. |
| Al crear se devuelve 200. | Se devuelve 201. Los DELETE devuelven 204 sin cuerpo y las restricciones, 409. |
| Borrar la fila del turno alcanza para deshacerlo. | No: si solo se borra la fila, la vida y el recurso del combate quedan como después del turno. Por eso la tabla `turnos` guarda la vida y el recurso de ambos **antes** del turno. |
| Se reutilizan `ParticipantesBatalla` y los SP de estadísticas. | El plan nuevo pide solo `personajes`, `combates` y `turnos`. El daño acumulado y el resultado salen de los turnos y de `GanadorId`. |

### Sobre la función almacenada que sugirió el profe

En el bimestre pasado el profe me sugirió una función almacenada que devuelva 1 o 0 según `ParticipantesBatalla.Resultado`, para no repetir tantos `SUM(CASE WHEN ... THEN 1 ELSE 0 END)`. En este plan **no la agregué**, porque:

1. La tabla `ParticipantesBatalla` y los SP de estadísticas ya no existen en el modelo nuevo.
2. Todas las consultas GET tienen que usar al usuario **invitado**, que tiene **únicamente SELECT**. Para usar una función almacenada MySQL pide el permiso **EXECUTE**, entonces dárselo al invitado iría en contra de la consigna.

El resultado de cada combate ahora es simplemente la columna `GanadorId`, que se lee con un SELECT normal.

---

# Actividad 2: DTO entre la API y el servicio

## Qué es un DTO

Un **DTO** (Data Transfer Object) es una clase que tiene **solo datos**, sin reglas ni métodos, y sirve para definir **qué entra y qué sale** de la API. Por ejemplo, en el proyecto son `record` de C#:

```csharp
public record CrearPersonajeDto(string? Nombre, string? Tipo);   // entrada
public record PersonajeCreadoDto(int Id, string Nombre);          // salida
```

Se diferencia de una **entidad del dominio** (`Personaje`, `Combate`) en que la entidad tiene comportamiento (`Atacar`, `EjecutarTurno`, validaciones) y datos internos, mientras que el DTO es solo "el formulario" que viaja por HTTP.

¿Para qué sirve?
- Para no exponer datos internos de la entidad.
- Para que si cambia la entidad, el contrato de la API (el JSON) no cambie, y al revés.
- Para que el cliente no pueda mandar campos que no debería (por ejemplo, la vida o el Id).

## La prueba

- [exploracion/dto/PersonajeDtos.cs](../exploracion/dto/PersonajeDtos.cs): los dos DTO.
- [exploracion/dto/ServicioPersonajes.cs](../exploracion/dto/ServicioPersonajes.cs): la entidad `PersonajeEntidad` y el servicio, con una lista en memoria.

El servicio recibe el DTO de entrada, arma la entidad (con Id, Vida y FechaCreacion), la guarda en la lista y devuelve el DTO de salida con solo Id y Nombre. Se puede probar con `POST /api/prueba/personajes` en Swagger.

### Ejemplo JSON de entrada

```json
{ "nombre": "Thorin", "tipo": "Guerrero" }
```

### Ejemplo JSON de salida (201 Created)

```json
{ "id": 1, "nombre": "Thorin" }
```

## Qué datos no deberían exponerse

- En la prueba, la `Vida` y la `FechaCreacion` de la entidad, que son datos internos que el cliente no pidió.
- En el proyecto final, la "foto" de cada turno (`VidaP1Antes`, `RecursoP1Antes`, etc.). Es un dato interno que solo sirve para deshacer el último turno, por eso `TurnoDto` no lo tiene.
- En general, cualquier cosa sensible o interna, como contraseñas, cadenas de conexión o detalles de la base.
- Tampoco la entidad entera, porque tiene métodos y estado que no tienen sentido en un JSON.

## Dónde se transforman los datos

En el **servicio** (capa Aplicación):

1. **Entrada (DTO → entidad):** el servicio recibe el DTO y crea la entidad. En el proyecto final, `ServicioPersonajes.CrearAsync` llama a `FabricaPersonajes.Crear(dto.Tipo, dto.Nombre)`.
2. **Salida (entidad → DTO):** antes de devolver, el servicio convierte la entidad en DTO. En el proyecto final eso está todo junto en [src/Aplicacion/Servicios/Mapeo.cs](../src/Aplicacion/Servicios/Mapeo.cs).

Así los endpoints solo ven DTOs y nunca una entidad.

---

# Actividad 3: Registrar dependencias y mostrar Swagger

Prueba en [exploracion/configuracion/](../exploracion/configuracion/): `Program.cs`, `Configuracion.csproj` y `RepositorioMemoria.cs`, que tiene el repositorio en memoria y el servicio. Se ejecuta con:

```bash
dotnet run --project exploracion/configuracion
```

## Cómo se registra y cómo llega a la ruta

En `Program.cs` se le dice al contenedor "cuando alguien pida esta interfaz, dale esta clase":

```csharp
builder.Services.AddSingleton<IRepositorioPersonajes, RepositorioPersonajesMemoria>();
builder.Services.AddScoped<IServicioPersonajes, ServicioPersonajes>();
```

Y Minimal API lo **inyecta como parámetro** de la ruta, sin hacer `new`:

```csharp
app.MapGet("/api/personajes", (IServicioPersonajes servicio) => Results.Ok(servicio.Listar()));
```

Además, el servicio recibe el repositorio por su constructor: `ServicioPersonajes(IRepositorioPersonajes repositorio)`. El contenedor arma toda la cadena solo.

## ¿Por qué depender de interfaces y no crear el repositorio en cada ruta?

- **Se puede cambiar la implementación sin tocar las rutas.** En la prueba el repositorio es en memoria; en el proyecto final es con MySQL (`PersonajeRepository`). Las rutas no cambian, solo cambia una línea de registro.
- **Se puede probar.** Si cada ruta hiciera `new PersonajeRepository(...)`, quedaría atada a MySQL y no se podría probar sin la base.
- **Hay un solo lugar de configuración.** Si cada ruta creara su repositorio, habría que repetir la cadena de conexión en todos lados. Con inyección se configura una vez en `Program.cs`.
- Es la **inversión de dependencias** de SOLID: la API depende de una abstracción (la interfaz) y no de una clase concreta.

## Tiempo de vida elegido

| Qué | Tiempo de vida | Por qué |
|---|---|---|
| Repositorio en memoria (prueba) | **Singleton** | La lista vive adentro del objeto. Si fuera Scoped, en cada request se crearía una lista nueva y se perderían los datos. |
| Servicio (prueba y final) | **Scoped** | Una instancia por request: no guarda nada entre solicitudes y así no se mezclan datos de dos clientes. |
| Repositorios con MySQL (final) | **Scoped** | No guardan estado, abren y cierran la conexión en cada operación. Uno por request es lo normal. |
| `IDbConnectionFactory` (final) | **Singleton** | Solo guarda las dos cadenas de conexión, que no cambian nunca, así que alcanza con una para toda la app. |

## Qué configuración habilita Swagger

Con el paquete `Swashbuckle.AspNetCore` en el `.csproj`, son 4 líneas:

```csharp
builder.Services.AddEndpointsApiExplorer();   // hace que las rutas de Minimal API se puedan "descubrir"
builder.Services.AddSwaggerGen();             // genera el documento OpenAPI a partir de esas rutas

app.UseSwagger();     // publica el documento en /swagger/v1/swagger.json
app.UseSwaggerUI();   // publica la interfaz web en /swagger
```

- Las dos primeras (en `builder.Services`) **generan el documento**.
- Las dos últimas (en `app`) **lo publican**: una el JSON y otra la interfaz.
- Lo que se ve de cada ruta (descripción, cuerpo y códigos de respuesta) sale de `.WithSummary(...)` y `.Produces<T>(código)`.

Comprobé que `GET /api/personajes` aparece en Swagger y responde desde la interfaz con la lista del repositorio en memoria, y que `GET /api/personajes/99` devuelve 404.

## Más correcciones a las hipótesis

| Lo que pensaba al principio | Lo que encontré |
|---|---|
| Los endpoints pueden devolver las entidades directo. | Se usan DTOs para entrar y salir, y la transformación se hace en el servicio. |
| Las operaciones importantes van en stored procedures. | Para que las reglas del combate no queden mezcladas con SQL, las reglas quedan en C# (la clase `Combate`) y el repositorio solo guarda. La transacción para guardar el turno y actualizar el combate se hace desde C# con Dapper (`BeginTransaction`, `Commit`, `Rollback`). |
| Las contraseñas van en `appsettings.json`. | Van en **variables de entorno** (`ConnectionStrings__Invitado` y `ConnectionStrings__Administrador`), así no se suben a GitHub. |
