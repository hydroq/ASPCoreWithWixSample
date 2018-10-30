using Microsoft.AspNetCore.Mvc;
using REFLEKT.ONEAuthor.Application.Helpers;
using System.IO;
using IOFile = System.IO.File;

namespace REFLEKT.ONEAuthor.WebAPI.Controllers
{
    public class ImagesController : Controller
    {
        [HttpGet]
        [Route("/viewer/images/{fileName}")]
        public IActionResult DownloadImage(string fileName)
        {
            string contentPath = PathHelper.GetViewerImagesFolder();
            string targetFullFileName = Path.Combine(contentPath, fileName);

            if (!IOFile.Exists(targetFullFileName))
            {
                return NotFound();
            }

            string mimeType = MimeTypeHelper.GetMimeType(Path.GetExtension(fileName));

            return PhysicalFile(targetFullFileName, mimeType, fileName);
        }
    }
}