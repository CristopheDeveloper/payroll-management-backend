using Planilla.Dominio.Tiempo;

namespace Planilla.Dominio.Tests;

public class RelojGuatemalaTests
{
    [Fact]
    public void Ahora_UsaUtcMenosSeis()
    {
        var utcAntes = DateTime.UtcNow;

        var ahora = RelojGuatemala.Ahora;

        var utcDespues = DateTime.UtcNow;
        Assert.InRange(ahora, utcAntes.AddHours(-6), utcDespues.AddHours(-6));
        Assert.Equal(DateTimeKind.Unspecified, ahora.Kind);
    }

    [Fact]
    public void Hoy_CorrespondeALaFechaDeGuatemala()
    {
        var fechaAntes = DateOnly.FromDateTime(RelojGuatemala.Ahora);

        var hoy = RelojGuatemala.Hoy;

        var fechaDespues = DateOnly.FromDateTime(RelojGuatemala.Ahora);
        Assert.Contains(hoy, new[] { fechaAntes, fechaDespues });
    }
}
