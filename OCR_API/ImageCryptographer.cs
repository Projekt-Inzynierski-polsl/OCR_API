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
                imageEncryptionKey = aes.Key;

                aes.IV = new byte[aes.BlockSize / 8];

                using (MemoryStream memoryStream = new MemoryStream())
                {
                    image.SaveAsPng(memoryStream);

                    memoryStream.Seek(0, SeekOrigin.Begin);

                    using (MemoryStream encryptedMemoryStream = new MemoryStream())
                    {
                        using (CryptoStream cryptoStream = new CryptoStream(encryptedMemoryStream, aes.CreateEncryptor(), CryptoStreamMode.Write))
                        {
                            await memoryStream.CopyToAsync(cryptoStream);
                            cryptoStream.FlushFinalBlock();
                        }
                        return (encryptedMemoryStream.ToArray(), imageEncryptionKey);
                    }
                }
            }
       }
        public async Task<byte[]> DecryptImageAsync(byte[] encryptedImage, byte[] hashedImageEncryptionKey)
        {
            byte[] decryptedImage;
            using (Aes aes = Aes.Create())
            {
                aes.IV = new byte[aes.BlockSize / 8];

                using (MemoryStream memoryStream = new MemoryStream(encryptedImage))
                {
                    using (CryptoStream cryptoStream = new CryptoStream(memoryStream, aes.CreateDecryptor(), CryptoStreamMode.Read))
                    {
                        using (MemoryStream decryptedStream = new MemoryStream())
                        {
                            await cryptoStream.CopyToAsync(decryptedStream);
                            decryptedImage = decryptedStream.ToArray();
                        }
                    }
                }

                byte[] decryptedImageEncryptionKey;
                using (SHA256 sha256 = SHA256.Create())
                {
                    decryptedImageEncryptionKey = sha256.ComputeHash(aes.Key);
                    if (!decryptedImageEncryptionKey.SequenceEqual(hashedImageEncryptionKey))
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
