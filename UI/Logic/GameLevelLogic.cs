using System;
using System.Collections.Generic;
using System.Linq;
using Currency.Core.Run;
using Gameplay.Script.Data;
using Gameplay.Script.Gameplay;
using Gameplay.Script.Manager;
using Gameplay.Script.MiniWorldGameplay;
using UnityEngine;
using UnityEngine.Events;

namespace Gameplay.Script.Logic
{
    public class GameLevelLogic : Single<GameLevelLogic>
    {
        private GameLevelInherentData _selectedInherentData;
        private List<GameLevelItemData> _selectedDatas;
        public GameLevelInherentData SelectedInherentData => _selectedInherentData;
        private List<int> _userBagPlants = null;

        private UnityEvent _bagItemChangedDispatcher = new();

        QuickGameMode _quickGameMode = QuickGameMode.Normal;
        public QuickGameMode QuickGameMode => _quickGameMode;
        public void SetQuickGameMode(QuickGameMode mode) => _quickGameMode = mode;

        public void RegisterOnBagItemChanged(UnityAction callback)
        {
            _bagItemChangedDispatcher.AddListener(callback);
        }
        
        public void UnRegisterOnBagItemChanged(UnityAction callback)
        {
            _bagItemChangedDispatcher.RemoveListener(callback);
        }
        
        public List<string> GetSingleGameLevels()
        {
            return GameLevelData.Instance.GetSingleLevelIds();
        }

        public GameLevelInherentData GetGameLevel(string gameLevel)
        {
            var data = GameLevelData.Instance.GetGameLevel(gameLevel);
            return data;
        }

        public void SetSelectedGameLevel(GameLevelInherentData data)
        {
            _selectedInherentData = data;
            if (_selectedInherentData == null) return;
            _selectedDatas = GameLevelData.Instance.GetGameLevelItem(data.id);
        }

        public List<string> GetGameEnemies()
        {
            List<string> list = new();
            if (_selectedDatas is { Count: > 0 })
            {
                _selectedDatas.ForEach(value =>
                {
                    list.AddRange(value.waveEnemyIds);
                });
                list = list.Distinct().ToList();
                list.Sort();
                list.Reverse();
            }
            return list;
        }

        public int GetGameLevelEnemyLevel(string enemyId)
        {
            int level = 0;
            if (_selectedDatas is { Count: > 0 })
            {
                List<int> levs = new();
                List<string> ids = new();
                _selectedDatas.Select(value => value.waveEnemyLevs).ToList().
                    ForEach(value => levs.AddRange(value));
                _selectedDatas.Select(value => value.waveEnemyIds).ToList().
                    ForEach(value => ids.AddRange(value));
                for (int i = 0; i < levs.Count; i++)
                {
                    if (i < ids.Count && enemyId == ids[i])
                    {
                        level = level < levs[i] ? levs[i] : level;
                    }
                }
            }
            return level;
        }

        public EnemyLevelData GetLevelEnemy(string enemyId, int level)
        {
            return EnemyData.Instance.GetEnemyLevel(enemyId, level);
        }
        
        public EnemyInherentData GetEnemy(string enemyId)
        {
            return EnemyData.Instance.GetEnemy(enemyId);
        }
        
        public List<string> GetGameAwards()
        {
            List<string> list = new();
            if (_selectedDatas is { Count: > 0 })
            {
                _selectedDatas.ForEach(value =>
                {
                    list.AddRange(value.waveDropOutItemIds);
                });
                list = list.Distinct().ToList();
                list.Sort();
                list.Reverse();
            }
            return list;
        }
        
        public AwardInherentData GetAward(string enemyId)
        {
            return AwardData.Instance.GetAward(enemyId);
        }

        public bool StartGameLevel(bool mr)
        {
            //todo: start game
            Debug.Log("Start GameLevel");
            var list = GetUserGameBags();
            if (list == null || list.Count == 0)
            {
                return false;
            }

            Debug.Log($"UserGameBags:{list.Count}");
            int count1 = 0;
            int count2 = 0;
            foreach (var item in list)
            {
                if (item < 0) continue;
                var userPlant = PlantUpgradeLogic.Instance.GetUserPlantData(item);
                if (userPlant == null) continue;
                var plantData = PlantUpgradeLogic.Instance.GetPlantData(userPlant.plantId);
                if (plantData == null) continue;
                Enum.TryParse<PlantType>(plantData.type, out var plantType);
                Debug.Log($"plant:{userPlant.plantId},type:{plantType}");
                if (plantType == PlantType.资源)
                    count1++;
                else
                    count2++;
            }

            if (count1 == 0 || count2 == 0)
            {
                return false;
            }
            if (mr)
            {
                GameplayLogic.Instance.EnterMRGameplay();
                return true;
            }
            // Debug.Log(string.Join(",", _userBagPlants));
            GameplayLogic.Instance.SetGameLevelData(_selectedInherentData, _selectedDatas);
            GameplayLogic.Instance.EnterGameplay();
            return true;
        }
        
        public List<int> GetUserGameBags()
        {
            Debug.Log($"GetUserGameBags:{_quickGameMode}");
            _userBagPlants = UserDataManager.Instance.GetBagItems(_quickGameMode);
            return _userBagPlants;
        }

        public List<UserPlantData> GetUserPlants()
        {
            return UserDataManager.Instance.GetUserPlants();
        }

        public void RemoveBagPlant(int userPlantId)
        {
            Debug.Log($"RemoveBagPlant:{_index}");
            if (_index < _userBagPlants.Count && _index >= 0) 
                _userBagPlants[_index] = -1;
            else
                _userBagPlants.Remove(userPlantId);
            UserDataManager.Instance.ChangeBagItems(_quickGameMode, _userBagPlants);
        }
        
        public void AddBagPlant(int userPlantId)
        {
            Debug.Log($"AddBagPlant:{_index}");
            if (_userBagPlants.Count <= 5 && !_userBagPlants.Contains(userPlantId))
            {
                if (_index < _userBagPlants.Count && _index >= 0) 
                    _userBagPlants[_index] = userPlantId;
                else
                {
                    while (_userBagPlants.Count < _index)
                        _userBagPlants.Add(-1);
                    _userBagPlants.Add(userPlantId);
                }
                UserDataManager.Instance.ChangeBagItems(_quickGameMode, _userBagPlants);
                _bagItemChangedDispatcher?.Invoke();
                // EventDispatcher.Instance.Dispatch((int)EventID.BagItemChanged);
            }
        }
        
        public UserElement GetUserElementDataList(int userPlantId)
        {
            var elementPlants = UserDataManager.Instance.UserDataJson?.userPlants ??
                                Array.Empty<UserPlantData>();
            var elementPlant = elementPlants.ToList().
                FirstOrDefault(value => value.userPlantId.Equals(userPlantId));
            var element = elementPlant?.elementType ?? new UserElement();
            return element;
        }

        private int _index = -1;
        public void SetIndex(int index)
        {
            _index = index;
        }
    }
}