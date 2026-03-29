// Decompiled with JetBrains decompiler
// Type: CodeBuddy.StringEncryptor
// Assembly: CodeBuddy.Editor, Version=3.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D84C74A1-B0EC-4416-9798-6553CF5A2D81
// Assembly location: C:\Users\Hoang PC\Documents\Idle-rpg\light_fight\Assets\CodeBuddy\Editor\CodeBuddy.Editor.dll

using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

#nullable enable
namespace CodeBuddy
{
  internal class StringEncryptor
  {
    private readonly byte[] key;

    internal StringEncryptor()
      : this("6Muat3WYDU8GxkKeJ1vtkwHWBB6rPMoJ")
    {
    }

    internal StringEncryptor(string key)
    {
      if (key == null)
        throw new ArgumentNullException(nameof (key));
      using (SHA256 shA256 = SHA256.Create())
        this.key = shA256.ComputeHash(Encoding.UTF8.GetBytes(key));
    }

    public string Encrypt(string plainText)
    {
      if (plainText == null)
        throw new ArgumentNullException(nameof (plainText));
      using (Aes aes = Aes.Create())
      {
        aes.Key = this.key;
        aes.GenerateIV();
        byte[] iv = aes.IV;
        using (ICryptoTransform encryptor = aes.CreateEncryptor(aes.Key, iv))
        {
          using (MemoryStream memoryStream = new MemoryStream())
          {
            memoryStream.Write(iv, 0, iv.Length);
            using (CryptoStream cryptoStream = new CryptoStream((Stream) memoryStream, encryptor, CryptoStreamMode.Write))
            {
              using (StreamWriter streamWriter = new StreamWriter((Stream) cryptoStream))
                streamWriter.Write(plainText);
            }
            return Convert.ToBase64String(memoryStream.ToArray());
          }
        }
      }
    }

    public string Decrypt(string encryptedText)
    {
      if (string.IsNullOrEmpty(encryptedText))
        return "";
      byte[] numArray = Convert.FromBase64String(encryptedText);
      using (Aes aes = Aes.Create())
      {
        aes.Key = this.key;
        byte[] destinationArray = new byte[aes.BlockSize / 8];
        Array.Copy((Array) numArray, 0, (Array) destinationArray, 0, destinationArray.Length);
        aes.IV = destinationArray;
        using (ICryptoTransform decryptor = aes.CreateDecryptor(aes.Key, aes.IV))
        {
          using (MemoryStream memoryStream = new MemoryStream(numArray, destinationArray.Length, numArray.Length - destinationArray.Length))
          {
            using (CryptoStream cryptoStream = new CryptoStream((Stream) memoryStream, decryptor, CryptoStreamMode.Read))
            {
              using (StreamReader streamReader = new StreamReader((Stream) cryptoStream))
                return streamReader.ReadToEnd();
            }
          }
        }
      }
    }
  }
}
