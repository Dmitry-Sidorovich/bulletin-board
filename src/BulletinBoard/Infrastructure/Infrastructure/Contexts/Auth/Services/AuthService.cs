using BulletinBoard.Application.Abstractions;
using BulletinBoard.Application.Contexts.Auth;
using BulletinBoard.Application.Contexts.Users;
using BulletinBoard.Application.Exceptions;
using BulletinBoard.Contracts.Auth;
using BulletinBoard.Domain.Entities;
using Microsoft.Extensions.Configuration;

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
    private readonly IConfiguration _configuration;

    /// <summary>
    /// Инициализирует сервис аутентификации.
    /// </summary>
    public AuthService(
        IUserRepository userRepository,
        IRefreshTokenRepository refreshTokenRepository,
        IPasswordHasher passwordHasher,
        IJwtTokenService jwtTokenService,
        IUnitOfWork unitOfWork,
        IConfiguration configuration)
    {
        _userRepository = userRepository;
        _refreshTokenRepository = refreshTokenRepository;
        _passwordHasher = passwordHasher;
        _jwtTokenService = jwtTokenService;
        _unitOfWork = unitOfWork;
        _configuration = configuration;
    }

    /// <inheritdoc />
    public async Task<LoginResponseDto> RegisterAsync(RegisterDto dto, CancellationToken cancellationToken = default)
    {
        // Проверяем, существует ли пользователь с таким email
        var existingUser = await _userRepository.GetByEmailAsync(dto.Email, cancellationToken);
        if (existingUser != null)
        {
            throw new ValidationException("Пользователь с таким email уже существует.");
        }

        // Хешируем пароль
        var passwordHash = _passwordHasher.HashPassword(dto.Password);

        // Создаём пользователя
        var user = new User(
            displayName: dto.DisplayName,
            email: dto.Email,
            passwordHash: passwordHash,
            phone: dto.Phone);

        await _userRepository.AddAsync(user, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Генерируем токены
        return await GenerateTokenResponseAsync(user, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<LoginResponseDto> LoginAsync(LoginDto dto, CancellationToken cancellationToken = default)
    {
        // Находим пользователя
        var user = await _userRepository.GetByEmailAsync(dto.Email, cancellationToken);
        if (user == null)
        {
            throw new ValidationException("Неверный email или пароль.");
        }

        // Проверяем пароль
        if (!_passwordHasher.VerifyPassword(dto.Password, user.PasswordHash))
        {
            throw new ValidationException("Неверный email или пароль.");
        }

        // Генерируем токены
        return await GenerateTokenResponseAsync(user, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<LoginResponseDto> RefreshTokenAsync(RefreshTokenDto dto, CancellationToken cancellationToken = default)
    {
        // Находим refresh token
        var refreshToken = await _refreshTokenRepository.GetByTokenAsync(dto.RefreshToken, cancellationToken);
        
        if (refreshToken == null || !refreshToken.IsActive)
        {
            throw new ValidationException("Недействительный или истёкший refresh token.");
        }

        // Отзываем старый токен
        refreshToken.Revoke();
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Генерируем новые токены
        return await GenerateTokenResponseAsync(refreshToken.User, cancellationToken);
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
    /// Генерирует токены для пользователя.
    /// </summary>
    private async Task<LoginResponseDto> GenerateTokenResponseAsync(User user, CancellationToken cancellationToken)
    {
        // Генерируем access token
        var accessToken = _jwtTokenService.GenerateAccessToken(user);

        // Генерируем refresh token
        var refreshTokenValue = _jwtTokenService.GenerateRefreshToken();
        var refreshTokenExpirationDays = int.Parse(_configuration["Jwt:RefreshTokenExpirationDays"]!);
        
        var refreshToken = new RefreshToken(
            userId: user.Id,
            token: refreshTokenValue,
            expiresAt: DateTimeOffset.UtcNow.AddDays(refreshTokenExpirationDays));

        await _refreshTokenRepository.AddAsync(refreshToken, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Формируем ответ
        return new LoginResponseDto
        {
            AccessToken = accessToken,
            RefreshToken = refreshTokenValue,
            ExpiresIn = int.Parse(_configuration["Jwt:AccessTokenExpirationMinutes"]!) * 60,
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