using Michsky.UI.Dark;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class RoomBtnUI : MonoBehaviour
{
    [SerializeField]
    private ButtonManager _name;
    [SerializeField]
    private TextMeshProUGUI _price;
    [SerializeField]
    private TextTraduction _traduction;
    public Button button;
    public Image image;

    public void SetName(string name)
    {
        //_name.buttonText = name.ToUpper();
        _traduction.AssignID(name);
        _name.UpdateUI();
    }

    public void SetPrice(int price)
    {
        _price.text = price.ToString() +" $";
    }

    public void SetPriceColor(Color color)
    {
        _price.color = color;
    }
}
