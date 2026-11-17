using System.Text;

namespace Fase
{
    /// <summary>
    /// Códigos para compartir niveles del editor: el mapa se comprime (tramos repetidos como
    /// "número+letra") y se pasa a base64 seguro para URL. Empiezan por "FASE-".
    /// </summary>
    public static class LevelCode
    {
        const string Prefix = "FASE-";

        public static string Encode(string[] rows)
        {
            var sb = new StringBuilder();
            foreach (var row in rows)
            {
                for (int i = 0; i < row.Length;)
                {
                    int run = 1;
                    while (i + run < row.Length && row[i + run] == row[i] && run < 99) run++;
                    if (run > 2) sb.Append(run).Append(row[i]);
                    else sb.Append(row[i], run);
                    i += run;
                }
                sb.Append('|');
            }
            string b64 = System.Convert.ToBase64String(Encoding.UTF8.GetBytes(sb.ToString()));
            return Prefix + b64.TrimEnd('=').Replace('+', '-').Replace('/', '_');
        }

        // Topes: los niveles del Taller miden 64 columnas; se deja margen para niveles más anchos.
        const int MaxCodeLength = 8192, MaxRow = 256;

        public static string[] Decode(string code)
        {
            if (string.IsNullOrEmpty(code) || code.Length > MaxCodeLength) return null;
            try
            {
                code = code.Trim();
                if (code.StartsWith(Prefix)) code = code.Substring(Prefix.Length);
                code = code.Replace('-', '+').Replace('_', '/');
                while (code.Length % 4 != 0) code += "=";
                string text = Encoding.UTF8.GetString(System.Convert.FromBase64String(code));
                var rows = text.Split('|');
                var result = new string[RuntimeLevel.Rows];
                for (int r = 0; r < result.Length; r++)
                {
                    var sb = new StringBuilder();
                    string row = r < rows.Length ? rows[r] : "";
                    for (int i = 0; i < row.Length; i++)
                    {
                        int run = 0;
                        while (i < row.Length && row[i] >= '0' && row[i] <= '9' && run <= MaxRow) run = run * 10 + (row[i++] - '0');
                        if (run > MaxRow) return null;
                        if (i < row.Length) sb.Append(row[i], run > 0 ? run : 1);
                        if (sb.Length > MaxRow) return null;
                    }
                    result[r] = sb.ToString();
                }
                return result;
            }
            catch
            {
                return null;
            }
        }
    }
}
