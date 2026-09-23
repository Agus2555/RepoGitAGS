using System;
using Persistencia.Entidades;
using Xunit;

namespace Tests;

public class GuerreroTests
{
    [Fact]
    public void Guerrero_AlAtacarSinFuria_SumaDiezDeFuria()
    {
        var atacadante = new Guerrero("G1", 100, 20, 5, furiaInicial: 0);
        var defensor = new Guerrero("G2", 100, 15, 5, furiaInicial: 0);

        atacadante.Atacar(defensor);

        Assert.Equal(10, atacadante.Furia);
    }

    [Fact]
    public void Personaje_VidaNuncaEsNegativa_CuandoDanioEsMayorAVida()
    {
        var atacante = new Guerrero("G1", 100, 200, 5, furiaInicial: 0);
        var defensor = new Guerrero("G2", 50, 10, 5, furiaInicial: 0);

        atacante.Atacar(defensor);

        Assert.Equal(0, defensor.Vida);
        Assert.False(defensor.EstaVivo);
    }

    [Fact]
    public void PersonajeDerrotado_AlIntentarAtacar_LanzaExcepcion()
    {
        var muerto = new Guerrero("G1", 100, 20, 5, furiaInicial: 0);
        var enemigo = new Guerrero("G2", 100, 20, 5, furiaInicial: 0);

        muerto.RecibirDanio(500);

        Assert.Throws<InvalidOperationException>(() => muerto.Atacar(enemigo));
    }
}