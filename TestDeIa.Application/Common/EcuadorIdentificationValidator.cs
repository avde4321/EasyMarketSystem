namespace TestDeIa.Application.Common;

public static class EcuadorIdentificationValidator
{
    public static void EnsureValid(string tipoIdentificacion, string identificacion, string subjectLabel)
    {
        var tipo = tipoIdentificacion.Trim();
        var numero = identificacion.Trim();

        if (string.IsNullOrWhiteSpace(numero))
        {
            throw new InvalidOperationException($"La identificacion de {subjectLabel} es obligatoria.");
        }

        if (tipo == "07")
        {
            if (numero != "9999999999999")
            {
                throw new InvalidOperationException($"Para consumidor final en {subjectLabel} se debe usar la identificacion 9999999999999.");
            }

            return;
        }

        if (tipo == "06")
        {
            if (numero.Length < 3)
            {
                throw new InvalidOperationException($"El pasaporte de {subjectLabel} debe tener al menos 3 caracteres.");
            }

            return;
        }

        if (!numero.All(char.IsDigit))
        {
            throw new InvalidOperationException($"La identificacion de {subjectLabel} debe contener solo digitos para el tipo seleccionado.");
        }

        if (tipo == "05")
        {
            if (numero.Length != 10 || !IsValidNaturalPersonCedula(numero))
            {
                throw new InvalidOperationException($"La cedula de {subjectLabel} no supera la validacion ecuatoriana modulo 10.");
            }

            return;
        }

        if (tipo == "04")
        {
            if (numero.Length != 13)
            {
                throw new InvalidOperationException($"El RUC de {subjectLabel} debe tener 13 digitos.");
            }

            var thirdDigit = numero[2] - '0';
            if (thirdDigit is >= 0 and <= 5)
            {
                if (!IsValidNaturalPersonRuc(numero))
                {
                    throw new InvalidOperationException($"El RUC de persona natural de {subjectLabel} no supera la validacion modulo 10.");
                }

                return;
            }

            if (thirdDigit == 6)
            {
                if (!IsValidPublicSocietyRuc(numero))
                {
                    throw new InvalidOperationException($"El RUC de sociedad publica de {subjectLabel} no supera la validacion modulo 11.");
                }

                return;
            }

            if (thirdDigit == 9)
            {
                if (!IsValidPrivateSocietyRuc(numero))
                {
                    throw new InvalidOperationException($"El RUC de sociedad privada de {subjectLabel} no supera la validacion modulo 11.");
                }

                return;
            }

            throw new InvalidOperationException($"El RUC de {subjectLabel} no tiene un tercer digito valido para Ecuador.");
        }

    }

    private static bool IsValidNaturalPersonCedula(string cedula)
    {
        if (!HasValidProvinceCode(cedula) || (cedula[2] - '0') > 5)
        {
            return false;
        }

        var factors = new[] { 2, 1, 2, 1, 2, 1, 2, 1, 2 };
        var sum = 0;

        for (var index = 0; index < factors.Length; index++)
        {
            var value = (cedula[index] - '0') * factors[index];
            sum += value > 9 ? value - 9 : value;
        }

        var verifier = GetModulo10Verifier(sum);
        return verifier == (cedula[9] - '0');
    }

    private static bool IsValidNaturalPersonRuc(string ruc)
    {
        return IsValidNaturalPersonCedula(ruc[..10]) && ruc.EndsWith("001", StringComparison.Ordinal);
    }

    private static bool IsValidPublicSocietyRuc(string ruc)
    {
        if (!HasValidProvinceCode(ruc) || !ruc.EndsWith("0001", StringComparison.Ordinal))
        {
            return false;
        }

        var factors = new[] { 3, 2, 7, 6, 5, 4, 3, 2 };
        var sum = 0;
        for (var index = 0; index < factors.Length; index++)
        {
            sum += (ruc[index] - '0') * factors[index];
        }

        var verifier = GetModulo11Verifier(sum);
        return verifier == (ruc[8] - '0');
    }

    private static bool IsValidPrivateSocietyRuc(string ruc)
    {
        if (!HasValidProvinceCode(ruc) || !ruc.EndsWith("001", StringComparison.Ordinal))
        {
            return false;
        }

        var factors = new[] { 4, 3, 2, 7, 6, 5, 4, 3, 2 };
        var sum = 0;
        for (var index = 0; index < factors.Length; index++)
        {
            sum += (ruc[index] - '0') * factors[index];
        }

        var verifier = GetModulo11Verifier(sum);
        return verifier == (ruc[9] - '0');
    }

    private static bool HasValidProvinceCode(string value)
    {
        var province = int.Parse(value[..2]);
        return province is >= 1 and <= 24 || province == 30;
    }

    private static int GetModulo10Verifier(int sum)
    {
        var remainder = sum % 10;
        return remainder == 0 ? 0 : 10 - remainder;
    }

    private static int GetModulo11Verifier(int sum)
    {
        var remainder = sum % 11;
        var result = remainder == 0 ? 0 : 11 - remainder;
        return result == 11 ? 0 : result;
    }
}
