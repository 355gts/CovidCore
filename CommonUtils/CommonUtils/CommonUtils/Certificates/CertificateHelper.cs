using System.Security.Cryptography.X509Certificates;

namespace CommonUtils.Certificates
{
    public sealed class CertificateHelper : ICertificateHelper
    {
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

        public bool TryFindCertificate(string subjectName, out X509Certificate2Collection certificate)
        {
            certificate = this.FindCertificate(
                StoreName.My, 
                StoreLocation.LocalMachine, 
                X509FindType.FindBySubjectName, 
                subjectName);

            return certificate.Count == 1;
        }
    }
}
