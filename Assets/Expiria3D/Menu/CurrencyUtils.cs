using System;
using System.Globalization;
using System.Text.RegularExpressions;

public static class CurrencyUtils
{
    /// <summary>
    /// Convierte una cadena de precio con formato variable a double,
    /// asumiendo que, si hay exactamente un separador (.,) y
    /// detrás de él hay 3 dígitos, se trata de un separador de millares.
    /// </summary>
    public static double FromPriceToDouble(string price)
    {
        if (string.IsNullOrWhiteSpace(price))
            return 0;

        // 1) Eliminar paréntesis (asumiendo que no indican negativo en este escenario).
        price = price.Replace("(", "").Replace(")", "");

        // 2) Conservar solo dígitos, puntos, comas y el signo '-'
        price = Regex.Replace(price, "[^\\d.,-]", "");

        // 3) Ver cuántos '.' y ',' hay
        int countDots = 0, countCommas = 0;
        foreach (char c in price)
        {
            if (c == '.') countDots++;
            if (c == ',') countCommas++;
        }

        // 4) Regla heurística: si hay exactamente 1 separador,
        //    y detrás de él hay 3 dígitos, asumimos que es de miles (no decimal)
        if ((countDots + countCommas) == 1)
        {
            int dotIndex = price.IndexOf('.');
            int commaIndex = price.IndexOf(',');
            int separatorIndex = (dotIndex >= 0) ? dotIndex : commaIndex;

            if (separatorIndex >= 0 && separatorIndex < price.Length - 1)
            {
                string afterSeparator = price.Substring(separatorIndex + 1);
                // ¿Son exactamente 3 dígitos?
                if (Regex.IsMatch(afterSeparator, @"^\d{3}$"))
                {
                    // Quitamos ese único separador, asumiendo que es millar
                    price = price.Remove(separatorIndex, 1);
                }
            }
        }

        // 5) Localizar el último '.' o ','
        int lastDotIndex = price.LastIndexOf('.');
        int lastCommaIndex = price.LastIndexOf(',');
        int lastSeparatorIndex = Math.Max(lastDotIndex, lastCommaIndex);
        bool hasDecimalSeparator = lastSeparatorIndex >= 0;

        if (hasDecimalSeparator)
        {
            // Tomamos la parte entera y la parte decimal
            string integerPart = price.Substring(0, lastSeparatorIndex);
            string fractionalPart = price.Substring(lastSeparatorIndex + 1);

            // Eliminamos todos los '.' y ',' en la parte entera (separadores de miles)
            integerPart = integerPart.Replace(".", "").Replace(",", "");
            // Lo mismo en la parte decimal
            fractionalPart = fractionalPart.Replace(".", "").Replace(",", "");

            // Reconstruimos usando '.' como separador decimal
            price = integerPart + "." + fractionalPart;
        }
        else
        {
            // Si no hay separador decimal, quitamos todos los '.' y ',' (separadores de miles)
            price = price.Replace(".", "").Replace(",", "");
        }

        // 6) Convertir a double con cultura invariable
        if (double.TryParse(price,
            NumberStyles.AllowLeadingSign | NumberStyles.AllowDecimalPoint,
            CultureInfo.InvariantCulture,
            out double result))
        {
            return result;
        }

        // Si no se pudo parsear
        return 0;
    }

    /// <summary>
    /// Retorna true si, al parsear el precio, este tiene parte decimal; false si es entero.
    /// </summary>
    public static bool HasDecimals(string price)
    {
        double valor = FromPriceToDouble(price);

        // Chequeo básico: si su parte fraccionaria es distinta de cero
        // (ten en cuenta que con floats/doubles muy grandes puede haber imprecisiones).
        double parteEntera = Math.Floor(Math.Abs(valor));
        double parteDecimal = Math.Abs(valor) - parteEntera;

        // Consideramos que si la diferencia es mayor que un epsilon pequeño, hay parte decimal.
        return parteDecimal > 1e-12;
    }

    /// <summary>
    /// Retorna la porción de texto antes de donde inicia el bloque que se interpreta como precio.
    /// 
    /// El criterio para "precio" es: la primera secuencia de caracteres que contenga dígitos (0-9)
    /// y/o '.', ',' o '-', sin interrupción por otro tipo de carácter.
    /// </summary>
    public static string GetTextBeforePrice(string input)
    {
        if (string.IsNullOrEmpty(input))
            return string.Empty;

        // Buscamos la primera posición donde comienza un bloque de [0-9.,-]
        var (start, end) = FindPriceIndexes(input);

        // Si no hay bloque numérico, devolvemos todo como "antes"
        if (start == -1)
            return input;

        return input.Substring(0, start);
    }

    /// <summary>
    /// Retorna la porción de texto después de donde termina el bloque que se interpreta como precio.
    /// </summary>
    public static string GetTextAfterPrice(string input)
    {
        if (string.IsNullOrEmpty(input))
            return string.Empty;

        var (start, end) = FindPriceIndexes(input);

        // Si no existe bloque numérico, no hay "después"
        if (start == -1)
            return string.Empty;

        // Si el precio termina al final, tampoco hay "después"
        if (end >= input.Length - 1)
            return string.Empty;

        return input.Substring(end + 1);
    }

    /// <summary>
    /// Encuentra el rango [start, end] en 'input' que corresponde
    /// a la primera secuencia de caracteres que sean dígitos, punto,
    /// coma o signo '-', de forma continua.
    /// Retorna (-1, -1) si no se encuentra nada parecido.
    /// </summary>
    private static (int start, int end) FindPriceIndexes(string input)
    {
        int start = -1;
        int end = -1;
        bool inSequence = false;

        for (int i = 0; i < input.Length; i++)
        {
            char c = input[i];
            bool isPriceChar = (char.IsDigit(c) || c == '.' || c == ',' || c == '-');

            if (isPriceChar)
            {
                if (!inSequence)
                {
                    // Iniciamos la secuencia
                    start = i;
                    inSequence = true;
                }
                // Mientras sea parte de la secuencia, vamos actualizando 'end'
                end = i;
            }
            else
            {
                // Si estábamos en secuencia y nos topamos con un carácter que no calza,
                // terminamos la búsqueda.
                if (inSequence)
                {
                    break;
                }
            }
        }

        // Si al final nunca se inició la secuencia, devolvemos (-1, -1)
        if (!inSequence) 
            return (-1, -1);

        return (start, end);
    }
}
