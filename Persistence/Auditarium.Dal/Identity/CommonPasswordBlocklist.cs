// SPDX-License-Identifier: MIT
namespace Auditarium.Dal.Identity;

/// <summary>
/// Offline baseline for passwords that must never be accepted. The comparison is deliberately
/// case-insensitive and ignores surrounding whitespace so trivial variants remain blocked.
/// The list is code-versioned and can be expanded without changing the credential model.
/// </summary>
internal static class CommonPasswordBlocklist
{
    private static readonly HashSet<string> Passwords = new(StringComparer.OrdinalIgnoreCase)
    {
        "123456", "1234567", "12345678", "123456789", "1234567890", "111111", "000000",
        "123123", "654321", "qwerty", "qwerty123", "qwertz", "qwertz123", "asdfgh", "asdfghjkl",
        "password", "password1", "password123", "password123456", "passwordpassword", "passwort", "passwort1", "passwort123", "passwortpasswort",
        "administrator", "administrator1", "administrator123", "admin", "admin123", "letmein", "welcome", "welcome123",
        "letmeinletmein", "welcome123456789", "willkommen", "willkommen1", "willkommenwillkommen", "iloveyou", "monkey", "dragon", "football", "baseball",
        "master", "shadow", "sunshine", "princess", "trustno1", "changeme", "changeme123",
        "changemechangeme", "secret", "secret123", "default", "default123", "temporary", "temporary123",
        "qwerty123456789", "auditarium", "auditarium1", "auditarium123", "auditarium123456"
    };

    public static bool Contains(string password) => Passwords.Contains(password.Trim());
}
