using System.ComponentModel.DataAnnotations;
namespace TestDeIa.Shared.Requests.Saas;

public sealed class SaasConsumoQuery
{
    [StringLength(150)] public string? Term { get; set; }
    [Range(0, int.MaxValue)] public int Skip { get; set; }
    [Range(1, 50)] public int Take { get; set; } = 20;
}
