using MediatR;

namespace CPPR.API.Use_Cases
{
    public sealed record SaveImage(IFormFile file) : IRequest<string>;

    public class SaveImageHandler(
        IWebHostEnvironment env,
        IHttpContextAccessor httpContextAccessor
    ) : IRequestHandler<SaveImage, string>
    {
        public async Task<string> Handle(SaveImage request, CancellationToken cancellationToken)
        {
            var savePath = Path.Combine(env.WebRootPath, "Images");
            if (!Directory.Exists(savePath))
            {
                Directory.CreateDirectory(savePath);
            }

            var fileName = $"{Guid.NewGuid()}{Path.GetExtension(request.file.FileName)}";
            var fullPath = Path.Combine(savePath, fileName);

            using var stream = new FileStream(fullPath, FileMode.Create);
            await request.file.CopyToAsync(stream, cancellationToken);

            return httpContextAccessor.HttpContext!.Request.Scheme + "://" +
                   httpContextAccessor.HttpContext.Request.Host +
                   "/Images/" + fileName;
        }
    }
}
