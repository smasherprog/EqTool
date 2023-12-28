namespace EQTool.Services
{
    public class EnrageParser
    {
        public class EnrageEvent
        {
            public string NpcName { get; set; }
        }

        public EnrageEvent EnrageCheck(string line)
        {
            if (line.Contains(" has become ENRAGED."))
            {
                return new EnrageEvent { NpcName = line.Replace(" has become ENRAGED.", string.Empty).Trim() };
            }

            return null;
        }
    }
}
