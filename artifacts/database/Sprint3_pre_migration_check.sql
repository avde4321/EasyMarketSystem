SET NOCOUNT ON;

PRINT 'Sprint 3 pre-migration schema check iniciado.';

IF OBJECT_ID(N'[dbo].[Facturas]', N'U') IS NOT NULL
BEGIN
    IF EXISTS (
        SELECT 1
        FROM [dbo].[Facturas]
        GROUP BY [EmpresaId], [ClaveAcceso]
        HAVING COUNT_BIG(*) > 1
    )
        THROW 51001, 'Pre-check fallido: existen facturas duplicadas por EmpresaId + ClaveAcceso.', 1;

    IF EXISTS (
        SELECT 1
        FROM [dbo].[Facturas]
        GROUP BY [EmpresaId], [Establecimiento], [PuntoEmision], [Secuencial]
        HAVING COUNT_BIG(*) > 1
    )
        THROW 51002, 'Pre-check fallido: existen facturas duplicadas por EmpresaId + Establecimiento + PuntoEmision + Secuencial.', 1;
END;

IF OBJECT_ID(N'[dbo].[ComprobanteCabecera]', N'U') IS NOT NULL
BEGIN
    IF EXISTS (
        SELECT 1
        FROM [dbo].[ComprobanteCabecera]
        GROUP BY [EmpresaId], [ClaveAcceso]
        HAVING COUNT_BIG(*) > 1
    )
        THROW 51003, 'Pre-check fallido: existen comprobantes SRI duplicados por EmpresaId + ClaveAcceso.', 1;
END;

IF OBJECT_ID(N'[dbo].[ComprobantesRetencion]', N'U') IS NOT NULL
BEGIN
    IF EXISTS (
        SELECT 1
        FROM [dbo].[ComprobantesRetencion]
        WHERE [ClaveAcceso] IS NOT NULL
        GROUP BY [EmpresaId], [ClaveAcceso]
        HAVING COUNT_BIG(*) > 1
    )
        THROW 51004, 'Pre-check fallido: existen retenciones duplicadas por EmpresaId + ClaveAcceso.', 1;
END;

IF OBJECT_ID(N'[dbo].[ExtractoBancarioDetalles]', N'U') IS NOT NULL
   AND OBJECT_ID(N'[dbo].[ExtractoBancarioHeaders]', N'U') IS NOT NULL
   AND OBJECT_ID(N'[dbo].[CuentasBancarias]', N'U') IS NOT NULL
BEGIN
    IF EXISTS (
        SELECT 1
        FROM [dbo].[ExtractoBancarioDetalles] AS detalle
        LEFT JOIN [dbo].[ExtractoBancarioHeaders] AS header
            ON detalle.[ExtractoHeaderId] = header.[Id]
        LEFT JOIN [dbo].[CuentasBancarias] AS cuenta
            ON header.[CuentaBancariaId] = cuenta.[Id]
        WHERE header.[Id] IS NULL OR cuenta.[Id] IS NULL
    )
        THROW 51005, 'Pre-check fallido: existen detalles de extracto bancario sin header o cuenta bancaria válida.', 1;
END;

PRINT 'Sprint 3 pre-migration schema check finalizado correctamente.';
