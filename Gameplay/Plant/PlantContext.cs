using System.Collections.Generic;
using Gameplay.Script.Data;
using UnityEngine;

namespace Gameplay.Script.Gameplay
{
    [System.Serializable]
    public class PlantContext : CharacterContext
    {
        public PlantAnimator Animator;
        public int Level;
        
        public PlantInherentData PlantInherentData;
        public PlantLevelData PlantLevelData;
        // public AttackSkill RunningSkill;
        // public AttackSkill NextSkill;
        public SkillScheduler Scheduler = new SkillScheduler();
        public void SetPlantData(string plantId)
        {
            var inherentData = PlantData.Instance.GetPlantInherentData(plantId);
            // var levelData = PlantData.Instance.GetPlantLevelData(plantId, 1);
            PlantInherentData = inherentData;
            PlantLevelData = new PlantLevelData
            {
                health = Health,
                level = Level
            };
        }
        
        public bool TryFindNearestEnemy(float range, out ZombieBehaviour zombie)
        {
            zombie = null;
            float distance = range;
            var list = ZombieSystem.Instance.ZombieBehaviours;
            for (int i = 0; i < list.Count; i++)
            {
                if (list[i] != null)
                {
                    Vector3 pos1 = list[i].transform.position;
                    Vector3 pos2 = Transform.position;
                    pos1.y = pos2.y = 0;
                    if (Vector3.Distance(pos1, pos2) <= distance && list[i].ZombieState < ZombieState.Dead)
                    {
                        distance = Vector3.Distance(pos1, pos2);
                        zombie = list[i];
                    }
                }
            }
            return zombie != null;
        }

        public bool TryFindNearbyEnemies(float range, out List<ZombieBehaviour> zombies)
        {
            zombies = new List<ZombieBehaviour>();
            float distance = range;
            var list = ZombieSystem.Instance.ZombieBehaviours;
            for (int i = 0; i < list.Count; i++)
            {
                if (list[i] != null)
                {
                    Vector3 pos1 = list[i].transform.position;
                    Vector3 pos2 = Transform.position;
                    pos1.y = pos2.y = 0;
                    if (Vector3.Distance(pos1, pos2) <= distance && list[i].ZombieState < ZombieState.Dead)
                    {
                        distance = Vector3.Distance(pos1, pos2);
                        zombies.Add(list[i]);
                    }
                }
            }

            return zombies.Count > 0;
        }
        
        public bool OnUpgrade(IntensifyData data)
        {
            Level++;
            if (Level <= 5)
                PlantLevelData.health = (int)(PlantLevelData.health * data.healthK);   
            Health = PlantLevelData.health;
            return Level <= 5;
        }

    }
}