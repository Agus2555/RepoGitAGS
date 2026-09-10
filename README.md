<h1 align="center">E.T. Nº12 D.E. 1º "Libertador Gral. José de San Martín"</h1>

<p align="center">
  <img src="https://et12.edu.ar/imgs/et12.svg">
</p>

# Proyecto Integrador: Código en combate
**Software 2.3**

| Atributo | Detalle |
| :--- | :--- |
| **Espacio Curricular** | Laboratorio de Programación Orientada a Objetos & Bases de Datos |
| **Plan Troncal** | Código en combate (Software 2.3) |
| **Ubicación Temporal** | 2.º Bimestre |
| **Duración** | 1 Bimestre |
| **Estado** | En proceso / Desarrollo Académico |

---

## 📋 Sinopsis del Proyecto

**Código en combate** es un proyecto académico integrador enfocado en el diseño y desarrollo de un simulador de combate táctico por turnos inspirado en videojuegos de rol (RPG). El sistema modela el comportamiento de diversas clases de personajes (**Guerreros**, **Magos**, **Arqueros** y **Asesinos**), cada uno con atributos, mecánicas de recursos (furia, maná, flechas, energía) y habilidades específicas.

El objetivo central del proyecto es poner en práctica el **polimorfismo dinámico**, la **herencia** y las **interfaces** para resolver el flujo de acciones de combate sin recurrir a estructuras condicionales por tipo (`if`/`switch`). La solución está construida sobre **.NET 8** organizada en una arquitectura multiproyecto, integrando persistencia en **MySQL** mediante **Dapper**, procedimientos almacenados (**Stored Procedures**) con manejo explícito de **transacciones**, **inyección de dependencias** y **pruebas unitarias**.

---

## Objetivos Académicos

* **Diseñar un Modelo de Dominio Sólido:** Aplicar los pilares de la Programación Orientada a Objetos (POO) para representar entidades de combate con comportamientos polimórficos.
* **Implementar Principios de Diseño:** Adoptar buenas prácticas de desarrollo (**SOLID**), Inversión de Control (**IoC**) e Inyección de Dependencias (**DI**).
* **Desacoplar Persistencia de Datos:** Utilizar el patrón Repositorio junto con **Dapper** para la abstracción de consultas y el mapeo objeto-relacional liviano.
* **Garantizar la Integridad Transaccional:** Diseñar procedimientos almacenados en MySQL que gestionen la consistencia de datos mediante transacciones explícitas (`START TRANSACTION`, `COMMIT`, `ROLLBACK`).
* **Segregación de Accesos:** Configurar perfiles de usuario diferenciados en la base de datos aplicando el principio de mínimo privilegio.
* **Validación Mediante Testing:** Construir una suite de pruebas unitarias para automatizar la verificación de las reglas de negocio y mecánicas de juego.

---

## Contenidos Académicos Integrados

### 1. Programación Orientada a Objetos & Diseño
* **Polimorfismo:** Métodos virtuales y abstractos, herencia, interfaces y composición.
* **Sobrecarga:** Sobrecarga de funciones y sobrecarga de operadores.
* **Buenas Prácticas:** Principios SOLID, Inversión de Control (IoC) e Inyección de Dependencias (DI).

### 2. Arquitectura de Software & Proyectos .NET
* **Estructura Multiproyecto:** Creación y organización de bibliotecas de clases (*Class Libraries*).
* **Gestión de Dependencias:** Vinculación modular entre proyectos dentro de una solución (`.sln`).
* **Testing:** Proyectos de pruebas unitarias para validación del dominio.

### 3. Base de Datos & Persistencia
* **Stored Procedures:** Operaciones complejas y atomicidad garantizada mediante `COMMIT` y `ROLLBACK`.
* **Persistencia C#:** Integración de **Dapper** para mapeo de datos y ejecución transaccional.
* **Gestión de Usuarios y Seguridad:**
  * **Usuario Administrador:** Acceso completo para tareas de mantenimiento y migración.
  * **Usuario Desarrollo:** Acceso restringido exclusivamente al esquema del proyecto (`db_combatCode`).

---

## 🗂️ Estructura de la Solución

El repositorio se organiza bajo una arquitectura modular por capas para mantener un bajo acoplamiento:

```text
combatCode/
├── scripts/                    # Scripts SQL (DDL, SPs, Seguridad, Reportes)
│   ├── DDL.SQL                 # Definición de tablas y restricciones
│   ├── SP.SQL                  # Procedimientos almacenados transaccionales
│   ├── ESTADISTICAS.SQL        # Consultas de agregación y reportes
│   └── USUARIOS.SQL            # Configuración de usuarios y privilegios
├── src/
│   ├── Aplicacion/             # Lógica de negocio, servicios y contratos
│   │   ├── Interfaces/
│   │   ├── Servicios/
│   │   └── Aplicacion.csproj
│   ├── Persistencia/           # Mapeo, repositorios y conexión a BD (Dapper)
│   │   ├── Entidades/
│   │   ├── Repositorios/
│   │   └── Persistencia.csproj
│   └── Test/                   # Pruebas unitarias
│       └── Test.csproj
└── Proyecto.sln                # Archivo de solución .NET
```

---

## ⚙️ Requisitos y Ejecución

### Prerrequisitos
* **.NET SDK** v8.0 o superior
* **MySQL Server** / **MariaDB**

### Pasos para la Configuración

1. **Clonar e Instalar:**
   ```bash
   git clone <URL_DEL_REPOSITORIO>
   cd combatCode
   ```

2. **Base de Datos:**
   Ejecutar secuencialmente los scripts SQL alojados en `/scripts`:
   * `DDL.SQL`
   * `SP.SQL`
   * `ESTADISTICAS.SQL`
   * `USUARIOS.SQL`

3. **Compilacion de la Solución:**
   ```bash
   dotnet build
   ```

4. **Ejecución de Pruebas Unitarias:**
   ```bash
   dotnet test
   ```

5. **Ejecución de la Aplicación:**
   ```bash
   dotnet run --project src/Aplicacion/Aplicacion.csproj