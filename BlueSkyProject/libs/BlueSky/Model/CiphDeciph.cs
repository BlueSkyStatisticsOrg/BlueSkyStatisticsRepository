using System;
using System.Text;
using System.Security.Cryptography;

namespace BlueSky.Model
{
    public class CiphDeciph
    {

        public string RijndaelCipherText(string plaintext)//
        {
            string key = "jhIBty*&%98BjKMhgu#@$_)mJEDYT%$J"; //32byte
            string iv = "jHYjnUyt435#$VHJ"; //16byte
            //RijndaelManaged
            byte[] plainbytes = UTF8Encoding.UTF8.GetBytes(plaintext); 
            RijndaelManaged rdm = new RijndaelManaged();
            rdm.BlockSize = 128;
            rdm.KeySize = 256;
            ICryptoTransform ict = rdm.CreateEncryptor(UTF8Encoding.UTF8.GetBytes(key), UTF8Encoding.UTF8.GetBytes(iv));//
            byte[] encryptedbytearr = ict.TransformFinalBlock(plainbytes, 0, plainbytes.Length);
            ict.Dispose();
            rdm.Dispose();
            return (Convert.ToBase64String(encryptedbytearr));
        }

        //Better remove it so that no can even use reflection on this to decode.
        //public string RijndaelDecipherText(string encryptedtext)//
        //{
        //    string key = "jhIBty*&%98BjKMhgu#@$_)mJEDYT%$J"; //32byte
        //    string iv = "jHYjnUyt435#$VHJ"; //16byte
        //    //RijndaelManaged
        //    byte[] encryptedbytes = Convert.FromBase64String(encryptedtext);
        //    RijndaelManaged rdm = new RijndaelManaged();
        //    rdm.BlockSize = 128;
        //    rdm.KeySize = 256;
        //    ICryptoTransform ict = rdm.CreateDecryptor(UTF8Encoding.UTF8.GetBytes(key), UTF8Encoding.UTF8.GetBytes(iv));//
        //    byte[] encryptedbytearr = ict.TransformFinalBlock(encryptedbytes, 0, encryptedbytes.Length);
        //    ict.Dispose();
        //    rdm.Dispose();
        //    return (UTF8Encoding.UTF8.GetString(encryptedbytearr));
        //}

    }
}
