namespace PopaDin.ExportService.Interfaces;

public interface IBlobStorageService
{
    Task<string> UploadPdfAsync(byte[] pdfContent, int userId);
}
