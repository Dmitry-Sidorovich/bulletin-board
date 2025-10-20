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
        When(x => x.DisplayName != null, () =>
        {
            RuleFor(x => x.DisplayName!)
                .NotEmpty().WithMessage("Имя не может быть пустым.")
                .Length(3, 100).WithMessage("Имя должно содержать от 3 до 100 символов.");
        });

        When(x => x.Email != null, () =>
        {
            RuleFor(x => x.Email!)
                .NotEmpty().WithMessage("Email не может быть пустым.")
                .EmailAddress().WithMessage("Некорректный формат email.")
                .MaximumLength(255);
        });

        When(x => x.Phone != null, () =>
        {
            RuleFor(x => x.Phone!)
                .Matches(@"^\+?\d{10,15}$")
                .WithMessage("Некорректный формат телефона.");
        });
    }
}