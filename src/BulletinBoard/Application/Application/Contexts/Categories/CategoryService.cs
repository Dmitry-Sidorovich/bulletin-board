using AutoMapper;
using BulletinBoard.Application.Abstractions;
using BulletinBoard.Contracts.Categories;
using BulletinBoard.Contracts.Common;
using BulletinBoard.Domain.Entities;

namespace BulletinBoard.Application.Contexts.Categories;

/// <inheritdoc />
public sealed class CategoryService : ICategoryService
{
    private readonly ICategoryRepository _categoryRepository;
    private readonly ICategoryReadRepository _categoryReadRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    /// <summary>
    /// Инициализирует экземпляр <see cref="CategoryService"/>.
    /// </summary>
    /// <param name="categoryRepository">Write-репозиторий доменных категорий.</param>
    /// <param name="categoryReadRepository">Read-репозиторий проекций категорий (DTO).</param>
    /// <param name="unitOfWork">Единица работы для атомарного сохранения.</param>
    /// <param name="mapper">AutoMapper для преобразования Domain→DTO.</param>
    public CategoryService(
        ICategoryRepository categoryRepository,
        ICategoryReadRepository categoryReadRepository,
        IUnitOfWork unitOfWork,
        IMapper mapper)
    {
        _categoryRepository = categoryRepository;
        _categoryReadRepository = categoryReadRepository;
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }
    
    /// <inheritdoc />
    public Task<IReadOnlyList<CategoryDto>> GetRootsAsync(CancellationToken cancellationToken = default)
    {
        return _categoryReadRepository.GetRootsAsync(cancellationToken);
    }

    /// <inheritdoc />
    public Task<PagedResult<CategoryDto>> GetChildrenAsync(
        Guid parentId,
        PageRequest page,
        CancellationToken cancellationToken = default)
    {
        if (parentId == Guid.Empty)
        {
            throw new ArgumentException("ParentId is required.", nameof(parentId));
        }

        return _categoryReadRepository.GetChildrenAsync(parentId, page, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<CategoryDto> CreateRootAsync(string name, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Name is required.", nameof(name));
        }
        

        var category = new Category(name);
        await _categoryRepository.AddAsync(category, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return _mapper.Map<CategoryDto>(category);
    }

    /// <inheritdoc />
    public async Task<CategoryDto> CreateChildAsync(
        Guid parentId, 
        string name, 
        CancellationToken cancellationToken = default)
    {
        if (parentId == Guid.Empty)
        {
            throw new ArgumentException("ParentId is required.", nameof(parentId));
        }

        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Name is required.", nameof(name));
        }
            
        var category = new Category(name, parentId);
        await _categoryRepository.AddAsync(category, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return _mapper.Map<CategoryDto>(category);
    }

    /// <inheritdoc />
    public async Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var category = await _categoryRepository.GetByIdAsync(id, cancellationToken);
        if (category is null)
        {
            return false;
        }

        await _categoryRepository.DeleteAsync(id, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return true;
    }
}