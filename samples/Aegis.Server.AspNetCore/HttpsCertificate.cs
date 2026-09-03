using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;

namespace Aegis.Server.AspNetCore;

/// <summary>
/// Ensures the Production HTTPS PFX exists under ProgramData (same path as appsettings.Production.json).
/// </summary>
public static class HttpsCertificate
{
    public const string DefaultPassword = "aegis-https";

    /// <summary>
    /// Creates a self-signed PFX at <see cref="ServicePaths.DefaultHttpsCertificatePath"/> when missing.
    /// Password must match <c>Kestrel:Endpoints:Https:Certificate:Password</c>.
    /// </summary>
    public static void EnsureExists(string password = DefaultPassword)
    {
        var path = ServicePaths.DefaultHttpsCertificatePath;
        if (File.Exists(path))
            return;

        Directory.CreateDirectory(ServicePaths.DataDirectory);

        using var rsa = RSA.Create(2048);
        var request = new CertificateRequest(
            "CN=Aegis Licencing Server",
            rsa,
            HashAlgorithmName.SHA256,
            RSASignaturePadding.Pkcs1);

        request.CertificateExtensions.Add(
            new X509BasicConstraintsExtension(false, false, 0, false));
        request.CertificateExtensions.Add(
            new X509SubjectKeyIdentifierExtension(request.PublicKey, false));

        var san = new SubjectAlternativeNameBuilder();
        san.AddDnsName("localhost");
        san.AddDnsName(Environment.MachineName);
        request.CertificateExtensions.Add(san.Build());

        using var certificate = request.CreateSelfSigned(
            DateTimeOffset.UtcNow.AddDays(-1),
            DateTimeOffset.UtcNow.AddYears(5));

        var pfx = certificate.Export(X509ContentType.Pfx, password);
        File.WriteAllBytes(path, pfx);
    }
}
