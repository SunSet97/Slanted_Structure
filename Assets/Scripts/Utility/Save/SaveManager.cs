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

        private static string SaveFilePath => $"{Application.persistentDataPath}/saveData{_idx}.save";
        private static string SaveCoverFilePath => $"{Application.persistentDataPath}/saveCoverData{_idx}.save";


        public static readonly byte[] EncryptKey = Encoding.UTF8.GetBytes("SA3*FDN&48SDFhuj34VMK34KV~3gd$");
        public static readonly byte[] EncryptIv = Encoding.UTF8.GetBytes("N&48SDFhuj34VMK3");

        static SaveManager()
        {
            Debug.Log(SaveFilePath);
            Init();
        }

        private static void Init()
        {
#if UNITY_IPHONE
        Environment.SetEnvironmentVariable("MONO_REFLECTION_SERIALIZER", "yes");
#endif
        }


        public static void Save(int idx, SaveData saveData, Action saveEndAction = null)
        {
            _idx = idx;
            RijndaelManaged rijn = new RijndaelManaged();
            rijn.Mode = CipherMode.ECB;
            rijn.Padding = PaddingMode.Zeros;
            rijn.BlockSize = 256;
            using (ICryptoTransform encryptor = rijn.CreateEncryptor(EncryptKey, EncryptIv))
            {
                using (var fileStream = File.Create(SaveCoverFilePath))
                {
                    try
                    {
                        Stream cryptoStream = new CryptoStream(fileStream, encryptor, CryptoStreamMode.Write);
                        new BinaryFormatter().Serialize(cryptoStream, saveData.saveCoverData);
                        cryptoStream.Close();
                    }
                    catch (Exception e)
                    {
                        fileStream.Close();
                        rijn.Clear();
                        DeleteCover(idx);
                        saveEndAction?.Invoke();
                        return;
                    }
                    fileStream.Close();
                }
                
                using (var fileStream = File.Create(SaveFilePath))
                {
                    try
                    {
                        Stream cryptoStream = new CryptoStream(fileStream, encryptor, CryptoStreamMode.Write);
                        new BinaryFormatter().Serialize(cryptoStream, saveData);
                        Debug.Log("일단 세이브 ");
                        cryptoStream.Close();
                    }
                    catch (Exception e)
                    {
                        fileStream.Close();
                        rijn.Clear();
                        Delete(idx);
                        saveEndAction?.Invoke();
                        return;
                    }
                    
                    fileStream.Close();
                }

                rijn.Clear();
            }
            saveEndAction?.Invoke();
        }

        public static bool Load(int idx, out SaveData saveData)
        {
            _idx = idx;
            if (!File.Exists(SaveFilePath))
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
                using (var fileStream = File.Open(SaveFilePath, FileMode.Open))
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
                        cryptoStream.Close();
                    }
                    catch (Exception e)
                    {
                        Debug.Log(e);
                        saveData = new SaveData
                        {
                            saveCoverData = new SaveCoverData
                            {
                                mapCode = "불러오기에 실패"
                            }
                        };
                    }
                    fileStream.Close();
                }

                rijn.Clear();
            }

            return true;
        }
        
        public static bool LoadCover(int idx, out SaveCoverData saveCoverData)
        {
            _idx = idx;
            if (!File.Exists(SaveFilePath))
            {
                saveCoverData = null;
                return false;
            }

            RijndaelManaged rijn = new RijndaelManaged();
            rijn.Mode = CipherMode.ECB;
            rijn.Padding = PaddingMode.Zeros;
            rijn.BlockSize = 256;

            using (ICryptoTransform decryptor = rijn.CreateDecryptor(EncryptKey, EncryptIv))
            {
                using (var fileStream = File.Open(SaveCoverFilePath, FileMode.Open))
                {
                    if (fileStream.Length <= 0)
                    {
                        saveCoverData = null;
                        Debug.Log("Load 오류 발생");
                        return false;
                    }

                    try
                    {
                        Stream cryptoStream = new CryptoStream(fileStream, decryptor, CryptoStreamMode.Read);
                        saveCoverData = (SaveCoverData)new BinaryFormatter().Deserialize(cryptoStream);
                        // 불러오는데 걸리는 시간 오류 발생, SaveData를 이중으로 보관하는 방법은 어떤지 (Diary 시각화 용도, 실제 데이터)
                        cryptoStream.Close();
                    }
                    catch (Exception e)
                    {
                        Debug.Log(e);

                        saveCoverData = new SaveCoverData
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
            if (File.Exists(SaveFilePath))
            {
                File.Delete(SaveFilePath);
            }
            DeleteCover(idx);
        }
        
        public static void DeleteCover(int idx)
        {
            _idx = idx;
            if (File.Exists(SaveCoverFilePath))
            {
                File.Delete(SaveCoverFilePath);
            }
        }

        public static bool Has(int idx)
        {
            _idx = idx;
            return File.Exists(SaveFilePath);
        }
    }
}