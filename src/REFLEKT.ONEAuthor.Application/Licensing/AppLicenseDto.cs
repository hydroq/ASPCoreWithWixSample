namespace REFLEKT.ONEAuthor.Application.Licensing
{
    public class AppLicenseDto
    {
        public AppLicense License { get; private set; }

        public bool IsValid { get; private set; }

        public string ErrorMessage { get; private set; }

        public AppLicenseDto(string errorMessage)
        {
            IsValid = false;
            ErrorMessage = errorMessage;
        }

        public AppLicenseDto(AppLicense license, bool isValid)
        {
            IsValid = isValid;
            License = license;
        }
    }
}