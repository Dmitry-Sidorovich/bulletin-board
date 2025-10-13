using BulletinBoard.Contracts.Users;
using FluentValidation;

namespace BulletinBoard.Application.Validators.Users;

/// <summary>
/// Валидатор для создания пользователя.
/// </summary>
public class CreateUserDtoValidator : AbstractValidator<CreateUserDto>
{
    /// <summary>
    /// Инициализирует валидатор с правилами валидации.
    /// </summary>
    public CreateUserDtoValidator()
    {
        RuleFor(x => x.DisplayName)
            .NotEmpty().WithMessage("Имя обязательно.")
            .Length(2, 50).WithMessage("Имя должно содержать от 2 до 50 символов.")
            .Matches(@"^[а-яА-ЯёЁa-zA-Z0-9\s\-]+$")
            .WithMessage("Имя содержит недопустимые символы.");

        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email обязателен.")
            .EmailAddress().WithMessage("Некорректный формат email.")
            .MaximumLength(100).WithMessage("Email слишком длинный.");

        RuleFor(x => x.Phone)
            .Matches(@"^\+?\d{10,15}$")
            .WithMessage("Некорректный формат телефона. Пример: +79991234567")
            .When(x => !string.IsNullOrEmpty(x.Phone));
    }
}