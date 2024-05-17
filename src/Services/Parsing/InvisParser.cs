namespace EQTool.Services.Parsing
{
    public class InvisParser
    {
        public enum InvisStatus
        {
            Fading,
            Faded,
            Applied
        }
        public InvisParser()
        {

        }

        public InvisStatus? Parse(string line)
        {
            return line == "You feel yourself starting to appear." ? InvisStatus.Fading : (InvisStatus?)null;
        }
    }
}
