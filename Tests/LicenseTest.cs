﻿/*
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
using System.Security.Cryptography;
using System.Xml;
using Dover.Framework.Model.License;
using Dover.LicenseGenerator;
using Microsoft.VisualStudio.TestTools.UnitTesting;

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

        private LicenseHeader CreateModulesTemplate()
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

            LicenseHeader header = new LicenseHeader();
            header.InstallNumber = "123456";
            header.SystemNumber = "54321";
            header.LicenseNamespace = "TEST";
            header.Items = modules;

            return header;
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