using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats;
using System.Security.Cryptography;
using System.Text;

namespace OCR_API
{
    public class ImageCryptographer
    {
        public async Task<(byte[], byte[])> EncryptImageAsync(Image image)
        {
            byte[] imageEncryptionKey;
            byte[] hashedImageEncryptionKey;

            using (Aes aes = Aes.Create())
            {
                aes.GenerateKey();
                aes.GenerateIV();
                imageEncryptionKey = aes.Key;

                using (MemoryStream memoryStream = new MemoryStream())
                {
                    image.SaveAsPng(memoryStream);
                    memoryStream.Seek(0, SeekOrigin.Begin);

                    using (MemoryStream encryptedMemoryStream = new MemoryStream())
                    {
                        encryptedMemoryStream.Write(aes.IV, 0, aes.IV.Length);

                        using (CryptoStream cryptoStream = new CryptoStream(encryptedMemoryStream, aes.CreateEncryptor(), CryptoStreamMode.Write))
                        {
                            await memoryStream.CopyToAsync(cryptoStream);
                            cryptoStream.FlushFinalBlock();
                        }

                        using (SHA256 sha256 = SHA256.Create())
                        {
                            hashedImageEncryptionKey = sha256.ComputeHash(imageEncryptionKey);
                        }

                        return (encryptedMemoryStream.ToArray(), hashedImageEncryptionKey);
                    }
                }
            }
        }
        public async Task<byte[]> DecryptImageAsync(byte[] encryptedImage, byte[] hashedImageEncryptionKey)
        {
            if (hashedImageEncryptionKey.Length != 32)
            {
                throw new ArgumentException("Invalid encryption key length. Key must be 256 bits (32 bytes) long.");
            }

            byte[] decryptedImage;
            byte[] imageEncryptionKey;

            using (Aes aes = Aes.Create())
            {
                using (MemoryStream memoryStream = new MemoryStream(encryptedImage))
                {
                    byte[] iv = new byte[aes.BlockSize / 8];
                    memoryStream.Read(iv, 0, iv.Length);
                    aes.IV = iv;

                    using (CryptoStream cryptoStream = new CryptoStream(memoryStream, aes.CreateDecryptor(), CryptoStreamMode.Read))
                    {
                        using (MemoryStream decryptedStream = new MemoryStream())
                        {
                            await cryptoStream.CopyToAsync(decryptedStream);
                            decryptedImage = decryptedStream.ToArray();
                        }
                    }
                }

                using (SHA256 sha256 = SHA256.Create())
                {
                    byte[] computedHashedImageEncryptionKey = sha256.ComputeHash(aes.Key);
                    if (!computedHashedImageEncryptionKey.SequenceEqual(hashedImageEncryptionKey))
                    {
                        throw new Exception("Invalid encryption key.");
                    }
                }
            }

            return decryptedImage;
        }


        public Image ConvertIFormFileToImage(IFormFile formFile)
        {
            using (var imageStream = formFile.OpenReadStream())
            {
                var image = Image.Load(imageStream);

                return image;
            }
        }
    }
}
