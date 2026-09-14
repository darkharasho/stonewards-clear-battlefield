namespace ClearBattlefield
{
    /// <summary>Player-facing strings, kept apart from Unity code so the wording is tested.</summary>
    public static class ClearText
    {
        public static string Weapons(int count) => count == 1 ? "1 dropped weapon" : $"{count} dropped weapons";

        public static string Confirmation(int count) =>
            count == 0
                ? "There are no weapons dropped by enemies to clear."
                : $"Remove {Weapons(count)} left by enemies? Everything else on the ground stays.";

        public static string Result(int count) =>
            count == 0 ? "No dropped weapons to clear" : $"Cleared {Weapons(count)}";
    }
}
