using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Dover.LicenseGenerator;
using System.Xml;
using System.IO;
using System.Security.Cryptography;

namespace Tests
{
    [TestClass]
    public class LicenseTest
    {
        private RSACryptoServiceProvider key;
        private RSACryptoServiceProvider publicKey;

        [TestInitialize]
        public void CreateKey()
        {
            key = CreateKeys.CreateKeyPair();
            publicKey = new RSACryptoServiceProvider();
            publicKey.FromXmlString(key.ToXmlString(false));
        }

        private List<LicenseModule> CreateModulesTemplate()
        {
            List<LicenseModule> modules = new List<LicenseModule>();
            LicenseModule module = new LicenseModule();
            module.Description = "Module 1";
            module.Name = "Module1";
            module.ExpirationDate = new DateTime(2020, 12, 31);
            modules.Add(module);
            module = new LicenseModule();
            module.Description = "Module 2";
            module.Name = "Module2";
            module.ExpirationDate = new DateTime(2015, 12, 31);
            modules.Add(module);

            return modules;
        }

        [TestMethod]
        public void CreateSignedLicenseAndSave()
        {
            // Este teste e usado para gerar um arquivo valido para testes no ambiente de testes.
            List<LicenseModule> modules = new List<LicenseModule>();
            LicenseModule module = new LicenseModule();
            module.Description = "Treinamento";
            module.Name = "Treinamento";
            module.ExpirationDate = new DateTime(2020, 12, 31);
            modules.Add(module);

            LicenseService service = new LicenseService();
            string xml = service.GenerateLicense(modules, key);
            File.WriteAllText(Path.Combine("c:\\temp", "License.xml"), xml);
        }

        [TestMethod]
        public void CreateSignedLicense()
        {
            LicenseService service = new LicenseService();
            string xml = service.GenerateLicense(CreateModulesTemplate(), key);

            XmlDocument doc = new XmlDocument();
            doc.LoadXml(xml);

            var modulesTags = doc.GetElementsByTagName("Module");
            Assert.AreEqual(modulesTags.Count, 2);

            Assert.AreEqual(modulesTags[0].SelectSingleNode("Name").InnerText, "Module1");
            Assert.AreEqual(modulesTags[0].SelectSingleNode("Description").InnerText, "Module 1");
            Assert.AreEqual(modulesTags[0].SelectSingleNode("ExpirationDate").InnerText, "2020-12-31T00:00:00");

            Assert.AreEqual(modulesTags[1].SelectSingleNode("Name").InnerText, "Module2");
            Assert.AreEqual(modulesTags[1].SelectSingleNode("Description").InnerText, "Module 2");
            Assert.AreEqual(modulesTags[1].SelectSingleNode("ExpirationDate").InnerText, "2015-12-31T00:00:00");
        }

        [TestMethod]
        public void TestSignature()
        {
            LicenseService service = new LicenseService();
            string xml = service.GenerateLicense(CreateModulesTemplate(), key);

            Assert.IsTrue(service.CheckSignature(xml, publicKey));
        }

        [TestMethod]
        public void TestWrongSignature()
        {
            LicenseService service = new LicenseService();
            string xml = service.GenerateLicense(CreateModulesTemplate(), key);

            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.PreserveWhitespace = true;
            xmlDoc.LoadXml(xml);


            var modulesTags = xmlDoc.GetElementsByTagName("Module");
            modulesTags[0].SelectSingleNode("//ExpirationDate").InnerText = "2099-12-31T00:00:00";
            Assert.IsFalse(service.CheckSignature(xmlDoc.InnerXml, publicKey));
        }
    }
}
