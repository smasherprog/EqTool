namespace EQTool.Services
{
    public class ChainData
    {
        public string HighestOrder { get; set; }
        public string YourChainOrder { get; set; }
    }

    public static class CHService
    {
        public static bool ShouldWarnOfChain(ChainData chaindata, Parsing.ChParser.ChParseData e)
        {
            if (string.IsNullOrWhiteSpace(e.Position))
            {
                return false;
            }

            var increasedorder = false;
            var firstrun = false;
            if (e.Caster == "You")
            {
                chaindata.YourChainOrder = e.Position;
                firstrun = true;
            }
            if (string.Compare(e.Position, chaindata.HighestOrder) == 1 || string.IsNullOrWhiteSpace(chaindata.HighestOrder))
            {
                chaindata.HighestOrder = e.Position;
                increasedorder = true;
            }

            if (string.IsNullOrWhiteSpace(chaindata.YourChainOrder) || string.IsNullOrWhiteSpace(chaindata.YourChainOrder) || firstrun)
            {
                return false;
            }

            if (char.IsLetter(chaindata.HighestOrder[0]) && char.IsLetter(e.Position[0]) && char.IsLetter(chaindata.YourChainOrder[0]))
            {
                int highest = char.ToLower(chaindata.HighestOrder[0]);
                int my = char.ToLower(chaindata.YourChainOrder[0]);
                int current = char.ToLower(e.Position[0]);
                var dif = my - current;
                if (my == 'a' && increasedorder && dif == 1)
                {
                    return false;
                }
                return highest == 'z' && my == 'a' && current == 'z' && !increasedorder || dif == 1;
            }

            if (char.IsDigit(chaindata.HighestOrder[0]) && char.IsDigit(e.Position[0]) && char.IsDigit(chaindata.YourChainOrder[0]))
            {
                _ = int.TryParse(chaindata.HighestOrder, out var highest);
                _ = int.TryParse(chaindata.YourChainOrder, out var my);
                _ = int.TryParse(e.Position, out var current);
                var dif = my - current;
                if (my == 1 && increasedorder && dif == 1)
                {
                    return false;
                }

                return current == highest && my == 1 && !increasedorder || dif == 1;
            }

            return false;
        }
    }
}
