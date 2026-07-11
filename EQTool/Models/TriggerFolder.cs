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

        // Set when this user folder lives directly under a built-in library folder (e.g.
        // "Encounters/Kael"). Built-in folders are synthesized from code on every load and have no
        // stable ids, so user folders anchor to them by "/"-separated path instead of ParentId.
        // Null when the folder has a user parent (ParentId) or sits at the root.
        public string BuiltInParentPath { get; set; }
    }
}
