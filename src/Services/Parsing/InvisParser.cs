namespace EQTool.Services
{
    public class InvisParser
    {
        public enum InvisStatus
        {
            Fading,
            Faded,
            Applied
        }

        public InvisStatus? Parse(string line)
        {
            if (line == "You feel yourself starting to appear.")
            {
                return InvisStatus.Fading;
            }
            return null;
        }
    }
}
