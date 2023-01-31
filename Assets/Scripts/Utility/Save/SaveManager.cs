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


        public static readonly byte[] EncryptKey = Encoding.UTF8.GetBytes("SA3*FDN&48SDFhuj34VMK34KV~3gd$");
        public static readonly byte[] EncryptIv = Encoding.UTF8.GetBytes("N&48SDFhuj34VMK3");

        static SaveManager()
        {
            Debug.Log(Savefilename);
            Init();
        }
        public static void Init()
        {
#if UNITY_IPHONE
        Environment.SetEnvironmentVariable("MONO_REFLECTION_SERIALIZER", "yes");
#endif
        }
        
        // public static SaveData GetSaveData()
        // {
        //     return _saveData;
        // }
        

        public static void Save(int idx, SaveData saveData)
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
                        new BinaryFormatter().Serialize(cryptoStream, saveData);
                    }

                    fileStream.Close();
                }
            }

            rijn.Clear();
        }

        public static bool Load(int idx, out SaveData saveData)
        {
            _idx = idx;
            if (!File.Exists(Savefilename))
            {
                saveData = null;
                return false;
            }
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
                        saveData = null;
                        Debug.Log("Load 오류 발생");
                        return false;
                    }

                    using (Stream cryptoStream = new CryptoStream(fileStream, decryptor, CryptoStreamMode.Read))
                    {
                        saveData = (SaveData)new BinaryFormatter().Deserialize(cryptoStream);
                    }

                    fileStream.Close();
                }
            }

            rijn.Clear();

            return true;
        }

        public static void Delete(int idx)
        {
            _idx = idx;
            if (File.Exists(Savefilename))
            {
                File.Delete(Savefilename);
            }
        }

        public static bool Has(int idx)
        {
            _idx = idx;
            return File.Exists(Savefilename);
        }
    }
}