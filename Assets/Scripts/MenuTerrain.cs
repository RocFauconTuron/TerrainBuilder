using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
public class MenuTerrain : MonoBehaviour
{
    [SerializeField] Slider sizeSlider;
    [SerializeField] Slider strenghtSlider;
    [SerializeField] Slider flatSlider;
    [SerializeField] TMP_Dropdown dropdown;
    [SerializeField] GameObject menuPaint;
    [SerializeField] GameObject menuFlat;
    [SerializeField] GameObject menuGrass;
    public int brushIndex;

    private void Start()
    {
        dropdown.onValueChanged.AddListener(OnDropdownValueChanged);
    }
    void OnDropdownValueChanged(int value)
    {
        switch (value)
        {
            case 2:
                menuPaint.SetActive(true);
                menuFlat.SetActive(false);
                menuGrass.SetActive(false);
                break;
            case 4:
                menuPaint.SetActive(false);
                menuFlat.SetActive(false);
                menuGrass.SetActive(true);
                break;
            case 5:
                menuPaint.SetActive(false);
                menuFlat.SetActive(true);
                menuGrass.SetActive(false);
                break;
            default:
                menuPaint.SetActive(false);
                menuFlat.SetActive(false);
                menuGrass.SetActive(false);
                break;
        } 
        if(value == 2)
        {
            menuPaint.SetActive(true);
        }
        else
        {
            menuPaint.SetActive(false);
        }
    }
    public int GetSize()
    {
        return Mathf.RoundToInt(sizeSlider.value);
    }
    public float GetStrenght()
    {
        return strenghtSlider.value;
    }
    public float GetFlat()
    {
        return flatSlider.value;
    }
    public void SetFlat(float value)
    {
        flatSlider.value = value;
    }
    public int GetMode()
    {
        return dropdown.value;
    }
    public void OnButtonPaint(int index)
    {
        brushIndex = index;
    }
}
