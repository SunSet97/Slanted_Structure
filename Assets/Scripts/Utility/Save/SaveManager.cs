using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Security.Cryptography;
using System.Text;
using Data;
using UnityEngine;
using Task = System.Threading.Tasks.Task;

namespace Utility.Save
{
    public static class SaveManager
    {
        private static readonly string SaveDirectoryPath = $"{Application.persistentDataPath}\\SaveData";
        private static string SaveFilePath(int index) => $"{SaveDirectoryPath}\\saveData{index}.save";
        private static string SaveCoverFilePath(int index) => $"{SaveDirectoryPath}\\saveCoverData{index}.save";


        private static readonly byte[] EncryptKey = Encoding.UTF8.GetBytes("SA3*FDN&48SDFhuj34VMK34KV~3gd$");
        private static readonly byte[] EncryptIv = Encoding.UTF8.GetBytes("N&48SDFhuj34VMK3");

        private static Dictionary<int, SaveData> _saveDatas;
        private static Dictionary<int, SaveCoverData> _saveCoverDatas;

        static SaveManager()
        {
            Debug.Log(SaveDirectoryPath);
            Init();
        }

        private static void Init()
        {
#if UNITY_IPHONE
            Environment.SetEnvironmentVariable("MONO_REFLECTION_SERIALIZER", "yes");
#endif
            _saveDatas = new Dictionary<int, SaveData>();
            _saveCoverDatas = new Dictionary<int, SaveCoverData>();
        }

        public static async Task SaveAsync(int idx, SaveData saveData, Action saveEndAction = null)
        {
            RijndaelManaged rijn = new RijndaelManaged();
            rijn.Mode = CipherMode.ECB;
            rijn.Padding = PaddingMode.Zeros;
            rijn.BlockSize = 256;
            using (ICryptoTransform encryptor = rijn.CreateEncryptor(EncryptKey, EncryptIv))
            {
                using (var fileStream = File.Open(SaveCoverFilePath(idx), FileMode.OpenOrCreate, FileAccess.ReadWrite))
                {
                    try
                    {
                        Stream cryptoStream = new CryptoStream(fileStream, encryptor, CryptoStreamMode.Write);
                        var binaryFormatter = new BinaryFormatter();
                        await Task.Run(() =>
                        {
                            binaryFormatter.Serialize(cryptoStream, saveData.saveCoverData);
                            AddSaveCoverData(idx, saveData.saveCoverData);
                        });

                        Debug.Log("Save Cover Async");

                        cryptoStream.Close();
                    }
                    catch (CryptographicException e)
                    {
                        fileStream.Close();
                        rijn.Clear();
                        saveEndAction?.Invoke();
                        return;
                    }

                    fileStream.Close();
                }

                using (var fileStream = File.Open(SaveFilePath(idx), FileMode.OpenOrCreate, FileAccess.ReadWrite))
                {
                    try
                    {
                        Stream cryptoStream = new CryptoStream(fileStream, encryptor, CryptoStreamMode.Write);
                        var binaryFormatter = new BinaryFormatter();
                        await Task.Run(() =>
                        {
                            binaryFormatter.Serialize(cryptoStream, saveData);
                            AddSaveData(idx, saveData);
                        });
                        Debug.Log("일단 세이브 ");
                        cryptoStream.Close();
                    }
                    catch (CryptographicException e)
                    {
                        fileStream.Close();
                        rijn.Clear();
                        saveEndAction?.Invoke();
                        return;
                    }

                    fileStream.Close();
                }

                rijn.Clear();
            }

            saveEndAction?.Invoke();
        }

        public static async Task LoadAsync(int idx)
        {
            if (!File.Exists(SaveFilePath(idx)) || IsLoaded(idx))
            {
                return;
            }

            RijndaelManaged rijn = new RijndaelManaged();
            rijn.Mode = CipherMode.ECB;
            rijn.Padding = PaddingMode.Zeros;
            rijn.BlockSize = 256;

            using (ICryptoTransform decryptor = rijn.CreateDecryptor(EncryptKey, EncryptIv))
            {
                using (var fileStream = File.Open(SaveFilePath(idx), FileMode.Open))
                {
                    if (fileStream.Length <= 0)
                    {
                        Debug.Log("Load 오류 발생");
                        return;
                    }

                    try
                    {
                        Stream cryptoStream = new CryptoStream(fileStream, decryptor, CryptoStreamMode.Read);
                        var binaryFormatter = new BinaryFormatter();
                        await Task.Run(() =>
                        {
                            for (int i = 0; i < 300000; i++)
                            {
                                Mathf.Pow(i, 3);
                            }

                            var saveData = (SaveData) binaryFormatter.Deserialize(cryptoStream);
                            AddSaveData(idx, saveData);
                            Debug.Log("원본 데이터 로드 111");
                        });
                        Debug.Log("원본 데이터 로드 22");
                        // 불러오는데 걸리는 시간 오류 발생, SaveData를 이중으로 보관하는 방법은 어떤지 (Diary 시각화 용도, 실제 데이터)
                        cryptoStream.Close();
                    }
                    catch (CryptographicException e)
                    {
                        Debug.Log(e);
                        var saveData = new SaveData
                        {
                            saveCoverData = new SaveCoverData
                            {
                                mapCode = "불러오기에 실패"
                            }
                        };
                        AddSaveData(idx, saveData);
                    }

                    fileStream.Close();
                }

                rijn.Clear();
            }
        }

        public static async Task LoadCoverAsync(int idx)
        {
            if (!File.Exists(SaveFilePath(idx)) || IsCoverLoaded(idx))
            {
                return;
            }

            RijndaelManaged rijn = new RijndaelManaged();
            rijn.Mode = CipherMode.ECB;
            rijn.Padding = PaddingMode.Zeros;
            rijn.BlockSize = 256;

            using (ICryptoTransform decryptor = rijn.CreateDecryptor(EncryptKey, EncryptIv))
            {
                using (var fileStream = File.Open(SaveCoverFilePath(idx), FileMode.Open))
                {
                    if (fileStream.Length <= 0)
                    {
                        Debug.Log("Load 오류 발생");
                        return;
                    }

                    try
                    {
                        Stream cryptoStream = new CryptoStream(fileStream, decryptor, CryptoStreamMode.Read);
                        // Debug.Log(Time.time);
                        await Task.Run(() =>
                        {
                            var saveCoverData = (SaveCoverData) new BinaryFormatter().Deserialize(cryptoStream);
                            AddSaveCoverData(idx, saveCoverData);
                            // Debug.Log("111");
                            // for (int i = 0; i < 300000; i++)
                            // {
                            //     Mathf.Pow(i, 3);
                            // }
                            // Debug.Log("하이111");
                        });
                        // Debug.Log(Time.time + "하이222");
                        // 불러오는데 걸리는 시간 오류 발생, SaveData를 이중으로 보관하는 방법은 어떤지 (Diary 시각화 용도, 실제 데이터)
                        cryptoStream.Close();
                    }
                    catch (CryptographicException e)
                    {
                        Debug.Log(e);

                        var saveCoverData = new SaveCoverData
                        {
                            mapCode = "불러오기에 실패"
                        };
                        AddSaveCoverData(idx, saveCoverData);
                    }

                    fileStream.Close();
                }

                rijn.Clear();
            }
        }

        public static void Save(int idx, SaveData saveData, Action saveEndAction = null)
        {
            RijndaelManaged rijn = new RijndaelManaged();
            rijn.Mode = CipherMode.ECB;
            rijn.Padding = PaddingMode.Zeros;
            rijn.BlockSize = 256;
            using (ICryptoTransform encryptor = rijn.CreateEncryptor(EncryptKey, EncryptIv))
            {
                using (var fileStream = File.Open(SaveCoverFilePath(idx), FileMode.OpenOrCreate, FileAccess.ReadWrite))
                {
                    try
                    {
                        Stream cryptoStream = new CryptoStream(fileStream, encryptor, CryptoStreamMode.Write);
                        new BinaryFormatter().Serialize(cryptoStream, saveData.saveCoverData);
                        AddSaveCoverData(idx, saveData.saveCoverData);

                        cryptoStream.Close();
                    }
                    catch (CryptographicException e)
                    {
                        fileStream.Close();
                        rijn.Clear();
                        saveEndAction?.Invoke();
                        return;
                    }

                    fileStream.Close();
                }

                using (var fileStream = File.Open(SaveFilePath(idx), FileMode.OpenOrCreate, FileAccess.ReadWrite))
                {
                    try
                    {
                        Stream cryptoStream = new CryptoStream(fileStream, encryptor, CryptoStreamMode.Write);
                        new BinaryFormatter().Serialize(cryptoStream, saveData);
                        AddSaveData(idx, saveData);
                        cryptoStream.Close();
                    }
                    catch (CryptographicException e)
                    {
                        fileStream.Close();
                        rijn.Clear();
                        saveEndAction?.Invoke();
                        return;
                    }

                    fileStream.Close();
                }

                rijn.Clear();
            }

            saveEndAction?.Invoke();
        }

        public static void Load(int idx)
        {
            if (!File.Exists(SaveFilePath(idx)) || IsLoaded(idx))
            {
                return;
            }

            RijndaelManaged rijn = new RijndaelManaged();
            rijn.Mode = CipherMode.ECB;
            rijn.Padding = PaddingMode.Zeros;
            rijn.BlockSize = 256;

            using (ICryptoTransform decryptor = rijn.CreateDecryptor(EncryptKey, EncryptIv))
            {
                using (var fileStream = File.Open(SaveFilePath(idx), FileMode.Open))
                {
                    if (fileStream.Length <= 0)
                    {
                        Debug.Log("Load 오류 발생");
                        return;
                    }

                    try
                    {
                        Stream cryptoStream = new CryptoStream(fileStream, decryptor, CryptoStreamMode.Read);
                        var saveData = (SaveData) new BinaryFormatter().Deserialize(cryptoStream);

                        AddSaveData(idx, saveData);

                        // 불러오는데 걸리는 시간 오류 발생, SaveData를 이중으로 보관하는 방법은 어떤지 (Diary 시각화 용도, 실제 데이터)
                        cryptoStream.Close();
                    }
                    catch (CryptographicException e)
                    {
                        Debug.Log(e);
                        var saveData = new SaveData
                        {
                            saveCoverData = new SaveCoverData
                            {
                                mapCode = "불러오기에 실패"
                            }
                        };
                        AddSaveData(idx, saveData);
                    }

                    fileStream.Close();
                }

                rijn.Clear();
            }
        }

        public static void LoadCover(int idx)
        {
            if (!File.Exists(SaveCoverFilePath(idx)) || IsCoverLoaded(idx))
            {
                return;
            }

            RijndaelManaged rijn = new RijndaelManaged();
            rijn.Mode = CipherMode.ECB;
            rijn.Padding = PaddingMode.Zeros;
            rijn.BlockSize = 256;

            using (ICryptoTransform decryptor = rijn.CreateDecryptor(EncryptKey, EncryptIv))
            {
                using (var fileStream = File.Open(SaveCoverFilePath(idx), FileMode.Open))
                {
                    if (fileStream.Length <= 0)
                    {
                        Debug.Log("Load 오류 발생");
                        return;
                    }

                    try
                    {
                        Stream cryptoStream = new CryptoStream(fileStream, decryptor, CryptoStreamMode.Read);
                        var saveCoverData = (SaveCoverData) new BinaryFormatter().Deserialize(cryptoStream);
                        AddSaveCoverData(idx, saveCoverData);
                        // 불러오는데 걸리는 시간 오류 발생, SaveData를 이중으로 보관하는 방법은 어떤지 (Diary 시각화 용도, 실제 데이터)
                        cryptoStream.Close();
                    }
                    catch (CryptographicException e)
                    {
                        Debug.Log(e);

                        var saveCoverData = new SaveCoverData
                        {
                            mapCode = "불러오기에 실패"
                        };
                        AddSaveCoverData(idx, saveCoverData);
                    }

                    fileStream.Close();
                }

                rijn.Clear();
            }
        }

        public static void Delete(int idx)
        {
            if (File.Exists(SaveFilePath(idx)))
            {
                File.Delete(SaveFilePath(idx));
            }

            if (File.Exists(SaveCoverFilePath(idx)))
            {
                File.Delete(SaveCoverFilePath(idx));
            }

            if (IsCoverLoaded(idx))
            {
                _saveCoverDatas.Remove(idx);
            }

            if (IsLoaded(idx))
            {
                _saveDatas.Remove(idx);
            }
        }

        public static SaveData GetSaveData(int idx)
        {
            return _saveDatas[idx];
        }

        public static SaveCoverData GetCoverData(int idx)
        {
            return _saveCoverDatas[idx];
        }

        public static bool Has(int idx)
        {
            return File.Exists(SaveFilePath(idx)) && File.Exists(SaveCoverFilePath(idx));
        }

        public static bool IsLoaded(int idx)
        {
            return _saveDatas.ContainsKey(idx);
        }

        public static bool IsCoverLoaded(int idx)
        {
            return _saveCoverDatas.ContainsKey(idx);
        }

        private static void AddSaveCoverData(int idx, SaveCoverData saveCoverData)
        {
            if (IsCoverLoaded(idx))
            {
                _saveCoverDatas[idx] = saveCoverData;
            }
            else
            {
                _saveCoverDatas.Add(idx, saveCoverData);
            }
        }

        private static void AddSaveData(int idx, SaveData saveData)
        {
            Debug.Log($"{idx} index Load");
            if (IsLoaded(idx))
            {
                _saveDatas[idx] = saveData;
            }
            else
            {
                _saveDatas.Add(idx, saveData);
            }
        }
    }
}