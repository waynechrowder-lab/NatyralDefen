namespace Gameplay.Script.NatureFrontlineClient
{
    public class Player
    {
        public int Id { get; set; }
        public string Username { get; set; }
        public float X { get; set; }
        public float Y { get; set; }
        public float Z { get; set; }
        
        // 不包含密码字段，避免泄露
    }
}