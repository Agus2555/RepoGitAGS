# Reflexión final: volver al punto de partida

Releí [punto-de-partida.md](punto-de-partida.md) y lo comparé con el proyecto terminado. Muchas cosas las había pensado como el bimestre pasado (una app de consola que simula todo de una) y al pasarlo a una API tuve que cambiarlas.

---

## Decisiones que cambié

### 1. De simular todo el combate a un turno por solicitud

**Al principio:** el POST de combate iba a simular toda la pelea, como `ServicioBatallas.SimularAsync`.

**Al final:** `POST /api/combates` solo crea el combate y cada turno es un `POST /api/combates/{id}/turnos` donde el cliente elige quién ataca y qué acción usa. Para que funcione entre una solicitud y otra, el estado del combate (vida y recurso de cada uno, estado y ganador) se guarda en la tabla `combates`, y en cada solicitud se reconstruye el objeto `Combate` desde la base.

**Qué aprendí:** en una API cada solicitud es independiente y el servidor no "se acuerda" de nada entre una y otra si no lo guarda. Por eso el estado tiene que estar en la base y no en memoria.

### 2. De stored procedures a reglas en C# con transacciones desde el repositorio

**Al principio:** registrar turnos y finalizar batallas con SP (`sp_RegistrarTurnoYActualizarDanio`, `sp_FinalizarBatalla`), como el bimestre pasado.

**Al final:** las reglas (a quién le toca, el daño, cuándo termina, cómo se deshace) están todas en la clase `Combate`, y el repositorio solo guarda. Guardar un turno toca dos tablas (INSERT en `turnos` y UPDATE en `combates`), entonces lo hago en **una transacción** desde C# con Dapper. Borrar el último turno es **otra transacción**.

**Qué aprendí:** que las reglas del dominio no se mezclen con las consultas hace que se puedan probar sin MySQL. Los 116 tests corren sin base de datos. La transacción sigue siendo necesaria, porque si falla el UPDATE no puede quedar el turno guardado a medias.

### 3. De un usuario de base de datos a dos usuarios con permisos distintos

**Al principio:** un solo usuario de desarrollo que hacía todo.

**Al final:** `invitado` con **solo SELECT** para todos los GET y `administrador` para todas las escrituras. En el código está separado en `IDbConnectionFactory` con `CrearConexionInvitado()` y `CrearConexionAdministrador()`, y cada método de los repositorios usa la que corresponde. Las contraseñas van por variables de entorno.

**Qué aprendí:** el principio de mínimo privilegio. Aunque haya un error en un GET, con el usuario invitado MySQL no deja modificar nada (`ERROR 1142`). También que hay permisos que no son obvios, como que para usar funciones almacenadas hace falta EXECUTE. Por eso no agregué la función que sugirió el profe: el invitado no podría usarla.

### 4. De devolver entidades a usar DTOs

**Al principio:** devolver `Personaje` o `Batalla` directo.

**Al final:** cada endpoint recibe y devuelve DTOs (`CrearPersonajeDto`, `PersonajeDto`, `TurnoDto`, etc.) y la conversión está en `Mapeo.cs`. Así no se exponen datos internos como la "foto" de antes del turno.

**Qué aprendí:** el JSON es un contrato con el cliente y no tiene por qué ser igual a mis clases.

### 5. Saqué la acción Defender y la esquiva del Asesino

**Al principio:** cada clase tenía Atacar, Defender y Habilidad, y el Asesino esquivaba el próximo golpe después de defender.

**Al final:** la consigna pide solo `atacar` y `habilidad`, entonces dejé esas dos como métodos `abstract`, así cada clase está obligada a tener las suyas. La esquiva era un estado "escondido" (un `bool` privado) que no se guardaba en la base ni se podía restaurar al deshacer un turno, así que la saqué.

**Qué aprendí:** al guardar el estado en una base hay que pensar qué parte del objeto se pierde entre solicitudes.

### 6. Deshacer un turno necesita guardar el "antes"

**Al principio:** pensaba que con borrar la fila del turno alcanzaba.

**Al final:** cada turno guarda `VidaP1Antes`, `VidaP2Antes`, `RecursoP1Antes` y `RecursoP2Antes`. Al borrar el último, `Combate.DeshacerTurno` vuelve a poner esos valores, el estado vuelve a `EnCurso` y se borra el ganador.

---

## Cómo se elige el comportamiento de cada personaje

En el turno, `Combate.EjecutarTurno` hace un `switch` **sobre la acción** que mandó el cliente ("atacar" o "habilidad") y llama a `atacante.Atacar(defensor)` o `atacante.UsarHabilidad(defensor)`. Nunca pregunta de qué clase es el personaje: C# ejecuta el `override` de la clase real del objeto (Guerrero, Mago, Arquero o Asesino). El único lugar que conoce las clases concretas es `FabricaPersonajes`, y solo se usa para **crear** el objeto a partir del texto "Mago" que llega en el JSON o que está guardado en la base.

---

## Dificultades y cómo las resolví

- **Cómo guardar el recurso de cada clase si cada una lo llama distinto** (Furia, Maná, Flechas, Energía). Lo resolví con la propiedad abstracta `Recurso` y el método `AsignarRecurso`, que cada clase implementa. Así la base guarda un solo número (`RecursoP1`) y cada clase sabe dónde ponerlo.
- **A quién le toca sin agregar columnas.** Si la cantidad de turnos es par le toca a P1 y si es impar a P2. Además, cuando se borra el último turno el orden se restaura solo.
- **Que las contraseñas no queden en GitHub.** Las cadenas de conexión se leen de variables de entorno, y si faltan la API avisa con un mensaje claro.
- **Traducir errores a códigos HTTP sin llenar las rutas de try/catch.** Los servicios lanzan `NoEncontradoException`, `DatosInvalidosException` o `ConflictoException`, y un solo middleware en `Program.cs` las convierte en 404, 400 o 409.
- **Pasar el proyecto de .NET 8 a .NET 10.** Cambié el `TargetFramework` de todos los proyectos y actualicé paquetes. MySqlConnector tenía una vulnerabilidad conocida en la versión vieja.

---

## Una mejora que haría si siguiera

**Autenticación con usuarios de la API (por ejemplo con tokens JWT).** Hoy cualquiera que llegue a la API puede crear, modificar o borrar. Agregaría usuarios con login y que solo el dueño de un combate pueda jugar sus turnos.

Otras ideas:
- Un endpoint de estadísticas (victorias por clase, daño promedio), como los reportes del bimestre pasado.
- Volver a agregar la acción "defender", guardando en la base el estado de esquiva para poder deshacerlo.
- Paginar `GET /api/personajes` y `GET /api/combates` si hay muchos registros.
