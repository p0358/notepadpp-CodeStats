using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CodeStats
{
    /*class UpdatePerformer
    {
        public UpdatePerformer();
    }*/

    class Updater
    {

        static public bool DeletePendingFiles(string dir)
        {
            return DeleteAllFilesByPattern(dir, "*_todelete_*");
        }

        // https://stackoverflow.com/questions/1344221/how-can-i-generate-random-alphanumeric-strings
        private static Random random = new Random();
        public static string RandomString(int length)
        {
            const string chars = "abcdefghijklmnopqrstuvwxyz0123456789";
            //return new string(Enumerable.Repeat(chars, length)
            //  .Select(s => s[random.Next(s.Length)]).ToArray());
            return new string(Enumerable.Range(1, length).Select(_ => chars[random.Next(chars.Length)]).ToArray());
        }

        static public bool DeleteAllFilesByPattern(string dir, string pattern)
        {
            bool success = true;

            foreach (string f in Directory.EnumerateFiles(dir, pattern))
            {
                try
                {
                    File.Delete(f);
                } catch
                {
                    success = false;
                }
            }

            return success;
        }

        static public bool VerifyFileSignature(string FileToVerifyPath, string base64signature)
        {
            // https://stackoverflow.com/questions/11847666/how-to-verify-a-signature-with-a-public-key-provided-in-pem-file

            RSACryptoServiceProvider RSAVerifier = new RSACryptoServiceProvider();

            // Read public key

            string publicKey = "<RSAKeyValue><Modulus>lq2YGtCRWh4Gt7ucSFQoJ2yyiofYC4AENCW2QtOg7DmuoqKQMLKdluqBdxEr0E93wManDv8PT0eT258YyZiX0GDQQR3JzdBvOOMm/Kmnm5dV0AKdunaONb8/D95WsvcXfLtTAWp4xPnTrvXADeTa/ElYDFT/29Fv2eWmWyxAGzFt3STNRC+OC1X1bQB1hndnJh9+o7fjT+0LljHIrtGeAxhSv2h6O/g5OOmMwZFFvlsK1n+tPiRUtPP+AQnu9lTJSIQ7+b/9HVSCpOH/xde3jZsH8OM0j0rVUoWM8J7IPGMTYKrHqBzYbS23dzLwlRAAKPhow8VgREOWUVnrkDdvqAzpTTfoeEVOY8cj0ckv9XHvM/jt4EHm5ppPVuyCUB6+12ooev7EQdkOffk5xM3vlME7Fp+zOqEgPP6NS8kyRFjnyOGFORYl/qGq5XpDVVIGVWXRjD9ejm/AeyG12MT1YMGXeqx0s5lPxlWslhxzGloTOhrtKxJsrSamxu0u3vbZu5EfBVOBfVdV9ZI10b9omTsi3F5o+y22vlsXe80wkJarFaCm/PyPUo3u5Bx3oXqZo1EJhw7j/WVps3ER2xi1oW++Tsg5NW1g1KywPbiEcj5hubAcxfU6D3XKdpu0EzVdnCQGgPZOFEWOD3o0sTzV3YF0+tF8s4N/L+yos3vmw2U=</Modulus><Exponent>AQAB</Exponent></RSAKeyValue>";

            // Adding public key to RSACryptoServiceProvider object

            RSAVerifier.FromXmlString(publicKey);

            // Reading the Signature to verify

            MemoryStream contents = new MemoryStream(Convert.FromBase64String(base64signature));
            BinaryReader contentsreader = new BinaryReader(contents);
            byte[] SignatureData = contentsreader.ReadBytes((int)contents.Length);

            // Reading the Signed File for Verification

            FileStream Verifyfile = new FileStream(FileToVerifyPath, FileMode.Open, FileAccess.Read);
            BinaryReader VerifyFileReader = new BinaryReader(Verifyfile);
            byte[] VerifyFileData = VerifyFileReader.ReadBytes((int)Verifyfile.Length);

            // Comparing

            bool isValidSignature = RSAVerifier.VerifyData(VerifyFileData, "SHA256", SignatureData);

            //Signature.Close();
            contents.Close();
            Verifyfile.Close();

            return isValidSignature;
        }

        static public void RenameTest()
        {
            //System.IO.File.Move(CodeStatsPackage.CoreAssembly.Location, Path.GetDirectoryName(CodeStatsPackage.CoreAssembly.Location) + "\\CodeStats_renamed.dll");
        }

        static public void SignatureVerificationTest()
        {
            // https://stackoverflow.com/questions/11847666/how-to-verify-a-signature-with-a-public-key-provided-in-pem-file

            RSACryptoServiceProvider RSAVerifier = new RSACryptoServiceProvider();

            // Read public key

            string publicKey = "<RSAKeyValue><Modulus>lq2YGtCRWh4Gt7ucSFQoJ2yyiofYC4AENCW2QtOg7DmuoqKQMLKdluqBdxEr0E93wManDv8PT0eT258YyZiX0GDQQR3JzdBvOOMm/Kmnm5dV0AKdunaONb8/D95WsvcXfLtTAWp4xPnTrvXADeTa/ElYDFT/29Fv2eWmWyxAGzFt3STNRC+OC1X1bQB1hndnJh9+o7fjT+0LljHIrtGeAxhSv2h6O/g5OOmMwZFFvlsK1n+tPiRUtPP+AQnu9lTJSIQ7+b/9HVSCpOH/xde3jZsH8OM0j0rVUoWM8J7IPGMTYKrHqBzYbS23dzLwlRAAKPhow8VgREOWUVnrkDdvqAzpTTfoeEVOY8cj0ckv9XHvM/jt4EHm5ppPVuyCUB6+12ooev7EQdkOffk5xM3vlME7Fp+zOqEgPP6NS8kyRFjnyOGFORYl/qGq5XpDVVIGVWXRjD9ejm/AeyG12MT1YMGXeqx0s5lPxlWslhxzGloTOhrtKxJsrSamxu0u3vbZu5EfBVOBfVdV9ZI10b9omTsi3F5o+y22vlsXe80wkJarFaCm/PyPUo3u5Bx3oXqZo1EJhw7j/WVps3ER2xi1oW++Tsg5NW1g1KywPbiEcj5hubAcxfU6D3XKdpu0EzVdnCQGgPZOFEWOD3o0sTzV3YF0+tF8s4N/L+yos3vmw2U=</Modulus><Exponent>AQAB</Exponent></RSAKeyValue>";

            // Adding public key to RSACryptoServiceProvider object

            RSAVerifier.FromXmlString(publicKey);

            // Reading the Signature to verify

            // openssl dgst -sha256 -sign <private>.pem -out <signature_file> <file_to_sign>

            /*FileStream Signature = new FileStream("C:\\Users\\p0358\\Documents\\GitHub\\notepadpp-CodeStats\\CodeStats\\bin\\Debug\\sha256.sign", FileMode.Open, FileAccess.Read);
            BinaryReader SignatureReader = new BinaryReader(Signature);
            byte[] SignatureData = SignatureReader.ReadBytes((int)Signature.Length);*/

            var signaturebytes = Convert.FromBase64String("UynsSiOo62Ib0cJETQcvfQQFUQRYpNmXPRyhJ3kC0P29Am513NvAp0To7tkC6MTUgzAdp3KiastQwwo9hssUxJsKNnbg0aBhuk3HlfpK+lw6X7cnSQMH8DHDAJprTUkqRW0utzFz+JuaWN7BMgwkwsQGlH+Pi9zHdDdmR1htoYtrtfs3SROzNRJJscAd7ipdrvgfMyHa6Wd0hp5WGTZ4u5GdxBQLyZQUmNRPk0BRy47zqiUzGyQVpa+qgfCdGCe8CJo5F5y4BwHCLf8idxQJwBsTjJ5v5r299+C/+aeFqtAxpj7gLkxTI9IOpAaUmFciLcUyZ7smQmhWFT0KUuTcltcjMjn2k7S66A/E+RiayO/lvrTojBs4TUzc+FSKhhULn/axjFmfWFNd/GdtwxAv5w+5MRtF5efdZZGRT2aHpTTEXYijHcJ20DIj9wx37Yb7eicD4581W/wzZf7BCMJGAVZ1+Unysq4Wj6Du33/S0+ecRHvQVCpBoEgCSbpMs3hX+pDKrRgNeV47+oNwLME05U8kUwG0lf9B97ODnqx1MQQJ7bVodg8SbUqa3DMFl8oCldqQeZHpdEGnIArUyR+AznQ1yI4ATNEebNZ88FclSWiI1/DqRnA8goPiYpl3NUxSYriiUS0PObxdK4+3+MqzdZuJA2FKQwMtZSda6yxKi80=");
            MemoryStream contents = new MemoryStream(signaturebytes);
            BinaryReader contentsreader = new BinaryReader(contents);
            byte[] SignatureData = contentsreader.ReadBytes((int)contents.Length);

            // Reading the Signed File for Verification

            FileStream Verifyfile = new FileStream("C:\\Users\\p0358\\Documents\\codestats_sign\\CodeStats.dll", FileMode.Open, FileAccess.Read);
            BinaryReader VerifyFileReader = new BinaryReader(Verifyfile);
            byte[] VerifyFileData = VerifyFileReader.ReadBytes((int)Verifyfile.Length);

            // Comparing

            bool isValidsignature = RSAVerifier.VerifyData(VerifyFileData, "SHA256", SignatureData);

            if (isValidsignature)
            {
                //Signature.Close();
                contents.Close();
                Verifyfile.Close();
                MessageBox.Show("Signature OK");
            }
            else
            {
                //Signature.Close();
                contents.Close();
                Verifyfile.Close();
                MessageBox.Show("Signature not OK :(");
            }


        }
    }
}
