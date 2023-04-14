using System;

namespace BehaviorTree
{
    [Serializable]
    public abstract class Task
    {
        // This is the abstract Task object.
        // There is nothing to be modified here.
        public abstract bool run (WorldState state);
    }
}
