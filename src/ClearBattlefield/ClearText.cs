namespace ClearBattlefield
{
    /// <summary>Player-facing strings, kept apart from Unity code so the wording is tested.</summary>
    public static class ClearText
    {
        public static string Items(int count) => count == 1 ? "1 enemy drop" : $"{count} enemy drops";

        public static string Scrap(int count) => count == 1 ? "1 piece of scrap" : $"{count} pieces of scrap";

        /// <summary>"12 enemy drops and 3 pieces of scrap", leaving out whichever is zero.</summary>
        public static string Describe(int drops, int scrap)
        {
            if (scrap == 0)
                return Items(drops);
            return drops == 0 ? Scrap(scrap) : $"{Items(drops)} and {Scrap(scrap)}";
        }

        public static string Confirmation(int drops, int scrap) =>
            drops + scrap == 0
                ? "There is nothing on the ground to clear."
                : $"Remove {Describe(drops, scrap)}? Everything else on the ground stays.";

        public static string Result(int drops, int scrap) =>
            drops + scrap == 0 ? "Nothing to clear" : $"Cleared {Describe(drops, scrap)}";
    }
}
