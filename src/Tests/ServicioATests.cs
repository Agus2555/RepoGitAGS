using System;
using Persistencia.Entidades;
using Xunit;

namespace Tests;

public class GuerreroTests
{
    [Fact]
    public void Guerrero_AlAtacarSinFuria_SumaDiezDeFuria()
    {
        // Arrange (Preparación)
        var atacadante = new Guerrero("G1", 100, 20, 5, furiaInicial: 0);
        var defensor = new Guerrero("G2", 100, 15, 5, furiaInicial: 0);

        // Act (Acción)
        atacadante.Atacar(defensor);

        // Assert (Verificación)
        Assert.Equal(10, atacadante.Furia);
    }

    [Fact]
    public void Personaje_VidaNuncaEsNegativa_CuandoDanioEsMayorAVida()
    {
        // Arrange
        var atacante = new Guerrero("G1", 100, 200, 5, furiaInicial: 0);
        var defensor = new Guerrero("G2", 50, 10, 5, furiaInicial: 0);

        // Act
        atacante.Atacar(defensor);

        // Assert
        Assert.Equal(0, defensor.Vida);
        Assert.False(defensor.EstaVivo);
    }

    [Fact]
    public void PersonajeDerrotado_AlIntentarAtacar_LanzaExcepcion()
    {
        // Arrange
        var muerto = new Guerrero("G1", 100, 20, 5, furiaInicial: 0);
        var enemigo = new Guerrero("G2", 100, 20, 5, furiaInicial: 0);

        // Forzamos la derrota del atacante mediante daño directo para probar la regla
        muerto.RecibirDanio(500);

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => muerto.Atacar(enemigo));
    }
}