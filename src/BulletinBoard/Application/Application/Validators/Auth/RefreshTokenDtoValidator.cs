using BulletinBoard.Contracts.Auth;
using FluentValidation;

namespace BulletinBoard.Application.Validators.Auth;

/// <summary>
/// Валидатор для обновления токена.
/// </summary>
public class RefreshTokenDtoValidator : AbstractValidator<RefreshTokenDto>
{
    /// <summary>
    /// Инициализирует валидатор с правилами валидации.
    /// </summary>
    public RefreshTokenDtoValidator()
    {
        RuleFor(x => x.RefreshToken)
            .NotEmpty().WithMessage("Refresh token обязателен.");
    }
}