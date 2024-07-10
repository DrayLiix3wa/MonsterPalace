using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DailyTaxes : MonoBehaviour
{
    public DayNightCycle cycle;
    public ArgentSO argent;

    [SerializeField]
    private int taxeHour = 8;

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

    void Update()
    {
        if (cycle.currentHour == taxeHour && !taxed)
        {
            Taxes();
        }

        if (cycle.currentHour == taxeHour + 1)
        {
            taxed = false;
        }
    }

    public void Taxes()
    {
        argent.playerMoney -= basicRoomTaxes * basicRoomNumber;
        argent.playerMoney -= specialRoomTaxes * specialRoomNumber;
        argent.playerMoney -= activityRoomTaxes * activityRoomNumber;
        taxed = true;
        Debug.Log("Payement quotidien effectu�!");
        Debug.Log(argent.playerMoney);
    }
}
