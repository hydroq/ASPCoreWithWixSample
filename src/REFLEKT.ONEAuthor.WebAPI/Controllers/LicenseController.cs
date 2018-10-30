using Microsoft.AspNetCore.Mvc;
using REFLEKT.ONEAuthor.Application.Licensing;
using REFLEKT.ONEAuthor.Application.Models;
using System.Threading.Tasks;

namespace REFLEKT.ONEAuthor.WebAPI.Controllers
{
    public class LicenseController : Controller
    {
        private readonly LicenseManager _licenseManager;

        public LicenseController(LicenseManager licenseManager)
        {
            _licenseManager = licenseManager;
        }

        [HttpGet]
        [Route("api/License")]
        public async Task<AppLicenseDto> CheckLisense() // TODO: change return type to AppLicenseDto when new UI arrives
        {
            var licenseCheckResult = await _licenseManager.CheckLicense();

            return licenseCheckResult;
        }

        [HttpPost]
        [Route("api/license")]
        public async Task<AppLicenseDto> RequestLicense([FromBody] ApiFormInput formData)
        {
            if (string.IsNullOrWhiteSpace(formData?.key))
            {
                return new AppLicenseDto("License key not provided");
            }

            System.Net.ServicePointManager.ServerCertificateValidationCallback += (o, certificate, chain, errors) => true;

            var licenseObtainResult = await _licenseManager.RequestLicenseOnline(formData.user, formData.key);

            return licenseObtainResult;
        }
    }
}