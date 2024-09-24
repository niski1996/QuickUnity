namespace QuickUnity.Common
{
    public static class AppRegex
    {
        public const string PasswordRegex = @"^(?=.*[0-9])(?=.*[a-z])(?=.*[A-Z])(?=.*\W)(?!.* ).{8,50}$";
        public const string PasswordInfo = "Password must contain one digit from 1 to 9, one lowercase letter, one uppercase letter, one special character, no space, and it must be 8-50 characters long..";
        
        public const string BaseRegex = "^[A-Za-z_\\.-]+$";
        public const string BaseInfo = "Must contain only letters, underscores, dots, or hyphens, and be at least 2 characters long.";
        
        public const string LastNameRegex = "^[A-Z][a-z]+(-[A-Z][a-z]+)?$";
        public const string LastNameInfo = "Last name must start with a capital letter and contain only letters. Hyphenated last names are allowed.";

        public const string PostalCodeRegex = "^[0-9]{2}-[0-9]{3}$";
        public const string PostalCodeInfo = "Postal code must be in the format 00-000.";

    }
}