using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats;
using System.Security.Cryptography;
using System.Text;

namespace OCR_API
{
    public class ImageCryptographer
    {
        private readonly CryptographySettings cryptographySettings;
        public ImageCryptographer()
        {
            cryptographySettings = new CryptographySettings()
            {
                EncryptionKey = Environment.GetEnvironmentVariable("EncryptionKey"),
            };
        }
        public async Task<(byte[], byte[])> EncryptImageAsync(Image image)
        {
            string encryptionKey = cryptographySettings.EncryptionKey;

            using (Aes aes = Aes.Create())
            {
                aes.Key = Encoding.UTF8.GetBytes(encryptionKey);
                aes.IV = new byte[aes.BlockSize / 8];

                aes.GenerateKey();
                byte[] imageEncryptionKey = aes.Key;

                byte[] encryptedImage;
                using (MemoryStream memoryStream = new MemoryStream())
                {
                    image.SaveAsPng(memoryStream);

                    memoryStream.Seek(0, SeekOrigin.Begin);

                    using (CryptoStream cryptoStream = new CryptoStream(memoryStream, aes.CreateEncryptor(), CryptoStreamMode.Read))
                    {
                        using (MemoryStream encryptedMemoryStream = new MemoryStream())
                        {
                            await cryptoStream.CopyToAsync(encryptedMemoryStream);
                            encryptedImage = encryptedMemoryStream.ToArray();
                        }
                    }
                }

                byte[] hashedImageEncryptionKey;
                using (SHA256 sha256 = SHA256.Create())
                {
                    hashedImageEncryptionKey = sha256.ComputeHash(imageEncryptionKey);
                }

                return (encryptedImage, hashedImageEncryptionKey);
            }
        }

        public async Task<byte[]> DecryptImageAsync(byte[] encryptedImage, byte[] hashedImageEncryptionKey)
        {
            string encryptionKey = cryptographySettings.EncryptionKey;

            byte[] decryptedImage;
            using (Aes aes = Aes.Create())
            {
                aes.Key = Encoding.UTF8.GetBytes(encryptionKey);
                aes.IV = new byte[aes.BlockSize / 8];

                byte[] decryptedImageEncryptionKey;
                using (SHA256 sha256 = SHA256.Create())
                {
                    decryptedImageEncryptionKey = sha256.ComputeHash(aes.Key);
                    if (!decryptedImageEncryptionKey.SequenceEqual(hashedImageEncryptionKey))
                    {
                        throw new Exception("Invalid encryption key.");
                    }
                }

                using (MemoryStream memoryStream = new MemoryStream(encryptedImage))
                using (CryptoStream cryptoStream = new CryptoStream(memoryStream, aes.CreateDecryptor(), CryptoStreamMode.Read))
                {
                    using (MemoryStream decryptedStream = new MemoryStream())
                    {
                        await cryptoStream.CopyToAsync(decryptedStream);
                        decryptedImage = decryptedStream.ToArray();
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
