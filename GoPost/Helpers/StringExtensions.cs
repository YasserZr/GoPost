namespace GoPost.Helpers
{
    public static class StringExtensions
    {
        /// <summary>
        /// Formats a username by removing email domain and capitalizing first letter
        /// </summary>
        public static string FormatUsername(this string? username)
        {
            if (string.IsNullOrWhiteSpace(username))
                return "Unknown User";

            // Remove email domain if present
            var name = username.Contains('@') 
                ? username.Substring(0, username.IndexOf('@')) 
                : username;

            // Capitalize first letter
            if (name.Length > 0)
            {
                name = char.ToUpper(name[0]) + (name.Length > 1 ? name.Substring(1) : "");
            }

            return name;
        }
    }
}
