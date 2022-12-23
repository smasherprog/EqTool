namespace EQTool.Models
{
    public class SpellParsingMatch
    {
        public string TargetName { get; set; }

        public Spell Spell { get; set; }

        public bool MultipleMatchesFound { get; set; }
    }

}
