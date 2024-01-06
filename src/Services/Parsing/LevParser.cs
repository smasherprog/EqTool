namespace EQTool.Services
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
            if (line == "You feel as if you are about to fall.")
            {
                return LevStatus.Fading;
            }

            return null;
        }
    }
}
