using System.Collections.Generic;
using System.Linq;
using Currency.Core.Run;
using Gameplay.Script.Data;
using Gameplay.Script.Manager;

namespace Gameplay.Script.Logic
{
    public class AchievementLogic : Single<AchievementLogic>
    {
        public List<string> GetAllAchievementIds()
        {
            return AchievementData.Instance.GetAchievementIds();
        }

        public AchievementInherentData GetAchievement(string id)
        {
            return AchievementData.Instance.GetAchievement(id);
        }
        
        public List<string> GetUserAchievements()
        {
            List<string> list = new();
            // list = UserDataManager.Instance.UserDataJson?.userPlantJson?.userAchievements?.ToList() ?? new List<string>();
            return list;
        }

        public void GetNewAchievement(string id)
        {
            
        }
    }
}