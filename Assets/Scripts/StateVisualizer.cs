using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Collections.Generic;
using UnityEngine;
using BehaviorTree;

public class StateVisualizer : MonoBehaviour
{
    // Links to the game objects that contain the game world floor and wall tiles for each of the four rooms.
    public GameObject courtyard;
    public GameObject outside;
    public GameObject entrance;
    public GameObject hallway;

    // A link to the gate game object that is built into the 'Entrance' game world room.
    public GameObject gate;

    // Links to the potion and chest prefabs that will be used to instantiate game objects at the correct WorldState location.
    public GameObject potionPrefab;
    public GameObject chestPrefab;

    // Links to the knight and merlin prefabs that will be used to instantiate game objects at the correct WorldState location.
    public GameObject knightPrefab;
    public GameObject merlinPrefab;

    // Used to store the root of the behavior tree constructed in the Unity editor and loaded from disk.
    private Task behaviorTreeRoot;

    // Maps the WorldState Location enum items to their Unity game object instances.
    private Dictionary<Location, GameObject> locationGameObjects = new Dictionary<Location, GameObject>();

    // Maps the WorldState Thing enum items to their Unity prefabs.
    private Dictionary<Thing, GameObject> thingPrefabs = new Dictionary<Thing, GameObject>();

    // Maps the WorldState Thing enum items to their Unity game object instances.
    private Dictionary<Thing, GameObject> thingGameObjects = new Dictionary<Thing, GameObject>();

    // Maps the WorldState Character enum items to their Unity prefabs.
    private Dictionary<Character, GameObject> characterPrefabs = new Dictionary<Character, GameObject>();

    // Maps the WorldState Character enum items to their Unity game object instances.
    private Dictionary<Character, GameObject> characterGameObjects = new Dictionary<Character, GameObject>();

    // Stores whether a given floor tile in the Unity game world is occupied by a Thing or Character.
    // This is tracked so we don't have two Things or Characters instantiated on the same floor tile.
    private Dictionary<GameObject, GameObject> occupied = new Dictionary<GameObject, GameObject>();

    // The underling WorldState object that will be used to: 1.) Run the behavior tree and 2.) update the Unity game world.
    private WorldState state = new WorldState();

    // Start is called before the first frame update
    void Start()
    {
        // Initialize the WorldState object.
        // If you change any of these properties it should update the Unity visualization as well.
        state.Open[Thing.Gate] = true;
        state.Open[Thing.Chest] = false;
        state.CharacterPosition[Character.Knight] = Location.Outside;
        state.CharacterPosition[Character.Merlin] = Location.Hallway;
        state.ThingPosition[Thing.Potion] = Location.Outside;
        state.ThingPosition[Thing.Chest] = Location.Courtyard;
        state.ThingPosition[Thing.Gate] = Location.Entrance;

        state.ConnectedLocations[Location.Courtyard] = new HashSet<Location>();
        state.ConnectedLocations[Location.Courtyard].Add(Location.Outside);

        state.ConnectedLocations[Location.Outside] = new HashSet<Location>();
        state.ConnectedLocations[Location.Outside].Add(Location.Courtyard);
        state.ConnectedLocations[Location.Outside].Add(Location.Entrance);

        state.ConnectedLocations[Location.Entrance] = new HashSet<Location>();
        state.ConnectedLocations[Location.Entrance].Add(Location.Outside);
        state.ConnectedLocations[Location.Entrance].Add(Location.Hallway);

        state.ConnectedLocations[Location.Hallway] = new HashSet<Location>();
        state.ConnectedLocations[Location.Hallway].Add(Location.Entrance);

        state.BetweenLocations[Location.Entrance] = new Tuple<Thing, Location>(Thing.Gate, Location.Hallway);
        state.BetweenLocations[Location.Hallway] = new Tuple<Thing, Location>(Thing.Gate, Location.Entrance);

        Debug.Log(state);


        // Connect the Location enum items with their Unity game object counterparts.
        locationGameObjects.Add(Location.Courtyard, courtyard);
        locationGameObjects.Add(Location.Outside, outside);
        locationGameObjects.Add(Location.Entrance, entrance);
        locationGameObjects.Add(Location.Hallway, hallway);

        // Connect the Thing.Gate enum item with its Unity game object counterpart.
        thingGameObjects.Add(Thing.Gate, gate);

        // Connect the remaining Thing enum items with their Unity game object counterparts.
        thingPrefabs.Add(Thing.Potion, potionPrefab);
        thingPrefabs.Add(Thing.Chest, chestPrefab);

        // Connect the Character enum items with their Unity game object counterparts.
        characterPrefabs.Add(Character.Knight, knightPrefab);
        characterPrefabs.Add(Character.Merlin, merlinPrefab);

        // This code checks if there is a Behavior Tree data file that has been saved to disk.
        // If so, it loads the data file into the behaviorTreeRoot variable.
        // If no file exists, it sets the behaviorTreeRoot to null.
        if (File.Exists(Application.dataPath + "/BehaviorTree/bt.dat"))
        {
            BinaryFormatter formatter = new BinaryFormatter();
            FileStream fs = File.Open(Application.dataPath + "/BehaviorTree/bt.dat", FileMode.Open);

            object obj = formatter.Deserialize(fs);
            behaviorTreeRoot = (Task)obj;
            fs.Flush();
            fs.Close();
            fs.Dispose();

            // If we have successfully loaded the behavior tree, we will run it using the current WorldState object.
            Debug.Log("The root is: " + behaviorTreeRoot.ToString());
            behaviorTreeRoot.run(state);
            Debug.Log(state);
        }
        else
        {
            behaviorTreeRoot = null;
        }

        // Loop through each Thing in the dictionary of Thing enum -> prefabs...
        foreach (Thing thing in thingPrefabs.Keys)
        {
            // Grab the location of the current Thing.
            // Need to add this null check for pickUP
            if (state.ThingPosition.ContainsKey(thing))
            {
                Location location = state.ThingPosition[thing];

                // Instantiate a game object at a random tile in the room from the WorldState using the Thing's prefab.
                GameObject thingGameObject = Draw(thingPrefabs[thing], locationGameObjects[location]);

                // Add the instantiated game object to the dictionary of Thing enum -> game objects.
                thingGameObjects.Add(thing, thingGameObject);
            }
        }

        // Loop through each Character in the dictionary of Character enum -> prefabs...
        foreach (Character character in characterPrefabs.Keys)
        {
            // Grab the location of the current Character.
            Location location = state.CharacterPosition[character];

            // Instantiate a game object at a random tile in the room from the WorldState using the Character's prefab.
            GameObject thingGameObject = Draw(characterPrefabs[character], locationGameObjects[location]);

            // Add the instantiated game object to the dictionary of Character enum -> game objects.
            characterGameObjects.Add(character, thingGameObject);
        }
    }

    // Update is called once per frame
    void Update()
    {
        // This is inefficient...
        // But what it does is change the sprite of every game object that is 'open'.
        foreach (Thing thing in state.Open.Keys)
        {
            if (state.Open[thing])
            {
                StateAnimator[] stateAnims = thingGameObjects[thing].GetComponentsInChildren<StateAnimator>();
                foreach (StateAnimator stateAnim in stateAnims) stateAnim.Open();
            }
        }
    }

    // Instantiates a given prefab at a random tile in the given room.
    private GameObject Draw (GameObject prefab, GameObject location)
    {
        Transform floor = location.transform.Find("Floor");
        int randomChild = UnityEngine.Random.Range(0, floor.childCount);
        GameObject childTile = floor.GetChild(randomChild).gameObject;

        while (occupied.ContainsKey(childTile))
        {
            randomChild = UnityEngine.Random.Range(0, floor.childCount);
            childTile = floor.GetChild(randomChild).gameObject;
        }

        GameObject drawnObject = Instantiate(prefab, childTile.transform.position, Quaternion.identity);
        occupied.Add(childTile, drawnObject);

        return drawnObject;
    }
}
