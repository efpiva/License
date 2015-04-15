using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Security.Cryptography.Xml;
using System.Xml;
using System.Xml.Serialization;

namespace Dover.LicenseGenerator
{
    public class LicenseService
    {
        private RSACryptoServiceProvider GetKey(string name)
        {
            var resource = GetResourceStream(name);
            string key = null;
            if (resource != null)
            {
                var textReader = new StreamReader(resource);
                key = textReader.ReadToEnd();
            }

            if (key != null)
            {
                var keyRSA = new RSACryptoServiceProvider();
                keyRSA.FromXmlString(key);
                return keyRSA;
            }

            return null;
        }

        private Stream GetResourceStream(string p)
        {
            Stream stream = typeof(LicenseService).Assembly.GetManifestResourceStream(p);
            return stream;
        }

        private bool CheckToken(string assembly, byte[] expectedToken)
        {
            if (assembly == null)
                throw new ArgumentNullException("assembly");
            if (expectedToken == null)
                throw new ArgumentNullException("expectedToken");

            try
            {
                // Get the public key token of the given assembly 
                Assembly asm = Assembly.LoadFrom(assembly);
                byte[] asmToken = asm.GetName().GetPublicKeyToken();

                // Compare it to the given token
                if (asmToken.Length != expectedToken.Length)
                    return false;

                for (int i = 0; i < asmToken.Length; i++)
                    if (asmToken[i] != expectedToken[i])
                        return false;

                return true;
            }
            catch (System.IO.FileNotFoundException)
            {
                // couldn't find the assembly
                return false;
            }
            catch (BadImageFormatException)
            {
                // the given file couldn't get through the loader
                return false;
            }
        }

        public bool CheckSignature(string xml, RSACryptoServiceProvider key)
        {
            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.PreserveWhitespace = true;
            xmlDoc.LoadXml(xml);
            SignedXml signedXml = new SignedXml(xmlDoc);

            StackFrame f = new StackFrame();
            Console.WriteLine(f.ToString());

            XmlNodeList nodeList = xmlDoc.GetElementsByTagName("Signature");

            signedXml.LoadXml((XmlElement)nodeList[0]);
            if (key != null)
                return signedXml.CheckSignature(key);

            return false;
        }

        public string GenerateLicense(List<LicenseModule> modules, RSACryptoServiceProvider key)
        {
            License license = new License();
            license.Items = modules.ToArray();

            var listSerializer = new XmlSerializer(license.GetType());
            var xnameSpace = new XmlSerializerNamespaces();
            xnameSpace.Add("", "");
            var stream = new MemoryStream();

            listSerializer.Serialize(stream, license, xnameSpace);
            
            XmlDocument signedDoc = new XmlDocument();
            stream.Position = 0;
            signedDoc.Load(stream);


            SignedXml signedXml = new SignedXml(signedDoc);
            signedXml.SigningKey = key;

            // Create a reference to be signed.
            Reference reference = new Reference();
            reference.Uri = "";

            // Add an enveloped transformation to the reference.
            XmlDsigEnvelopedSignatureTransform env = new XmlDsigEnvelopedSignatureTransform();
            reference.AddTransform(env);

            // Add the reference to the SignedXml object.
            signedXml.AddReference(reference);

            // Compute the signature.
            signedXml.ComputeSignature();

            // Get the XML representation of the signature and save
            // it to an XmlElement object.
            XmlElement xmlDigitalSignature = signedXml.GetXml();

            // Append the element to the XML document.
            signedDoc.DocumentElement.AppendChild(signedDoc.ImportNode(xmlDigitalSignature, true));

            return signedDoc.InnerXml;
        }

    }
}
