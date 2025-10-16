using AutoMapper;
using BulletinBoard.Application.Abstractions;
using BulletinBoard.Application.Common;
using BulletinBoard.Application.Exceptions;
using BulletinBoard.Contracts.Common;
using BulletinBoard.Contracts.Users;

namespace BulletinBoard.Application.Contexts.Users;

/// <inheritdoc />
public class UserService : IUserService
{
    private readonly IUserRepository _userRepository;
    private readonly IUserReadRepository _userReadRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ICurrentUserService _currentUserService;

    /// <summary>
    /// Инициализирует экземпляр <see cref="UserService"/>.
    /// </summary>
    /// <param name="userRepository">Write-репозиторий доменных пользователей.</param>
    /// <param name="userReadRepository">Read-репозиторий проекций пользователей (DTO).</param>
    /// <param name="unitOfWork">Единица работы для атомарного сохранения.</param>
    /// <param name="mapper">AutoMapper для преобразования Domain→DTO.</param>
    /// /// <param name="currentUserService">Сервис для получения информации о текущем пользователе.</param>
    public UserService(
        IUserRepository userRepository,
        IUserReadRepository userReadRepository,
        IUnitOfWork unitOfWork,
        IMapper mapper,
        ICurrentUserService currentUserService)
    {
        _userRepository = userRepository;
        _userReadRepository = userReadRepository;
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _currentUserService = currentUserService;
    }
    
    /// <inheritdoc />
    public Task<UserDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return _userReadRepository.GetByIdAsync(id, cancellationToken);
    }

    /// <inheritdoc />
    public Task<PagedResult<UserDto>> GetPageAsync(
        PageRequest page, 
        CancellationToken cancellationToken = default)
    {
        page.ThrowIfInvalid();

        return _userReadRepository.GetPageAsync(page, cancellationToken);
    }

    // /// <inheritdoc />
    // public async Task<UserDto> CreateAsync(CreateUserDto dto, CancellationToken cancellationToken = default)
    // {
    //     ArgumentNullException.ThrowIfNull(dto);
    //     if (string.IsNullOrWhiteSpace(dto.DisplayName))
    //     {
    //         throw new ArgumentException("DisplayName is required.", nameof(dto));
    //     }
    //     if (string.IsNullOrWhiteSpace(dto.Email))
    //     {
    //         throw new ArgumentException("Email is required.", nameof(dto));
    //     }
    //
    //     var user = new User(dto.DisplayName, dto.Email, dto.Phone);
    //     await _userRepository.AddAsync(user, cancellationToken);
    //     await _unitOfWork.SaveChangesAsync(cancellationToken);
    //
    //     return _mapper.Map<UserDto>(user);
    // }

    /// <inheritdoc />
    public async Task<UserDto?> UpdateAsync(
        Guid id,
        UpdateUserDto dto,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(dto);
        
        ValidateOwnership(id, "редактирования");

        var user = await _userRepository.GetByIdAsync(id, cancellationToken);
        if (user is null)
        {
            throw new NotFoundException(nameof(user), id.ToString());
        }

        user.UpdateUser(dto.DisplayName, dto.Email, dto.Phone);

        await _userRepository.UpdateAsync(user, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return _mapper.Map<UserDto>(user);
    }
    
    /// <summary>
    /// Проверяет права доступа к профилю пользователя.
    /// </summary>
    /// <param name="userId">ID пользователя.</param>
    /// <param name="operationName">Название операции для сообщения об ошибке.</param>
    /// <exception cref="UnauthorizedAccessException">Если нет прав доступа.</exception>
    private void ValidateOwnership(Guid userId, string operationName)
    {
        if (!_currentUserService.IsOwnerOrAdmin(userId))
        {
            throw new UnauthorizedAccessException($"У вас нет прав для {operationName} этого профиля.");
        }
    }
}