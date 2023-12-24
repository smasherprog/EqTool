namespace EQTool.Services
{
    public class POFDTParser
    {
        public class POF_DT_Event
        {
            public string NpcName { get; set; }
            public string DTReceiver { get; set; }
        }

        public POF_DT_Event DtCheck(string line)
        {
            if (line.StartsWith("Fright says '"))
            {
                var firsttick = line.IndexOf("'") + 1;
                var lasttick = line.LastIndexOf("'") - firsttick;
                var possiblename = line.Substring(firsttick, lasttick);
                if (!possiblename.Contains(" "))
                {
                    return new POF_DT_Event { NpcName = "Fright", DTReceiver = possiblename };
                }
            }

            if (line.StartsWith("Dread says '"))
            {
                var firsttick = line.IndexOf("'") + 1;
                var lasttick = line.LastIndexOf("'") - firsttick;
                var possiblename = line.Substring(firsttick, lasttick);
                if (!possiblename.Contains(" "))
                {
                    return new POF_DT_Event { NpcName = "Dread", DTReceiver = possiblename };
                }
            }
            return null;
        }
    }
}
