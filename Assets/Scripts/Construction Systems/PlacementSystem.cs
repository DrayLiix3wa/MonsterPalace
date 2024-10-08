using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using System.Linq;

public class PlacementSystem : MonoBehaviour
{
    [SerializeField]
    private GameObject _mouseIndicator;
    [SerializeField]
    private InputManager _inputManager;
    [SerializeField]
    private Grid _grid;
    [SerializeField]
    private SO_Hotel _hotel;
    [SerializeField]
    private SO_RoomType _dataBase;
    [SerializeField]
    private SO_RoomType _stairRoom;

    private SO_RoomType _selectedRoom;

    [SerializeField]
    private GameObject gridVisualization;

    private GameObject _roomIndicator;
    private PreviewRoom _previewRoom;
    private MoneyManager _moneyManager;

    public event Action OnRoomPlaced, OnStairPlaced;
    public UnityEvent OnRoomBuild = new UnityEvent();
    public UnityEvent OnStairBuild = new UnityEvent();
    public UnityEvent OnNoEnoughMoney = new UnityEvent();
    public bool IsPlacingRoom => _selectedRoom != null;

    private void Start()
    {
        StopPlacement();
        _mouseIndicator.SetActive(false);
        _moneyManager = FindObjectOfType<MoneyManager>();
    }

    public void StartPlacement(SO_RoomType room)
    {
        StopPlacement();

        _selectedRoom = room;
        gridVisualization.SetActive(true);

        _roomIndicator = Instantiate(_selectedRoom.prefab, _mouseIndicator.transform.position, Quaternion.identity);
        _previewRoom = _roomIndicator.GetComponentInChildren<PreviewRoom>();


        _inputManager.OnClicked += PlaceRoom;
        _inputManager.OnExit += StopPlacement;
    }

    private void PlaceRoom()
    {
        if (_inputManager.IsPointerOverUI())
            return;

        if (!CheckIfEnoughMoney(_selectedRoom.cost))
        {
            return;
        }

        Vector3 mousePosition = _inputManager.GetSelectedMapPosition();
        Vector3Int gridPosition = _grid.WorldToCell(mousePosition);

        Bounds localBounds = _grid.GetBoundsLocal(gridPosition, _selectedRoom.roomSize);
        if (IsPositionFree(localBounds))
        {
            if (IsRoomNearby(localBounds, gridPosition.y, _selectedRoom.roomSize.y, out string dir))
            {
                GameObject newRoom = Instantiate(_selectedRoom.prefab);
                newRoom.transform.position = _grid.CellToWorld(gridPosition);
                newRoom.name = newRoom.GetInstanceID().ToString();

                Room room = new Room(_selectedRoom, gridPosition, newRoom.GetInstanceID().ToString());

                foreach (Transform child in newRoom.transform)
                {
                    if (child.CompareTag("Target"))
                    {
                        room.targets.Add(new TargetInRoom(child.position, false));
                    }
                }

                room.AddRoomPlacement(gridPosition.y < 0 ? RoomPlacement.UNDERGROUND : RoomPlacement.OVERGROUND);
                room.AddRoomPlacement(gridPosition.y < 0 ? RoomPlacement.DARK : RoomPlacement.LIGHT);

                newRoom.GetComponent<RoomController>().ToggleLights();

                _hotel.AddRoom(room);
                _moneyManager.PayRoom( _selectedRoom.cost );

                OnRoomPlaced?.Invoke();
                OnRoomBuild.Invoke();
            } else
            {
                Debug.Log("Room is too far from other rooms");
            }
        }
        else
        {
            Debug.Log("Position is not free");
        }
    }

    public void CancelPlacement()
    {
       StopPlacement();
    }

    private void StopPlacement()
    {
        _selectedRoom = null;

        if (_previewRoom != null)
            _previewRoom.DisablePreview();
        _previewRoom = null;

        if (_roomIndicator != null)
            Destroy(_roomIndicator);

        //gridVisualization.SetActive(false);
        _inputManager.OnClicked -= PlaceRoom;
        _inputManager.OnExit -= StopPlacement;

    }

    private void Update()
    {
        if ( _selectedRoom == null )
            return;

        Vector3 mousePosition = _inputManager.GetSelectedMapPosition();
        Vector3Int gridPosition = _grid.WorldToCell( mousePosition );

        _roomIndicator.transform.position = _grid.CellToWorld( gridPosition );

        Bounds localBounds = _grid.GetBoundsLocal(gridPosition, _selectedRoom.roomSize);

        if ( !IsPositionFree( localBounds ) )
        {
            _previewRoom.EnablePreview();
        }
        else
        {
            if ( IsRoomNearby( localBounds, gridPosition.y, _selectedRoom.roomSize.y, out string dir ) )
            {
                _previewRoom.DisablePreview();
            } else
            {
                _previewRoom.EnablePreview();
            }

        }
    }

    private bool IsPositionFree( Bounds newRoomBounds )
    {
        newRoomBounds.Expand(-0.1f);
        foreach (Room room in _hotel.rooms)
        {
            Bounds existingRoomBounds = _grid.GetBoundsLocal( room.positionInGrid, room.roomType.roomSize );
            if ( newRoomBounds.Intersects( existingRoomBounds ) )
                return false;
        }
        return true;
    }

    private bool IsRoomNearby(Bounds newRoomBounds, int y, int roomSizeY, out string direction)
    {
        Dictionary<string, Bounds> directionalBounds = GetDirectionalBounds(newRoomBounds);

        bool negatif = y < 0;
        string position = y < 0 ? "top" : "bottom";

        if ( negatif == true )
        {
            if ((y / 5) < FindStageLevel(negatif))
            {
                direction = "";
                return false;
            }
        }
        else
        {
            if ((y / 5) > FindStageLevel(negatif))
            {
                direction = "";
                return false;
            }
        }

        int mask = 0;

        foreach (Room room in _hotel.rooms)
        {
            GameObject gameObject = GameObject.Find(room.roomID);

            Bounds existingRoomBounds = new Bounds(gameObject.transform.TransformPoint( gameObject.GetComponent<BoxCollider>().center ), gameObject.GetComponent<BoxCollider>().size);

            if (GetSubStage(y) == "GROUND" )
            {
                if (directionalBounds["right"].Intersects(existingRoomBounds))
                {
                    mask += 1; 
                }else if (directionalBounds["left"].Intersects(existingRoomBounds))
                {
                    mask += 4;
                }
            }
            else
            {
                if ( directionalBounds["right"].Intersects(existingRoomBounds ))
                {
                    mask += 1;
                }
                else if (directionalBounds["left"].Intersects(existingRoomBounds))
                {
                    mask += 4;

                } else if (directionalBounds["bottom"].Intersects(existingRoomBounds))
                {
                    mask += 8;

                }else if (directionalBounds["top"].Intersects(existingRoomBounds))
                {
                    mask += 2;
                }
            }
        }
        
        if ( GetSubStage(y) == "GROUND" && !negatif )
        {
            if (mask == 1)
            {
                direction = "right";
                return true;

            }
            else if (mask == 4)
            {
                direction = "left";
                return true;
            }
        }
        else if ( !negatif )
        {
            if ( mask == 12 )
            {
                direction = "left";
                return true;
            }else if ( mask == 9 )
            {
                direction = "right";
                return true;
            }
        }

        if ( negatif )
        {
            if (mask == 6)
            {
                direction = "left";
                return true;
            }else if (mask == 3)
            {
                direction = "right";
                return true;
            }
        }

        direction = "";

        return false;
    }

    private string IntersectsAxisX(Dictionary<string, Bounds> directionalBounds, Bounds existingRoomBounds )
    {
        string direction = "";

        if (directionalBounds["right"].Intersects(existingRoomBounds))
        {
            direction = "right";
        }
        else if (directionalBounds["left"].Intersects(existingRoomBounds))
        {
            direction = "left";
        }else if (directionalBounds["left"].Intersects(existingRoomBounds) && directionalBounds["right"].Intersects(existingRoomBounds))
        {
            direction = "both";
        }

        return direction;
    }

    private string GetStage(int y)
    {
        if (y < 0)
            return "UNDERGROUND";
        else
            return "UPPERGROUND";
    }

    private string GetSubStage(int y)
    {
        if (y < 1 && y > -4)
            return "GROUND";
        else
        {
            return "NONE";
        }
    }

    private Dictionary<string, Bounds> GetDirectionalBounds(Bounds bounds)
    {
        return new Dictionary<string, Bounds>
        {
            {"top", new Bounds(
                new Vector3(bounds.center.x,  (bounds.size.z / 2f) + bounds.center.z, bounds.center.y),
                new Vector3(0.5f, 0.5f, 0.5f)
                )},
            {"bottom", new Bounds(
                new Vector3(bounds.center.x,  bounds.center.z - (bounds.size.z / 2f), bounds.center.y),
                new Vector3(0.5f, 0.5f, 0.5f)
                )},
            {"left", new Bounds(
                new Vector3(bounds.center.x - (bounds.size.z / 1.3f), bounds.center.z, bounds.center.y),
                new Vector3(0.5f, 0.5f, 0.5f)
                )},
            {"right", new Bounds(
                new Vector3(bounds.center.x + (bounds.size.z / 1.3f), bounds.center.z, bounds.center.y),
                new Vector3(0.5f, 0.5f, 0.5f)
                )}
        };
    }

    public void AddUnderStaire()
    {
        List<Room> baseRooms = _hotel.rooms.FindAll(room => room.roomType.roomType == RoomType.BASE);

        if ( baseRooms.Count == 0 )
        {
            Console.WriteLine("Aucune pi�ce de type BASE trouv�e.");
            return;
        }

        if (!CheckIfEnoughMoney(_stairRoom.cost))
        {
            return;
        }

        int newPosY = 0;
        int level = 0;

        level = baseRooms.Min(x => x.level) + -1;
        newPosY = -_stairRoom.roomSize.y * Math.Abs(level);

        Vector3Int position = new Vector3Int( 0, newPosY, 0 );

        GameObject roomInstance = Instantiate( _stairRoom.prefab, position, Quaternion.identity );
        roomInstance.name = roomInstance.GetInstanceID().ToString();

        // On r�cup�re la pi�ce la plus proche du haut pour ouvire son sol et acc�der � l'escalier
        Room close_room = _hotel.rooms.Find(x => x.level == baseRooms.Min(x => x.level) && x.roomType.roomType == RoomType.BASE);
        GameObject closeRoom = GameObject.Find(close_room.roomID);

        if ( close_room.level == 0 )
        {
            closeRoom.GetComponent<StairCaseController>().DesactivateStairWall();
            closeRoom.GetComponent<StairCaseController>().DesactivateGround();
            roomInstance.GetComponent<StairCaseController>().ActivateStarMiniWall();

            if (CheckIfBaseRoomUpper(0))
            {
                roomInstance.GetComponent<StairCaseController>().DesactivateStarMiniWall();
            }

        }
        else
        {
            closeRoom.GetComponent<StairCaseController>().DesactivateGround();
            roomInstance.GetComponent<StairCaseController>().DesactivateStarMiniWall();
        }

        roomInstance.GetComponent<StairCaseController>().ActivateStair();
        roomInstance.GetComponent<StairCaseController>().ActivateGround();

        _hotel.rooms.Add(new Room( _stairRoom, position, roomInstance.name, level ));
        _moneyManager.PayRoom(_stairRoom.cost);

        OnStairBuild.Invoke();
        OnStairPlaced?.Invoke();
    }

    public void AddUpperStaire()
    {
        List<Room> baseRooms = _hotel.rooms.FindAll(room => room.roomType.roomType == RoomType.BASE);
        if (baseRooms.Count == 0)
        {
            Console.WriteLine("Aucune pi�ce de type BASE trouv�e.");
            return;
        }
        if (!CheckIfEnoughMoney(_stairRoom.cost))
        {
            return;
        }
        int newPosY = 0;
        int level = baseRooms.Max(x => x.level) + 1;
        newPosY = _stairRoom.roomSize.y * level;
        Vector3Int position = new Vector3Int(0, newPosY, 0);
        GameObject roomInstance = Instantiate(_stairRoom.prefab, position, Quaternion.identity);
        roomInstance.name = roomInstance.GetInstanceID().ToString();

        // On r�cup�re la pi�ce la plus proche du haut pour ouvrir son sol et acc�der � l'escalier
        Room close_room = _hotel.rooms.Find(x => x.level == baseRooms.Max(x => x.level) && x.roomType.roomType == RoomType.BASE);
        GameObject closeRoom = GameObject.Find(close_room.roomID);
        closeRoom.GetComponent<StairCaseController>().ActivateStair();

        // Gestion du miniwall pour la pi�ce actuelle et celle juste en dessous
        if (level == 1)
        {
            // on r�cup�re la piece du -1
            Room oneLevelBelow = _hotel.rooms.Find(x => x.level == level - 1 && x.roomType.roomType == RoomType.BASE);
            if (oneLevelBelow != null)
            {

                GameObject oneLevelBelowRoom = GameObject.Find(oneLevelBelow.roomID);
                oneLevelBelowRoom.GetComponent<StairCaseController>().DesactivateStarMiniWall();
            }

            if (CheckIfBaseRoomBelow(0))
            {
                Debug.Log("Il y a une pi�ce de type BASE en dessous de 0");
                GetBaseRoomLevel(-1).GetComponent<StairCaseController>().DesactivateStarMiniWall();
            }

            closeRoom.GetComponent<StairCaseController>().DesactivateStairWall();
            closeRoom.GetComponent<StairCaseController>().ActivateStarMiniWall();
        }
        else if (level >= 1)
        {
            closeRoom.GetComponent<StairCaseController>().ActivateStarMiniWall();

            // Logique pour g�rer le miniwall de la pi�ce deux niveaux en dessous
            if (level >= 2)
            {
                Room twoLevelsBelow = _hotel.rooms.Find(x => x.level == level - 2 && x.roomType.roomType == RoomType.BASE);
                if (twoLevelsBelow != null)
                {
                    GameObject twoLevelsBelowRoom = GameObject.Find(twoLevelsBelow.roomID);
                    twoLevelsBelowRoom.GetComponent<StairCaseController>().DesactivateStarMiniWall();
                    Debug.Log("Miniwall d�sactiv� pour le niveau " + twoLevelsBelow.level);
                }
            }

        }

        roomInstance.GetComponent<StairCaseController>().DesactivateStarMiniWall();
        roomInstance.GetComponent<StairCaseController>().DesactivateGround();
        _hotel.rooms.Add(new Room(_stairRoom, position, roomInstance.name, level));
        _moneyManager.PayRoom(_stairRoom.cost);
        OnStairBuild.Invoke();
        OnStairPlaced?.Invoke();
    }


    private bool CheckIfBaseRoomBelow(int level)
    {
        List<Room> baseRooms = _hotel.rooms.FindAll(room => room.roomType.roomType == RoomType.BASE);
        return baseRooms.Any(x => x.level == level - 1);
    }

    private bool CheckIfBaseRoomUpper(int level)
    {
        List<Room> baseRooms = _hotel.rooms.FindAll(room => room.roomType.roomType == RoomType.BASE);
        return baseRooms.Any(x => x.level == level + 1);
    }

    private GameObject GetBaseRoomLevel(int level)
    {   Room room = _hotel.rooms.Find(x => x.level == level && x.roomType.roomType == RoomType.BASE);

        return GameObject.Find(room.roomID);
    }

    private int FindStageLevel(bool negatif)
    {
        List<Room> baseRooms = _hotel.rooms.FindAll(room => room.roomType.roomType == RoomType.BASE);

        if (negatif)
        {
            return baseRooms.Min(x => x.level);

        }
        else
        {
            return baseRooms.Max(x => x.level);
        }

    }

    public void ToggleGridVisualization()
    {
        gridVisualization.SetActive( !gridVisualization.activeSelf );
    }

    private bool CheckIfEnoughMoney(float cost)
    {
        if ( _moneyManager.playerMoney < cost )
        {
            OnNoEnoughMoney.Invoke();
            Debug.Log("Not enough money");
            return false ;
        }
        else
        {
            return true;
        }
    }
}
