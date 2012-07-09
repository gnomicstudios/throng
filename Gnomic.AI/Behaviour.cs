// Behaviour.cs
//

namespace Gnomic.AI
{
    public abstract class Behaviour
    {
        public enum Result
        {
            Invalid,            // invalid for actor
            Success,            // successful outcome
            Failure,            // failure outcome
            Running             // continue to update
        }

        public abstract Result Tick(Search search);
        public abstract int Weight { get; }

        protected abstract bool IsValid(IActor actor);
    }
}