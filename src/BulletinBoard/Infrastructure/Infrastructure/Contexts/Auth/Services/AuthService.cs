using BulletinBoard.Application.Abstractions;
using BulletinBoard.Application.Contexts.Auth;
using BulletinBoard.Application.Contexts.Users;
using BulletinBoard.Application.Exceptions;
using BulletinBoard.Contracts.Auth;
using BulletinBoard.Domain.Entities;
using BulletinBoard.Infrastructure.Contexts.Auth.Options;
using Microsoft.Extensions.Options;

namespace BulletinBoard.Infrastructure.Contexts.Auth.Services;

/// <summary>
/// Реализация <see cref="IAuthService"/> для аутентификации и авторизации.
/// </summary>
public class AuthService : IAuthService
{
    private readonly IUserRepository _userRepository;
    private readonly IRefreshTokenRepository _refreshTokenRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IJwtTokenService _jwtTokenService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly JwtOptions _jwtOptions;

    /// <summary>
    /// Инициализирует сервис аутентификации.
    /// </summary>
    /// <param name="userRepository">Репозиторий для операций с пользователями.</param>
    /// <param name="refreshTokenRepository">Репозиторий для операций с refresh-токенами.</param>
    /// <param name="passwordHasher">Сервис хеширования паролей.</param>
    /// <param name="jwtTokenService">Сервис генерации JWT-токенов.</param>
    /// <param name="unitOfWork">Единица работы для сохранения изменений.</param>
    /// <param name="jwtOptions">Параметры конфигурации JWT (время жизни токенов).</param>
    public AuthService(
        IUserRepository userRepository,
        IRefreshTokenRepository refreshTokenRepository,
        IPasswordHasher passwordHasher,
        IJwtTokenService jwtTokenService,
        IUnitOfWork unitOfWork,
        IOptions<JwtOptions> jwtOptions)
    {
        _userRepository = userRepository;
        _refreshTokenRepository = refreshTokenRepository;
        _passwordHasher = passwordHasher;
        _jwtTokenService = jwtTokenService;
        _unitOfWork = unitOfWork;
        _jwtOptions = jwtOptions.Value ?? new JwtOptions();
    }

    /// <inheritdoc />
    public async Task<LoginResponseDto> RegisterAsync(RegisterDto dto, CancellationToken cancellationToken = default)
    {
        var existingUser = await _userRepository.GetByEmailAsync(dto.Email, cancellationToken);
        if (existingUser != null)
        {
            throw new ValidationException("Пользователь с таким email уже существует.");
        }

        var passwordHash = _passwordHasher.HashPassword(dto.Password);

        var user = new User(
            displayName: dto.DisplayName,
            email: dto.Email,
            passwordHash: passwordHash,
            phone: dto.Phone);

        await _userRepository.AddAsync(user, cancellationToken);

        var refreshTokenValue = await CreateRefreshTokenAsync(user.Id, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return BuildLoginResponse(user, refreshTokenValue);
    }

    /// <inheritdoc />
    public async Task<LoginResponseDto> LoginAsync(LoginDto dto, CancellationToken cancellationToken = default)
    {
        var user = await _userRepository.GetByEmailAsync(dto.Email, cancellationToken);
        if (user == null)
        {
            throw new ValidationException("Неверный email или пароль.");
        }

        if (!_passwordHasher.VerifyPassword(dto.Password, user.PasswordHash))
        {
            throw new ValidationException("Неверный email или пароль.");
        }

        var refreshTokenValue = await CreateRefreshTokenAsync(user.Id, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return BuildLoginResponse(user, refreshTokenValue);
    }

    /// <inheritdoc />
    public async Task<LoginResponseDto> RefreshTokenAsync(RefreshTokenDto dto, CancellationToken cancellationToken = default)
    {
        var refreshToken = await _refreshTokenRepository.GetByTokenAsync(dto.RefreshToken, cancellationToken);

        if (refreshToken == null || !refreshToken.IsActive)
        {
            throw new ValidationException("Недействительный или истёкший refresh token.");
        }

        var user = await _userRepository.GetByIdAsync(refreshToken.UserId, cancellationToken);
        if (user is null)
        {
            throw new NotFoundException("User", "Пользователь для refresh-токена не найден.");
        }
        
        refreshToken.Revoke();

        var newRefreshTokenValue = await CreateRefreshTokenAsync(user.Id, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return BuildLoginResponse(user, newRefreshTokenValue);
    }

    /// <inheritdoc />
    public async Task LogoutAsync(string refreshToken, CancellationToken cancellationToken = default)
    {
        var token = await _refreshTokenRepository.GetByTokenAsync(refreshToken, cancellationToken);
        
        if (token != null && token.IsActive)
        {
            token.Revoke();
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }
    }

    /// <summary>
    /// Создаёт refresh token для пользователя и добавляет его в контекст (БЕЗ сохранения).
    /// Сохранение происходит на уровне публичных методов для атомарности операций.
    /// </summary>
    /// <param name="userId">ID пользователя.</param>
    /// <param name="cancellationToken">Токен для отмены асинхронной операции.</param>
    /// <returns>Значение refresh token.</returns>
    private async Task<string> CreateRefreshTokenAsync(Guid userId, CancellationToken cancellationToken)
    {
        var refreshTokenValue = _jwtTokenService.GenerateRefreshToken();

        var refreshToken = new RefreshToken(
            userId: userId,
            token: refreshTokenValue,
            expiresAt: DateTimeOffset.UtcNow.AddDays(_jwtOptions.RefreshTokenExpirationDays));

        await _refreshTokenRepository.AddAsync(refreshToken, cancellationToken);
        return refreshTokenValue;
    }

    /// <summary>
    /// Формирует ответ входа с токенами и информацией о пользователе.
    /// Этот метод только собирает данные, НЕ выполняет I/O операций.
    /// </summary>
    /// <param name="user">Пользователь.</param>
    /// <param name="refreshToken">Значение refresh token.</param>
    /// <returns>DTO с токенами и информацией о пользователе.</returns>
    private LoginResponseDto BuildLoginResponse(User user, string refreshToken)
    {
        return new LoginResponseDto
        {
            AccessToken = _jwtTokenService.GenerateAccessToken(user),
            RefreshToken = refreshToken,
            ExpiresIn = _jwtOptions.AccessTokenExpirationMinutes * 60,
            User = new UserInfoDto
            {
                Id = user.Id,
                DisplayName = user.DisplayName,
                Email = user.Email,
                Role = user.Role.ToString()
            }
        };
    }
}