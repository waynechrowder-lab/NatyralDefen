namespace Gameplay.Script.Gameplay
{
    public class SkillScheduler {
        private AttackSkill _running;
        private AttackSkill _next;

        public bool TryRequest(AttackSkill skill) {
            if (_running == null) {
                _running = skill;
                return true;
            }
            if (_running == skill) return true;

            // 如果已有 next，但权重更低，替换
            if (_next == null || skill.Data.weight > _next.Data.weight) {
                _next = skill;
            }
            return false;
        }

        public void OnSkillFinished(AttackSkill skill) {
            if (_running == skill) {
                _running = _next;
                _next = null;
            }
        }
    }
}