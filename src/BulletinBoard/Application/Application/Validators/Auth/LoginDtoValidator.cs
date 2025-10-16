using BulletinBoard.Contracts.Auth;
using FluentValidation;

namespace BulletinBoard.Application.Validators.Auth;

/// <summary>
/// Валидатор для входа пользователя.
/// </summary>
public class LoginDtoValidator : AbstractValidator<LoginDto>
{
    /// <summary>
    /// Инициализирует валидатор с правилами валидации.
    /// </summary>
    public LoginDtoValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email обязателен.")
            .EmailAddress().WithMessage("Некорректный формат email.");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Пароль обязателен.");
    }
}