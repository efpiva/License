using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Security.Cryptography;
using System.IO;
using System.Diagnostics;

namespace Dover.LicenseGenerator
{
    public static class CreateKeys
    {
        public static RSACryptoServiceProvider CreateKeyPair()
        {
            CspParameters parameters = new CspParameters();

            // Create a new CspParameters object to specify
            // a key container.
            CspParameters cspParams = new CspParameters();
            cspParams.KeyContainerName = "XML_DSIG_RSA_KEY";

            // Create a new RSA signing key and save it in the container. 
            return new RSACryptoServiceProvider(2048, cspParams);
        }

        public static void CreateKeyPair(string publicKeyPath, string privateKeyPath)
        {
            RSACryptoServiceProvider privateKey;
            privateKey = CreateKeyPair();

            string privateKeyXml = privateKey.ToXmlString(true);
            string publicKeyXml = privateKey.ToXmlString(false);

            Debug.Assert(privateKey.KeySize == 2048);

            File.WriteAllText(publicKeyPath, publicKeyXml);
            File.WriteAllText(privateKeyPath, privateKeyXml);
        }
    }
}
