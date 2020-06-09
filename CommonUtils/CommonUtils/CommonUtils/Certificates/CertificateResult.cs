using System.Security.Cryptography.X509Certificates;

namespace CommonUtils.Certificates
{
    public class CertificateResult
    {
        public bool Success { get; set; }

        public string Message { get; set; }

        public X509Certificate2Collection Certificates { get; set; }
    }
}
