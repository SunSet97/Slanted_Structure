using System;
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

        private static void Init()
        {
#if UNITY_IPHONE
        Environment.SetEnvironmentVariable("MONO_REFLECTION_SERIALIZER", "yes");
#endif
        }


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
                    try
                    {
                        Stream cryptoStream = new CryptoStream(fileStream, encryptor, CryptoStreamMode.Write);
                        new BinaryFormatter().Serialize(cryptoStream, saveData);
                    }
                    catch (Exception e)
                    {
                        Debug.Log(e);
                        fileStream.Close();
                        rijn.Clear();
                        Delete(idx);
                        return;
                    }

                    fileStream.Close();
                }

                rijn.Clear();
            }
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

                    try
                    {
                        Stream cryptoStream = new CryptoStream(fileStream, decryptor, CryptoStreamMode.Read);
                        saveData = (SaveData) new BinaryFormatter().Deserialize(cryptoStream);
                        // 불러오는데 걸리는 시간 오류 발생, SaveData를 이중으로 보관하는 방법은 어떤지 (Diary 시각화 용도, 실제 데이터)
                    }
                    catch (Exception e)
                    {
                        Debug.Log(e);
                        saveData = new SaveData
                        {
                            mapCode = "불러오기에 실패"
                        };
                    }
                    
                    fileStream.Close();
                }

                rijn.Clear();
            }

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