using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewRoomType", menuName = "MonsterPalace/Room Type")]
public class SO_RoomType : ScriptableObject
{
    public string roomName;

    [Header("Room size")]
    [Space(10)]
    public Vector3Int roomSize;

    public RoomType roomType;
    public ActivityType activityType;
    public MonsterType monsterTypeOfRoom;

    [Space(10)]
    public int maxUsers;

    [Space(10)]
    public int cost;
    public int coinCost;
    public int quantityBuyable = 1;
    public GameObject prefab;
    public bool isUnlocked = false;
}

[Serializable]
public class Room
{
    public SO_RoomType roomType;
    public Vector3Int positionInGrid;

    public string roomID;

    public Vector3Int roomSize;
    public string roomName;

    public ActivityType activityType;
    public RoomType type;
    public MonsterType monsterTypeOfRoom;
    public RoomPlacement[] roomPlacement = new RoomPlacement[0];
    public SO_Food foodAssigned;
    public string monsterID;

    public SO_Monster monsterDataCurrentCustomer;

    public int maxUsers;
    public int currentUsers;

    public int cost;

    public GameObject roomObject;
    public List<TargetInRoom> targets;

    public int level = 0;

    public Room( SO_RoomType roomType, Vector3Int positionInGrid, string roomId, int level = 0 )
    {
        this.roomType = roomType;
        this.positionInGrid = positionInGrid;
        this.roomSize = roomType.roomSize;
        this.roomName = roomType.roomName;
        this.type = roomType.roomType;
        this.maxUsers = roomType.maxUsers;
        this.roomID = roomId;
        this.level = level;
        
        if ( roomType.roomType == RoomType.ACTIVITY )
        {
            this.activityType = roomType.activityType;
        }

        this.targets = new List<TargetInRoom>();

        this.cost = roomType.cost;
        this.roomObject = roomType.prefab;
    }

    public void AddRoomPlacement( RoomPlacement newPlacement )
    {
        // Cr�er un nouveau tableau avec une taille augment�e de 1
        RoomPlacement[] newPlacements = new RoomPlacement[roomPlacement.Length + 1];

        // Copier les �l�ments existants
        for (int i = 0; i < roomPlacement.Length; i++)
        {
            newPlacements[i] = roomPlacement[i];
        }

        // Ajouter le nouvel �l�ment
        newPlacements[roomPlacement.Length] = newPlacement;

        // Assigner le nouveau tableau
        roomPlacement = newPlacements;
    }

    public void RemoveRoomPlacement( RoomPlacement placementToRemove )
    {
        // Trouver l'index de l'�l�ment � supprimer
        int index = Array.IndexOf( roomPlacement, placementToRemove );
        if (index < 0) return; // Si l'�l�ment n'est pas trouv�, ne rien faire

        // Cr�er un nouveau tableau avec une taille r�duite de 1
        RoomPlacement[] newPlacements = new RoomPlacement[roomPlacement.Length - 1];

        // Copier les �l�ments en omettant l'�l�ment � supprimer
        for (int i = 0, j = 0; i < roomPlacement.Length; i++)
        {
            if (i == index) continue;
            newPlacements[j++] = roomPlacement[i];
        }

        // Assigner le nouveau tableau
        roomPlacement = newPlacements;
    }
}

[Serializable]
public class TargetInRoom
{
    [field: SerializeField]
    public Vector3 target { get; private set; }
    [field: SerializeField]
    public bool isOccupied { get; private set; }

    public TargetInRoom(Vector3 target, bool isOccupied)
    {
        this.target = target;
        this.isOccupied = isOccupied;
    }

    public void SetIsOccupied(bool isOccupied)
    {
        this.isOccupied = isOccupied;
    }
}