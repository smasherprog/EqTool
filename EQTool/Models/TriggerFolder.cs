using System;

namespace EQTool.Models
{
    // Represents a (possibly nested) folder within the Triggers branch of the
    // settings tree. Folders are organizational only - they group Triggers and
    // other Folders. The hierarchy is stored as a flat list and reconstructed
    // using ParentId (null == top level of the Triggers branch).
    [Serializable]
    public class TriggerFolder
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Name { get; set; } = "New Folder";
        public Guid? ParentId { get; set; }
    }
}
