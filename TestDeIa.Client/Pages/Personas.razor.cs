using Microsoft.AspNetCore.Components;
using TestDeIa.Client.Services.Catalogos;
using TestDeIa.Client.Services.Personas;
using TestDeIa.Shared.Requests.Personas;
using TestDeIa.Shared.Responses.Catalogos;
using TestDeIa.Shared.Responses.Personas;
using TestDeIa.Shared.Validation;

namespace TestDeIa.Client.Pages;

public partial class Personas
{
    [Inject]
    private PersonasApiClient PersonasApiClient { get; set; } = default!;

    [Inject]
    private CatalogosApiClient CatalogosApiClient { get; set; } = default!;

    private readonly List<PersonaResponse> personas = [];
    private readonly List<CatalogoItemResponse> tiposIdentificacion = [];
    private PersonaRequest personaRequest = new();
    private PersonaNaturalForm personaNaturalForm = new();
    private EmpresaClienteForm empresaClienteForm = new();
    private TipoRegistroDocumento tipoRegistroDocumento = TipoRegistroDocumento.Cedula;
    private Guid? editingPersonaId;
    private bool isLoading = true;
    private bool isSaving;
    private bool isEditorOpen;
    private bool esPersonaJuridica;
    private RegistroCrudTipo? registroCrudTipo;
    private string? errorMessage;
    private string? validationMessage;
    private string searchTerm = string.Empty;
    private const int PageSize = 10;
    private int totalCount;
    private int currentSkip;
    private IEnumerable<PersonaResponse> VisiblePersonas => personas;
    private bool EsPersonaJuridica
    {
        get => esPersonaJuridica;
        set => esPersonaJuridica = value;
    }

    private bool CanGoPrevious => currentSkip > 0;
    private bool CanGoNext => currentSkip + PageSize < totalCount;
    private int PageNumber => (currentSkip / PageSize) + 1;
    private int TotalPages => Math.Max(1, (int)Math.Ceiling(totalCount / (double)PageSize));

    protected override async Task OnInitializedAsync()
    {
        await LoadCatalogosAsync();
        await LoadPersonasAsync(resetPaging: true);
    }

    private async Task LoadCatalogosAsync()
    {
        tiposIdentificacion.Clear();
        tiposIdentificacion.AddRange(await CatalogosApiClient.GetItemsAsync("TIPO_IDENTIFICACION", true));
    }

    private async Task LoadPersonasAsync(bool resetPaging = false)
    {
        if (resetPaging)
        {
            currentSkip = 0;
        }

        isLoading = true;
        errorMessage = null;

        try
        {
            var page = await PersonasApiClient.GetPagedAsync(searchTerm, currentSkip, PageSize);
            personas.Clear();
            personas.AddRange(page.Items);
            totalCount = page.TotalCount;
        }
        catch (HttpRequestException)
        {
            errorMessage = "No se pudo cargar la lista de personas.";
        }
        finally
        {
            isLoading = false;
        }
    }

    private void OpenCreateModal()
    {
        editingPersonaId = null;
        personaRequest = new PersonaRequest
        {
            TipoIdentificacion = tiposIdentificacion.FirstOrDefault()?.Codigo ?? "05"
        };
        personaNaturalForm = new PersonaNaturalForm();
        empresaClienteForm = new EmpresaClienteForm();
        tipoRegistroDocumento = TipoRegistroDocumento.Cedula;
        esPersonaJuridica = false;
        registroCrudTipo = null;
        errorMessage = null;
        validationMessage = null;
        isEditorOpen = true;
    }

    private void OpenEditModal(PersonaResponse persona)
    {
        editingPersonaId = persona.Id;
        errorMessage = null;
        personaRequest = new PersonaRequest
        {
            TipoIdentificacion = persona.TipoIdentificacion,
            Identificacion = persona.Identificacion,
            RazonSocialONombresCompletos = persona.RazonSocialONombresCompletos,
            NombreComercial = persona.NombreComercial,
            FechaNacimiento = persona.FechaNacimiento,
            CorreoElectronicoPrincipal = persona.CorreoElectronicoPrincipal,
            TelefonoCelular = persona.TelefonoCelular,
            DireccionPrincipal = persona.DireccionPrincipal,
            RegionCodigo = persona.RegionCodigo,
            ProvinciaCodigo = persona.ProvinciaCodigo,
            CiudadCodigo = persona.CiudadCodigo,
            SectorCodigo = persona.SectorCodigo,
            Genero = persona.Genero,
            IsActive = persona.IsActive
        };
        tipoRegistroDocumento = persona.TipoIdentificacion switch
        {
            "04" => TipoRegistroDocumento.Ruc,
            "06" => TipoRegistroDocumento.Pasaporte,
            _ => TipoRegistroDocumento.Cedula
        };
        esPersonaJuridica = persona.EsPersonaJuridica;
        registroCrudTipo = persona.EsEmpresa
            ? RegistroCrudTipo.Empresa
            : RegistroCrudTipo.Persona;
        personaNaturalForm = BuildPersonaNaturalForm(persona);
        empresaClienteForm = BuildEmpresaClienteForm(persona);
        validationMessage = null;
        isEditorOpen = true;
    }

    private void CloseModal()
    {
        isEditorOpen = false;
        isSaving = false;
        registroCrudTipo = null;
        errorMessage = null;
        validationMessage = null;
    }

    private void SeleccionarCrudRegistro(RegistroCrudTipo tipo)
    {
        registroCrudTipo = tipo;
        esPersonaJuridica = false;
        tipoRegistroDocumento = tipo == RegistroCrudTipo.Empresa ? TipoRegistroDocumento.Ruc : TipoRegistroDocumento.Cedula;
        validationMessage = null;
        errorMessage = null;
    }

    private void VolverASeleccionCrud()
    {
        registroCrudTipo = null;
        validationMessage = null;
        errorMessage = null;
    }

    private async Task SavePersonaAsync()
    {
        isSaving = true;
        errorMessage = null;
        validationMessage = null;

        try
        {
            if (!TryBuildPersonaRequest(out var request))
            {
                return;
            }

            var result = editingPersonaId.HasValue
                ? await PersonasApiClient.UpdateAsync(editingPersonaId.Value, request)
                : await PersonasApiClient.CreateAsync(request);

            if (!result.Succeeded)
            {
                errorMessage = result.ErrorMessage;
                return;
            }

            CloseModal();
            await LoadPersonasAsync();
        }
        catch (HttpRequestException)
        {
            errorMessage = "No se pudo guardar la persona.";
        }
        finally
        {
            isSaving = false;
        }
    }

    private void SetTipoRegistroDocumento(TipoRegistroDocumento tipo)
    {
        tipoRegistroDocumento = tipo;
        validationMessage = null;
        errorMessage = null;
    }

    private bool TryBuildPersonaRequest(out PersonaRequest request)
    {
        request = registroCrudTipo == RegistroCrudTipo.Empresa
            ? BuildFromEmpresaCliente()
            : BuildFromPersonaNatural();

        if (registroCrudTipo == RegistroCrudTipo.Persona && tipoRegistroDocumento is TipoRegistroDocumento.Cedula)
        {
            if (!ValidadorEcuador.EsCedulaValida(personaNaturalForm.NumeroDocumento))
            {
                validationMessage = "La cedula no es valida para Ecuador.";
                return false;
            }
        }
        else if (registroCrudTipo == RegistroCrudTipo.Persona && tipoRegistroDocumento is TipoRegistroDocumento.Ruc)
        {
            var isValid = esPersonaJuridica
                ? ValidadorEcuador.EsRucPersonaJuridicaValido(personaNaturalForm.NumeroDocumento)
                : ValidadorEcuador.EsRucNaturalValido(personaNaturalForm.NumeroDocumento);

            if (!isValid)
            {
                validationMessage = esPersonaJuridica
                    ? "El RUC de persona juridica no es valido para Ecuador."
                    : "El RUC de persona natural no es valido para Ecuador. Debe tener 13 digitos y terminar en 001.";
                return false;
            }
        }
        else if (registroCrudTipo == RegistroCrudTipo.Empresa && !ValidadorEcuador.EsRucPersonaJuridicaValido(empresaClienteForm.Ruc))
        {
            validationMessage = "El RUC de empresa no es valido para Ecuador. Debe ser juridico, tercer digito 9 y terminar en 001.";
            return false;
        }

        return true;
    }

    private PersonaRequest BuildFromPersonaNatural()
    {
        var fullName = string.Join(
            " ",
            new[]
            {
                personaNaturalForm.PrimerNombre,
                personaNaturalForm.SegundoNombre,
                personaNaturalForm.PrimerApellido,
                personaNaturalForm.SegundoApellido
            }.Where(value => !string.IsNullOrWhiteSpace(value))).Trim();

        return new PersonaRequest
        {
            TipoIdentificacion = tipoRegistroDocumento == TipoRegistroDocumento.Ruc ? "04" : tipoRegistroDocumento == TipoRegistroDocumento.Pasaporte ? "06" : "05",
            Identificacion = personaNaturalForm.NumeroDocumento.Trim(),
            RazonSocialONombresCompletos = fullName,
            NombreComercial = null,
            CorreoElectronicoPrincipal = personaNaturalForm.Email,
            TelefonoCelular = personaNaturalForm.Telefono,
            DireccionPrincipal = personaNaturalForm.Direccion,
            RegionCodigo = personaNaturalForm.RegionCodigo,
            ProvinciaCodigo = personaNaturalForm.ProvinciaCodigo,
            CiudadCodigo = personaNaturalForm.CiudadCodigo,
            SectorCodigo = personaNaturalForm.SectorCodigo,
            EsPersonaJuridica = esPersonaJuridica,
            EsEmpresa = false,
            IsActive = personaRequest.IsActive
        };
    }

    private PersonaRequest BuildFromEmpresaCliente()
    {
        return new PersonaRequest
        {
            TipoIdentificacion = "04",
            Identificacion = empresaClienteForm.Ruc.Trim(),
            RazonSocialONombresCompletos = empresaClienteForm.RazonSocial.Trim(),
            NombreComercial = empresaClienteForm.NombreComercial,
            CorreoElectronicoPrincipal = empresaClienteForm.EmailFacturacion,
            TelefonoCelular = empresaClienteForm.Telefono,
            DireccionPrincipal = empresaClienteForm.DireccionMatriz,
            RegionCodigo = empresaClienteForm.RegionCodigo,
            ProvinciaCodigo = empresaClienteForm.ProvinciaCodigo,
            CiudadCodigo = empresaClienteForm.CiudadCodigo,
            SectorCodigo = empresaClienteForm.SectorCodigo,
            EsPersonaJuridica = true,
            EsEmpresa = true,
            IsActive = personaRequest.IsActive
        };
    }

    private static PersonaNaturalForm BuildPersonaNaturalForm(PersonaResponse persona)
    {
        var parts = persona.RazonSocialONombresCompletos.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        return new PersonaNaturalForm
        {
            PrimerNombre = parts.ElementAtOrDefault(0) ?? string.Empty,
            SegundoNombre = parts.ElementAtOrDefault(1),
            PrimerApellido = parts.ElementAtOrDefault(2) ?? string.Empty,
            SegundoApellido = parts.ElementAtOrDefault(3),
            NumeroDocumento = persona.TipoIdentificacion == "04" ? persona.Identificacion[..Math.Min(10, persona.Identificacion.Length)] : persona.Identificacion,
            Email = persona.CorreoElectronicoPrincipal,
            Telefono = persona.TelefonoCelular,
            Direccion = persona.DireccionPrincipal,
            RegionCodigo = persona.RegionCodigo,
            ProvinciaCodigo = persona.ProvinciaCodigo,
            CiudadCodigo = persona.CiudadCodigo,
            SectorCodigo = persona.SectorCodigo
        };
    }

    private static EmpresaClienteForm BuildEmpresaClienteForm(PersonaResponse persona)
    {
        return new EmpresaClienteForm
        {
            RazonSocial = persona.RazonSocialONombresCompletos,
            NombreComercial = persona.NombreComercial,
            Ruc = persona.Identificacion,
            EmailFacturacion = persona.CorreoElectronicoPrincipal,
            Telefono = persona.TelefonoCelular,
            DireccionMatriz = persona.DireccionPrincipal,
            RegionCodigo = persona.RegionCodigo,
            ProvinciaCodigo = persona.ProvinciaCodigo,
            CiudadCodigo = persona.CiudadCodigo,
            SectorCodigo = persona.SectorCodigo
        };
    }

    private Task SearchAsync() => LoadPersonasAsync(resetPaging: true);

    private async Task GoToPreviousPageAsync()
    {
        if (!CanGoPrevious)
        {
            return;
        }

        currentSkip = Math.Max(0, currentSkip - PageSize);
        await LoadPersonasAsync();
    }

    private async Task GoToNextPageAsync()
    {
        if (!CanGoNext)
        {
            return;
        }

        currentSkip += PageSize;
        await LoadPersonasAsync();
    }

    private enum TipoRegistroDocumento
    {
        Cedula,
        Ruc,
        Pasaporte
    }

    private enum RegistroCrudTipo
    {
        Persona,
        Empresa
    }

    private sealed class PersonaNaturalForm
    {
        public string PrimerNombre { get; set; } = string.Empty;
        public string? SegundoNombre { get; set; }
        public string PrimerApellido { get; set; } = string.Empty;
        public string? SegundoApellido { get; set; }
        public string NumeroDocumento { get; set; } = string.Empty;
        public string? Email { get; set; }
        public string? Telefono { get; set; }
        public string Direccion { get; set; } = string.Empty;
        public string? RegionCodigo { get; set; }
        public string? ProvinciaCodigo { get; set; }
        public string? CiudadCodigo { get; set; }
        public string? SectorCodigo { get; set; }
    }

    private sealed class EmpresaClienteForm
    {
        public string RazonSocial { get; set; } = string.Empty;
        public string? NombreComercial { get; set; }
        public string Ruc { get; set; } = string.Empty;
        public string? RepresentanteLegal { get; set; }
        public bool ObligadoLlevarContabilidad { get; set; }
        public string? ContribuyenteEspecial { get; set; }
        public string? EmailFacturacion { get; set; }
        public string? Telefono { get; set; }
        public string DireccionMatriz { get; set; } = string.Empty;
        public string? RegionCodigo { get; set; }
        public string? ProvinciaCodigo { get; set; }
        public string? CiudadCodigo { get; set; }
        public string? SectorCodigo { get; set; }
    }
}
