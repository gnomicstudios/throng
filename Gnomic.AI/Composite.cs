// Composite.cs
//

using System.Collections.Generic;

namespace Gnomic.AI
{
    public abstract class Composite
    {
        private List<Node> m_children = new List<Node>();

        public List<Node> Children
        {
            get { return m_children; }
        }
    }
}