using BulletinBoard.Application.Abstractions;

namespace BulletinBoard.Infrastructure.Services.Auth;

/// <summary>
/// Реализация <see cref="IPasswordHasher"/> с использованием BCrypt.
/// </summary>
public class PasswordHasher : IPasswordHasher
{
    /// <inheritdoc />
    public string HashPassword(string password)
    {
        if (string.IsNullOrWhiteSpace(password))
        {
            throw new ArgumentException("Пароль не может быть пустым.", nameof(password));
        }

        return BCrypt.Net.BCrypt.HashPassword(password, workFactor: 12);
    }

    /// <inheritdoc />
    public bool VerifyPassword(string password, string passwordHash)
    {
        if (string.IsNullOrWhiteSpace(password))
        {
            throw new ArgumentException("Пароль не может быть пустым.", nameof(password));
        }

        if (string.IsNullOrWhiteSpace(passwordHash))
        {
            throw new ArgumentException("Хеш пароля не может быть пустым.", nameof(passwordHash));
        }

        return BCrypt.Net.BCrypt.Verify(password, passwordHash);
    }
}