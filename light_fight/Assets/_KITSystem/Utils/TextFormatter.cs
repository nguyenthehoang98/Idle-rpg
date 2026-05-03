namespace _KITSystem.Utils
{
    public static class TextFormatter
    {
        public static string SplitCamelCase(string input)
        {
            if (string.IsNullOrEmpty(input))
                return input;

            var result = new System.Text.StringBuilder();

            result.Append(input[0]);

            for (int i = 1; i < input.Length; i++)
            {
                if (char.IsUpper(input[i]) && !char.IsWhiteSpace(input[i - 1]))
                {
                    result.Append(' ');
                }

                result.Append(input[i]);
            }

            return result.ToString();
        }
    }
}