namespace Assignment1.Services
{
    public class PasswordPolicy
    {
        public static readonly TimeSpan MinPasswordAge = TimeSpan.FromDays(1);  // Minimum password age (1 day)
        public static readonly TimeSpan MaxPasswordAge = TimeSpan.FromDays(90); // Maximum password age (90 days)
    }
}
