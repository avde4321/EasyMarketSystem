[CmdletBinding()]
param(
    [string]$UserName = 'AdminSaasSudo',
    [string]$Email,
    [string]$NombreCompleto = 'Administrador de la plataforma',
    [ValidateSet('Development','Staging','Production')][string]$Environment = 'Development',
    [Nullable[Guid]]$RetirarCajeroId,
    [Guid]$EmpresaId = '22222222-2222-2222-2222-222222222222'
)

$ErrorActionPreference = 'Stop'
$project = Join-Path (Split-Path $PSScriptRoot -Parent) 'TestDeIa.Api/TestDeIa.Api.csproj'
if (-not $Email) { $Email = Read-Host 'Correo privado del propietario de la plataforma' }
$password = Read-Host 'Contraseña inicial (16–128 caracteres, mayúscula, minúscula, número y símbolo)' -AsSecureString
$confirmation = Read-Host 'Confirma la contraseña inicial' -AsSecureString
$pointer = [IntPtr]::Zero
$confirmationPointer = [IntPtr]::Zero
$names = @('Saas__SuperAdminBootstrap__UserName','Saas__SuperAdminBootstrap__Email','Saas__SuperAdminBootstrap__NombreCompleto','Saas__SuperAdminBootstrap__Password','Saas__SuperAdminBootstrap__EmpresaId','Saas__SuperAdminBootstrap__RetirarCajeroId','ASPNETCORE_ENVIRONMENT','DOTNET_ENVIRONMENT')
$previous = @{}
foreach ($name in $names) { $previous[$name] = [Environment]::GetEnvironmentVariable($name, 'Process') }
try {
    $pointer = [Runtime.InteropServices.Marshal]::SecureStringToBSTR($password)
    $confirmationPointer = [Runtime.InteropServices.Marshal]::SecureStringToBSTR($confirmation)
    $plain = [Runtime.InteropServices.Marshal]::PtrToStringBSTR($pointer)
    $confirmed = [Runtime.InteropServices.Marshal]::PtrToStringBSTR($confirmationPointer)
    if ($plain -cne $confirmed) { throw 'Las contraseñas no coinciden.' }
    $env:Saas__SuperAdminBootstrap__UserName = $UserName
    $env:Saas__SuperAdminBootstrap__Email = $Email
    $env:Saas__SuperAdminBootstrap__NombreCompleto = $NombreCompleto
    $env:Saas__SuperAdminBootstrap__Password = $plain
    $env:Saas__SuperAdminBootstrap__EmpresaId = $EmpresaId.ToString()
    if ($RetirarCajeroId) { $env:Saas__SuperAdminBootstrap__RetirarCajeroId = $RetirarCajeroId.ToString() }
    $env:ASPNETCORE_ENVIRONMENT = $Environment
    $env:DOTNET_ENVIRONMENT = $Environment
    # Usa la conexión ya configurada por User Secrets o ConnectionStrings__DefaultConnection.
    # La contraseña NO viaja en argumentos, archivos, SQL ni salida de consola.
    & dotnet run --project $project --no-launch-profile -- --bootstrap-superadmin
    if ($LASTEXITCODE -ne 0) { throw 'No se completó la inicialización. Revisa el error sin compartir secretos.' }
    Write-Host 'Inicialización completada o ya realizada previamente. Inicia sesión con tu usuario privado.'
}
finally {
    foreach ($name in $names) { [Environment]::SetEnvironmentVariable($name, $previous[$name], 'Process') }
    if ($pointer -ne [IntPtr]::Zero) { [Runtime.InteropServices.Marshal]::ZeroFreeBSTR($pointer) }
    if ($confirmationPointer -ne [IntPtr]::Zero) { [Runtime.InteropServices.Marshal]::ZeroFreeBSTR($confirmationPointer) }
    $plain = $null; $confirmed = $null
    $password.Dispose(); $confirmation.Dispose()
}
