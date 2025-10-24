using AutoMapper;
using BulletinBoard.Application.Abstractions;
using BulletinBoard.Application.Constants;
using BulletinBoard.Application.Contexts.Files.Repositories;
using BulletinBoard.Contracts.Files;
using Microsoft.Extensions.Logging;
using File = BulletinBoard.Domain.Entities.File;

namespace BulletinBoard.Application.Contexts.Files.Services;

/// <inheritdoc />
public sealed class FileService : IFileService
{
    private readonly IFileRepository _fileRepository;
    private readonly IFileStorageService _fileStorageService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ILogger<FileService> _logger;

    /// <summary>
    /// Инициализирует сервис для работы с файлами.
    /// </summary>
    public FileService(
        IFileRepository fileRepository,
        IFileStorageService fileStorageService,
        IUnitOfWork unitOfWork,
        IMapper mapper,
        ILogger<FileService> logger)
    {
        _fileRepository = fileRepository;
        _fileStorageService = fileStorageService;
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task<FileUploadResultDto> UploadAsync(
        Stream stream,
        string fileName,
        string contentType,
        long fileSize,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(stream);
        
        if (fileSize <= 0 || fileSize > FileConstants.MaxFileSize)
        {
            throw new ArgumentException($"Размер файла должен быть от 1 байта до {FileConstants.MaxFileSize / 1024 / 1024} MB.", nameof(fileSize));
        }
        
        _logger.LogInformation("Загрузка файла: {FileName}, размер: {FileSize} байт", fileName, fileSize);
        
        var finalContentType = contentType;
        if (string.IsNullOrWhiteSpace(finalContentType))
        {
            var extension = Path.GetExtension(fileName).ToLowerInvariant();
            finalContentType = extension switch
            {
                ".jpg" => "image/jpeg",
                ".jpeg" => "image/jpeg",
                ".png" => "image/png",
                ".webp" => "image/webp",
                _ => "application/octet-stream"
            };
        }
        
        var filePath = await _fileStorageService.SaveFileAsync(stream, fileName, cancellationToken);
        var file = new File(fileName, filePath, finalContentType, fileSize);

        await _fileRepository.AddAsync(file, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        
        _logger.LogInformation("Файл успешно загружен: {FileId}, путь: {FilePath}", file.Id, filePath);

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
        {
            return null;
        }

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
        {
            return null;
        }

        var stream = await _fileStorageService.ReadFileAsync(file.FilePath, cancellationToken);
        if (stream == null)
        {
            return null;
        }

        return (stream, file.FileName, file.ContentType);
    }

    /// <inheritdoc />
    public async Task<bool> DeleteAsync(Guid fileId, CancellationToken cancellationToken = default)
    {
        var file = await _fileRepository.GetByIdAsync(fileId, cancellationToken);
        if (file == null)
        {
            return false;
        }

        await _fileRepository.DeleteAsync(fileId, cancellationToken);
        
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        await _fileStorageService.DeleteFileAsync(file.FilePath, cancellationToken);

        return true;
    }
}