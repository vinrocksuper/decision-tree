using System;
using System.Collections.Generic;

namespace BehaviorTree
{
    // An object that represents the game's current world state.
    public class WorldState
    {
        // A boolean that controls how much debug information is printed to the Unity Console.
        public bool Debug { get; set; } = true;

        // A dictionary that maps Things to a bool that describes whether or not they are open.
        // If a Thing does not have a key in the dictionary, it is assumed that the Thing is not open.
        public Dictionary<Thing, bool> Open { get; set; } = new Dictionary<Thing, bool>();

        // A dictionary that maps Characters to the Location they are currently at.
        public Dictionary<Character, Location> CharacterPosition { get; set; } = new Dictionary<Character, Location>();

        // A dictionary that maps Things to the Location they are currently at.
        public Dictionary<Thing, Location> ThingPosition { get; set; } = new Dictionary<Thing, Location>();

        // A dictionary that maps Locations to a set of other Locations that they are connected to.
        // If two locations are connected, that means a game Character can move from one location to another.
        public Dictionary<Location, HashSet<Location>> ConnectedLocations { get; set; } = new Dictionary<Location, HashSet<Location>>();

        // This dictionary stores whether there is a door between two Locations.
        // It maps a starting Location to a tuple that contains the door Thing and the second Location on the other side of the door.
        // This will be useful when determining whether a door must be open in order for a Character to move between two Locations connected by the door.
        public Dictionary<Location, Tuple<Thing, Location>> BetweenLocations { get; set; } = new Dictionary<Location, Tuple<Thing, Location>>();


        public Dictionary<Character, Thing> Has { get; set; } = new Dictionary<Character, Thing>();

        // Formats and returns a string that represents the current world state.
        public override string ToString()
        {
            string output = "Debug: " + Debug + Environment.NewLine;

            foreach (Thing thing in Open.Keys)
                output += thing + " open: " + Open[thing] + Environment.NewLine;

            foreach (Character character in CharacterPosition.Keys)
                output += character + " at " + CharacterPosition[character] + Environment.NewLine;

            foreach (Thing thing in ThingPosition.Keys)
                output += thing + " at " + ThingPosition[thing] + Environment.NewLine;

            foreach (Location location in ConnectedLocations.Keys)
                foreach (Location connected in ConnectedLocations[location])
                    output += location + " connected to " + connected + Environment.NewLine;

            foreach (Location location in BetweenLocations.Keys)
                output += BetweenLocations[location].Item1 + " is between " + location + " and " + BetweenLocations[location].Item2 + Environment.NewLine;

            foreach(Character chara in Has.Keys)
            {
                output += chara + " has " + Has[chara];
            }
            return output;
        }
    }

    // An enum of the possible world locations.
    public enum Location
    {
        None, Courtyard, Outside, Entrance, Hallway
    }

    // An enum of the possible things in the world.
    public enum Thing
    {
        None, Gate, Potion, Chest
    }

    // An enum of the possible characters in the world.
    public enum Character
    {
        None, Knight, Merlin
    }
}
