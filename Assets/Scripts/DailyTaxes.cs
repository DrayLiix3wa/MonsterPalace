using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class DailyTaxes : MonoBehaviour
{
    private DayNightCycle _cycle;
    private MoneyManager _moneyManager;
    private HotelController _hotel;

    [SerializeField]
    private int taxeHour = 8;

    [SerializeField] bool useTaxeDay = false;
    [SerializeField] private string taxeDay = "Monday";

    private bool taxed = false;

    [Header("Room Number")]
    [Tooltip("Nombre de pi�ces de chaque type pr�sentes dans l'h�tel")]
    public int basicRoomNumber;
    public int specialRoomNumber;
    public int activityRoomNumber;

    [Header("Room Taxes")]
    [Tooltip("Co�t quotidien pour chaque type de pi�ces")]
    public float basicRoomTaxes = 12f;
    public float specialRoomTaxes = 16f;
    public float activityRoomTaxes = 22f;

    public UnityEvent OnTaxesPaid = new UnityEvent();

    private void Start()
    {
        _cycle = FindObjectOfType<DayNightCycle>();
        _moneyManager = FindObjectOfType<MoneyManager>();
        _hotel = FindObjectOfType<HotelController>();
    }

    void Update()
    {
        if (useTaxeDay)
        {
            if (_cycle.currentDayOfTheWeek == taxeDay && !taxed)
            {
                Taxes();
            }

            if (_cycle.currentDayOfTheWeek != taxeDay)
            {
                taxed = false;
            }
        }
        else
        {
            if (_cycle.currentHour == taxeHour && !taxed)
            {
                Taxes();
            }

            if (_cycle.currentHour == taxeHour + 1)
            {
                taxed = false;
            }
        }
    }

    public void Taxes()
    {

        _moneyManager.PayTaxe(basicRoomTaxes * FindCountRoomByType(RoomType.BASE));
        _moneyManager.PayTaxe(specialRoomTaxes * FindCountRoomByType(RoomType.BEDROOM));
        _moneyManager.PayTaxe(activityRoomTaxes * FindCountRoomByType(RoomType.ACTIVITY));

        taxed = true;

        OnTaxesPaid.Invoke();

        //Debug.Log("Payement quotidien effectu�!");
    }

    public int FindCountRoomByType(RoomType type)
    {
        int count = _hotel._hotel.rooms.FindAll(room => room.type == type).Count;

        if ( count == 0 )
        {
            //Debug.LogWarning("Aucune pi�ce de type " + type + " n'a �t� trouv�e.");
            return 0;
        }
        else
        {
            return count;
        }
    }
}
