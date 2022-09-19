using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Security.Cryptography;
using System.Text;
using Data;
using UnityEngine;

namespace Utility.Save
{
    public static class SaveManager
    {
        private static int _idx;

        private static string Savefilename => $"{Application.persistentDataPath}/saveData{_idx}.save";

        private static SaveData _saveData;

        public static readonly byte[] EncryptKey = Encoding.UTF8.GetBytes("abcdefg_abcdefg_");
        public static readonly byte[] EncryptIv = Encoding.UTF8.GetBytes("abcdefg_");


        public static void Init()
        {
            _saveData = new SaveData();
#if UNITY_IPHONE
        Environment.SetEnvironmentVariable("MONO_REFLECTION_SERIALIZER", "yes");
#endif
        }

        public static SaveData GetSaveData()
        {
            return _saveData;
        }

        public static void Save(int idx)
        {
            _idx = idx;
            RijndaelManaged rijn = new RijndaelManaged();
            rijn.Mode = CipherMode.ECB;
            rijn.Padding = PaddingMode.Zeros;
            rijn.BlockSize = 256;
            using (ICryptoTransform encryptor = rijn.CreateEncryptor(EncryptKey, EncryptIv))
            {
                using (var fileStream = File.Create(Savefilename))
                {
                    using (Stream cryptoStream = new CryptoStream(fileStream, encryptor, CryptoStreamMode.Write))
                    {
                        new BinaryFormatter().Serialize(cryptoStream, _saveData);
                    }

                    fileStream.Close();
                }
            }

            rijn.Clear();
        }

        public static bool Load(int idx)
        {
            Debug.Log(idx);
            _idx = idx;
            Debug.Log(Savefilename);
            if (!File.Exists(Savefilename)) return false;
            RijndaelManaged rijn = new RijndaelManaged();
            rijn.Mode = CipherMode.ECB;
            rijn.Padding = PaddingMode.Zeros;
            rijn.BlockSize = 256;

            using (ICryptoTransform decryptor = rijn.CreateDecryptor(EncryptKey, EncryptIv))
            {
                using (var fileStream = File.Open(Savefilename, FileMode.Open))
                {
                    if (fileStream.Length <= 0)
                    {
                        return false;
                    }

                    using (Stream cryptoStream = new CryptoStream(fileStream, decryptor, CryptoStreamMode.Read))
                    {
                        _saveData = (SaveData)new BinaryFormatter().Deserialize(cryptoStream);

                    }

                    fileStream.Close();
                }
            }

            rijn.Clear();

            return true;
        }
    }
}