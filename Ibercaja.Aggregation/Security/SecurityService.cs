using log4net;
using Meniga.Core.BusinessModels;
using System.Security.Cryptography.X509Certificates;
using System;
using System.Security.Cryptography;
using System.Text;
using System.Xml;
using System.Collections.Generic;

namespace Ibercaja.Aggregation.Security
{
    public class SecurityService : ISecurityService
    {
        private static readonly ILog Logger = LogManager.GetLogger(typeof(SecurityService));
        private readonly string _encryptionFile;
        private readonly string _certificateFile;

        public SecurityService(string encryptionFile, string certificateFile)
        {
            _encryptionFile = encryptionFile;
            _certificateFile = certificateFile;
        }

        public string EncryptValue(string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                Logger.Warn($"Trying to encrypt null or empty string");
                return value;
            }

            var doc = new XmlDocument();
            doc.Load(_encryptionFile);
            var xmlstring = doc.InnerXml;

            using (var rsa = new RSACryptoServiceProvider())
            {
                rsa.FromXmlString(xmlstring);
                return Convert.ToBase64String(rsa.Encrypt(Encoding.UTF8.GetBytes(value), false));
            }
        }

        public string DecryptValue(string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                Logger.Warn($"Trying to decrypt null or empty string");
                return value;
            }

            var doc = new XmlDocument();
            doc.Load(_encryptionFile);
            var xmlstring = doc.InnerXml;

            using (var rsa = new RSACryptoServiceProvider())
            {
                try
                {
                    rsa.FromXmlString(xmlstring);
                    return Encoding.UTF8.GetString(rsa.Decrypt(Convert.FromBase64String(value), false));
                }
                catch (CryptographicException ex)
                {
                    Logger.Error($"Failed to decrypt {value}: {ex.Message}");
                    return value;
                }
            }
        }

        public IEnumerable<Parameter> EncryptCredentials(IEnumerable<Parameter> parameters)
        {
            var certificate = new X509Certificate2(_certificateFile);

            using (var rsa = certificate.GetRSAPublicKey())
            {
                foreach (var p in parameters)
                {
                    // Parameters encrypted and converted to Base64 have a length of 344 characters, so we must skip them
                    if (p.Value.Length != 344)
                    {
                        if (p.Value.Length > 245)
                        {
                            Logger.Error("Cannot encrypt strings longer then 245 characters, so it will be skipped");
                            continue; // because we do not want to include unencrypted values
                        }

                        Logger.Debug($"p.Value.Length of p.Name {p.Name} is {p.Value.Length}");
                        var utf8Bytes = Encoding.UTF8.GetBytes(p.Value);
                        var encryptedBytes = rsa.Encrypt(utf8Bytes, RSAEncryptionPadding.Pkcs1);
                        p.Value = Convert.ToBase64String(encryptedBytes);
                    }

                    yield return p;

                }

            }
        }

        public Parameter EncryptParameter(Parameter parameter)
        {
            var certificate = new X509Certificate2(_certificateFile);

            using (var rsa = certificate.GetRSAPublicKey())
            {
                // Parameters encrypted and converted to Base64 have a length of 344 characters, so we must skip them
                if (parameter.Value.Length != 344)
                {
                    if (parameter.Value.Length > 245)
                    {
                        Logger.Error("Cannot encrypt strings longer then 245 characters, so it will be skipped");
                        return null; // because we do not want to include unencrypted values
                    }

                    Logger.Debug($"parameter.Value.Length of parameter.Name {parameter.Name} is {parameter.Value.Length}");
                    var utf8Bytes = Encoding.UTF8.GetBytes(parameter.Value);
                    var encryptedBytes = rsa.Encrypt(utf8Bytes, RSAEncryptionPadding.Pkcs1);
                    parameter.Value = Convert.ToBase64String(encryptedBytes);
                }

                return parameter;
            }
        }
    }
}