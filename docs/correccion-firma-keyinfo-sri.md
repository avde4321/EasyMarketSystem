# Corrección de firma XAdES-BES: certificado firmante

Se comparó el XML autorizado proporcionado por el usuario con SriXadesBesSigner. El ejemplo contiene referencias firmadas al documento, SignedProperties y KeyInfo. El generador anterior solo incluía las primeras dos referencias, aunque sí incorporaba el certificado en X509Data.

La sección 6.5 de la ficha técnica del SRI exige incluir el certificado firmante en KeyInfo y firmar esa información para impedir sustituciones. Se agregó un identificador único de KeyInfo, una referencia SHA-1 con canonicalización y su resolución durante ComputeSignature. No se cambió el algoritmo RSA-SHA1 utilizado por este flujo de comprobantes ni se copiaron certificados del ejemplo.

La comprobación local ahora recupera el X509Certificate desde KeyInfo, compara sus bytes con el certificado utilizado y verifica la firma con ese certificado recuperado. La prueba incluye recuperación del certificado, las tres referencias, verificación criptográfica y rechazo de una referencia a KeyInfo manipulada.

Es una corrección de conformidad comprobada localmente, no una certificación de autorización del SRI ni una validación de revocación/confianza remota. El mensaje 39 puede tener causas adicionales. No se realizaron envíos al SRI ni modificaciones a comprobantes persistidos, claves privadas, certificados o XML autorizados.

Reiniciar la API es necesario para cargar el nuevo firmador. Los documentos previamente firmados conservan sus bytes originales: reenviar esos bytes no incorpora esta corrección. Antes de regenerar un documento rechazado debe verificarse su estado real y utilizar un flujo controlado que conserve su identidad y auditoría; no debe alterarse una factura autorizada ni generarse una venta duplicada como solución.

## Hallazgo posterior: conversión incorrecta de la serie del certificado

Se inspeccionó el XML rechazado persistido. El certificado embebido tiene serie hexadecimal `00C2035650` (decimal `3254998608`). SignedProperties declaraba correctamente `3254998608`, pero KeyInfo/X509IssuerSerial declaraba `216167712264`. La API .NET KeyInfoX509Data.AddIssuerSerial interpreta su argumento como hexadecimal y lo convierte a decimal al serializar. Se le estaba pasando una cadena ya convertida a decimal.

Se cambió ese argumento por certificate.GetSerialNumberString(). Se mantiene la conversión decimal explícita únicamente al construir SignedProperties. La validación local ahora también exige coincidencia de ambas series, ambos emisores y el digest del certificado; verificar únicamente la operación RSA no detectaba el metadato incorrecto.

Tres pruebas deterministas reprodujeron el defecto antes del cambio, incluida la serie exacta del rechazo, y pasan después. Se añadió un diagnóstico opt-in con EASYMARKET_SRI_DIAGNOSTIC_FACTURA_ID y EASYMARKET_TEST_SQL: lee el XML base y el certificado vigente de la misma empresa, firma exclusivamente en memoria y comprueba certificado, series y firma. No escribe en BD ni invoca SOAP. Se ejecutó con la factura afectada y pasó; la huella del certificado coincide con el registro activo de CertificadosDigitalesEmpresa.

El archivo p12 y su contraseña NO se incrustan en el XML: se publica únicamente el certificado X.509 (DER/base64), nunca la clave privada. La clave privada permanece en memoria para calcular SignatureValue.

La corrección elimina una inconsistencia demostrada de identidad del firmante, pero no certifica por sí sola la aceptación remota ni descarta causas adicionales del código 39. El endpoint manual actual no permite reprocesar documentos RECHAZADO: se conservó esa protección y no se actualizó el estado de la factura para forzar un reenvío.
