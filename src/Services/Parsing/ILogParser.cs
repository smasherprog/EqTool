namespace EQTool.Services.Parsing
{
    public interface ILogParser
    {
        bool Evaluate(string line, string previousline);
    }
}
