using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace Dover.LicenseGenerator
{
    class KeyGeneratorApp
    {
        public static void Main(string[] args)
        {
            if (args.Length != 2)
            {
                Console.WriteLine("Parameters: private_key public_key");
                return;
            }
            
            CreateKeys.CreateKeyPair(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, args[1]),
                                        Path.Combine(AppDomain.CurrentDomain.BaseDirectory, args[0]));
        }
    }
}
