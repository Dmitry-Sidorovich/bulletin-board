using AutoMapper;
using BulletinBoard.Application.Abstractions;
using BulletinBoard.Application.Contexts.Files.Repositories;
using BulletinBoard.Application.Contexts.Files.Services;
using BulletinBoard.Contracts.Files;
using File = BulletinBoard.Domain.Entities.File;

namespace BulletinBoard.Infrastructure.Contexts.Files.Services;

/// <inheritdoc />
public sealed class FileService : IFileService
{
    private readonly IFileRepository _fileRepository;
    private readonly IFileStorageService _fileStorageService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    /// <summary>
    /// Инициализирует сервис для работы с файлами.
    /// </summary>
    public FileService(
        IFileRepository fileRepository,
        IFileStorageService fileStorageService,
        IUnitOfWork unitOfWork,
        IMapper mapper)
    {
        _fileRepository = fileRepository;
        _fileStorageService = fileStorageService;
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    /// <inheritdoc />
    public async Task<FileUploadResultDto> UploadAsync(
        Stream stream,
        string fileName,
        string contentType,
        long fileSize,
        CancellationToken cancellationToken = default)
    {
        var filePath = await _fileStorageService.SaveFileAsync(stream, fileName, cancellationToken);

        var file = new File(fileName, filePath, contentType, fileSize);

        await _fileRepository.AddAsync(file, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new FileUploadResultDto
        {
            FileId = file.Id,
            Url = _fileStorageService.GetFileUrl(filePath)
        };
    }

    /// <inheritdoc />
    public async Task<FileInfoDto?> GetInfoAsync(Guid fileId, CancellationToken cancellationToken = default)
    {
        var file = await _fileRepository.GetByIdAsync(fileId, cancellationToken);
        if (file == null)
            return null;

        return new FileInfoDto
        {
            Id = file.Id,
            FileName = file.FileName,
            Url = _fileStorageService.GetFileUrl(file.FilePath),
            ContentType = file.ContentType,
            FileSize = file.FileSize,
            CreatedAt = file.CreatedAt
        };
    }

    /// <inheritdoc />
    public async Task<(Stream Stream, string FileName, string ContentType)?> DownloadAsync(
        Guid fileId,
        CancellationToken cancellationToken = default)
    {
        var file = await _fileRepository.GetByIdAsync(fileId, cancellationToken);
        if (file == null)
            return null;

        var stream = await _fileStorageService.ReadFileAsync(file.FilePath, cancellationToken);
        if (stream == null)
            return null;

        return (stream, file.FileName, file.ContentType);
    }

    /// <inheritdoc />
    public async Task<bool> DeleteAsync(Guid fileId, CancellationToken cancellationToken = default)
    {
        var file = await _fileRepository.GetByIdAsync(fileId, cancellationToken);
        if (file == null)
            return false;

        await _fileStorageService.DeleteFileAsync(file.FilePath, cancellationToken);
        await _fileRepository.DeleteAsync(fileId, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return true;
    }
}