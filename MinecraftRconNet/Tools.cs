using System.Text;

namespace MinecraftRconNet
{

    public static class Tools
    {

        private static readonly Dictionary<string, string> ColorCodeMap = new()
        {
            { "§0", string.Empty },
            { "§1", string.Empty },
            { "§2", string.Empty },
            { "§3", string.Empty },
            { "§4", string.Empty },
            { "§5", string.Empty },
            { "§6", string.Empty },
            { "§7", string.Empty },
            { "§8", string.Empty },
            { "§9", string.Empty },
            { "§a", string.Empty },
            { "§b", string.Empty },
            { "§c", string.Empty },
            { "§d", string.Empty },
            { "§e", string.Empty },
            { "§f", string.Empty }
        };

        /// <summary>
        /// Removes color codes (e.g., Minecraft § codes) from a string.
        /// </summary>
        /// <param name="text">The input text containing color codes.</param>
        /// <returns>A string with color codes removed.</returns>
        public static string RemoveColorCodes(this string text)
        {
            if (!text.Contains('§'))
            {
                return text;
            }

            StringBuilder sb = new(text);
            foreach (KeyValuePair<string, string> kvp in ColorCodeMap)
            {
                sb.Replace(kvp.Key, kvp.Value);
            }

            return sb.ToString();
        }
    }
}