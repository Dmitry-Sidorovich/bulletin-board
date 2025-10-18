using BulletinBoard.Contracts.Users;
using FluentValidation;

namespace BulletinBoard.Application.Contexts.Users.Validators;

/// <summary>
/// Валидатор для обновления пользователя.
/// </summary>
public class UpdateUserDtoValidator : AbstractValidator<UpdateUserDto>
{
    /// <summary>
    /// Инициализирует валидатор с правилами валидации.
    /// </summary>
    public UpdateUserDtoValidator()
    {
        RuleFor(x => x.DisplayName)
            .Length(3, 100).WithMessage("Имя должно содержать от 3 до 100 символов.")
            .Matches(@"^[а-яА-ЯёЁa-zA-Z0-9\s\-]+$")
            .WithMessage("Имя содержит недопустимые символы.")
            .When(x => !string.IsNullOrWhiteSpace(x.DisplayName));

        RuleFor(x => x.Email)
            .EmailAddress().WithMessage("Некорректный формат email.")
            .MaximumLength(255).WithMessage("Email не может превышать 255 символов.")
            .When(x => !string.IsNullOrWhiteSpace(x.Email));

        RuleFor(x => x.Phone)
            .Matches(@"^\+?\d{10,15}$")
            .WithMessage("Некорректный формат телефона. Пример: +79991234567")
            .When(x => !string.IsNullOrWhiteSpace(x.Phone));
    }
}