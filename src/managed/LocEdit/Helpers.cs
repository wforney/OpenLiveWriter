namespace LocEdit
{
    using System.Text;

    /// <summary>
    /// Class Helpers.
    /// </summary>
    internal static class Helpers
    {
        /// <summary>
        /// Csvizes the string.
        /// </summary>
        /// <param name="input">The input.</param>
        /// <returns>System.String.</returns>
        public static string CsvizeString(string input)
        {
            var sb = new StringBuilder();
            var shouldQuote = input.Contains(",") || input.Contains("\n") || (input != string.Empty && input[0] == '"');

            if (shouldQuote)
            {
                sb.Append('"');
                sb.Append(input.Replace("\"", "\"\"")); // Replace double-quotes with double-double-quotes
                sb.Append('"');
            }
            else
            {
                sb.Append(input);
            }

            return sb.ToString();
        }
    }
}
