namespace Project.Services.FileService;

public interface IFileService
{
    /// <summary>
    /// Сохраняет файл и возвращает URL сохраненного файла
    /// </summary>
    Task<string> SaveFileAsync(IFormFile file);
}
