using Microsoft.EntityFrameworkCore;
using TestDeIa.Infrastructure.Persistence.Entities;

namespace TestDeIa.Infrastructure.Persistence;

public sealed class GeoEcuadorSeed(TestDeIaDbContext dbContext)
{
    public async Task EnsureSeededAsync(CancellationToken cancellationToken = default)
    {
        if (await dbContext.GeoRegiones.IgnoreQueryFilters().AnyAsync(cancellationToken))
        {
            return;
        }

        var regiones = new[]
        {
            Region("SIERRA_NORTE", "Sierra Norte", 1),
            Region("SIERRA_CENTRO", "Sierra Centro", 2),
            Region("SIERRA_SUR", "Sierra Sur", 3),
            Region("COSTA", "Costa", 4),
            Region("AMAZONIA", "Amazonia", 5),
            Region("INSULAR", "Insular", 6)
        };

        dbContext.GeoRegiones.AddRange(regiones);
        await dbContext.SaveChangesAsync(cancellationToken);

        var regionByCodigo = regiones.ToDictionary(current => current.Codigo, StringComparer.OrdinalIgnoreCase);
        var provincias = ProvinciaSeed
            .Select((item, index) => new GeoProvinciaEntity
            {
                Id = StableGuid("provincia", item.Codigo),
                RegionId = regionByCodigo[item.RegionCodigo].Id,
                Codigo = item.Codigo,
                Nombre = item.Nombre,
                Orden = index + 1,
                IsActive = true
            })
            .ToArray();

        dbContext.GeoProvincias.AddRange(provincias);
        await dbContext.SaveChangesAsync(cancellationToken);

        var provinciaByCodigo = provincias.ToDictionary(current => current.Codigo, StringComparer.OrdinalIgnoreCase);
        var ciudades = CantonSeed
            .Select((item, index) => new GeoCiudadEntity
            {
                Id = StableGuid("ciudad", item.Codigo),
                ProvinciaId = provinciaByCodigo[item.ProvinciaCodigo].Id,
                Codigo = item.Codigo,
                Nombre = item.Nombre,
                Orden = index + 1,
                IsActive = true
            })
            .ToArray();

        dbContext.GeoCiudades.AddRange(ciudades);
        await dbContext.SaveChangesAsync(cancellationToken);

        var sectores = ciudades.Select(ciudad => new GeoSectorEntity
        {
            Id = StableGuid("sector", $"{ciudad.Codigo}_GENERAL"),
            CiudadId = ciudad.Id,
            Codigo = $"{ciudad.Codigo}_GENERAL",
            Nombre = "General / sin sector especifico",
            Orden = 1,
            IsActive = true
        }).ToArray();

        dbContext.GeoSectores.AddRange(sectores);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    private static GeoRegionEntity Region(string codigo, string nombre, int orden) => new()
    {
        Id = StableGuid("region", codigo),
        Codigo = codigo,
        Nombre = nombre,
        Orden = orden,
        IsActive = true
    };

    private static Guid StableGuid(string scope, string code)
    {
        var bytes = System.Security.Cryptography.MD5.HashData(System.Text.Encoding.UTF8.GetBytes($"geo-ecuador-{scope}-{code}"));
        return new Guid(bytes);
    }

    private static readonly (string Codigo, string Nombre, string RegionCodigo)[] ProvinciaSeed =
    [
        ("AZUAY", "Azuay", "SIERRA_SUR"),
        ("BOLIVAR", "Bolivar", "SIERRA_CENTRO"),
        ("CANAR", "Canar", "SIERRA_SUR"),
        ("CARCHI", "Carchi", "SIERRA_NORTE"),
        ("CHIMBORAZO", "Chimborazo", "SIERRA_CENTRO"),
        ("COTOPAXI", "Cotopaxi", "SIERRA_CENTRO"),
        ("EL_ORO", "El Oro", "COSTA"),
        ("ESMERALDAS", "Esmeraldas", "COSTA"),
        ("GALAPAGOS", "Galapagos", "INSULAR"),
        ("GUAYAS", "Guayas", "COSTA"),
        ("IMBABURA", "Imbabura", "SIERRA_NORTE"),
        ("LOJA", "Loja", "SIERRA_SUR"),
        ("LOS_RIOS", "Los Rios", "COSTA"),
        ("MANABI", "Manabi", "COSTA"),
        ("MORONA_SANTIAGO", "Morona Santiago", "AMAZONIA"),
        ("NAPO", "Napo", "AMAZONIA"),
        ("ORELLANA", "Orellana", "AMAZONIA"),
        ("PASTAZA", "Pastaza", "AMAZONIA"),
        ("PICHINCHA", "Pichincha", "SIERRA_NORTE"),
        ("SANTA_ELENA", "Santa Elena", "COSTA"),
        ("SANTO_DOMINGO", "Santo Domingo de los Tsachilas", "COSTA"),
        ("SUCUMBIOS", "Sucumbios", "AMAZONIA"),
        ("TUNGURAHUA", "Tungurahua", "SIERRA_CENTRO"),
        ("ZAMORA_CHINCHIPE", "Zamora Chinchipe", "AMAZONIA")
    ];

    private static readonly (string ProvinciaCodigo, string Codigo, string Nombre)[] CantonSeed =
    [
        ("AZUAY", "CUENCA", "Cuenca"), ("AZUAY", "GIRON", "Giron"), ("AZUAY", "GUALACEO", "Gualaceo"), ("AZUAY", "PAUTE", "Paute"),
        ("BOLIVAR", "GUARANDA", "Guaranda"), ("BOLIVAR", "CHILLANES", "Chillanes"), ("BOLIVAR", "SAN_MIGUEL", "San Miguel"),
        ("CANAR", "AZOGUES", "Azogues"), ("CANAR", "BIBLIAN", "Biblian"), ("CANAR", "CANAR", "Canar"), ("CANAR", "LA_TRONCAL", "La Troncal"),
        ("CARCHI", "TULCAN", "Tulcan"), ("CARCHI", "BOLIVAR_CARCHI", "Bolivar"), ("CARCHI", "ESPEJO", "Espejo"), ("CARCHI", "MONTUFAR", "Montufar"),
        ("CHIMBORAZO", "RIOBAMBA", "Riobamba"), ("CHIMBORAZO", "ALAUSI", "Alausi"), ("CHIMBORAZO", "GUANO", "Guano"),
        ("COTOPAXI", "LATACUNGA", "Latacunga"), ("COTOPAXI", "LA_MANA", "La Mana"), ("COTOPAXI", "PUJILI", "Pujili"), ("COTOPAXI", "SALCEDO", "Salcedo"),
        ("EL_ORO", "MACHALA", "Machala"), ("EL_ORO", "PASAJE", "Pasaje"), ("EL_ORO", "SANTA_ROSA", "Santa Rosa"), ("EL_ORO", "HUAQUILLAS", "Huaquillas"),
        ("ESMERALDAS", "ESMERALDAS", "Esmeraldas"), ("ESMERALDAS", "ATACAMES", "Atacames"), ("ESMERALDAS", "QUININDE", "Quininde"),
        ("GALAPAGOS", "SAN_CRISTOBAL", "San Cristobal"), ("GALAPAGOS", "SANTA_CRUZ", "Santa Cruz"), ("GALAPAGOS", "ISABELA", "Isabela"),
        ("GUAYAS", "GUAYAQUIL", "Guayaquil"), ("GUAYAS", "DURAN", "Duran"), ("GUAYAS", "SAMBORONDON", "Samborondon"), ("GUAYAS", "MILAGRO", "Milagro"), ("GUAYAS", "DAULE", "Daule"),
        ("IMBABURA", "IBARRA", "Ibarra"), ("IMBABURA", "OTAVALO", "Otavalo"), ("IMBABURA", "ANTONIO_ANTE", "Antonio Ante"), ("IMBABURA", "COTACACHI", "Cotacachi"),
        ("LOJA", "LOJA", "Loja"), ("LOJA", "CATAMAYO", "Catamayo"), ("LOJA", "MACARA", "Macara"), ("LOJA", "SARAGURO", "Saraguro"),
        ("LOS_RIOS", "BABAHOYO", "Babahoyo"), ("LOS_RIOS", "QUEVEDO", "Quevedo"), ("LOS_RIOS", "VINCES", "Vinces"), ("LOS_RIOS", "VENTANAS", "Ventanas"),
        ("MANABI", "PORTOVIEJO", "Portoviejo"), ("MANABI", "MANTA", "Manta"), ("MANABI", "CHONE", "Chone"), ("MANABI", "JIPIJAPA", "Jipijapa"),
        ("MORONA_SANTIAGO", "MORONA", "Morona"), ("MORONA_SANTIAGO", "GUALAQUIZA", "Gualaquiza"), ("MORONA_SANTIAGO", "SUCÚA", "Sucua"),
        ("NAPO", "TENA", "Tena"), ("NAPO", "ARCHIDONA", "Archidona"), ("NAPO", "EL_CHACO", "El Chaco"),
        ("ORELLANA", "FRANCISCO_DE_ORELLANA", "Francisco de Orellana"), ("ORELLANA", "AGUARICO", "Aguarico"), ("ORELLANA", "LA_JOYA_DE_LOS_SACHAS", "La Joya de los Sachas"),
        ("PASTAZA", "PASTAZA", "Pastaza"), ("PASTAZA", "MERA", "Mera"), ("PASTAZA", "SANTA_CLARA", "Santa Clara"),
        ("PICHINCHA", "QUITO", "Quito"), ("PICHINCHA", "CAYAMBE", "Cayambe"), ("PICHINCHA", "RUMINAHUI", "Ruminahui"), ("PICHINCHA", "MEJIA", "Mejia"),
        ("SANTA_ELENA", "SANTA_ELENA", "Santa Elena"), ("SANTA_ELENA", "LA_LIBERTAD", "La Libertad"), ("SANTA_ELENA", "SALINAS", "Salinas"),
        ("SANTO_DOMINGO", "SANTO_DOMINGO", "Santo Domingo"), ("SANTO_DOMINGO", "LA_CONCORDIA", "La Concordia"),
        ("SUCUMBIOS", "LAGO_AGRIO", "Lago Agrio"), ("SUCUMBIOS", "SHUSHUFINDI", "Shushufindi"), ("SUCUMBIOS", "CUYABENO", "Cuyabeno"),
        ("TUNGURAHUA", "AMBATO", "Ambato"), ("TUNGURAHUA", "BANOS", "Banos"), ("TUNGURAHUA", "PELILEO", "Pelileo"),
        ("ZAMORA_CHINCHIPE", "ZAMORA", "Zamora"), ("ZAMORA_CHINCHIPE", "YANTZAZA", "Yantzaza"), ("ZAMORA_CHINCHIPE", "NANGARITZA", "Nangaritza")
    ];
}
