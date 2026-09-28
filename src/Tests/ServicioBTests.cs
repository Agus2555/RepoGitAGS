using Aplicacion.Interfaces;
using Aplicacion.Servicios;
using Persistencia.Entidades;
using Xunit;

namespace Tests;

/// <summary>
/// Pruebas sobre la resolución del combate (ServicioCombate.Combatir).
/// Sin repositorios, sin BD: solo lógica de resolución en memoria.
/// </summary>
public class ServicioCombateResolucionTests
{
    private class EstrategiaAtacar : IEstrategiaTurno
    {
        public TipoAccion ElegirAccion(Personaje actor, Personaje objetivo) => TipoAccion.Atacar;
    }

    private static ServicioCombate CrearServicio() => new(new EstrategiaAtacar());

    [Fact]
    public void Combatir_TerminaConGanador_CuandoUnPersonajeMuere()
    {
        var fuerte = new Guerrero("Fuerte", 100, 50, 2, 0); // ataque alto
        var debil  = new Guerrero("Débil",   10,  5, 1, 0); // poca vida

        var resultado = CrearServicio().Combatir(fuerte, debil);

        Assert.NotNull(resultado.Ganador);
        Assert.Equal("Fuerte", resultado.Ganador.Nombre);
        Assert.False(debil.EstaVivo);
    }

    [Fact]
    public void Combatir_GanadorEsElQueQuedaVivo()
    {
        var p1 = new Mago("Mago",     50, 10, 2, 0);   // poca vida
        var p2 = new Guerrero("G", 200, 50, 1, 0);     // ataque alto

        var resultado = CrearServicio().Combatir(p1, p2);

        Assert.NotNull(resultado.Ganador);
        Assert.True(resultado.Ganador.EstaVivo);
    }

    [Fact]
    public void Combatir_TerminaEnEmpate_CuandoSeAlcanzaMaxTurnos()
    {
        // Defensa tan alta que nadie puede matar al otro en 3 turnos.
        var p1 = new Guerrero("T1", 10_000, 1, 999, 0);
        var p2 = new Guerrero("T2", 10_000, 1, 999, 0);

        var resultado = CrearServicio().Combatir(p1, p2, maxTurnos: 3);

        Assert.Null(resultado.Ganador);           // empate
        Assert.Equal(3, resultado.TurnosJugados); // se consumieron los 3 turnos
    }

    [Fact]
    public void Combatir_LanzaExcepcion_SiAmbosPersonajesSonElMismo()
    {
        var p = new Guerrero("P", 100, 20, 5, 0);

        Assert.Throws<ArgumentException>(() => CrearServicio().Combatir(p, p));
    }

    [Fact]
    public void Combatir_RegistraAlMenosUnTurno()
    {
        var p1 = new Guerrero("A", 100, 50, 2, 0);
        var p2 = new Guerrero("B",  10,  5, 1, 0);

        var resultado = CrearServicio().Combatir(p1, p2);

        Assert.NotEmpty(resultado.Turnos);
    }

    [Fact]
    public void Combatir_PrimerTurno_EsDeP1ContraP2()
    {
        var p1 = new Guerrero("A", 100, 50, 2, 0);
        var p2 = new Guerrero("B", 100,  5, 1, 0);

        var resultado = CrearServicio().Combatir(p1, p2);

        var primero = resultado.Turnos[0];
        Assert.Equal("A", primero.NombreActor);
        Assert.Equal("B", primero.NombreObjetivo);
        Assert.Equal(1, primero.Numero);
    }

    [Fact]
    public void Combatir_DanioEnTurnos_SiempreEsMayorIgualACero()
    {
        var p1 = new Guerrero("A", 100, 20, 5, 0);
        var p2 = new Mago("M",     80, 15, 4, 30);

        var resultado = CrearServicio().Combatir(p1, p2);

        Assert.All(resultado.Turnos, t => Assert.True(t.Danio >= 0));
    }
}
