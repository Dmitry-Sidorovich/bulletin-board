using BulletinBoard.Contracts.Advertisements;
using FluentValidation;

namespace BulletinBoard.Application.Validators.Advertisements;

/// <summary>
/// Валидатор для обновления объявления.
/// </summary>
public class UpdateAdvertisementDtoValidator : AbstractValidator<UpdateAdvertisementDto>
{
    /// <summary>
    /// Инициализирует валидатор с правилами валидации.
    /// </summary>
    public UpdateAdvertisementDtoValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Заголовок обязателен.")
            .Length(3, 100).WithMessage("Заголовок должен содержать от 3 до 100 символов.")
            .Matches(@"^[а-яА-ЯёЁa-zA-Z0-9\s\-,.!?]+$")
            .WithMessage("Заголовок содержит недопустимые символы.");

        RuleFor(x => x.Description)
            .MaximumLength(1000).WithMessage("Описание не может превышать 1000 символов.")
            .When(x => !string.IsNullOrEmpty(x.Description));

        RuleFor(x => x.CategoryId)
            .NotEmpty().WithMessage("Категория обязательна.");

        RuleFor(x => x.Contact)
            .NotNull().WithMessage("Контактные данные обязательны.")
            .SetValidator(new ContactDtoValidator());
    }
}