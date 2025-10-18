using BulletinBoard.Contracts.Advertisements;
using FluentValidation;

namespace BulletinBoard.Application.Validators.Advertisements;

/// <summary>
/// Валидатор для изменения статуса объявления.
/// </summary>
public class ChangeAdvertisementStatusDtoValidator : AbstractValidator<ChangeAdvertisementStatusDto>
{
    /// <summary>
    /// Инициализирует валидатор с правилами валидации.
    /// </summary>
    public ChangeAdvertisementStatusDtoValidator()
    {
        RuleFor(x => x.StatusDto)
            .IsInEnum().WithMessage("Некорректный статус объявления.");
    }
}