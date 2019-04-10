﻿
using System;
using System.Runtime.InteropServices.ComTypes;
using System;
using System.Collections.Generic;

[Serializable]
public static class StaticInfo
{
    public static String name;
    public static String roomNumber;
    public static String numberOfPlayer;
    public static String level;
    public static JSONObject game_info;
    public static int[] Location;

    public static Role role=Role.None;

    public static String numOfHazmat;
    public static String numOfHotspot;

    public static bool StartingPosition=true;//change to false after wards
    public static bool StartingAmbulancePosition=false;
    public static bool StartingEnginePosition=false;
    public static Boolean LoadGame = false;
    public static Dictionary<int[], int> hWallMemo;
    public static Dictionary<int[], int> vWallMemo;
    public static int[,] tiles;
    public static Dictionary<int[], int> defaultHorizontalDoors;
    public static Dictionary<int[], int> defaultVerticalDoors;
    public static Dictionary<int[], int> poi;
    public static Dictionary<int[], int> movingPOI;
    public static Dictionary<int[], int> treatedPOI;
    public static int random;

    public static POIManager poiManager;
    
    public static String joinedPlayers;
    public static String confirmedPosition;
    public static String selectedRoles;
}
