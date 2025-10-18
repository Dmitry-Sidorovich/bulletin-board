using BulletinBoard.Contracts.Auth;
using FluentValidation;

namespace BulletinBoard.Application.Validators.Auth;

/// <summary>
/// Валидатор для регистрации пользователя.
/// </summary>
public class RegisterDtoValidator : AbstractValidator<RegisterDto>
{
    /// <summary>
    /// Инициализирует валидатор с правилами валидации.
    /// </summary>
    public RegisterDtoValidator()
    {
        RuleFor(x => x.DisplayName)
            .NotEmpty().WithMessage("Имя обязательно.")
            .Length(3, 50).WithMessage("Имя должно содержать от 2 до 50 символов.")
            .Matches(@"^[а-яА-ЯёЁa-zA-Z0-9\s\-]+$")
            .WithMessage("Имя содержит недопустимые символы.");

        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email обязателен.")
            .EmailAddress().WithMessage("Некорректный формат email.")
            .MaximumLength(100).WithMessage("Email слишком длинный.");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Пароль обязателен.")
            .MinimumLength(6).WithMessage("Пароль должен содержать минимум 6 символов.")
            .MaximumLength(100).WithMessage("Пароль слишком длинный.")
            .Matches(@"[A-Z]").WithMessage("Пароль должен содержать хотя бы одну заглавную букву.")
            .Matches(@"[a-z]").WithMessage("Пароль должен содержать хотя бы одну строчную букву.")
            .Matches(@"\d").WithMessage("Пароль должен содержать хотя бы одну цифру.");

        RuleFor(x => x.Phone)
            .Matches(@"^\+?\d{10,15}$")
            .WithMessage("Некорректный формат телефона. Пример: +79991234567")
            .When(x => !string.IsNullOrEmpty(x.Phone));
    }
}