using BulletinBoard.Contracts.Users;
using FluentValidation;

namespace BulletinBoard.Application.Validators.Users;

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
            .Length(2, 50).WithMessage("Имя должно содержать от 2 до 50 символов.")
            .Matches(@"^[а-яА-ЯёЁa-zA-Z0-9\s\-]+$")
            .WithMessage("Имя содержит недопустимые символы.")
            .When(x => !string.IsNullOrEmpty(x.DisplayName));

        RuleFor(x => x.Email)
            .EmailAddress().WithMessage("Некорректный формат email.")
            .MaximumLength(100).WithMessage("Email слишком длинный.")
            .When(x => !string.IsNullOrEmpty(x.Email));

        RuleFor(x => x.Phone)
            .Matches(@"^\+?\d{10,15}$")
            .WithMessage("Некорректный формат телефона. Пример: +79991234567")
            .When(x => !string.IsNullOrEmpty(x.Phone));
    }
}