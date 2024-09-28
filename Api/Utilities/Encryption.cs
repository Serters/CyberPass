using System;
using System.Security.Cryptography;
using Konscious.Security.Cryptography;
using System.Text;

namespace CyberPass.Utilities
{
    public class Encryption
    {
        // Method to encrypt the password using the master key
        public static byte[] EncryptPassword(string password, byte[] masterKey)
        {
            using (Aes aes = Aes.Create())
            {
                aes.Key = masterKey; // Use the master key
                aes.GenerateIV(); // Generate a new IV for encryption

                using (var encryptor = aes.CreateEncryptor(aes.Key, aes.IV))
                {
                    using (var ms = new MemoryStream())
                    {
                        ms.Write(aes.IV, 0, aes.IV.Length); // Prepend the IV to the encrypted data

                        using (var cryptoStream = new CryptoStream(ms, encryptor, CryptoStreamMode.Write))
                        {
                            using (var writer = new StreamWriter(cryptoStream))
                            {
                                writer.Write(password);
                            }
                        }

                        return ms.ToArray(); // Return the encrypted data
                    }
                }
            }
        }

        // Method to decrypt the password using the master key
        public static string DecryptPassword(byte[] encryptedData, byte[] masterKey)
        {
            using (Aes aes = Aes.Create())
            {
                using (var ms = new MemoryStream(encryptedData))
                {
                    byte[] iv = new byte[16]; // Adjust based on your IV size
                    ms.Read(iv, 0, iv.Length); // Read the IV from the beginning of the stream

                    aes.Key = masterKey; // Use the master key
                    aes.IV = iv; // Set the IV

                    using (var decryptor = aes.CreateDecryptor(aes.Key, aes.IV))
                    {
                        using (var cryptoStream = new CryptoStream(ms, decryptor, CryptoStreamMode.Read))
                        {
                            using (var reader = new StreamReader(cryptoStream))
                            {
                                return reader.ReadToEnd(); // Return the decrypted password
                            }
                        }
                    }
                }
            }
        }


    }
}

