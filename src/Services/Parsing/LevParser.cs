namespace EQTool.Services.Parsing
{
    public class LevParser
    {
        public enum LevStatus
        {
            Fading,
            Faded,
            Applied
        }

        public LevStatus? Parse(string line)
        {
            return line == "You feel as if you are about to fall." ? LevStatus.Fading : (LevStatus?)null;
        }
    }
}
