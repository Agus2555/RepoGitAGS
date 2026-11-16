using Persistencia.Entidades;
using Xunit;

namespace Tests;

/// <summary>
/// Pruebas de las reglas de cada clase de personaje: daño, habilidades y personaje derrotado.
/// Sin MySQL ni API: se llama directo a Atacar / UsarHabilidad del dominio.
/// Cada mecánica se cubre con [Theory] e [InlineData] variando stats, recursos y resultados esperados.
/// </summary>
public class PersonajesTests
{
    // ── Guerrero ─────────────────────────────────────────────────────────────

    // Fórmula sin furia: daño neto = max(Ataque - defEnemy, 1); furia += 10
    [Theory]
    [InlineData(20,  0,  2, 18, 10)]  // básico: 20-2=18, furia 0→10
    [InlineData(30,  0,  4, 26, 10)]  // ataque alto: 30-4=26
    [InlineData( 8,  0, 50,  1, 10)]  // defensa enemiga altísima → mínimo 1
    [InlineData( 1,  0,  1,  1, 10)]  // ataque mínimo → siempre 1
    [InlineData(20, 10,  2, 18, 20)]  // furia <20: no activa bono; 10→20
    public void Guerrero_AtacarSinFuria_DanioYFuria(
        int ataque, int furiaInicial, int defEnemy, int danioEsperado, int furiaFinal)
    {
        var guerrero = new Guerrero("G", 200, ataque, 5, furiaInicial);
        var objetivo = new Guerrero("E", 500, 5, defEnemy, 0);

        int danio = guerrero.Atacar(objetivo);

        Assert.Equal(danioEsperado, danio);
        Assert.Equal(furiaFinal, guerrero.Furia);
    }

    // Fórmula con furia (>=20): daño neto = max(Ataque+15 - defEnemy, 1); furia -= 20
    [Theory]
    [InlineData(20, 20,  2, 33,  0)]  // estándar: 20+15-2=33, 20→0
    [InlineData(20, 25,  2, 33,  5)]  // furia extra: mismo daño, 25→5
    [InlineData(25, 20,  3, 37,  0)]  // ataque mayor: 25+15-3=37
    [InlineData(10, 20,100,  1,  0)]  // defensa altísima → mínimo 1
    [InlineData(15, 30,  5, 25, 10)]  // 15+15-5=25, 30→10
    public void Guerrero_AtacarConFuria_DanioYConsumo(
        int ataque, int furiaInicial, int defEnemy, int danioEsperado, int furiaFinal)
    {
        var guerrero = new Guerrero("G", 200, ataque, 5, furiaInicial);
        var objetivo = new Guerrero("E", 500, 5, defEnemy, 0);

        int danio = guerrero.Atacar(objetivo);

        Assert.Equal(danioEsperado, danio);
        Assert.Equal(furiaFinal, guerrero.Furia);
    }

    // Golpe Devastador (>=20 furia): daño neto = max(Ataque + Ataque/2 - defEnemy, 1); furia -= 20
    [Theory]
    [InlineData(20, 20,  1, 29,  0)]  // 20+10-1=29, 20→0
    [InlineData(30, 20,  5, 40,  0)]  // 30+15-5=40
    [InlineData(10, 25,  2, 13,  5)]  // 10+5-2=13, 25→5
    [InlineData(40, 20, 10, 50,  0)]  // 40+20-10=50
    [InlineData( 1, 20,  1,  1,  0)]  // 1+0-1=0 → mínimo 1
    public void Guerrero_GolpeDevastador_DanioYConsumo(
        int ataque, int furiaInicial, int defEnemy, int danioEsperado, int furiaFinal)
    {
        var guerrero = new Guerrero("G", 200, ataque, 5, furiaInicial);
        var objetivo = new Guerrero("E", 500, 5, defEnemy, 0);

        int danio = guerrero.UsarHabilidad(objetivo);

        Assert.Equal(danioEsperado, danio);
        Assert.Equal(furiaFinal, guerrero.Furia);
    }

    // Sin furia suficiente: habilidad falla, no consume furia ni daña al objetivo
    [Theory]
    [InlineData( 0, 100)]
    [InlineData( 5, 100)]
    [InlineData(10, 200)]
    [InlineData(19,  50)]
    public void Guerrero_HabilidadSinFuria_NoDanaNiConsumeFuria(int furiaInicial, int vidaObjetivo)
    {
        var guerrero = new Guerrero("G", 100, 20, 5, furiaInicial);
        var objetivo = new Guerrero("E", vidaObjetivo, 5, 2, 0);

        int danio = guerrero.UsarHabilidad(objetivo);

        Assert.Equal(0, danio);
        Assert.Equal(furiaInicial, guerrero.Furia);
        Assert.Equal(vidaObjetivo, objetivo.Vida);
    }

    // ── Mago ─────────────────────────────────────────────────────────────────

    // Ataque básico: daño neto = max(Ataque - defEnemy, 1); maná no cambia
    [Theory]
    [InlineData(20, 30,  2, 18)]  // básico: 20-2=18
    [InlineData(15,  0,  3, 12)]  // sin maná pero ataca igual: 15-3=12
    [InlineData(25, 10,  5, 20)]  // ataque mayor: 25-5=20
    [InlineData(10, 50, 10,  1)]  // defensa alta → mínimo 1
    [InlineData( 5,  5,  1,  4)]  // ataque bajo: 5-1=4
    public void Mago_AtacarNoConsumeMana(int ataque, int manaInicial, int defEnemy, int danioEsperado)
    {
        var mago     = new Mago("M", 200, ataque, 5, manaInicial);
        var objetivo = new Guerrero("E", 500, 5, defEnemy, 0);

        int danio = mago.Atacar(objetivo);

        Assert.Equal(danioEsperado, danio);
        Assert.Equal(manaInicial, mago.Mana);  // maná intacto
    }

    // Bola de Fuego (>=10 maná): daño neto = max(Ataque*2 - defEnemy, 1); maná -= 10
    [Theory]
    [InlineData(20, 50,  1, 39, 40)]  // 20*2-1=39, 50→40
    [InlineData(20, 10,  2, 38,  0)]  // maná exacto: 10→0
    [InlineData(15, 30,  3, 27, 20)]  // 15*2-3=27, 30→20
    [InlineData(10, 10,  1, 19,  0)]  // 10*2-1=19, 10→0
    [InlineData(30, 20,  5, 55, 10)]  // 30*2-5=55, 20→10
    public void Mago_BolaDeFuego_DanioYConsumo(
        int ataque, int manaInicial, int defEnemy, int danioEsperado, int manaFinal)
    {
        var mago     = new Mago("M", 200, ataque, 5, manaInicial);
        var objetivo = new Guerrero("E", 500, 5, defEnemy, 0);

        int danio = mago.UsarHabilidad(objetivo);

        Assert.Equal(danioEsperado, danio);
        Assert.Equal(manaFinal, mago.Mana);
    }

    // Sin maná suficiente (<10): habilidad falla, maná y vida objetivo sin cambio
    [Theory]
    [InlineData(0, 100)]
    [InlineData(5, 100)]
    [InlineData(9, 100)]  // justo bajo el umbral de 10
    public void Mago_HabilidadSinMana_NoDanaNiConsumeMana(int manaInicial, int vidaObjetivo)
    {
        var mago     = new Mago("M", 100, 20, 5, manaInicial);
        var objetivo = new Guerrero("E", vidaObjetivo, 5, 2, 0);

        int danio = mago.UsarHabilidad(objetivo);

        Assert.Equal(0, danio);
        Assert.Equal(manaInicial, mago.Mana);
        Assert.Equal(vidaObjetivo, objetivo.Vida);
    }

    // ── Arquero ──────────────────────────────────────────────────────────────

    // Con flechas: daño neto = max(Ataque+10 - defEnemy, 1); flechas -= 1
    [Theory]
    [InlineData(20,  5,  1, 29, 4)]  // 20+10-1=29, 5→4
    [InlineData(15,  3,  2, 23, 2)]  // 15+10-2=23, 3→2
    [InlineData(10,  1,  5, 15, 0)]  // 10+10-5=15, 1→0
    [InlineData(20, 10, 15, 15, 9)]  // 20+10-15=15, 10→9
    [InlineData( 5,  2, 20,  1, 1)]  // 5+10-20=-5 → mínimo 1, 2→1
    public void Arquero_AtacarConFlechas_DanioYConsumo(
        int ataque, int flechasIniciales, int defEnemy, int danioEsperado, int flechasFinal)
    {
        var arquero  = new Arquero("A", 200, ataque, 5, flechasIniciales);
        var objetivo = new Guerrero("E", 500, 5, defEnemy, 0);

        int danio = arquero.Atacar(objetivo);

        Assert.Equal(danioEsperado, danio);
        Assert.Equal(flechasFinal, arquero.Flechas);
    }

    // Sin flechas: daño neto = max(Ataque/2 - defEnemy, 1); flechas sin cambio
    [Theory]
    [InlineData(20,  1,  9)]  // 20/2-1=9
    [InlineData(10,  1,  4)]  // 10/2-1=4
    [InlineData( 8, 10,  1)]  // 8/2-10=-6 → mínimo 1
    [InlineData(30,  5, 10)]  // 30/2-5=10
    [InlineData( 1,  1,  1)]  // 1/2=0, 0-1=-1 → mínimo 1
    public void Arquero_AtacarSinFlechas_DanioMitadDelAtaque(int ataque, int defEnemy, int danioEsperado)
    {
        var arquero  = new Arquero("A", 200, ataque, 5, flechasIniciales: 0);
        var objetivo = new Guerrero("E", 500, 5, defEnemy, 0);

        int danio = arquero.Atacar(objetivo);

        Assert.Equal(danioEsperado, danio);
        Assert.Equal(0, arquero.Flechas);
    }

    // Lluvia de Flechas (>=3 flechas): daño neto = max(Ataque*2 - defEnemy, 1); flechas -= 3
    [Theory]
    [InlineData(20,  3,  1, 39, 0)]  // 20*2-1=39, 3→0
    [InlineData(15,  5,  3, 27, 2)]  // 15*2-3=27, 5→2
    [InlineData(10, 10,  5, 15, 7)]  // 10*2-5=15, 10→7
    [InlineData(20,  4, 20, 20, 1)]  // 20*2-20=20, 4→1
    [InlineData(25,  3,  1, 49, 0)]  // 25*2-1=49, 3→0
    public void Arquero_LluviaDeFlechas_DanioYConsumo(
        int ataque, int flechasIniciales, int defEnemy, int danioEsperado, int flechasFinal)
    {
        var arquero  = new Arquero("A", 200, ataque, 5, flechasIniciales);
        var objetivo = new Guerrero("E", 500, 5, defEnemy, 0);

        int danio = arquero.UsarHabilidad(objetivo);

        Assert.Equal(danioEsperado, danio);
        Assert.Equal(flechasFinal, arquero.Flechas);
    }

    // Sin flechas suficientes (<3): habilidad falla, flechas y vida objetivo sin cambio
    [Theory]
    [InlineData(0, 100)]
    [InlineData(1, 100)]
    [InlineData(2, 100)]  // justo bajo el umbral de 3
    public void Arquero_HabilidadSinFlechasSuficientes_NoDanaNoConsumeFlechas(
        int flechasIniciales, int vidaObjetivo)
    {
        var arquero  = new Arquero("A", 100, 20, 5, flechasIniciales);
        var objetivo = new Guerrero("E", vidaObjetivo, 5, 1, 0);

        int danio = arquero.UsarHabilidad(objetivo);

        Assert.Equal(0, danio);
        Assert.Equal(flechasIniciales, arquero.Flechas);
        Assert.Equal(vidaObjetivo, objetivo.Vida);
    }

    // ── Asesino ──────────────────────────────────────────────────────────────

    // Atacar con energía (>=15): daño neto = max(Ataque*3 - defEnemy, 1); energía -= 15
    [Theory]
    [InlineData(20, 15,  1, 59,  0)]  // 20*3-1=59, 15→0
    [InlineData(20, 20,  1, 59,  5)]  // mismo daño, 20→5
    [InlineData(10, 15,  2, 28,  0)]  // 10*3-2=28, 15→0
    [InlineData(15, 30,  3, 42, 15)]  // 15*3-3=42, 30→15
    [InlineData(20, 15, 50, 10,  0)]  // 20*3-50=10, defensa alta
    public void Asesino_AtacarConEnergia_DanioTripleYConsumo(
        int ataque, int energiaInicial, int defEnemy, int danioEsperado, int energiaFinal)
    {
        var asesino  = new Asesino("As", 200, ataque, 5, energiaInicial);
        var objetivo = new Guerrero("E", 500, 5, defEnemy, 0);

        int danio = asesino.Atacar(objetivo);

        Assert.Equal(danioEsperado, danio);
        Assert.Equal(energiaFinal, asesino.Energia);
    }

    // Atacar sin energía (<15): daño neto = max(Ataque - defEnemy, 1); energía += 5
    [Theory]
    [InlineData(20,  0,  1, 19,  5)]  // 20-1=19, 0→5
    [InlineData(15,  5,  3, 12, 10)]  // 15-3=12, 5→10
    [InlineData(10, 10,  1,  9, 15)]  // 10-1=9, 10→15
    [InlineData(20, 14,  5, 15, 19)]  // 14<15: 20-5=15, 14→19
    [InlineData( 5,  0, 10,  1,  5)]  // 5-10=-5 → mínimo 1, 0→5
    public void Asesino_AtacarSinEnergia_DanioNormalYRecupera(
        int ataque, int energiaInicial, int defEnemy, int danioEsperado, int energiaFinal)
    {
        var asesino  = new Asesino("As", 200, ataque, 5, energiaInicial);
        var objetivo = new Guerrero("E", 500, 5, defEnemy, 0);

        int danio = asesino.Atacar(objetivo);

        Assert.Equal(danioEsperado, danio);
        Assert.Equal(energiaFinal, asesino.Energia);
    }

    // Puñalada Letal (>=15 energía): daño neto = max(Ataque*3 - defEnemy, 1); energía -= 15
    [Theory]
    [InlineData(20, 20,  1, 59,  5)]  // 20*3-1=59, 20→5
    [InlineData(10, 15,  2, 28,  0)]  // 10*3-2=28, 15→0
    [InlineData(15, 30,  3, 42, 15)]  // 15*3-3=42, 30→15
    [InlineData(20, 15,  5, 55,  0)]  // 20*3-5=55, 15→0
    [InlineData( 5, 20,  1, 14,  5)]  // 5*3-1=14, 20→5
    public void Asesino_PunaladaLetal_DanioTripleYConsumo(
        int ataque, int energiaInicial, int defEnemy, int danioEsperado, int energiaFinal)
    {
        var asesino  = new Asesino("As", 200, ataque, 5, energiaInicial);
        var objetivo = new Guerrero("E", 500, 5, defEnemy, 0);

        int danio = asesino.UsarHabilidad(objetivo);

        Assert.Equal(danioEsperado, danio);
        Assert.Equal(energiaFinal, asesino.Energia);
    }

    // Sin energía suficiente (<15): habilidad falla, energía y vida objetivo sin cambio
    [Theory]
    [InlineData( 0, 100)]
    [InlineData(10, 100)]
    [InlineData(14, 100)]  // justo bajo el umbral de 15
    public void Asesino_HabilidadSinEnergia_NoDanaNiConsumeEnergia(int energiaInicial, int vidaObjetivo)
    {
        var asesino  = new Asesino("As", 100, 20, 5, energiaInicial);
        var objetivo = new Guerrero("E", vidaObjetivo, 5, 1, 0);

        int danio = asesino.UsarHabilidad(objetivo);

        Assert.Equal(0, danio);
        Assert.Equal(energiaInicial, asesino.Energia);
        Assert.Equal(vidaObjetivo, objetivo.Vida);
    }

    // ── Reglas generales (polimórficas) ──────────────────────────────────────

    [Fact]
    public void VidaNuncaEsNegativa_CuandoElDanioSuperaLaVida()
    {
        var atacante = new Guerrero("G", 100, 200, 5, 0);
        var defensor = new Guerrero("D",  50,  10, 1, 0);

        atacante.Atacar(defensor);

        Assert.Equal(0, defensor.Vida);
        Assert.False(defensor.EstaVivo);
    }

    [Fact]
    public void DefensaMayorAlAtaque_SiempreHaceAlMenosUnoDeDanio()
    {
        var atacante = new Guerrero("G", 100, 5, 5, 0);
        var tanque   = new Guerrero("T", 100, 5, 999, 0);

        int danio = atacante.Atacar(tanque);

        Assert.Equal(1, danio);
        Assert.Equal(99, tanque.Vida);
    }

    [Fact]
    public void Ataque_QueMataAlObjetivo_SoloAplicaLaVidaQueQuedaba()
    {
        var actor    = new Guerrero("Actor", 100, 50, 5, 0);
        var objetivo = new Guerrero("Objetivo", 10, 5, 1, 0);

        int danio = actor.Atacar(objetivo);

        Assert.Equal(10, danio);
        Assert.False(objetivo.EstaVivo);
    }

    // Personaje derrotado: no puede atacar ni usar la habilidad, sea de la clase que sea.
    [Theory]
    [InlineData("Guerrero")]
    [InlineData("Mago")]
    [InlineData("Arquero")]
    [InlineData("Asesino")]
    public void PersonajeDerrotado_NoPuedeAtacarNiUsarHabilidad(string tipo)
    {
        var muerto   = FabricaPersonajes.Crear(tipo, "Muerto");
        var objetivo = new Guerrero("E", 100, 20, 5, 0);
        muerto.RecibirDanio(9999);

        Assert.False(muerto.EstaVivo);
        Assert.Throws<InvalidOperationException>(() => muerto.Atacar(objetivo));
        Assert.Throws<InvalidOperationException>(() => muerto.UsarHabilidad(objetivo));
        Assert.Equal(100, objetivo.Vida);
    }

    // Misma llamada (Atacar) sobre la referencia base Personaje → cada clase responde distinto.
    [Fact]
    public void Polimorfismo_MismaLlamada_DaniosDiferentesPorClase()
    {
        List<Personaje> atacantes =
        [
            new Guerrero("G",  100, 20, 5,  0),
            new Mago("M",      100, 20, 5,  0),
            new Arquero("A",   100, 20, 5,  5),
            new Asesino("As",  100, 20, 5, 20)
        ];

        var danios = atacantes.Select(a => a.Atacar(new Guerrero("E", 500, 5, 1, 0))).ToList();

        // Guerrero 20-1=19, Mago 20-1=19, Arquero 30-1=29, Asesino 60-1=59
        Assert.Equal([19, 19, 29, 59], danios);
    }

    [Theory]
    [InlineData("Guerrero", "Golpe Devastador", "Furia")]
    [InlineData("Mago",     "Bola de Fuego",    "Maná")]
    [InlineData("Arquero",  "Lluvia de Flechas", "Flechas")]
    [InlineData("Asesino",  "Puñalada Letal",   "Energía")]
    public void CadaClase_TieneSuHabilidadYSuRecurso(string tipo, string habilidad, string recurso)
    {
        var personaje = FabricaPersonajes.Crear(tipo, "X");

        Assert.Equal(habilidad, personaje.NombreHabilidad);
        Assert.Equal(recurso, personaje.NombreRecurso);
    }

    // ── Fábrica y estadísticas por defecto ───────────────────────────────────

    [Theory]
    [InlineData("Guerrero", 120, 20, 8,  0)]
    [InlineData("Mago",      80, 14, 4, 40)]
    [InlineData("Arquero",  100, 16, 6, 10)]
    [InlineData("Asesino",   90, 15, 5, 30)]
    public void Fabrica_CreaLaClaseConSusEstadisticasPorDefecto(
        string tipo, int vida, int ataque, int defensa, int recurso)
    {
        var personaje = FabricaPersonajes.Crear(tipo, "Nuevo");

        Assert.Equal(tipo, personaje.Tipo);
        Assert.Equal(vida, personaje.Vida);
        Assert.Equal(vida, personaje.VidaMaxima);
        Assert.Equal(ataque, personaje.Ataque);
        Assert.Equal(defensa, personaje.Defensa);
        Assert.Equal(recurso, personaje.Recurso);
    }

    [Theory]
    [InlineData("mago")]
    [InlineData("MAGO")]
    [InlineData("  Mago  ")]
    public void Fabrica_NoDistingueMayusculasNiEspacios(string tipo)
    {
        Assert.Equal("Mago", FabricaPersonajes.Crear(tipo, "Gandalf").Tipo);
    }

    [Theory]
    [InlineData("Paladin")]
    [InlineData("")]
    [InlineData(null)]
    public void Fabrica_TipoInvalido_LanzaExcepcion(string? tipo)
    {
        Assert.Throws<ArgumentException>(() => FabricaPersonajes.Crear(tipo, "X"));
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    [InlineData("NombreDemasiadoLargoNombreDemasiadoLargoNombreDemasiad")]  // más de 50 caracteres
    public void NombreInvalido_LanzaExcepcion(string? nombre)
    {
        Assert.Throws<ArgumentException>(() => FabricaPersonajes.Crear("Guerrero", nombre));
    }

    [Fact]
    public void CambiarNombre_SoloCambiaElNombre()
    {
        var personaje = FabricaPersonajes.Crear("Arquero", "Legolas");

        personaje.CambiarNombre("  Legolas Hojaverde ");

        Assert.Equal("Legolas Hojaverde", personaje.Nombre);
        Assert.Equal(100, personaje.Vida);
        Assert.Equal(10, personaje.Recurso);
    }
}
