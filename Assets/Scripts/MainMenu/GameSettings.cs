using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class GameSettings 
{
    public static List<Setting> settingsList = new List<Setting>();

    public static void AddSetting(Setting setting)
    {
        settingsList.Add(setting);
        //Debug.Log(setting.playerName + " + " + setting.selectedType + " + " + setting.selectedColor);
    }
    //public static void ReadSetting()
    //{
    //}
}
public class Setting
{
    //Õ¿—“–Œ… » Œ¡ »√–Œ ¿’
    public string playerName;
    public int selectedType;
    public int selectedColor;
    public Setting(string _name, int type, int color)
    {
        playerName = _name;
        selectedType = type;
        selectedColor = color;
    }
}

