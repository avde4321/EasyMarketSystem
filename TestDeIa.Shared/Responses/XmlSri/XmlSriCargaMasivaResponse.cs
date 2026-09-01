namespace TestDeIa.Shared.Responses.XmlSri;

public sealed class XmlSriCargaMasivaResponse
{
    public int Recibidos { get; set; }

    public int Insertados { get; set; }

    public int Actualizados { get; set; }

    public int Duplicados { get; set; }

    public int Errores { get; set; }

    public IReadOnlyCollection<string> Mensajes { get; set; } = Array.Empty<string>();
}
