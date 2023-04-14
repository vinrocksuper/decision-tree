using System;
using System.Collections.Generic;
using UnityEngine;

namespace BehaviorTree
{
    [Serializable]
    public class Sequence : Task
    {
        // An ordered list of this Sequence's children tasks.
        public List<Task> children { get; set; } = new List<Task>();

        // This method implements the sequence behavior.
        public override bool run(WorldState state)
        {
            if (state.Debug) Debug.Log("Sequencer Start");
            foreach (Task task in children)
            {
                if (!task.run(state))
                {
                    // Fill in your sequence logic here:
                    if (state.Debug) Debug.Log("Sequencer Fail");
                    return false;
                }
            }




            if (state.Debug) Debug.Log("Sequencer Success");
            return true;
        }
    }
}
