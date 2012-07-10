// Single.cs
//

namespace Gnomic.AI
{
    public delegate void BehaviourResultHandler(Search search);

    public abstract class Single : Behaviour
    {
        private int m_index = -1;

        public event BehaviourResultHandler Success;
        public event BehaviourResultHandler Failure;

        public override int Index
        {
            get { return m_index; }
            set { m_index = value; }
        }
    }
}