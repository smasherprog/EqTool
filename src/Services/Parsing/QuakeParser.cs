namespace EQTool.Services.Parsing
{
    public class QuakeParser
    {
        public bool IsQuake(string line)
        {
            return line.Contains("You feel you should get somewhere safe as soon as possible");
        }
    }
}
