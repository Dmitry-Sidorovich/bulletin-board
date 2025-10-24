namespace BulletinBoard.Application.Abstractions;

/// <summary>
/// Сервис для хеширования и проверки паролей.
/// </summary>
public interface IPasswordHasher
{
    /// <summary>
    /// Хеширует пароль с использованием BCrypt.
    /// </summary>
    /// <param name="password">Пароль в открытом виде.</param>
    /// <returns>Хеш пароля.</returns>
    string HashPassword(string password);

    /// <summary>
    /// Проверяет, соответствует ли пароль хешу.
    /// </summary>
    /// <param name="password">Пароль в открытом виде.</param>
    /// <param name="passwordHash">Сохранённый хеш пароля.</param>
    /// <returns>True, если пароль верный; иначе false.</returns>
    bool VerifyPassword(string password, string passwordHash);
}