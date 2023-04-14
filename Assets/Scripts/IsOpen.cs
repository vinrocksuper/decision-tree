using System;
using UnityEngine;

namespace BehaviorTree
{
    [Serializable]
    public class IsOpen : Task
    {
        // We will determine whether this thing is open.
        private Thing what;
        public Thing What
        {
            get { return what; }
            set { what = value; }
        }

        // This constructor defaults the thing to None.
        public IsOpen() : this(Thing.None) { }

        // This constructor takes a parameter for the thing.
        public IsOpen (Thing what)
        {
            What = what;
        }

        // This method runs the IsOpen condition on the given WorldState object.
        public override bool run (WorldState state)
        {
            // Fill in your condition check here:
            if (state.Debug)
            {
                Debug.Log(this + " is open? Print the answer here.");
            }

            if (state.Open.ContainsKey(what))
            {
                return state.Open[what];
            }
            return false;
        }

        // Creates and returns a string describing the IsOpen condition.
        public override string ToString()
        {
            return "Is " + what + " open?";
        }
    }
}
