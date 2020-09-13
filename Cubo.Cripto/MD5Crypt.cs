using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Cubo.Cripto
{
    public class MD5Crypt
    {
        const string senha = "ouy1gh68";
        public async Task<string> Criptografar(string Message)
        {
               string v = await Task.Run(() =>
               {
                   byte[] Results;
                   System.Text.UTF8Encoding UTF8 = new System.Text.UTF8Encoding();
                   using (MD5CryptoServiceProvider HashProvider = new MD5CryptoServiceProvider())
                   {
                       byte[] TDESKey = HashProvider.ComputeHash(UTF8.GetBytes(senha));
                       TripleDESCryptoServiceProvider TDESAlgorithm = new TripleDESCryptoServiceProvider();
                       TDESAlgorithm.Key = TDESKey;
                       TDESAlgorithm.Mode = CipherMode.ECB;
                       TDESAlgorithm.Padding = PaddingMode.PKCS7;
                       byte[] DataToEncrypt = UTF8.GetBytes(Message);
                       try
                       {
                           ICryptoTransform Encryptor = TDESAlgorithm.CreateEncryptor();
                           Results = Encryptor.TransformFinalBlock(DataToEncrypt, 0, DataToEncrypt.Length);
                       }
                       finally
                       {
                           TDESAlgorithm.Clear();
                           HashProvider.Clear();
                       }
                   }
                   return Convert.ToBase64String(Results);
               });
            
               return v;
        }
        public async Task<string> Descriptografar(string Message, string auxMessagem = null)
        {
            string v = await Task.Run(() =>
            {

                if (auxMessagem != Message || auxMessagem == null)
                {
                    byte[] Results;
                    System.Text.UTF8Encoding UTF8 = new System.Text.UTF8Encoding();
                    MD5CryptoServiceProvider HashProvider = new MD5CryptoServiceProvider();
                    byte[] TDESKey = HashProvider.ComputeHash(UTF8.GetBytes(senha));
                    TripleDESCryptoServiceProvider TDESAlgorithm = new TripleDESCryptoServiceProvider();
                    TDESAlgorithm.Key = TDESKey;
                    TDESAlgorithm.Mode = CipherMode.ECB;
                    TDESAlgorithm.Padding = PaddingMode.PKCS7;


                    byte[] DataToDecrypt = Convert.FromBase64String(Message);
                    try
                    {
                        ICryptoTransform Decryptor = TDESAlgorithm.CreateDecryptor();
                        Results = Decryptor.TransformFinalBlock(DataToDecrypt, 0, DataToDecrypt.Length);
                    }

                    finally
                    {
                        TDESAlgorithm.Clear();
                        HashProvider.Clear();
                    }

                    return UTF8.GetString(Results);
                }
                else
                {
                    return Message;
                }
            });
            return v;
        }
    }
}
