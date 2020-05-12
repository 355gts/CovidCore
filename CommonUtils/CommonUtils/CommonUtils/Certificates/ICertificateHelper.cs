using System.Security.Cryptography.X509Certificates;

namespace CommonUtils.Certificates
{
    public interface ICertificateHelper
    {
        /// <summary>
        /// Finds a certificate in the specified store using the <paramref name="findType"/> and <paramref name="findValue"/>
        /// </summary>
        /// <param name="storeName">The Name of the store to search in</param>
        /// <param name="storeLocation">The Location of the store to search in</param>
        /// <param name="findType">The type of find to perform</param>
        /// <param name="findValue">The value to use when searching</param>
        /// <returns></returns>
        X509Certificate2Collection FindCertificate(
                                        StoreName storeName,
                                        StoreLocation storeLocation,
                                        X509FindType findType,
                                        string findValue);

        /// <summary>
        /// Finds a certificate by subject name in the local machine personal store.
        /// </summary>
        /// <param name="subjectName">The subject name of the certificate to find.</param>
        /// <param name="certificate">The certificate that was found.</param>
        /// <returns>True if a certificate was found, false otherwise.</returns>
        bool TryFindCertificate(string subjectName, out X509Certificate2Collection certificate);
    }
}
