// Composite.cs
//

using System.Collections.Generic;

namespace Gnomic.AI
{
    public abstract class Composite : Behaviour
    {
        private List<Behaviour> m_children = new List<Behaviour>();

        public List<Behaviour> Children
        {
            get { return m_children; }
        }
    }
}