using System;
using UnityEngine;

namespace BehaviorTree
{
    [Serializable]
    public class IsHere : Task
    {
        private Character chara;
        public Character Chara
        {
            get { return chara; }
            set { chara = value; }
        }

        // We will determine whether this thing is open.
        private Location where;
        public Location Where
        {
            get { return where; }
            set { where = value; }
        }

        // This constructor defaults the thing to None.
        public IsHere() : this(Character.None, Location.None) { }

        // This constructor takes a parameter for the thing.
        public IsHere(Character chara, Location where)
        {
            Chara = chara;
            Where = where;
        }

        // This method runs the IsHere condition on the given WorldState object.
        public override bool run(WorldState state)
        {
            return state.CharacterPosition[chara] == where;
        }

        // Creates and returns a string describing the IsOpen condition.
        public override string ToString()
        {
            return "Is " + where;
        }
    }
}
