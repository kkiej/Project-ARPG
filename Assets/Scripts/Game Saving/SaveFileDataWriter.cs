using System;
using System.IO;
using UnityEngine;

namespace LZ
{
    public class SaveFileDataWriter
    {
        public string saveDataDirectoryPath = "";
        public string saveFileName = "";
        
        // 在我们创建一个新的存档文件之前，我们必须检查这个角色槽是否已经存在（最多10个角色槽）
        public bool CheckToSeeIfFileExists()
        {
            if (File.Exists(Path.Combine(saveDataDirectoryPath, saveFileName)))
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        
        // 用于删除角色保存文件
        public void DeleteSaveFile()
        {
            File.Delete(Path.Combine(saveDataDirectoryPath, saveFileName));
        }

        // 用于在开始新游戏时创建存档文件
        public void CreateNewCharacterSaveFile(CharacterSaveData characterData)
        {
            // 创建一个路径来保存文件（在机器上的一个位置）
            string savePath = Path.Combine(saveDataDirectoryPath, saveFileName);

            try
            {
                // 如果还不存在，创建文件将要写入的目录
                Directory.CreateDirectory(Path.GetDirectoryName(savePath));
                Debug.Log("Creating save file, at save path: " + savePath);
                
                // 将C#游戏数据对象序列化为JSON格式
                string dataToStore = JsonUtility.ToJson(characterData, true);
                
                // 将文件写入我们的系统
                using (FileStream stream = new FileStream(savePath, FileMode.Create))
                {
                    using (StreamWriter fileWriter = new StreamWriter(stream))
                    {
                        fileWriter.Write(dataToStore);
                    }
                }
            }
            catch (Exception e)
            {
                Debug.LogError("Error whilst trying to save character data, Game not saved" + savePath + "\n" + e);
            }
        }
        
        // 用于在加载先前游戏时加载存档文件
        public CharacterSaveData LoadSaveFile()
        {
            CharacterSaveData characterData = null;
            
            // 创建一个路径来加载文件（在机器上的一个位置）
            string loadPath = Path.Combine(saveDataDirectoryPath, saveFileName);

            if (File.Exists(loadPath))
            {
                try
                {
                    string dataToLoad = "";

                    using (FileStream stream = new FileStream(loadPath, FileMode.Open))
                    {
                        using (StreamReader reader = new StreamReader(stream))
                        {
                            dataToLoad = reader.ReadToEnd();
                        }
                    }
                
                    // 从Json反序列化数据到Unity
                    characterData = JsonUtility.FromJson<CharacterSaveData>(dataToLoad);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    throw;
                }
                
            }

            return characterData;
        }
    }
}