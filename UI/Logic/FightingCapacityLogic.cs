using Currency.Core.Run;

namespace Gameplay.Script.Logic
{
    public class FightingCapacityLogic : Single<FightingCapacityLogic>
    {
        public static int GetFightingCapacity(int baseAttack, int baseLevel, int baseGrade, int skill = 0, int attribute = 0)
        {
            int result = 0;
            result = baseAttack + baseLevel * 15 + baseGrade * 100 + skill + attribute;
            return result;
        }
    }
}