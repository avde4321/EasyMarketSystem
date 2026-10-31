namespace TestDeIa.Shared.Validation;

public static class ValidadorEcuador
{
    public static bool EsCedulaValida(string? cedula)
    {
        var digits = OnlyDigits(cedula);
        if (digits.Length != 10 || !ProvinciaValida(digits))
        {
            return false;
        }

        var total = 0;
        for (var index = 0; index < 9; index++)
        {
            var value = digits[index] - '0';
            if (index % 2 == 0)
            {
                value *= 2;
                if (value > 9)
                {
                    value -= 9;
                }
            }

            total += value;
        }

        var expected = total % 10 == 0 ? 0 : 10 - (total % 10);
        return expected == digits[9] - '0';
    }

    public static bool EsRucPersonaJuridicaValido(string? ruc)
    {
        var digits = OnlyDigits(ruc);
        if (digits.Length != 13 || !ProvinciaValida(digits) || digits[2] != '9' || digits[^3..] != "001")
        {
            return false;
        }

        var coefficients = new[] { 4, 3, 2, 7, 6, 5, 4, 3, 2 };
        var total = 0;
        for (var index = 0; index < coefficients.Length; index++)
        {
            total += (digits[index] - '0') * coefficients[index];
        }

        var mod = total % 11;
        var expected = mod == 0 ? 0 : 11 - mod;
        return expected == digits[9] - '0';
    }

    public static bool EsRucNaturalValido(string? ruc)
    {
        var digits = OnlyDigits(ruc);
        return digits.Length == 13 && digits[^3..] == "001" && EsCedulaValida(digits[..10]);
    }

    private static bool ProvinciaValida(string digits)
    {
        var province = int.Parse(digits[..2]);
        return province is >= 1 and <= 24;
    }

    private static string OnlyDigits(string? value)
    {
        return string.IsNullOrWhiteSpace(value)
            ? string.Empty
            : new string(value.Where(char.IsDigit).ToArray());
    }
}
