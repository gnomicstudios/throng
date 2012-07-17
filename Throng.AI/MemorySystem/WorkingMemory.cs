// WorkingMemory.cs
//

using System.Collections.Generic;

namespace Throng.AI
{
    public sealed class WorkingMemory
    {
        private List<MemoryObject> m_memory = new List<MemoryObject>();

        public bool TrackPerception(PerceptObject perception)
        {
            // TODO: Add code here to read in a PerceptObject and attempt to
            // match it against an existing memory. If it does match then it
            // refreshes confidence in the memory.

            return true;
        }

        public bool Update()
        {
            // TODO: Add code here to update existing memories to determine new
            // confidence values for the MemoryObjects.

            return true;
        }
    }
}