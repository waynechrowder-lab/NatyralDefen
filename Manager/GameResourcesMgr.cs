using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Currency.Core.Run;
using Gameplay.Script.Data;
using Gameplay.Script.Gameplay;
using Script.Core.Tools;
using UnityEngine;
using UnityEngine.Networking;

namespace Gameplay.Script.Manager
{
    public class GameResourcesMgr : MonoSingle<GameResourcesMgr>
    { 
        public List<ZombieAsset> ZombieAssets { get; private set; } = new();
        public List<PlantAsset> PlantAssets { get; private set; } = new();
        public List<LevelAsset> LevelAssets { get; private set; } = new();

        public int GetGameLevelCount => LevelAssets.Count;

        public LevelAsset GetGameLevel(int levelIndex)
            => levelIndex < LevelAssets.Count ? LevelAssets[levelIndex] : LevelAssets[^1];
        
        private IEnumerator Start()
        {
            yield return StartCoroutine(ReadPlantData());
            // var plantIds = PlantData.Instance.GetPlantIds();
            // for (int i = 0; i < plantIds.Count; i++)
            // {
            //     yield return StartCoroutine(ReadPlantLevelData(plantIds[i]));
            //     yield return StartCoroutine(ReadPlantGradeData(plantIds[i]));
            // }
            // Debug.Log($"plantIds:{plantIds.Count}");
            
            // yield return StartCoroutine(ReadLevelData());
            // var levelIds = GameLevelData.Instance.GetLevelIds();
            // for (int i = 0; i < levelIds.Count; i++)
            // {
            //     yield return StartCoroutine(ReadLevelItemData(levelIds[i]));
            // }
            // Debug.Log($"levelIds:{levelIds.Count}");
            
            // yield return StartCoroutine(ReadEnemyData());
            // var enemyIds = EnemyData.Instance.GetEnemyIds();
            // for (int i = 0; i < enemyIds.Count; i++)
            // {
            //     yield return StartCoroutine(ReadEnemyLevelData(enemyIds[i]));
            // }
            // Debug.Log($"enemyIds:{enemyIds.Count}");
            //
            // yield return StartCoroutine(ReadAwardData());
            // yield return StartCoroutine(ReadAchievementData());
            
            // for (int i = 0; i < (int)TreasureType.UltimateTreasure + 1; i++)
            // {
            //     yield return StartCoroutine(ReadTreasureData((TreasureType)i));
            // }
            
            // yield return StartCoroutine(ReadElementData());
            // var elementIds = ElementData.Instance.GetElementIds();
            // for (int i = 0; i < elementIds.Count; i++)
            // {
            //     yield return StartCoroutine(ReadElementLevelData(elementIds[i]));
            // }
            // yield return StartCoroutine(ReadSignInData());
            
            LoadMonster();
            LoadPlant();
            LoadGameLevel();
        }
        
        void LoadGameLevel()
        {
            var res = Resources.LoadAll<LevelAsset>("GameAssets/Level");
            LevelAssets = new List<LevelAsset>(res);
            LevelAssets = LevelAssets.OrderBy(value => value.levelIndex).ToList();
        }

        void LoadMonster()
        {
            var res = Resources.LoadAll<ZombieAsset>("GameAssets/Zombie");
            ZombieAssets = new List<ZombieAsset>(res);
        }
        
        void LoadPlant()
        {
            var res = Resources.LoadAll<PlantAsset>("GameAssets/Plant");
            PlantAssets = new List<PlantAsset>(res);
        }

        IEnumerator ReadPlantData()
        {
            string url = $"{Application.persistentDataPath}/Excel2Json/PlantData.json";
            if (!IOHelper.IsExistFile(url) || Application.platform == RuntimePlatform.WindowsEditor)
            {
                IOHelper.DeleteFolderOrFile(url);
                string originUrl = $"{Application.streamingAssetsPath}/Excel2Json/PlantData.json";
                if (Application.platform == RuntimePlatform.WindowsEditor)
                    originUrl = $"file://{originUrl}";
                using (UnityWebRequest request = UnityWebRequest.Get(originUrl))
                {
                    yield return request.SendWebRequest();
                    if (request.isDone && string.IsNullOrEmpty(request.error))
                    {
                        byte[] data = request.downloadHandler.data;
                        IOHelper.CreateFile(url, data);
                    }
                    else
                        Debug.LogError(request.error);
                }
            }
            if (!IOHelper.IsExistFile(url))
            {
                Debug.LogError($"file not exit : {url}");
                yield break;
            }
            PlantData.ReadDataFromPath(url);
        }

        IEnumerator ReadPlantLevelData(string plantId)
        {
            string url = $"{Application.persistentDataPath}/Excel2Json/Level_{plantId}.json";
            if (!IOHelper.IsExistFile(url) || Application.platform == RuntimePlatform.WindowsEditor)
            {
                IOHelper.DeleteFolderOrFile(url);
                string originUrl = $"{Application.streamingAssetsPath}/Excel2Json/Level_{plantId}.json";
                if (Application.platform == RuntimePlatform.WindowsEditor)
                    originUrl = $"file://{originUrl}";
                using (UnityWebRequest request = UnityWebRequest.Get(originUrl))
                {
                    yield return request.SendWebRequest();
                    if (request.isDone && string.IsNullOrEmpty(request.error))
                    {
                        byte[] data = request.downloadHandler.data;
                        IOHelper.CreateFile(url, data);
                    }
                    else
                        Debug.LogError(request.error);
                }
            }

            if (!IOHelper.IsExistFile(url))
            {
                Debug.LogError($"file not exit : {url}");
                yield break;
            }
            PlantData.ReadLevelFromPath(plantId, url);
        }

        IEnumerator ReadPlantGradeData(string plantId)
        {
            string url = $"{Application.persistentDataPath}/Excel2Json/Grade_{plantId}.json";
            if (!IOHelper.IsExistFile(url) || Application.platform == RuntimePlatform.WindowsEditor)
            {
                IOHelper.DeleteFolderOrFile(url);
                string originUrl = $"{Application.streamingAssetsPath}/Excel2Json/Grade_{plantId}.json";
                if (Application.platform == RuntimePlatform.WindowsEditor)
                    originUrl = $"file://{originUrl}";
                using (UnityWebRequest request = UnityWebRequest.Get(originUrl))
                {
                    yield return request.SendWebRequest();
                    if (request.isDone && string.IsNullOrEmpty(request.error))
                    {
                        byte[] data = request.downloadHandler.data;
                        IOHelper.CreateFile(url, data);
                    }
                    else
                        Debug.LogError(request.error);
                }
            }

            if (!IOHelper.IsExistFile(url))
            {
                Debug.LogError($"file not exit : {url}");
                yield break;
            }
            PlantData.ReadGradeFromPath(plantId, url);
        }
        
        IEnumerator ReadLevelData()
        {
            string url = $"{Application.persistentDataPath}/Excel2Json/LevelData.json";
            if (!IOHelper.IsExistFile(url) || Application.platform == RuntimePlatform.WindowsEditor)
            {
                IOHelper.DeleteFolderOrFile(url);
                string originUrl = $"{Application.streamingAssetsPath}/Excel2Json/LevelData.json";
                if (Application.platform == RuntimePlatform.WindowsEditor)
                    originUrl = $"file://{originUrl}";
                using (UnityWebRequest request = UnityWebRequest.Get(originUrl))
                {
                    yield return request.SendWebRequest();
                    if (request.isDone && string.IsNullOrEmpty(request.error))
                    {
                        byte[] data = request.downloadHandler.data;
                        IOHelper.CreateFile(url, data);
                    }
                    else
                        Debug.LogError(request.error);
                }
            }
            if (!IOHelper.IsExistFile(url))
            {
                Debug.LogError($"file not exit : {url}");
                yield break;
            }
            GameLevelData.ReadDataFormPath(url);
        }
        
        IEnumerator ReadLevelItemData(string levelId)
        {
            string url = $"{Application.persistentDataPath}/Excel2Json/{levelId}.json";
            if (!IOHelper.IsExistFile(url) || Application.platform == RuntimePlatform.WindowsEditor)
            {
                IOHelper.DeleteFolderOrFile(url);
                string originUrl = $"{Application.streamingAssetsPath}/Excel2Json/{levelId}.json";
                if (Application.platform == RuntimePlatform.WindowsEditor)
                    originUrl = $"file://{originUrl}";
                using (UnityWebRequest request = UnityWebRequest.Get(originUrl))
                {
                    yield return request.SendWebRequest();
                    if (request.isDone && string.IsNullOrEmpty(request.error))
                    {
                        byte[] data = request.downloadHandler.data;
                        IOHelper.CreateFile(url, data);
                    }
                    else
                        Debug.LogError(request.error);
                }
            }

            if (!IOHelper.IsExistFile(url))
            {
                Debug.LogError($"file not exit : {url}");
                yield break;
            }
            GameLevelData.ReadLevelItemFromPath(levelId, url);
        }
        
        IEnumerator ReadEnemyData()
        {
            string url = $"{Application.persistentDataPath}/Excel2Json/EnemyData.json";
            if (!IOHelper.IsExistFile(url) || Application.platform == RuntimePlatform.WindowsEditor)
            {
                IOHelper.DeleteFolderOrFile(url);
                string originUrl = $"{Application.streamingAssetsPath}/Excel2Json/EnemyData.json";
                if (Application.platform == RuntimePlatform.WindowsEditor)
                    originUrl = $"file://{originUrl}";
                using (UnityWebRequest request = UnityWebRequest.Get(originUrl))
                {
                    yield return request.SendWebRequest();
                    if (request.isDone && string.IsNullOrEmpty(request.error))
                    {
                        byte[] data = request.downloadHandler.data;
                        IOHelper.CreateFile(url, data);
                    }
                    else
                        Debug.LogError(request.error);
                }
            }
            if (!IOHelper.IsExistFile(url))
            {
                Debug.LogError($"file not exit : {url}");
                yield break;
            }
            EnemyData.ReadDataFormPath(url);
        }
        
        IEnumerator ReadEnemyLevelData(string enemyId)
        {
            string url = $"{Application.persistentDataPath}/Excel2Json/{enemyId}.json";
            if (!IOHelper.IsExistFile(url) || Application.platform == RuntimePlatform.WindowsEditor)
            {
                IOHelper.DeleteFolderOrFile(url);
                string originUrl = $"{Application.streamingAssetsPath}/Excel2Json/{enemyId}.json";
                if (Application.platform == RuntimePlatform.WindowsEditor)
                    originUrl = $"file://{originUrl}";
                using (UnityWebRequest request = UnityWebRequest.Get(originUrl))
                {
                    yield return request.SendWebRequest();
                    if (request.isDone && string.IsNullOrEmpty(request.error))
                    {
                        byte[] data = request.downloadHandler.data;
                        IOHelper.CreateFile(url, data);
                    }
                    else
                        Debug.LogError(request.error);
                }
            }

            if (!IOHelper.IsExistFile(url))
            {
                Debug.LogError($"file not exit : {url}");
                yield break;
            }
            EnemyData.ReadEnemyLevelFromPath(enemyId, url);
        }
        
        IEnumerator ReadAwardData()
        {
            string url = $"{Application.persistentDataPath}/Excel2Json/AwardData.json";
            if (!IOHelper.IsExistFile(url) || Application.platform == RuntimePlatform.WindowsEditor)
            {
                IOHelper.DeleteFolderOrFile(url);
                string originUrl = $"{Application.streamingAssetsPath}/Excel2Json/AwardData.json";
                if (Application.platform == RuntimePlatform.WindowsEditor)
                    originUrl = $"file://{originUrl}";
                using (UnityWebRequest request = UnityWebRequest.Get(originUrl))
                {
                    yield return request.SendWebRequest();
                    if (request.isDone && string.IsNullOrEmpty(request.error))
                    {
                        byte[] data = request.downloadHandler.data;
                        IOHelper.CreateFile(url, data);
                    }
                    else
                        Debug.LogError(request.error);
                }
            }
            if (!IOHelper.IsExistFile(url))
            {
                Debug.LogError($"file not exit : {url}");
                yield break;
            }
            AwardData.ReadDataFormPath(url);
        }
        
        IEnumerator ReadAchievementData()
        {
            string url = $"{Application.persistentDataPath}/Excel2Json/AchievementData.json";
            if (!IOHelper.IsExistFile(url) || Application.platform == RuntimePlatform.WindowsEditor)
            {
                IOHelper.DeleteFolderOrFile(url);
                string originUrl = $"{Application.streamingAssetsPath}/Excel2Json/AchievementData.json";
                if (Application.platform == RuntimePlatform.WindowsEditor)
                    originUrl = $"file://{originUrl}";
                using (UnityWebRequest request = UnityWebRequest.Get(originUrl))
                {
                    yield return request.SendWebRequest();
                    if (request.isDone && string.IsNullOrEmpty(request.error))
                    {
                        byte[] data = request.downloadHandler.data;
                        IOHelper.CreateFile(url, data);
                    }
                    else
                        Debug.LogError(request.error);
                }
            }
            if (!IOHelper.IsExistFile(url))
            {
                Debug.LogError($"file not exit : {url}");
                yield break;
            }
            AchievementData.ReadDataFormPath(url);
        }

        IEnumerator ReadTreasureData(TreasureType treasure)
        {
            string url = $"{Application.persistentDataPath}/Excel2Json/{treasure.ToString()}.json";
            if (!IOHelper.IsExistFile(url) || Application.platform == RuntimePlatform.WindowsEditor)
            {
                IOHelper.DeleteFolderOrFile(url);
                string originUrl = $"{Application.streamingAssetsPath}/Excel2Json/{treasure.ToString()}.json";
                if (Application.platform == RuntimePlatform.WindowsEditor)
                    originUrl = $"file://{originUrl}";
                using (UnityWebRequest request = UnityWebRequest.Get(originUrl))
                {
                    yield return request.SendWebRequest();
                    if (request.isDone && string.IsNullOrEmpty(request.error))
                    {
                        byte[] data = request.downloadHandler.data;
                        IOHelper.CreateFile(url, data);
                    }
                    else
                        Debug.LogError(request.error);
                }
            }
            if (!IOHelper.IsExistFile(url))
            {
                Debug.LogError($"file not exit : {url}");
                yield break;
            }
            TreasureData.ReadDataFormPath(treasure, url);
        }
        
        IEnumerator ReadElementData()
        {
            string url = $"{Application.persistentDataPath}/Excel2Json/ElementData.json";
            if (!IOHelper.IsExistFile(url) || Application.platform == RuntimePlatform.WindowsEditor)
            {
                IOHelper.DeleteFolderOrFile(url);
                string originUrl = $"{Application.streamingAssetsPath}/Excel2Json/ElementData.json";
                if (Application.platform == RuntimePlatform.WindowsEditor)
                    originUrl = $"file://{originUrl}";
                using (UnityWebRequest request = UnityWebRequest.Get(originUrl))
                {
                    yield return request.SendWebRequest();
                    if (request.isDone && string.IsNullOrEmpty(request.error))
                    {
                        byte[] data = request.downloadHandler.data;
                        IOHelper.CreateFile(url, data);
                    }
                    else
                        Debug.LogError(request.error);
                }
            }
            if (!IOHelper.IsExistFile(url))
            {
                Debug.LogError($"file not exit : {url}");
                yield break;
            }
            ElementData.ReadDataFromPath(url);
        }

        IEnumerator ReadElementLevelData(string elementId)
        {
            string url = $"{Application.persistentDataPath}/Excel2Json/{elementId}.json";
            if (!IOHelper.IsExistFile(url) || Application.platform == RuntimePlatform.WindowsEditor)
            {
                IOHelper.DeleteFolderOrFile(url);
                string originUrl = $"{Application.streamingAssetsPath}/Excel2Json/{elementId}.json";
                if (Application.platform == RuntimePlatform.WindowsEditor)
                    originUrl = $"file://{originUrl}";
                using (UnityWebRequest request = UnityWebRequest.Get(originUrl))
                {
                    yield return request.SendWebRequest();
                    if (request.isDone && string.IsNullOrEmpty(request.error))
                    {
                        byte[] data = request.downloadHandler.data;
                        IOHelper.CreateFile(url, data);
                    }
                    else
                        Debug.LogError(request.error);
                }
            }

            if (!IOHelper.IsExistFile(url))
            {
                Debug.LogError($"file not exit : {url}");
                yield break;
            }
            ElementData.ReadLevelFromPath(elementId, url);
        }
        
        IEnumerator ReadSignInData()
        {
            string url = $"{Application.persistentDataPath}/Excel2Json/SignIn.json";
            if (!IOHelper.IsExistFile(url) || Application.platform == RuntimePlatform.WindowsEditor)
            {
                IOHelper.DeleteFolderOrFile(url);
                string originUrl = $"{Application.streamingAssetsPath}/Excel2Json/SignIn.json";
                if (Application.platform == RuntimePlatform.WindowsEditor)
                    originUrl = $"file://{originUrl}";
                using (UnityWebRequest request = UnityWebRequest.Get(originUrl))
                {
                    yield return request.SendWebRequest();
                    if (request.isDone && string.IsNullOrEmpty(request.error))
                    {
                        byte[] data = request.downloadHandler.data;
                        IOHelper.CreateFile(url, data);
                    }
                    else
                        Debug.LogError(request.error);
                }
            }
            if (!IOHelper.IsExistFile(url))
            {
                Debug.LogError($"file not exit : {url}");
                yield break;
            }
            SignInData.ReadDataFromPath(url);
        }
        
    }
}