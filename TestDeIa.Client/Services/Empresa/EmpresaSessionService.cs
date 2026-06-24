using Microsoft.JSInterop;

namespace TestDeIa.Client.Services.Empresa;

public sealed class EmpresaSessionService
{
    private const string EmpresaIdKey = "testdeia.empresa.activa";
    private readonly IJSRuntime jsRuntime;

    public EmpresaSessionService(IJSRuntime jsRuntime)
    {
        this.jsRuntime = jsRuntime;
    }

    public ValueTask<string?> GetEmpresaIdAsync()
    {
        return jsRuntime.InvokeAsync<string?>("localStorage.getItem", EmpresaIdKey);
    }

    public ValueTask SetEmpresaIdAsync(Guid empresaId)
    {
        return jsRuntime.InvokeVoidAsync("localStorage.setItem", EmpresaIdKey, empresaId.ToString());
    }

    public ValueTask ClearEmpresaIdAsync()
    {
        return jsRuntime.InvokeVoidAsync("localStorage.removeItem", EmpresaIdKey);
    }
}
