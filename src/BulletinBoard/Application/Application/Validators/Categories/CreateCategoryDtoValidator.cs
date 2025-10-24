using BulletinBoard.Contracts.Categories;
using FluentValidation;

namespace BulletinBoard.Application.Validators.Categories;

/// <summary>
/// Валидатор для создания категории.
/// </summary>
public class CreateCategoryDtoValidator : AbstractValidator<CreateCategoryDto>
{
    /// <summary>
    /// Инициализирует валидатор с правилами валидации.
    /// </summary>
    public CreateCategoryDtoValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Название категории обязательно.")
            .Length(2, 50).WithMessage("Название должно содержать от 2 до 50 символов.")
            .Matches(@"^[а-яА-ЯёЁa-zA-Z0-9\s\-]+$")
            .WithMessage("Название содержит недопустимые символы.");
    }
}