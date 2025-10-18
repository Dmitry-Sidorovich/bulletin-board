using AutoMapper;
using BulletinBoard.Application.Abstractions;
using BulletinBoard.Application.Contexts.Advertisements.Mapping;
using BulletinBoard.Application.Exceptions;
using BulletinBoard.Contracts.Advertisements;
using BulletinBoard.Contracts.Common;
using BulletinBoard.Domain.Entities;
using BulletinBoard.Domain.ValueObjects;

namespace BulletinBoard.Application.Contexts.Advertisements;

/// <inheritdoc />
public sealed class AdvertisementService : IAdvertisementService
{
    private readonly IAdvertisementRepository _advertisementRepository;
    private readonly IAdvertisementReadRepository _advertisementReadRepository;
    private readonly IAdvertisementFileRepository _advertisementFileRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ICurrentUserService _currentUserService;
    private readonly ICacheService _cacheService;
    
    /// <summary>
    /// Инициализирует экземпляр <see cref="AdvertisementService"/>.
    /// </summary>
    /// <param name="advertisementRepository">Репозиторий для записи объявлений.</param>
    /// <param name="advertisementReadRepository">Репозиторий для чтения объявлений.</param>
    /// <param name="advertisementFileRepository">Репозиторий для работы с файлами.</param>
    /// <param name="unitOfWork">Unit of Work для управления транзакциями.</param>
    /// <param name="mapper">Маппер для преобразования между Domain и DTO.</param>
    /// <param name="currentUserService">Сервис для получения информации о текущем пользователе.</param>
    /// <param name="cacheService">Сервис для кеширования.</param>
    public AdvertisementService(
        IAdvertisementRepository advertisementRepository,
        IAdvertisementReadRepository advertisementReadRepository,
        IAdvertisementFileRepository advertisementFileRepository,
        IUnitOfWork unitOfWork,
        IMapper mapper,
        ICurrentUserService currentUserService,
        ICacheService cacheService)
    {
        _advertisementRepository = advertisementRepository;
        _advertisementReadRepository = advertisementReadRepository;
        _advertisementFileRepository = advertisementFileRepository;
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _currentUserService = currentUserService;
        _cacheService = cacheService;
    }
    
    /// <inheritdoc />
    public Task<AdvertisementDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return _advertisementReadRepository.GetByIdAsync(id, cancellationToken);
    }

    /// <inheritdoc />
    public Task<PagedResult<AdvertisementDto>> GetByCategoryAsync(
        Guid categoryId,
        PageRequest page,
        CancellationToken cancellationToken = default)
    {
        return _advertisementReadRepository.GetByCategoryAsync(categoryId, page, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<AdvertisementDto> CreateAsync(CreateAdvertisementDto dto, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(dto);
        if (string.IsNullOrWhiteSpace(dto.Title))
        {
            throw new ArgumentException("Title is required.", nameof(dto));
        }
        if (dto.CategoryId == Guid.Empty)
        {
            throw new ArgumentException("CategoryId is required.", nameof(dto));
        }
        if (dto.AuthorId == Guid.Empty)
        {
            throw new ArgumentException("AuthorId is required.", nameof(dto));
        }
        if (dto.Contact == null)
        {
            throw new ArgumentException("Contact is required.", nameof(dto));
        }
        
        var contact = new Contact(
            dto.Contact.Name,
            dto.Contact.Email,
            dto.Contact.Phone
        );
        
        var advertisement = new Advertisement(
            dto.Title,
            dto.Description,
            dto.Price,
            dto.CategoryId,
            dto.AuthorId,
            contact
        );
        
        await _advertisementRepository.AddAsync(advertisement, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        
        return _mapper.Map<AdvertisementDto>(advertisement);
    }
    
    /// <inheritdoc />
    public async Task<AdvertisementDto?> UpdateAsync(
        Guid id, 
        UpdateAdvertisementDto dto,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(dto);
        if (string.IsNullOrWhiteSpace(dto.Title))
        {
            throw new ArgumentException("Title is required.", nameof(dto));
        }
        if (dto.CategoryId == Guid.Empty)
        {
            throw new ArgumentException("CategoryId is required.", nameof(dto));
        }
        if (dto.Contact == null)
        {
            throw new ArgumentException("Contact is required.", nameof(dto));
        }
        
        var advertisement = await GetAndValidateOwnershipAsync(id, "редактирования", cancellationToken);
        
        advertisement.UpdateText(dto.Title, dto.Description, dto.Price);
        advertisement.ChangeCategory(dto.CategoryId);
        
        var contact = new Contact(
            dto.Contact.Name,
            dto.Contact.Email,
            dto.Contact.Phone
        );
        advertisement.UpdateContact(contact);
        
        await _advertisementRepository.UpdateAsync(advertisement, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        
        await _cacheService.RemoveAsync($"ad:{id}", cancellationToken);
        
        return _mapper.Map<AdvertisementDto>(advertisement);
    }
    
    /// <inheritdoc />
    public async Task<bool> ChangeStatusAsync(
        Guid id, 
        ChangeAdvertisementStatusDto dto,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(dto);
        
        var advertisement = await GetAndValidateOwnershipAsync(id, "изменения статуса", cancellationToken);
        
        var newStatus = dto.StatusDto.ToDomain();
        if (advertisement.Status == newStatus)
        {
            return true;
        }
        
        advertisement.ChangeStatus(newStatus);
        await _advertisementRepository.UpdateAsync(advertisement, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        await _cacheService.RemoveAsync($"ad:{id}", cancellationToken);
        
        return true;
    }

    /// <inheritdoc />
    public async Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var advertisement = await _advertisementRepository.GetByIdAsync(id, cancellationToken);
        if (advertisement == null)
        {
            return false;
        }

        await _advertisementRepository.DeleteAsync(id, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
            
        await _cacheService.RemoveAsync($"ad:{id}", cancellationToken);

        return true;
    }
    
    /// <inheritdoc />
    public async Task<bool> AttachFileAsync(Guid advertisementId, Guid fileId, CancellationToken cancellationToken = default)
    {
        var adExists = await _advertisementFileRepository.AdvertisementExistsAsync(advertisementId, cancellationToken);
        if (!adExists) return false;
        
        await GetAndValidateOwnershipAsync(advertisementId, "прикрепления файлов к", cancellationToken);

        var fileExists = await _advertisementFileRepository.FileExistsAsync(fileId, cancellationToken);
        if (!fileExists) return false;

        var alreadyAttached = await _advertisementFileRepository.IsFileAttachedAsync(advertisementId, fileId, cancellationToken);
        if (alreadyAttached) return true;

        var maxOrder = await _advertisementFileRepository.GetMaxOrderAsync(advertisementId, cancellationToken);

        var advertisementFile = new AdvertisementFile(advertisementId, fileId, maxOrder + 1);
        await _advertisementFileRepository.AddAsync(advertisementFile, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        await _cacheService.RemoveAsync($"ad:{advertisementId}", cancellationToken);
        
        return true;
    }

    /// <inheritdoc />
    public async Task<bool> DetachFileAsync(Guid advertisementId, Guid fileId, CancellationToken cancellationToken = default)
    {
        await GetAndValidateOwnershipAsync(advertisementId, "открепления файлов от", cancellationToken);
        
        var deleted =  await _advertisementFileRepository.DeleteAsync(advertisementId, fileId, cancellationToken);

        if (deleted)
        {
            await _cacheService.RemoveAsync($"ad:{advertisementId}", cancellationToken);
        }
        return deleted;
    }
    
    /// <summary>
    /// Получает объявление и проверяет права доступа (владелец или админ).
    /// </summary>
    /// <param name="id">ID объявления.</param>
    /// <param name="operationName">Название операции для сообщения об ошибке.</param>
    /// <param name="cancellationToken">Токен отмены.</param>
    /// <returns>Объявление.</returns>
    /// <exception cref="NotFoundException">Если объявление не найдено.</exception>
    /// <exception cref="UnauthorizedAccessException">Если нет прав доступа.</exception>
    private async Task<Advertisement> GetAndValidateOwnershipAsync(
        Guid id,
        string operationName,
        CancellationToken cancellationToken)
    {
        var advertisement = await _advertisementRepository.GetByIdAsync(id, cancellationToken);
        if (advertisement == null)
        {
            throw new NotFoundException(nameof(Advertisement), id.ToString());
        }
        
        if (!_currentUserService.IsOwnerOrAdmin(advertisement.AuthorId))
        {
            throw new UnauthorizedAccessException($"У вас нет прав для {operationName} этого объявления.");
        }

        return advertisement;
    }
    
    /// <inheritdoc />
    public Task<PagedResult<AdvertisementDto>> GetByAuthorAsync(
        Guid authorId,
        PageRequest page,
        CancellationToken cancellationToken = default)
    {
        return _advertisementReadRepository.GetByAuthorAsync(authorId, page, cancellationToken);
    }
    
    /// <inheritdoc />
    public Task<PagedResult<AdvertisementDto>> SearchAsync(
        AdvertisementFilterDto filter, 
        CancellationToken cancellationToken = default)
    {
        return _advertisementReadRepository.SearchAsync(filter, cancellationToken);
    }
}