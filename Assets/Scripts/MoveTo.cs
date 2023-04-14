using System;
using UnityEngine;

namespace BehaviorTree
{
    [Serializable]
    public class MoveTo : Task
    {
        // The character that is moving to a different location.
        private Character mover;
        public Character Mover
        {
            get { return mover; }
            set { mover = value; }
        }

        // The location the character is moving to.
        private Location where;
        public Location Where
        {
            get { return where; }
            set { where = value; }
        }

        // This constructor defaults the character and location to None.
        public MoveTo() : this(Character.None, Location.None) { }

        // This constructor takes parameters for the character and location.
        public MoveTo (Character mover, Location where)
        {
            Mover = mover;
            Where = where;
        }

        // This method runs the MoveTo action on the given WorldState object.
        public override bool run (WorldState state)
        {
            if(where != state.CharacterPosition[mover])
            {
                if (state.ConnectedLocations[state.CharacterPosition[mover]].Contains(where))
                {
                    if (state.BetweenLocations.ContainsKey(where))
                    {
                        // Item 1 is the Gate, Item 2 is the new location 
                        if (state.BetweenLocations[where].Item1 == Thing.Gate)
                        {
                            if (state.Open.ContainsKey(Thing.Gate))
                            {
                                state.CharacterPosition[mover] = where;
                                return true;
                            }
                            return false;

                        }
                    }
                        else
                    {
                        state.CharacterPosition[mover] = where;
                        return true;
                    }
                }
            }

            if (state.Debug) Debug.Log(this + " Fail");
            return false;
        }

        // Creates and returns a string describing the MoveTo action.
        public override string ToString()
        {
            return mover + " moves to " + where;
        }
    }
}
