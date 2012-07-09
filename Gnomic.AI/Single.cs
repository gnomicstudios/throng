// Single.cs
//

namespace Gnomic.AI
{
    public delegate void BehaviourResultHandler(Search search);

    public abstract class Single : Node
    {
        public event BehaviourResultHandler Success;
        public event BehaviourResultHandler Failure;
    }
}