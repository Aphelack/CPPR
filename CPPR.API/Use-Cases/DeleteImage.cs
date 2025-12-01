using MediatR;

namespace CPPR.API.Use_Cases
{
    public sealed record DeleteImage(string ImageUrl) : IRequest;

    public class DeleteImageHandler(IWebHostEnvironment env) : IRequestHandler<DeleteImage>
    {
        public Task Handle(DeleteImage request, CancellationToken cancellationToken)
        {
            if (string.IsNullOrEmpty(request.ImageUrl))
            {
                return Task.CompletedTask;
            }

            try
            {
                var uri = new Uri(request.ImageUrl);
                var fileName = Path.GetFileName(uri.LocalPath);
                
                // Don't delete the default image
                if (fileName.Equals("noimage.jpg", StringComparison.OrdinalIgnoreCase))
                {
                    return Task.CompletedTask;
                }

                var filePath = Path.Combine(env.WebRootPath, "Images", fileName);

                if (File.Exists(filePath))
                {
                    File.Delete(filePath);
                }
            }
            catch (Exception)
            {
                // Log error or ignore if URL is invalid
            }

            return Task.CompletedTask;
        }
    }
}
