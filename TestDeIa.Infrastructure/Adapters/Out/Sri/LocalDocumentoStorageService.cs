using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using TestDeIa.Infrastructure.Options;

namespace TestDeIa.Infrastructure.Adapters.Out.Sri;

public sealed class LocalDocumentoStorageService(
    IOptions<DocumentoStorageOptions> options,
    IConfiguration configuration) : TenantStorageService(options, configuration);
