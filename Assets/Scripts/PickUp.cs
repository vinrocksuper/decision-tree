using System;
using UnityEngine;

namespace BehaviorTree
{
    [Serializable]
    public class PickUp : Task
    {
        // The character that is opening the thing.
        private Character chara;
        public Character Chara
        {
            get { return chara; }
            set { chara = value; }
        }

        // The thing that is being opened.
        private Thing pickUpThis;
        public Thing PickUpThis
        {
            get { return pickUpThis; }
            set { pickUpThis = value; }
        }

        // This constructor defaults the character and thing to None.
        public PickUp() : this(Character.None, Thing.None) { }

        // This constructor takes parameters for the character and thing.
        public PickUp(Character chara, Thing openThis)
        {
            Chara = chara;
            PickUpThis = openThis;
        }

        // This method runs the PickUp action on the given WorldState object.
        public override bool run(WorldState state)
        {
            if (state.CharacterPosition[chara] == state.ThingPosition[pickUpThis])
            {
                state.Has[chara] = pickUpThis;
                state.ThingPosition.Remove(pickUpThis);

                return true;
            }
            return false;
        }

        // Creates and returns a string describing the Open action.
        public override string ToString()
        {
            return chara + " picks up " + pickUpThis;
        }
    }
}
