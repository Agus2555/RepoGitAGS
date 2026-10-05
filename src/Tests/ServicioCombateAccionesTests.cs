using Aplicacion.Interfaces;
using Aplicacion.Servicios;
using Persistencia.Entidades;
using Xunit;

namespace Tests;

/// <summary>
/// Pruebas sobre ServicioCombate (capa de Aplicacion).
/// Sin repositorios, Dapper ni conexión a la BD.
/// Estrategias fijas inyectadas para aserciones deterministas.
/// Cada mecánica de personaje se cubre con [Theory] e [InlineData]
/// variando stats, recursos y resultados esperados.
/// </summary>
public class ServicioCombateAccionesTests
{
    // ── Estrategias fijas para tests ─────────────────────────────────────────
    private class EstrategiaAtacar : IEstrategiaTurno
    {
        public TipoAccion ElegirAccion(Personaje actor, Personaje objetivo) => TipoAccion.Atacar;
    }

    private class EstrategiaHabilidad : IEstrategiaTurno
    {
        public TipoAccion ElegirAccion(Personaje actor, Personaje objetivo) => TipoAccion.UsarHabilidad;
    }

    private class EstrategiaDefender : IEstrategiaTurno
    {
        public TipoAccion ElegirAccion(Personaje actor, Personaje objetivo) => TipoAccion.Defender;
    }

    private static ServicioCombate ConEstrategia(IEstrategiaTurno e) => new(e);

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

        var turno = ConEstrategia(new EstrategiaAtacar()).EjecutarTurno(guerrero, objetivo, 1);

        Assert.Equal(danioEsperado, turno.Danio);
        Assert.Equal(furiaFinal, guerrero.Furia);
        Assert.Equal("Ataque Básico", turno.NombreAccion);
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

        var turno = ConEstrategia(new EstrategiaAtacar()).EjecutarTurno(guerrero, objetivo, 1);

        Assert.Equal(danioEsperado, turno.Danio);
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

        var turno = ConEstrategia(new EstrategiaHabilidad()).EjecutarTurno(guerrero, objetivo, 1);

        Assert.Equal(danioEsperado, turno.Danio);
        Assert.Equal(furiaFinal, guerrero.Furia);
        Assert.Equal("Golpe Devastador", turno.NombreAccion);
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

        var turno = ConEstrategia(new EstrategiaHabilidad()).EjecutarTurno(guerrero, objetivo, 1);

        Assert.Equal(0, turno.Danio);
        Assert.Equal(furiaInicial, guerrero.Furia);
        Assert.Equal(vidaObjetivo, objetivo.Vida);
    }

    // Defender acumula 5 de furia; sin tope
    [Theory]
    [InlineData( 0,  5)]
    [InlineData( 5, 10)]
    [InlineData(15, 20)]
    [InlineData(20, 25)]  // ya tiene >=20, pero defender siempre suma 5
    [InlineData(30, 35)]  // sin tope de furia
    public void Guerrero_Defender_AcumulaFuria(int furiaInicial, int furiaFinal)
    {
        var guerrero = new Guerrero("G", 100, 20, 5, furiaInicial);
        var dummy    = new Guerrero("D", 100, 5, 2, 0);

        var turno = ConEstrategia(new EstrategiaDefender()).EjecutarTurno(guerrero, dummy, 1);

        Assert.Equal(furiaFinal, guerrero.Furia);
        Assert.Equal(0, turno.Danio);
        Assert.Equal("Defensa", turno.NombreAccion);
    }

    [Fact]
    public void Guerrero_AlUsarGolpeDevastador_ConsumeFuriaYHaceDanioExtra()
    {
        var guerrero = new Guerrero("G", 100, 20, 5, furiaInicial: 20);
        var objetivo = new Guerrero("E", 500, 5, 1, furiaInicial: 0);

        var turno = ConEstrategia(new EstrategiaHabilidad()).EjecutarTurno(guerrero, objetivo, 1);

        Assert.Equal(0, guerrero.Furia);
        Assert.Equal("Golpe Devastador", turno.NombreAccion);
        Assert.True(turno.Danio > 0);
    }

    // ── Mago ─────────────────────────────────────────────────────────────────

    // Ataque básico: daño neto = max(Ataque - defEnemy, 1); mana no cambia
    [Theory]
    [InlineData(20, 30,  2, 18)]  // básico: 20-2=18
    [InlineData(15,  0,  3, 12)]  // sin mana pero ataca igual: 15-3=12
    [InlineData(25, 10,  5, 20)]  // ataque mayor: 25-5=20
    [InlineData(10, 50, 10,  1)]  // defensa alta → mínimo 1
    [InlineData( 5,  5,  1,  4)]  // ataque bajo: 5-1=4
    public void Mago_AtacarNoConsumeMana(int ataque, int manaInicial, int defEnemy, int danioEsperado)
    {
        var mago     = new Mago("M", 200, ataque, 5, manaInicial);
        var objetivo = new Guerrero("E", 500, 5, defEnemy, 0);

        var turno = ConEstrategia(new EstrategiaAtacar()).EjecutarTurno(mago, objetivo, 1);

        Assert.Equal(danioEsperado, turno.Danio);
        Assert.Equal(manaInicial, mago.Mana);  // mana intacto
        Assert.Equal("Ataque Básico", turno.NombreAccion);
    }

    // Bola de Fuego (>=10 mana): daño neto = max(Ataque*2 - defEnemy, 1); mana -= 10
    [Theory]
    [InlineData(20, 50,  1, 39, 40)]  // 20*2-1=39, 50→40
    [InlineData(20, 10,  2, 38,  0)]  // mana exacto: 10→0
    [InlineData(15, 30,  3, 27, 20)]  // 15*2-3=27, 30→20
    [InlineData(10, 10,  1, 19,  0)]  // 10*2-1=19, 10→0
    [InlineData(30, 20,  5, 55, 10)]  // 30*2-5=55, 20→10
    public void Mago_BolaDeFuego_DanioYConsumo(
        int ataque, int manaInicial, int defEnemy, int danioEsperado, int manaFinal)
    {
        var mago     = new Mago("M", 200, ataque, 5, manaInicial);
        var objetivo = new Guerrero("E", 500, 5, defEnemy, 0);

        var turno = ConEstrategia(new EstrategiaHabilidad()).EjecutarTurno(mago, objetivo, 1);

        Assert.Equal(danioEsperado, turno.Danio);
        Assert.Equal(manaFinal, mago.Mana);
        Assert.Equal("Bola de Fuego", turno.NombreAccion);
    }

    // Sin mana suficiente (<10): habilidad falla, mana y vida objetivo sin cambio
    [Theory]
    [InlineData(0, 100)]
    [InlineData(5, 100)]
    [InlineData(9, 100)]  // justo bajo el umbral de 10
    public void Mago_HabilidadSinMana_NoDanaNiConsumeMana(int manaInicial, int vidaObjetivo)
    {
        var mago     = new Mago("M", 100, 20, 5, manaInicial);
        var objetivo = new Guerrero("E", vidaObjetivo, 5, 2, 0);

        var turno = ConEstrategia(new EstrategiaHabilidad()).EjecutarTurno(mago, objetivo, 1);

        Assert.Equal(0, turno.Danio);
        Assert.Equal(manaInicial, mago.Mana);
        Assert.Equal(vidaObjetivo, objetivo.Vida);
    }

    // Defender regenera exactamente 5 de mana
    [Theory]
    [InlineData( 0,  5)]
    [InlineData( 3,  8)]
    [InlineData(10, 15)]
    [InlineData(20, 25)]
    [InlineData(50, 55)]
    public void Mago_Defender_RegeneraMana(int manaInicial, int manaFinal)
    {
        var mago  = new Mago("M", 100, 20, 5, manaInicial);
        var dummy = new Guerrero("D", 100, 5, 2, 0);

        ConEstrategia(new EstrategiaDefender()).EjecutarTurno(mago, dummy, 1);

        Assert.Equal(manaFinal, mago.Mana);
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

        var turno = ConEstrategia(new EstrategiaAtacar()).EjecutarTurno(arquero, objetivo, 1);

        Assert.Equal(danioEsperado, turno.Danio);
        Assert.Equal(flechasFinal, arquero.Flechas);
        Assert.Equal("Ataque Básico", turno.NombreAccion);
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

        var turno = ConEstrategia(new EstrategiaAtacar()).EjecutarTurno(arquero, objetivo, 1);

        Assert.Equal(danioEsperado, turno.Danio);
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

        var turno = ConEstrategia(new EstrategiaHabilidad()).EjecutarTurno(arquero, objetivo, 1);

        Assert.Equal(danioEsperado, turno.Danio);
        Assert.Equal(flechasFinal, arquero.Flechas);
        Assert.Equal("Lluvia de Flechas", turno.NombreAccion);
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

        var turno = ConEstrategia(new EstrategiaHabilidad()).EjecutarTurno(arquero, objetivo, 1);

        Assert.Equal(0, turno.Danio);
        Assert.Equal(flechasIniciales, arquero.Flechas);
        Assert.Equal(vidaObjetivo, objetivo.Vida);
    }

    // Defender recupera exactamente 1 flecha
    [Theory]
    [InlineData( 0,  1)]
    [InlineData( 2,  3)]
    [InlineData( 5,  6)]
    [InlineData( 3,  4)]  // en el umbral de lluvia, sigue recuperando normalmente
    [InlineData(10, 11)]
    public void Arquero_Defender_RecuperaFlecha(int flechasIniciales, int flechasFinal)
    {
        var arquero = new Arquero("A", 100, 20, 5, flechasIniciales);
        var dummy   = new Guerrero("D", 100, 5, 2, 0);

        ConEstrategia(new EstrategiaDefender()).EjecutarTurno(arquero, dummy, 1);

        Assert.Equal(flechasFinal, arquero.Flechas);
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

        var turno = ConEstrategia(new EstrategiaAtacar()).EjecutarTurno(asesino, objetivo, 1);

        Assert.Equal(danioEsperado, turno.Danio);
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

        var turno = ConEstrategia(new EstrategiaAtacar()).EjecutarTurno(asesino, objetivo, 1);

        Assert.Equal(danioEsperado, turno.Danio);
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

        var turno = ConEstrategia(new EstrategiaHabilidad()).EjecutarTurno(asesino, objetivo, 1);

        Assert.Equal(danioEsperado, turno.Danio);
        Assert.Equal(energiaFinal, asesino.Energia);
        Assert.Equal("Puñalada Letal", turno.NombreAccion);
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

        var turno = ConEstrategia(new EstrategiaHabilidad()).EjecutarTurno(asesino, objetivo, 1);

        Assert.Equal(0, turno.Danio);
        Assert.Equal(energiaInicial, asesino.Energia);
        Assert.Equal(vidaObjetivo, objetivo.Vida);
    }

    // Defender activa esquiva: el primer golpe recibido es ignorado (daño=0, vida intacta)
    [Theory]
    [InlineData(50, 5)]  // golpe fuerte esquivado → vida intacta
    [InlineData(10, 5)]  // golpe débil esquivado → igual vida intacta
    [InlineData( 1, 5)]  // daño mínimo también esquivado
    public void Asesino_Defender_EsquivaUnGolpe_VidaIntacta(int danioRecibido, int defAsesino)
    {
        var asesino = new Asesino("As", 90, 20, defAsesino, energiaInicial: 0);
        var dummy   = new Guerrero("D", 100, 5, 2, 0);

        ConEstrategia(new EstrategiaDefender()).EjecutarTurno(asesino, dummy, 1);

        int vidaAntes      = asesino.Vida;
        int danioAplicado  = asesino.RecibirDanio(danioRecibido);

        Assert.Equal(0, danioAplicado);
        Assert.Equal(vidaAntes, asesino.Vida);
    }

    [Fact]
    public void Asesino_EsquivaSoloUnGolpe_ElSegundoLoDania()
    {
        var asesino = new Asesino("As", 90, 20, 5, energiaInicial: 0);
        var dummy   = new Guerrero("D", 100, 5, 2, 0);

        ConEstrategia(new EstrategiaDefender()).EjecutarTurno(asesino, dummy, 1);

        Assert.Equal(0, asesino.RecibirDanio(50));   // primer golpe: esquivado
        Assert.Equal(45, asesino.RecibirDanio(50));  // segundo: 50-5=45; ya no esquiva
        Assert.Equal(45, asesino.Vida);
    }

    [Fact]
    public void Asesino_Esquivando_ElTurnoDelAtacanteRegistraCeroDanio()
    {
        var asesino  = new Asesino("As", 90, 20, 5, energiaInicial: 0);
        var atacante = new Guerrero("G", 100, 50, 5, furiaInicial: 0);

        ConEstrategia(new EstrategiaDefender()).EjecutarTurno(asesino, atacante, 1);
        var turno = ConEstrategia(new EstrategiaAtacar()).EjecutarTurno(atacante, asesino, 1);

        Assert.Equal(0, turno.Danio);
        Assert.Equal(90, asesino.Vida);
    }

    // ── Reglas generales (polimórficas) ──────────────────────────────────────

    [Fact]
    public void VidaNuncaEsNegativa_CuandoElDanioSuperaLaVida()
    {
        var atacante = new Guerrero("G", 100, 200, 5, 0);
        var defensor = new Guerrero("D",  50,  10, 1, 0);

        ConEstrategia(new EstrategiaAtacar()).EjecutarTurno(atacante, defensor, 1);

        Assert.Equal(0, defensor.Vida);
        Assert.False(defensor.EstaVivo);
    }

    [Fact]
    public void PersonajeDerrotado_NoPuedeActuar_LanzaExcepcion()
    {
        var muerto   = new Guerrero("M", 100, 20, 5, 0);
        var objetivo = new Guerrero("E", 100, 20, 5, 0);
        muerto.RecibirDanio(999);

        Assert.Throws<InvalidOperationException>(
            () => ConEstrategia(new EstrategiaAtacar()).EjecutarTurno(muerto, objetivo, 1));
    }

    [Fact]
    public void PolimorfismoTurno_MismaCiamada_DaniosDiferentesPorClase()
    {
        var objetivo1 = new Guerrero("E1", 500, 5, 1, 0);
        var objetivo2 = new Guerrero("E2", 500, 5, 1, 0);
        var objetivo3 = new Guerrero("E3", 500, 5, 1, 0);
        var objetivo4 = new Guerrero("E4", 500, 5, 1, 0);

        var servicio = ConEstrategia(new EstrategiaAtacar());

        int danioGuerrero = servicio.EjecutarTurno(new Guerrero("G",  100, 20, 5,  0),  objetivo1, 1).Danio;
        int danioMago     = servicio.EjecutarTurno(new Mago("M",      100, 20, 5,  0),  objetivo2, 1).Danio;
        int danioArquero  = servicio.EjecutarTurno(new Arquero("A",   100, 20, 5,  5),  objetivo3, 1).Danio;
        int danioAsesino  = servicio.EjecutarTurno(new Asesino("As",  100, 20, 5, 20),  objetivo4, 1).Danio;

        int[] danos = [danioGuerrero, danioMago, danioArquero, danioAsesino];
        Assert.True(danos.Distinct().Count() > 1,
            "Se espera que las cuatro clases produzcan al menos dos valores de daño distintos (polimorfismo).");
    }

    [Theory]
    [InlineData("Guerrero", "Golpe Devastador")]
    [InlineData("Mago",     "Bola de Fuego")]
    [InlineData("Arquero",  "Lluvia de Flechas")]
    [InlineData("Asesino",  "Puñalada Letal")]
    public void Habilidad_NombreAccion_CorrespondeALaClase(string clase, string nombreEsperado)
    {
        var actor    = FabricaPersonajes.Crear(clase, "X", 100, 20, 5, 50);
        var objetivo = new Guerrero("E", 500, 5, 1, 0);

        var turno = ConEstrategia(new EstrategiaHabilidad()).EjecutarTurno(actor, objetivo, 1);

        Assert.Equal(nombreEsperado, turno.NombreAccion);
        Assert.True(turno.Danio > 0);
    }

    [Theory]
    [InlineData("Guerrero")]
    [InlineData("Mago")]
    [InlineData("Arquero")]
    [InlineData("Asesino")]
    public void Defender_NuncaHaceDanio_EnNingunaClase(string clase)
    {
        var actor    = FabricaPersonajes.Crear(clase, "X", 100, 20, 5, 50);
        var objetivo = new Guerrero("E", 100, 5, 1, 0);

        var turno = ConEstrategia(new EstrategiaDefender()).EjecutarTurno(actor, objetivo, 1);

        Assert.Equal(0, turno.Danio);
        Assert.Equal(100, objetivo.Vida);
        Assert.Equal("Defensa", turno.NombreAccion);
    }

    [Fact]
    public void DefensaMayorAlAtaque_SiempreHaceAlMenosUnoDeDanio()
    {
        var atacante = new Guerrero("G", 100, 5, 5, 0);
        var tanque   = new Guerrero("T", 100, 5, 999, 0);

        var turno = ConEstrategia(new EstrategiaAtacar()).EjecutarTurno(atacante, tanque, 1);

        Assert.Equal(1, turno.Danio);
        Assert.Equal(99, tanque.Vida);
    }

    [Fact]
    public void Turno_RegistraNumeroNombresYEstadoDeAmbos()
    {
        var actor    = new Guerrero("Actor", 100, 20, 5, 0);
        var objetivo = new Guerrero("Objetivo", 100, 5, 2, 0);

        var turno = ConEstrategia(new EstrategiaAtacar()).EjecutarTurno(actor, objetivo, 7);

        Assert.Equal(7, turno.Numero);
        Assert.Equal("Actor", turno.NombreActor);
        Assert.Equal("Objetivo", turno.NombreObjetivo);
        Assert.True(turno.ActorVivo);
        Assert.True(turno.ObjetivoVivo);
    }

    [Fact]
    public void Turno_QueMataAlObjetivo_RegistraObjetivoMuerto()
    {
        var actor    = new Guerrero("Actor", 100, 50, 5, 0);
        var objetivo = new Guerrero("Objetivo", 10, 5, 1, 0);

        var turno = ConEstrategia(new EstrategiaAtacar()).EjecutarTurno(actor, objetivo, 1);

        Assert.True(turno.ActorVivo);
        Assert.False(turno.ObjetivoVivo);
        Assert.Equal(10, turno.Danio);  // solo se aplica la vida que quedaba
    }

    [Fact]
    public void PersonajeDerrotado_NoPuedeDefender_LanzaExcepcion()
    {
        var muerto   = new Mago("M", 100, 20, 5, 0);
        var objetivo = new Guerrero("E", 100, 20, 5, 0);
        muerto.RecibirDanio(999);

        Assert.Throws<InvalidOperationException>(
            () => ConEstrategia(new EstrategiaDefender()).EjecutarTurno(muerto, objetivo, 1));
    }

    [Fact]
    public void PersonajeDerrotado_NoPuedeUsarHabilidad_LanzaExcepcion()
    {
        var muerto   = new Arquero("A", 100, 20, 5, 10);
        var objetivo = new Guerrero("E", 100, 20, 5, 0);
        muerto.RecibirDanio(999);

        Assert.Throws<InvalidOperationException>(
            () => ConEstrategia(new EstrategiaHabilidad()).EjecutarTurno(muerto, objetivo, 1));
    }

    [Fact]
    public void ServicioCombate_SinEstrategia_LanzaExcepcion()
    {
        Assert.Throws<ArgumentNullException>(() => new ServicioCombate(null!));
    }
}
