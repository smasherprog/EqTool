namespace EQTool.Services
{
    public class DragonRoarParser
    {
        public bool IsDragonRoar(string line)
        {
            return line == "You flee in terror." || line == "You resist the Dragon Roar spell!";
        }
    }
}
