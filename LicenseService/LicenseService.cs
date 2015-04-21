/*
 *  Dover Framework - OpenSource Development framework for SAP Business One
 *  Copyright (C) 2015  Eduardo Piva
 *
 *  This program is free software: you can redistribute it and/or modify
 *  it under the terms of the GNU General Public License as published by
 *  the Free Software Foundation, either version 3 of the License, or
 *  (at your option) any later version.
 *
 *  This program is distributed in the hope that it will be useful,
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of
 *  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 *  GNU General Public License for more details.
 *
 *  You should have received a copy of the GNU General Public License
 *  along with this program.  If not, see <http://www.gnu.org/licenses/>.
 *  
 *  Contact me at <efpiva@gmail.com>
 * 
 */
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
using Dover.Framework.Model.License;

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

        public string GenerateLicense(LicenseHeader license, RSACryptoServiceProvider key)
        {
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
