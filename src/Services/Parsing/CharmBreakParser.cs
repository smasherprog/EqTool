namespace EQTool.Services
{
    public class CharmBreakParser
    {
        public bool DidCharmBreak(string line)
        {
            return line == "Your charm spell has worn off.";
        }
    }
}
