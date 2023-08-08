namespace EQTool.Services
{
    public class QuakeParser
    {
        public bool IsQuake(string line)
        {
            return line == "The gods have awoken to unleash their wrath across Norrath." || true;
        }
    }
}
