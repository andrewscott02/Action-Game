using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.AI.Navigation;

//Old Script, do not need any more

public class GrammarsDungeonGeneration : MonoBehaviour
{
    #region Setup

    // static GrammarsDungeonGeneration instance;
    public GrammarsDungeonData grammarsDungeonData;
    public int preloadRooms = 2;

    #endregion

    private void Start()
    {
        //instance = this;
        GenerateGrammarsDungeon();
        GameCanvasManager.instance.ShowRegionText(firstTheme.regionName);
    }

    [ContextMenu("Generate Grammars Dungeon")]
    public void GenerateGrammarsDungeon()
    {
        CleanupDungeon();

        int randTheme = Random.Range(0, DifficultyManager.instance.difficulty.startingThemes.Count);
        firstTheme = DifficultyManager.instance.difficulty.startingThemes[randTheme];
        currentTheme = firstTheme;

        List<E_RoomTypes> rooms = new List<E_RoomTypes>() { E_RoomTypes.Start, E_RoomTypes.Healing, E_RoomTypes.Boss, E_RoomTypes.End };

        List<E_RoomTypes> additionalRooms = GenerateAdditionalRooms();
        additionalRooms = GenerateHealingRooms(additionalRooms);
        additionalRooms = DetermineRoomChanges(additionalRooms);

        grammarsDungeonData.ReplaceDuplicates(additionalRooms);
        grammarsDungeonData.EnsureMinimums(additionalRooms);
        
        foreach (var item in additionalRooms)
        {
            rooms.Insert(1, item);
        }

        string dungeonLayout = ConvertToString(rooms);

        //Debug.Log(dungeonLayout);

        GenerateDungeonRooms(rooms);
        BakeNavmesh();

        PopulateRooms();
    }

    [ContextMenu("Cleanup Dungeon")]
    public void CleanupDungeon()
    {
        PCGRoom[] roomData = GetComponentsInChildren<PCGRoom>();

        foreach (var item in roomData)
        {
            item.DeleteRoom();
        }

        List<GameObject> children = new List<GameObject>();

        for (int i = 0; i < transform.childCount; i++)
        {
            children.Add(transform.GetChild(i).gameObject);
        }

        for (int i = 0; i < children.Count; i++)
        {
            DestroyImmediate(children[i]);
        }

        createdRooms = new List<PCGRoom>();

        grammarsDungeonData.ResetAllDungeonData();
    }

    #region Creating Rooms

    ThemeData firstTheme;
    ThemeData currentTheme;

    List<E_RoomTypes> GenerateAdditionalRooms()
    {
        List<E_RoomTypes> rooms = new List<E_RoomTypes>();

        int emptyRoomsCount = Random.Range(grammarsDungeonData.roomsCountMinMax.x, grammarsDungeonData.roomsCountMinMax.y + 1);

        for (int i = 0; i < emptyRoomsCount; i++)
        {
            E_RoomTypes roomType = grammarsDungeonData.GetRandomRoomType();
            rooms.Add(roomType);
        }

        return rooms;
    }

    List<E_RoomTypes> GenerateHealingRooms(List<E_RoomTypes> rooms)
    {
        int healingRoomsCount = grammarsDungeonData.additionalHealingRooms;
        float fInterval = (float)rooms.Count / (float)(healingRoomsCount + 1);
        int interval = Mathf.RoundToInt(fInterval);

        for (int i = 0; i < healingRoomsCount; i++)
        {
            int insertIndex = ((i + 1) * interval);
            //Debug.Log("Insert healing room at " + insertIndex + " from interval " + interval);
            rooms.Insert(insertIndex + i, E_RoomTypes.Healing);
        }

        return rooms;
    }

    List<E_RoomTypes> DetermineRoomChanges(List<E_RoomTypes> rooms)
    {
        int changeRoomsCount = Random.Range(grammarsDungeonData.themeChanges.y, grammarsDungeonData.themeChanges.y + 1);
        float fInterval = (float)rooms.Count / (float)(changeRoomsCount + 1);
        int interval = Mathf.RoundToInt(fInterval);

        for (int i = 0; i < changeRoomsCount; i++)
        {
            int insertIndex = ((i + 1) * interval);
            //Debug.Log("Insert healing room at " + insertIndex + " from interval " + interval);
            rooms.Insert(insertIndex + i, E_RoomTypes.ChangeTheme);
        }

        return rooms;
    }

    string ConvertToString(List<E_RoomTypes> rooms)
    {
        string dungeonLayout = "";

        foreach (var item in rooms)
            dungeonLayout += item.ToString() + ">";

        dungeonLayout.Remove(dungeonLayout.Length - 1);

        return dungeonLayout;
    }

    List<PCGRoom> createdRooms;

    List<Object> DetermineDungeonRooms(List<E_RoomTypes> rooms, out List<ThemeData> themes, out List<bool> reversedRooms)
    {
        List<Object> prefabs = new List<Object>();
        themes = new List<ThemeData>();

        reversedRooms = new List<bool>();
        /*
        for(int i = 0; i < rooms.Count; i++)
        {
            Object prefab = grammarsDungeonData.GetRandomRoomPrefab(rooms[i], currentTheme, out ThemeData nextRoom, out bool reversed);
            if (prefab != null)
            {
                prefabs.Add(prefab);
                themes.Add(currentTheme);
                reversedRooms.Add(reversed);
            }
            else
            {
                Debug.LogWarning("No prefab added for theme " + currentTheme + " to " + nextRoom + " at index " + i);
            }
            currentTheme = nextRoom;
        }

        //TODO: Use grammars to change rooms
        */
        return prefabs;
    }

    void GenerateDungeonRooms(List<E_RoomTypes> rooms)
    {
        createdRooms = new List<PCGRoom>();

        List<Object> prefabs = DetermineDungeonRooms(rooms, out List<ThemeData> themes, out List<bool> reversedRooms);
        themes.Add(themes[themes.Count - 1]);

        for (int i = 0; i < rooms.Count; i++)
        {
            foreach(var data in grammarsDungeonData.roomData)
            {
                if (data.roomType == rooms[i])
                {
                    GameObject go = Instantiate(prefabs[i], transform) as GameObject;
                    PCGRoom goRoom = go.GetComponent<PCGRoom>();
                    //goRoom.Setup(rooms[i], grammarsDungeonData, themes[i], themes[i + 1], i, reversedRooms[i]);

                    if (data.roomType != E_RoomTypes.Start)
                    {
                        ObjectSpawner doorPoint = createdRooms[i - 1].GetRandomDoorPoint();
                        createdRooms[i - 1].TrySetMainDoorPoint(doorPoint);
                        doorPoint = createdRooms[i - 1].mainDoorPoint;

                        go.transform.position = doorPoint.transform.position;
                        Quaternion rot = doorPoint.transform.rotation;

                        if (reversedRooms[i])
                        {
                            //Debug.Log("Reverse room " + i);

                            rot.y += 180;
                            go.transform.rotation = rot;

                            Vector3 offset = go.transform.position - doorPoint.transform.position;

                            go.transform.position = go.transform.position + offset;

                            doorPoint.transform.position = go.transform.position;
                            doorPoint.transform.rotation = Quaternion.identity;
                        }
                    }
                    else
                    {
                        go.transform.position = transform.position;
                        go.transform.rotation = Quaternion.identity;
                    }

                    createdRooms.Add(goRoom);
                }
            };
        }
    }

    public void GenerateSideRoom(PCGRoom previousRoom, ObjectSpawner previousDoorPoint)
    {
        /*
        E_RoomTypes roomType = grammarsDungeonData.GetRandomRoomTypeIgnoreLimits();
        ThemeData theme = previousDoorPoint.changeTheme ? previousRoom.nextTheme : previousRoom.theme;

        Object prefab = grammarsDungeonData.GetRandomRoomPrefab(roomType, theme, out ThemeData nextRoom, out bool reversed);
        GameObject go = Instantiate(prefab, transform) as GameObject;
        PCGRoom goRoom = go.GetComponent<PCGRoom>();
        //goRoom.Setup(roomType, grammarsDungeonData, theme, theme, 0, reversed);

        foreach(var data in grammarsDungeonData.roomData)
        {
            if (data.roomType != E_RoomTypes.Start)
            {
                go.transform.position = previousDoorPoint.transform.position;
                Quaternion rot = previousDoorPoint.transform.rotation;

                if (reversed)
                {
                    //Debug.Log("Reverse room " + i);

                    rot.y += 180;
                    go.transform.rotation = rot;

                    Vector3 offset = go.transform.position - previousDoorPoint.transform.position;

                    go.transform.position = go.transform.position + offset;

                    previousDoorPoint.transform.position = go.transform.position;
                    previousDoorPoint.transform.rotation = Quaternion.identity;
                }
            }
            else
            {
                go.transform.position = transform.position;
                go.transform.rotation = Quaternion.identity;
            }

            createdRooms.Add(goRoom);
        }
        */
    }

    void PopulateRooms()
    {
        int generatedRooms = 0;

        for (int i = 0; i < createdRooms.Count; i++)
        {
            PopulateRoom(i);
            generatedRooms++;
            if (generatedRooms >= preloadRooms)
                return;
        }
    }

    public void PopulateRoom(int index)
    {
        if (index >= createdRooms.Count || index < 0) return;

        createdRooms[index].PopulateRoom();
    }

    public void CullRooms(int index)
    {
        if (index >= createdRooms.Count || index < 0) return;

        for (int i = 0; i < createdRooms.Count; i++)
        {
            Debug.Log("Culling room " + i);
            createdRooms[i].CullRoom(i < index);
        }

        //createdRooms[index - 1].CloseDoor();
    }

    #endregion

    public NavMeshSurface navMeshSurface;

    void BakeNavmesh()
    {
        navMeshSurface.BuildNavMesh();
    }
}