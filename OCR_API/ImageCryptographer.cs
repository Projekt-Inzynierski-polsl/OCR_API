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
                        }

                        return (encryptedMemoryStream.ToArray(), imageEncryptionKey);
                    }
                }
            }
        }

        public async Task<byte[]> DecryptImageAsync(byte[] encryptedImage, byte[] encryptionKey)
        {
            if (encryptionKey.Length != 32)
            {
                throw new ArgumentException("Invalid hashed encryption key length. Key must be 256 bits (32 bytes) long.");
            }

            byte[] decryptedImage;

            using (Aes aes = Aes.Create())
            {
                aes.Key = encryptionKey;

                using (MemoryStream memoryStream = new MemoryStream(encryptedImage))
                {
                    byte[] iv = new byte[aes.BlockSize / 8]; // Inicjalizujesz tablicę na IV
                    memoryStream.Read(iv, 0, iv.Length); // Odczytaj IV z zaszyfrowanego obrazu
                    aes.IV = iv; // Ustaw IV w obiekcie AES

                    using (CryptoStream cryptoStream = new CryptoStream(memoryStream, aes.CreateDecryptor(), CryptoStreamMode.Read))
                    {
                        using (MemoryStream decryptedStream = new MemoryStream())
                        {
                            await cryptoStream.CopyToAsync(decryptedStream);
                            decryptedImage = decryptedStream.ToArray();
                        }
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