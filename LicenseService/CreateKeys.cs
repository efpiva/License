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
