using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using BehaviorTree;

public class BehaviorTreeEditor : EditorWindow
{
    // Counts IDs up from 0 to assign to new windows.
    private int idCounter;

    // Maps a given ID to a Behavior Tree task node.
    private Dictionary<int, Task> idToNode;

    // Maps a given ID to a window rect.
    private Dictionary<int, Rect> idToWindow;

    // Maps a given ID to a list of children IDs.
    private Dictionary<int, List<int>> idToChildren;

    [MenuItem("Window/Behavior Tree Editor")]
    static void ShowEditor()
    {
        BehaviorTreeEditor editor = EditorWindow.GetWindow<BehaviorTreeEditor>();
        editor.Init();
    }

    public void Init()
    {
        // Initialize the ID counter to 0.
        idCounter = 0; 

        // Initialize the ID to node dictionary.
        idToNode = new Dictionary<int, Task>();

        // Initialize the 0th ID to a null behavior tree task.
        idToNode.Add(0, null);

        // Initialize the ID to window dictionary.
        idToWindow = new Dictionary<int, Rect>();

        // Initialize the 0th ID window.
        idToWindow.Add(0, new Rect(250, 10, 200, 100));

        // Initialize the ID to children dictionary.
        idToChildren = new Dictionary<int, List<int>>();

        // Initialize the 0th ID to a new list.
        idToChildren.Add(0, new List<int>());
    }

    void OnGUI()
    {
        // Allow the user to reset to an empty behavior tree.
        if (GUI.Button(new Rect(5, 20, 190, 20), "Reset Behavior Tree"))
        {
            ShowEditor();
        }

        // Allow the user to save the current behavior tree to disk.
        if (GUI.Button(new Rect(5, 45, 190, 20), "Save Behavior Tree"))
        {
            // Only save the tree if the root is not null.
            if (idToNode[0] != null)
            {
                // This code formats the root to a binary format and saves it to the Assets/BehaviorTree folder as 'bt.dat'.
                System.IO.Stream ms = File.OpenWrite(Application.dataPath + "/BehaviorTree/bt.dat");
                BinaryFormatter formatter = new BinaryFormatter();
                Task root = idToNode[0];

                formatter.Serialize(ms, root);
                ms.Flush();
                ms.Close();
                ms.Dispose();
            }
        }

        // Allow the user to load an existing behavior tree.
        if (GUI.Button(new Rect(5, 70, 190, 20), "Load Behavior Tree"))
        {
            // Only try to load if the 'bt.dat' file exists in the Assets/BehaviorTree folder.
            if (File.Exists(Application.dataPath + "/BehaviorTree/bt.dat"))
            {
                // This code loads the 'bt.dat' file and deserializes the binary file to the root task.
                BinaryFormatter formatter = new BinaryFormatter();
                FileStream fs = File.Open(Application.dataPath + "/BehaviorTree/bt.dat", FileMode.Open);

                object obj = formatter.Deserialize(fs);
                Task root = (Task)obj;
                fs.Flush();
                fs.Close();
                fs.Dispose();

                // Once the root is loaded, nothing else happens.
                // One UI task is to update the view based on the loaded tree.
                Debug.Log("The root is: " + root.ToString());
            }
        }

        // Draw the editor windows.
        BeginWindows();

        // If the root is null, draw the initial empty window.
        if (idToNode[0] == null)
        {
            // The empty window.
            idToWindow[0] = GUI.Window(0, idToWindow[0], DrawWindow, "Behavior Tree Root");
        }
        else
        {
            // Otherwise, iterate through all the window IDs...
            for (int i = 0; i < idCounter; i++)
            {
                // Grab the type of the current behavior tree task.
                string currentType = TypeToString(idToNode[i].GetType());

                // Format the window and save it to the window dictionary.
                idToWindow[i] = GUI.Window(i, idToWindow[i], DrawWindow, currentType);

                // Draw the edges between the children/parent nodes.
                foreach (int child in idToChildren[i])
                {
                    DrawNodeCurve(idToWindow[i], idToWindow[child]);
                }
            }
        }

        // End the editor windows.
        EndWindows();
    }

    // Given an ID, format its window.
    void DrawWindow(int windowID)
    {
        // If there are no behavior tree nodes, this is the initial empty window.
        if (idToNode[windowID] == null)
        {
            if (GUI.Button(new Rect(5, 20, 190, 20), "Select Root Task"))
            {
                GenericMenu menu = new GenericMenu();
                foreach (Type type in GetSubtypes())
                {
                    string typeString = TypeToString(type);
                    menu.AddItem(new GUIContent(typeString), false, () =>
                    {
                        Debug.Log("Root task is now: " + typeString);
                        idToNode[windowID] = Activator.CreateInstance(type) as Task;
                        idCounter++;
                    });
                }
                menu.ShowAsContext();
            }
        }
        // This section formats the sequence and selector windows.
        else if (TypeToString(idToNode[windowID].GetType()) == "Sequence" || TypeToString(idToNode[windowID].GetType()) == "Selector")
        {
            if (GUI.Button(new Rect(5, 20, 190, 20), "Add Child"))
            {
                GenericMenu menu = new GenericMenu();
                foreach (Type type in GetSubtypes())
                {
                    string typeString = TypeToString(type);
                    menu.AddItem(new GUIContent(typeString), false, () =>
                    {
                        float xPosition = 0;
                        float yPosition = idToWindow[windowID].y + 110;


                        if (idToChildren[windowID].Count > 0)
                        {
                            List<int> childrenIDs = idToChildren[windowID];
                            int lastChild = childrenIDs[childrenIDs.Count - 1];
                            xPosition = idToWindow[lastChild].xMax;
                            yPosition = idToWindow[lastChild].yMin;
                        }
                        
                        Debug.Log("Created child: " + typeString);
                        Task newBTNode = Activator.CreateInstance(type) as Task;

                        if (TypeToString(idToNode[windowID].GetType()) == "Sequence")
                        {
                            Sequence sequence = idToNode[windowID] as Sequence;
                            sequence.children.Add(newBTNode);
                            idToNode[windowID] = sequence;
                        }
                        else
                        {
                            Selector selector = idToNode[windowID] as Selector;
                            selector.children.Add(newBTNode);
                            idToNode[windowID] = selector;
                        }

                        Rect newWindow = new Rect(xPosition + 10, yPosition, 200, 100);
                        idToNode.Add(idCounter, newBTNode);
                        idToWindow.Add(idCounter, newWindow);
                        idToChildren.Add(idCounter, new List<int>());
                        idToChildren[windowID].Add(idCounter);
                        idCounter++;
                    });
                }
                menu.ShowAsContext();
            }
        }
        // This formats the Is Open window.
        else if (TypeToString(idToNode[windowID].GetType()) == "IsOpen")
        {
            IsOpen isOpenTask = idToNode[windowID] as IsOpen;

            if (GUI.Button(new Rect(5, 20, 190, 20), "Is open?: " + isOpenTask.What.ToString()))
            {
                GenericMenu menu = new GenericMenu();
                foreach (Thing thing in Enum.GetValues(typeof(Thing)))
                {
                    string thingString = thing.ToString();
                    menu.AddItem(new GUIContent(thingString), false, () =>
                    {                        
                        Debug.Log("Check if this is open: " + thingString);
                        isOpenTask.What = thing;
                        idToNode[windowID] = isOpenTask;
                    });
                }
                menu.ShowAsContext();
            }
        }
        else if (TypeToString(idToNode[windowID].GetType()) == "IsHere")
        {
            IsHere isHereTask = idToNode[windowID] as IsHere;

            if (GUI.Button(new Rect(5, 20, 190, 20), "Is here?: " + isHereTask.Where.ToString()))
            {
                GenericMenu menu = new GenericMenu();
                foreach (Location where in Enum.GetValues(typeof(Thing)))
                {
                    string thingString = where.ToString();
                    menu.AddItem(new GUIContent(thingString), false, () =>
                    {
                        Debug.Log("Check if this is open: " + thingString);
                        isHereTask.Where = where;
                        idToNode[windowID] = isHereTask;
                    });
                }
                menu.ShowAsContext();
            }
        }
        else if (TypeToString(idToNode[windowID].GetType()) == "PickUp")
        {
            PickUp pickUpTask = idToNode[windowID] as PickUp;

            if (GUI.Button(new Rect(5, 20, 190, 20), "Opener: " + pickUpTask.Chara.ToString()))
            {
                GenericMenu menu = new GenericMenu();
                foreach (Character character in Enum.GetValues(typeof(Character)))
                {
                    string characterString = character.ToString();
                    menu.AddItem(new GUIContent(characterString), false, () =>
                    {
                        Debug.Log("Opener set to: " + characterString);
                        pickUpTask.Chara = character;
                        idToNode[windowID] = pickUpTask;
                    });
                }
                menu.ShowAsContext();
            }

            if (GUI.Button(new Rect(5, 45, 190, 20), "Open this: " + pickUpTask.PickUpThis.ToString()))
            {
                GenericMenu menu = new GenericMenu();
                foreach (Thing thing in Enum.GetValues(typeof(Thing)))
                {
                    string thingString = thing.ToString();
                    menu.AddItem(new GUIContent(thingString), false, () =>
                    {
                        Debug.Log("Open this set to: " + thingString);
                        pickUpTask.PickUpThis = thing;
                        idToNode[windowID] = pickUpTask;
                    });
                }
                menu.ShowAsContext();
            }
        }
        // This section formats the Open window.
        else if (TypeToString(idToNode[windowID].GetType()) == "Open")
        {
            Open openTask = idToNode[windowID] as Open;

            if (GUI.Button(new Rect(5, 20, 190, 20), "Opener: " + openTask.Opener.ToString()))
            {
                GenericMenu menu = new GenericMenu();
                foreach (Character character in Enum.GetValues(typeof(Character)))
                {
                    string characterString = character.ToString();
                    menu.AddItem(new GUIContent(characterString), false, () =>
                    {                        
                        Debug.Log("Opener set to: " + characterString);
                        openTask.Opener = character;
                        idToNode[windowID] = openTask;
                    });
                }
                menu.ShowAsContext();
            }

            if (GUI.Button(new Rect(5, 45, 190, 20), "Open this: " + openTask.OpenThis.ToString()))
            {
                GenericMenu menu = new GenericMenu();
                foreach (Thing thing in Enum.GetValues(typeof(Thing)))
                {
                    string thingString = thing.ToString();
                    menu.AddItem(new GUIContent(thingString), false, () =>
                    {                        
                        Debug.Log("Open this set to: " + thingString);
                        openTask.OpenThis = thing;
                        idToNode[windowID] = openTask;
                    });
                }
                menu.ShowAsContext();
            }
        }
        // This section formats the Move To window.
        else if (TypeToString(idToNode[windowID].GetType()) == "MoveTo")
        {
            MoveTo moveToTask = idToNode[windowID] as MoveTo;

            if (GUI.Button(new Rect(5, 20, 190, 20), "Mover: " + moveToTask.Mover.ToString()))
            {
                GenericMenu menu = new GenericMenu();
                foreach (Character character in Enum.GetValues(typeof(Character)))
                {
                    string characterString = character.ToString();
                    menu.AddItem(new GUIContent(characterString), false, () =>
                    {                        
                        Debug.Log("Mover set to: " + characterString);
                        moveToTask.Mover = character;
                        idToNode[windowID] = moveToTask;
                    });
                }
                menu.ShowAsContext();
            }

            if (GUI.Button(new Rect(5, 45, 190, 20), "Move to: " + moveToTask.Where.ToString()))
            {
                GenericMenu menu = new GenericMenu();
                foreach (Location location in Enum.GetValues(typeof(Location)))
                {
                    string locationString = location.ToString();
                    menu.AddItem(new GUIContent(locationString), false, () =>
                    {                        
                        Debug.Log("Where set to: " + locationString);
                        moveToTask.Where = location;
                        idToNode[windowID] = moveToTask;
                    });
                }
                menu.ShowAsContext();
            }
        }
        
        GUI.DragWindow();
    }

    // Draws an edge between two windows.
    void DrawNodeCurve(Rect start, Rect end)
    {
        Vector3 startPos = new Vector3(start.x + (start.width / 2), start.y + start.height, 0);
        Vector3 endPos = new Vector3(end.x + (start.width / 2), end.y, 0);
        Vector3 startTan = startPos + Vector3.down * 50;
        Vector3 endTan = endPos + Vector3.up * 50;
        Color shadowCol = new Color(0, 0, 0, 0.06f);
        for (int i = 0; i < 3; i++) // Draw a shadow
            Handles.DrawBezier(startPos, endPos, startTan, endTan, shadowCol, null, (i + 1) * 5);
        Handles.DrawBezier(startPos, endPos, startTan, endTan, Color.black, null, 1);
    }

    // Gets the subtypes of the Behavior Tree Task.
    private IEnumerable<Type> GetSubtypes()
    {
        return typeof(Task).Assembly.GetTypes().Where(type => type.IsSubclassOf(typeof(Task)));
    }

    // Converts a C# object type to a string.
    private string TypeToString(Type type)
    {
        return type.ToString().Split('.')[1];
    }
}