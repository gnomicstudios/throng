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

        public void AddChild(Single child)
        {
            child.Success += OnChildSuccess;
            child.Failure += OnChildFailure;
            m_children.Add(child);
        }

        public void RemoveChild(Single child)
        {
            child.Success -= OnChildSuccess;
            child.Failure -= OnChildFailure;
            m_children.Remove(child);
        }

        public abstract void OnChildSuccess(Search search);
        public abstract void OnChildFailure(Search search);
    }
}