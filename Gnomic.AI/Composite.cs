// Composite.cs
//

using System.Collections.Generic;
using System.Linq;

namespace Gnomic.AI
{
    public abstract class Composite : Behaviour
    {
        private List<Behaviour> m_children = new List<Behaviour>();

        public override int Index
        {
            get { return m_children.Max(c => c.Index); }
            set { /* do nothing */ }
        }

        public List<Behaviour> Children
        {
            get { return m_children; }
        }
    }
}