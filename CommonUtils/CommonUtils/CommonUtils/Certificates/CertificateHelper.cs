using CommonUtils.IO;
using log4net;
using System;
using System.IO;
using System.Security.Cryptography.X509Certificates;

namespace CommonUtils.Certificates
{
    public sealed class CertificateHelper : ICertificateHelper
    {
        private readonly ILog _logger = LogManager.GetLogger(typeof(CertificateHelper));

        private readonly IFileHelper _fileHelper;

        public CertificateHelper(IFileHelper fileHelper)
        {
            _fileHelper = fileHelper ?? throw new ArgumentNullException(nameof(fileHelper));
        }

        public X509Certificate2Collection FindCertificate(
            StoreName storeName,
            StoreLocation storeLocation,
            X509FindType findType,
            string findValue)
        {
            using (var store = new X509Store(storeName, storeLocation))
            {
                store.Open(OpenFlags.ReadOnly | OpenFlags.OpenExistingOnly);
                return store.Certificates.Find(findType, findValue, true);
            }
        }

        public CertificateResult TryFindCertificate(string subjectName)
        {
            var certificateCollection = this.FindCertificate(
                StoreName.My,
                StoreLocation.LocalMachine,
                X509FindType.FindBySubjectName,
                subjectName);

            if (certificateCollection.Count != 1)
            {
                string message = $"Found '{certificateCollection.Count}' certificates for subject name '{subjectName}' when expected 1.";
                _logger.Error(message);
                return new CertificateResult() { Success = false, Message = message };
            }

            return new CertificateResult() { Success = true, Certificates = certificateCollection };
        }

        public CertificateResult TryLoadCertificate(string subjectName, string certificatePath, string certificatePassword = null)
        {
            if (string.IsNullOrEmpty(subjectName))
                throw new ArgumentException("Is null or empty", nameof(subjectName));

            if (string.IsNullOrEmpty(certificatePath))
                throw new ArgumentException("Is null or empty", nameof(certificatePath));

            if (!_fileHelper.FileExists(certificatePath))
            {
                string message = $"Certificate file '{certificatePath}' could not be found.";
                _logger.Error(message);
                return new CertificateResult() { Success = false, Message = message };
            }

            var certificateCollection = new X509Certificate2Collection();
            try
            {
                var certificate = !string.IsNullOrEmpty(certificatePassword)
                    ? new X509Certificate2(File.ReadAllBytes(certificatePath), certificatePassword)
                    : new X509Certificate2(File.ReadAllBytes(certificatePath));

                if (certificate.GetNameInfo(X509NameType.SimpleName, false) != subjectName)
                {
                    string message = $"Certificate '{certificatePath}' is not related to specified subject '{subjectName}'.";
                    _logger.Warn(message);
                    return new CertificateResult() { Success = false, Message = message };
                }
                certificateCollection.Add(certificate);

                return new CertificateResult() { Success = true, Certificates = certificateCollection };
            }
            catch (Exception ex)
            {
                var message = $"An unexpected exception occurred loading certficate '{certificatePath}', error details - {ex.Message}";
                _logger.Error(message);
                return new CertificateResult() { Success = false, Message = message };
            }
        }
    }
}
