﻿using System;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using System.Collections;
using SocketIO;
using UnityEngine.EventSystems;

public class OperationManager
{
    public GameManager gm;

    public List<Button> prefabs;

    List<Operation> possibleOp = new List<Operation>();
    List<Button> buttons = new List<Button>();

    public GameObject opPanel;

    public List<GameObject> options;

    private int x, z;

    public SocketIOComponent socket;

    public bool inCommand = false;
    public Fireman controlled;
    public int commandMoves = 1;

    public bool askingForRide=false;

    public OperationManager(GameManager gm)
    {
        this.gm = gm;

        prefabs = gm.prefabs;
        opPanel = gm.opPanel;
        options = gm.options;

        opPanel.SetActive(false);


    }

    public void OnSelectOption(string str,Vector3 position){
        
        // gm.tooltipPanel.transform.position = new Vector3(position.x,position.y+25,position.z);
        gm.tooltipPanel.SetActive(true);
        gm.tooltipPanel.transform.position=new Vector3(position.x,position.y,position.z+2);
        gm.tooltip.text=str;
    }

    public void OnMouseExit(){
        gm.tooltipPanel.SetActive(false);
    }
    
    public void selectTile(int x, int z)
    {
        DestroyAll();
        opPanel.SetActive(false);
        Debug.Log(buttons.Count);

        this.x = x;
        this.z = z;

        possibleOp = new List<Operation>();

        getPossibleOps();

        Debug.Log(possibleOp.Count);

        if(possibleOp.Count>0) opPanel.SetActive(true);

        for(int i = 0; i < possibleOp.Count; i++)
        {
            Button newObject = gm.instantiateOp(possibleOp[i].prefab, options[i].transform,true);
            EventTrigger trigger= newObject.gameObject.GetComponent<EventTrigger>();
            EventTrigger.Entry entry= new EventTrigger.Entry();
            EventTrigger.Entry exit=new EventTrigger.Entry();
            entry.eventID = EventTriggerType.PointerEnter;
            entry.callback = new EventTrigger.TriggerEvent();
            exit.eventID = EventTriggerType.PointerExit;
            exit.callback = new EventTrigger.TriggerEvent();

            string tip="";
            Vector3 position=options[i].transform.position;
            
            switch (possibleOp[i].type) {
                case OperationType.Move:
                    newObject.onClick.AddListener(move);
                    tip="Move";
                    break;
                case OperationType.Treat:
                    newObject.onClick.AddListener(treat);
                    tip="Treat";
                    break;
                case OperationType.CarryV:
                    newObject.onClick.AddListener(carryV);
                    tip="Carry Victim";
                    break;
                case OperationType.LeadV:
                    newObject.onClick.AddListener(leadV);
                    tip="Lead Victim";
                    break;
                case OperationType.CarryHazmat:
                    newObject.onClick.AddListener(carryHazmat);
                    tip="Carry Hazmat";
                    break;
                case OperationType.RemoveHazmat:
                    newObject.onClick.AddListener(removeHazmat);
                    tip="Remove Hazmat";
                    break;
                case OperationType.Command:
                    newObject.onClick.AddListener(command);
                    tip="Command";
                    break;
                case OperationType.Imaging:
                    newObject.onClick.AddListener(imaging);
                    tip="Imaging";
                    break;
                case OperationType.ExtingSmoke:
                    newObject.onClick.AddListener(extingSmoke);
                    tip="Extinguish Once";
                    break;
                case OperationType.ExtingFire:
                    newObject.onClick.AddListener(extingFire);
                    tip="Extinguish Twice";
                    break;
                case OperationType.Drive:
                    newObject.onClick.AddListener(drive);
                    tip="Drive";
                    break;
                case OperationType.Remote:
                    newObject.onClick.AddListener(remote);
                    tip="Call Over";
                    break;
                case OperationType.Ride:
                    newObject.onClick.AddListener(ride);
                    tip="Ride";
                    break;
                //case OperationType.StopDrive:
                    //newObject.onClick.AddListener(stopDrive);
                    //break;
                //case OperationType.GetOff:
                    //newObject.onClick.AddListener(getOff);
                    //break;
                case OperationType.DeckGun:
                    newObject.onClick.AddListener(deckGun);
                    tip="Fire DeckGun";
                    break;
                case OperationType.DropV:
                    newObject.onClick.AddListener(dropeV);
                    tip="Drop Victim";
                    break;
                case OperationType.DropHazmat:
                    newObject.onClick.AddListener(dropHazmat);
                    tip="Drop Hazmat";
                    break;
                case OperationType.StopCommand:
                    newObject.onClick.AddListener(stopCommand);
                    tip="Stop Command";
                    break;
            }
            UnityEngine.Events.UnityAction<BaseEventData> l_callback = new UnityEngine.Events.UnityAction<BaseEventData>((eventData)=>OnSelectOption(tip,position));
            entry.callback.AddListener(l_callback);
            UnityEngine.Events.UnityAction<BaseEventData> exit_callback = new UnityEngine.Events.UnityAction<BaseEventData>((eventData)=>OnMouseExit());
            exit.callback.AddListener(exit_callback);
            trigger.triggers.Add(entry);
            trigger.triggers.Add (exit);
            buttons.Add(newObject);
        }
    }

    void getPossibleOps()
    {

        int currentX = gm.tileMap.selectedUnit.currentX/6;
        int currentZ = gm.tileMap.selectedUnit.currentZ/6;

        Debug.Log(currentX);
        Debug.Log(currentZ);

        int diffX = Math.Abs(x - currentX);
        int diffZ = Math.Abs(z - currentZ);

        Debug.Log(diffX);
        Debug.Log(diffZ);

        Boolean extingFire = false;
        Boolean extingsmoke = false;

        if (diffX + diffZ == 0) //same tile (hazmat, poi)
        {
            int[] key = new int[] { x, z };
            Fireman fireman = gm.fireman;
            Debug.Log("same place");
            Debug.Log(gm.pOIManager.placedPOI.Count);
            //------ POI---------------
            if (gm.pOIManager.containsKey(key[0], key[1], gm.pOIManager.placedPOI))
            {
                if (!fireman.carryingVictim)
                {
                    Operation op = new Operation(this, OperationType.CarryV);
                    possibleOp.Add(op);
                }
                if (fireman.role == Role.Paramedic && fireman.FreeAP >= 1)
                {
                    Operation op = new Operation(this, OperationType.Treat);
                    possibleOp.Add(op);
                }
            }

            if (gm.pOIManager.containsKey(key[0], key[1], gm.pOIManager.treated))
            {
                Debug.Log("has treated");
                POI p = gm.pOIManager.getPOI(key[0], key[1], gm.pOIManager.treated);
                if (p.status == POIStatus.Treated)
                {
                    Operation op = new Operation(this, OperationType.LeadV);
                    possibleOp.Add(op);
                }
            }

            if (!gm.pOIManager.containsKey(key[0], key[1], gm.pOIManager.placedPOI)&&!gm.pOIManager.containsKey(key[0], key[1], gm.pOIManager.treated))
            {
                if (fireman.carriedPOI != null || fireman.ledPOI != null)
                {
                    Operation op = new Operation(this, OperationType.DropV);
                    possibleOp.Add(op);
                }

            }

            //------Hazmat------------
            if (gm.hazmatManager.containsKey(key[0], key[1], gm.hazmatManager.placedHazmat))
            {
                if (fireman.role == Role.HazmatTech&&fireman.FreeAP>=2)
                {
                    Operation op = new Operation(this, OperationType.RemoveHazmat);
                    possibleOp.Add(op);
                }
                if (!fireman.carryingVictim)
                {
                    Operation op = new Operation(this, OperationType.CarryHazmat);
                    possibleOp.Add(op);
                }
            }

            if(!gm.hazmatManager.containsKey(key[0], key[1], gm.hazmatManager.placedHazmat)&&!gm.hazmatManager.containsKey(key[0],key[1],gm.hazmatManager.placedHotspot))
            {
                if (fireman.carriedHazmat != null)
                {
                    Operation op = new Operation(this, OperationType.DropHazmat);
                    possibleOp.Add(op);
                }
            }

            if (gm.tileMap.tiles[x, z] == 3 ) // fire deck gun && ride
            {
                double vx = (double)gm.enG.x / 6;
                double vz = (double)gm.enG.z / 6;

                // Debug.Log("int same place deck gun");

                if (Math.Abs(currentX - vx) < 1 && (Math.Abs(currentZ - vz) < 1)&&!StaticInfo.level.Equals("Family"))
                {

                    int requiredAP=4;
                    if(fireman.role==Role.Driver){
                        requiredAP=2;
                    }
                    //if (fireman.driving)
                    //{
                    //    Operation op = new Operation(this, OperationType.StopDrive);
                    //    possibleOp.Add(op);
                    //}

                    int minX=0, maxX = 0;
                    int minZ=0, maxZ = 0;
                    if (x < 5 && z < 4)
                    {
                        minX = 1;
                        maxX = 4;
                        minZ = 1;
                        maxZ = 3;
                    }
                    else if (x >= 5 && z < 4)
                    {
                        minX = 5;
                        maxX = 8;
                        minZ = 1;
                        maxZ = 3;
                    }
                    else if (x >= 5 && z >= 4)
                    {
                        minX = 5;
                        maxX = 8;
                        minZ = 4;
                        maxZ = 6;
                    }
                    else if (x < 5 && z >= 4)
                    {
                        minX = 1;
                        maxX = 4;
                        minZ = 4;
                        maxZ = 6;
                    }
                    bool existsF = false;
                    foreach (JSONObject o in gm.players.Values)
                    {
                        for(int i = minX; i <= maxX; i++)
                        {
                            for(int j = minZ; j <= maxZ; j++)
                            {
                                if (o["Location"].Equals("\""+i * 6 + "," + j * 6+"\""))
                                {
                                    existsF = true;
                                    Debug.Log("another fireman here");
                                    break;
                                }
                            }
                            if (existsF) break;
                        }
                        if (existsF) break;
                    }
                    if (!existsF&&fireman.FreeAP>=requiredAP)
                    {
                        Operation op = new Operation(this, OperationType.DeckGun);
                        possibleOp.Add(op);
                    }

                    //if (fireman.riding)
                    //{
                    //    Operation op = new Operation(this, OperationType.GetOff);
                    //    possibleOp.Add(op);
                    //}

                    if (!fireman.riding&&!fireman.driving) // ------------------
                    {
                        Operation op = new Operation(this, OperationType.Ride);
                        possibleOp.Add(op);
                    }
                }
            }

            if(gm.tileMap.tiles[x, z] == 4&&!StaticInfo.level.Equals("Family"))
            {
                double vx = (double)gm.amB.x / 6;
                double vz = (double)gm.amB.z / 6;

                if (fireman.FreeAP >= 2 && (Math.Abs(currentX - vx) != 9 && Math.Abs(currentZ - vz) != 7)&&(Math.Abs(vx-x)>1||Math.Abs(vz-z)>1)){
                    Operation op = new Operation(this, OperationType.Remote);
                    possibleOp.Add(op);
                }
                if (fireman.FreeAP >= 4 && (Math.Abs(currentX - vx) == 9 || Math.Abs(currentZ - vz) == 7)&&(Math.Abs(vx-x)>1||Math.Abs(vz-z)>1))
                {
                    Operation op = new Operation(this, OperationType.Remote);
                    possibleOp.Add(op);
                }
            }

            // command move
            if(inCommand){
                bool moveTo=false;
                if(Math.Abs(controlled.currentX-x)+Math.Abs(controlled.currentZ-z)==1&&(controlled.currentX==x&&controlled.currentZ<z&&(!gm.wallManager.checkIfHWall(x,z)&&!gm.doorManager.checkIfHDoor(x,z)||gm.doorManager.checkIfOpenHDoor(x,z))||controlled.currentX==x&&controlled.currentZ>z&&(!gm.wallManager.checkIfHWall(controlled.currentX,controlled.currentZ)&&!gm.doorManager.checkIfHDoor(controlled.currentX,controlled.currentZ)||gm.doorManager.checkIfOpenHDoor(controlled.currentX,controlled.currentZ))
                ||controlled.currentZ==z&&(controlled.currentX<x&&!gm.wallManager.checkIfVWall(x,z)&&!gm.doorManager.checkIfVDoor(x,z)||gm.doorManager.checkIfOpenVDoor(x,z))||controlled.currentZ==z&&controlled.currentX>x&&(!gm.wallManager.checkIfVWall(controlled.currentX,controlled.currentZ)&&!gm.doorManager.checkIfVDoor(controlled.currentX,controlled.currentZ)||gm.doorManager.checkIfOpenVDoor(controlled.currentX,controlled.currentZ)))){
                    moveTo=true;
                }
                if(gm.tileMap.tiles[x,z]==2&&(controlled.carryingVictim||controlled.ledPOI!=null)) moveTo=false;
                int requiredAP=1;
                if(gm.tileMap.tiles[x,z]==2||controlled.carryingVictim){
                    requiredAP=2;
                }
                if(controlled.carriedPOI!=null&&controlled.role==Role.Dog){
                    requiredAP=4;
                }
                if(moveTo){
                    if(controlled.role==Role.CAFS&&this.commandMoves+fireman.FreeAP>=requiredAP){
                        Operation op=new Operation(this,OperationType.Move);
                        possibleOp.Add(op);
                    }else{
                        if(fireman.FreeAP+fireman.remainingSpecAp>=requiredAP&&controlled.role!=Role.CAFS){
                            Operation op=new Operation(this,OperationType.Move);
                            possibleOp.Add(op);
                        }
                    }
                }
            }

            if(!inCommand){
                if (gm.fireman.role == Role.Captain)
                {
                    foreach(JSONObject o in gm.players.Values)
                    {
                        if (o["Location"].Equals("\""+ x*6 + "," + z*6+"\""))
                        {
                            Operation op = new Operation(this, OperationType.Command);
                            possibleOp.Add(op);
                        }
                    }
                }
            }
            
        }
        else if (diffX + diffZ == 1) // neighbor tile (fire, move)
        {
            Debug.Log("neighbor");

            // check obstable (wall/closed door)
            if (currentX - x == 0)// same column, check above and below
            {
                if (currentZ < z) // below the target
                {
                    int[] key = new int[] { x, z };
                    if (!gm.wallManager.checkIfHWall(key[0], key[1])&& !gm.doorManager.checkIfHDoor(key[0], key[1])||gm.doorManager.checkIfOpenHDoor(key[0],key[1])) // no wall or destroyed wall
                    {
                        if (gm.tileMap.tiles[x, z] >= 1 && gm.tileMap.tiles[x, z] <= 2) 
                        {
                            extingsmoke = true;
                        }

                        if (gm.tileMap.tiles[x, z] == 2)
                        {
                            extingFire = true;
                        }
                    }
                }
                else // above the target
                {
                    int[] key = new int[] { currentX, currentZ };
                    if (!gm.wallManager.checkIfHWall(key[0], key[1])&& !gm.doorManager.checkIfHDoor(key[0], key[1])||gm.doorManager.checkIfOpenHDoor(key[0],key[1])) // no wall or destroyed wall
                    {
                        if (gm.tileMap.tiles[x, z] >= 1 && gm.tileMap.tiles[x, z] <= 2)
                        {
                            extingsmoke = true;
                        }

                        if (gm.tileMap.tiles[x, z] == 2)
                        {
                            extingFire = true;
                        }
                    }
                }
            }
            else if (currentZ == z) // same row, check right and left
            {
                if (currentX < x) // left of the target
                {
                    int[] key = new int[] { x, z };
                    if (!gm.wallManager.checkIfVWall(key[0], key[1])&& !gm.doorManager.checkIfVDoor(key[0], key[1])||gm.doorManager.checkIfOpenVDoor(key[0],key[1])) // no wall or destroyed wall
                    {
                        if (gm.tileMap.tiles[x, z] >= 1 && gm.tileMap.tiles[x, z] <= 2)
                        {
                            extingsmoke = true;
                        }

                        if (gm.tileMap.tiles[x, z] == 2)
                        {
                            extingFire = true;
                        }
                    }
                }
                else // right to the target
                {
                    int[] key = new int[] { currentX, currentZ };
                    if (!gm.wallManager.checkIfVWall(key[0], key[1])&&!gm.doorManager.checkIfVDoor(key[0], key[1])||gm.doorManager.checkIfOpenVDoor(key[0],key[1])) // no wall or destroyed wall
                    {
                        if (gm.tileMap.tiles[x, z] >= 1 && gm.tileMap.tiles[x, z] <= 2)
                        {
                            extingsmoke = true;
                        }

                        if (gm.tileMap.tiles[x, z] == 2)
                        {
                            extingFire = true;
                        }
                    }
                }
            }

            // check for AP

            if (gm.tileMap.selectedUnit.role == Role.Paramedic || gm.tileMap.selectedUnit.role == Role.RescueSpec) // paramedic and rescue spec: double AP
            {
                if (extingsmoke&&gm.tileMap.selectedUnit.FreeAP>=2)
                {
                    Operation op = new Operation(this, OperationType.ExtingSmoke);
                    possibleOp.Add(op);
                }
                if (extingFire && gm.tileMap.selectedUnit.FreeAP >= 4)
                {
                    Operation op = new Operation(this, OperationType.ExtingFire);
                    possibleOp.Add(op);
                }
            }
            else if (gm.tileMap.selectedUnit.role == Role.CAFS) // cafs
            {
                if(extingsmoke && (gm.tileMap.selectedUnit.FreeAP+ gm.tileMap.selectedUnit.remainingSpecAp) >= 1)
                {
                    Operation op = new Operation(this, OperationType.ExtingSmoke);
                    possibleOp.Add(op);
                }
                if (extingFire && (gm.tileMap.selectedUnit.FreeAP + gm.tileMap.selectedUnit.remainingSpecAp) >= 2)
                {
                    Operation op = new Operation(this, OperationType.ExtingFire);
                    possibleOp.Add(op);
                }

            }
            else
            {
                if (extingsmoke && gm.tileMap.selectedUnit.FreeAP >= 1)
                {
                    Operation op = new Operation(this, OperationType.ExtingSmoke);
                    possibleOp.Add(op);
                }
                if (extingFire && gm.tileMap.selectedUnit.FreeAP >= 2)
                {
                    Operation op = new Operation(this, OperationType.ExtingFire);
                    possibleOp.Add(op);
                }
            }

            //----for move
            Boolean moveTo = false;
            int[] keyM = new int[2];
            if (currentX == x) // same column
            {
                if (currentZ < z) // below the target
                {
                    keyM[0] = x;
                    keyM[1] = z;
                }
                else // above
                {
                    keyM[0] = currentX;
                    keyM[1] = currentZ;
                }
                /*
                if (!gm.wallManager.checkIfHWall(keyM[0], keyM[1])) {
                    moveTo = true;
                }
                if (!gm.doorManager.checkIfHDoor(keyM[0], keyM[1]))
                {
                    moveTo = true;
                }
                */
                
                if (!gm.wallManager.checkIfHWall(keyM[0],keyM[1])&&!gm.doorManager.checkIfHDoor(keyM[0],keyM[1])||gm.doorManager.checkIfOpenHDoor(keyM[0],keyM[1]))
                {
                    moveTo = true;
                    
                }
            }
            else // same row
            {
                if (currentX < x) // left
                {
                    keyM[0] = x;
                    keyM[1] = z;
                }
                else
                {
                    keyM[0] = currentX;
                    keyM[1] = currentZ;
                }
                if (!gm.wallManager.checkIfVWall(keyM[0],keyM[1])&&!gm.doorManager.checkIfVDoor(keyM[0],keyM[1])||gm.doorManager.checkIfOpenVDoor(keyM[0],keyM[1]))
                {
                    moveTo = true;
                    
                }
            }
            if (gm.tileMap.tiles[x, z] == 2 && (gm.tileMap.selectedUnit.carryingVictim||gm.tileMap.selectedUnit.ledPOI!=null)) moveTo = false;
            //if (gm.tileMap.selectedUnit.driving) moveTo = false;
            // if (inCommand)
            // {
            //     int distantX = Math.Abs(controlled.currentX/6 - x);
            //     int distantZ = Math.Abs(controlled.currentZ/6 - z);
            //     if (distantX + distantZ != 1)
            //     {
            //         moveTo = false;
            //     }
            // }

            Debug.Log(extingFire);
            Debug.Log(gm.tileMap.selectedUnit.FreeAP);
            Debug.Log(moveTo);
            if (moveTo) {
                Fireman fireman = gm.tileMap.selectedUnit;
                int requiredAP=1;
                if(fireman.carryingVictim||extingFire){
                    requiredAP=2;
                }
                if(fireman.carryingVictim&&fireman.role==Role.Dog){
                    requiredAP=4;
                }
                if (fireman.role == Role.RescueSpec)
                {
                    if (extingFire && fireman.FreeAP + fireman.remainingSpecAp >= 2)
                    {
                        Operation op = new Operation(this, OperationType.Move);
                        possibleOp.Add(op);
                    }else if (!extingFire && fireman.FreeAP + fireman.remainingSpecAp >= 1)
                    {
                        Operation op = new Operation(this, OperationType.Move);
                        possibleOp.Add(op);
                    }
                }
                else
                {
                    Debug.Log("else");
                    if(extingFire && fireman.FreeAP >= requiredAP)
                    {
                        Operation op = new Operation(this, OperationType.Move);
                        possibleOp.Add(op);
                    }else if (!extingFire && fireman.FreeAP >= requiredAP)
                    {

                        Operation op = new Operation(this, OperationType.Move);
                        possibleOp.Add(op);
                    }
                }
            }

            if (gm.tileMap.tiles[x, z] == 3 ) // fire deck gun && ride
            {
                // Debug.Log("in deckgun");
                double vx = (double)gm.enG.x / 6;
                double vz = (double)gm.enG.z / 6;
                Fireman fireman=gm.fireman;

                // Debug.Log(currentX+" "+currentZ);
                // Debug.Log(vx+" "+vz);

                if (Math.Abs(currentX - vx) < 1 && (Math.Abs(currentZ - vz) < 1)&&!StaticInfo.level.Equals("\"Family\""))
                {
                    //if (fireman.driving)
                    //{
                    //    Operation op = new Operation(this, OperationType.StopDrive);
                    //    possibleOp.Add(op);
                    //}
                    // Debug.Log("in parking");

                    int requiredAP=4;
                    if(fireman.role==Role.Driver){
                        requiredAP=2;
                    }

                    int minX=0, maxX = 0;
                    int minZ=0, maxZ = 0;
                    if (x < 5 && z < 4)
                    {
                        minX = 1;
                        maxX = 4;
                        minZ = 1;
                        maxZ = 3;
                    }
                    else if (x >= 5 && z < 4)
                    {
                        minX = 5;
                        maxX = 8;
                        minZ = 1;
                        maxZ = 3;
                    }
                    else if (x >= 5 && z >= 4)
                    {
                        minX = 5;
                        maxX = 8;
                        minZ = 4;
                        maxZ = 6;
                    }
                    else if (x < 5 && z >= 4)
                    {
                        minX = 1;
                        maxX = 4;
                        minZ = 4;
                        maxZ = 6;
                    }
                    // Debug.Log(minX+" "+minZ);
                    bool existsF = false;
                    foreach (JSONObject o in gm.players.Values)
                    {
                        for(int i = minX; i <= maxX; i++)
                        {
                            for(int j = minZ; j <= maxZ; j++)
                            {
                                // Debug.Log(o["Location"]);
                                if (o["Location"].Equals("\""+i * 6 + "," + j * 6+"\""))
                                {
                                    existsF = true;
                                    // Debug.Log("another fireman here");
                                    break;
                                }
                            }
                            if (existsF) break;
                        }
                        if (existsF) break;
                    }
                    // Debug.Log(existsF);
                    if (!existsF&&fireman.FreeAP>=requiredAP)
                    {
                        Operation op = new Operation(this, OperationType.DeckGun);
                        possibleOp.Add(op);
                    }

                    //if (fireman.riding)
                    //{
                    //    Operation op = new Operation(this, OperationType.GetOff);
                    //    possibleOp.Add(op);
                    //}

                    if (!fireman.riding&&!fireman.driving) // ------------------
                    {
                        Operation op = new Operation(this, OperationType.Ride);
                        possibleOp.Add(op);
                    }
                }
            }

            if (gm.tileMap.tiles[x, z] == 4) // ambulance
            {
                double vx = (double)gm.amB.x / 6;
                double vz = (double)gm.amB.z / 6;

                Fireman fireman=gm.fireman;

                if (fireman.FreeAP >= 2 && (Math.Abs(vx - x) != 9 && Math.Abs(z - vz) != 7)&&(Math.Abs(vx-x)>1||Math.Abs(vz-z)>1)&&!StaticInfo.level.Equals("\"Family\""))
                {
                    // if ((Math.Abs(currentX - vx) <= 0.5 && (Math.Abs(currentZ - vz) <= 0.5)))
                    // {
                    //     Operation op = new Operation(this, OperationType.Drive);
                    //     possibleOp.Add(op);
                    // }

                    Operation op1 = new Operation(this, OperationType.Remote);
                    possibleOp.Add(op1);
                }

                if (fireman.FreeAP >= 4 && (Math.Abs(vx - x) == 9 || Math.Abs(vz - z) == 7)&&(Math.Abs(vx-x)>1||Math.Abs(vz-z)>1)&&!StaticInfo.level.Equals("\"Family\""))
                {
                    // if ((Math.Abs(currentX - vx) <= 0.5 && (Math.Abs(currentZ - vz) <= 0.5)))
                    // {
                    //     Operation op = new Operation(this, OperationType.Drive);
                    //     possibleOp.Add(op);
                    // }
                    Operation op1 = new Operation(this, OperationType.Remote);
                    possibleOp.Add(op1);
                }
            }

            if (inCommand)
            {
                if (x == controlled.currentX/6 && z == controlled.currentZ/6)
                {
                    Operation op = new Operation(this, OperationType.StopCommand);
                    possibleOp.Add(op);
                }

                bool moveTo1=false;
                if(Math.Abs(controlled.currentX-x)+Math.Abs(controlled.currentZ-z)==1&&(controlled.currentX==x&&controlled.currentZ<z&&(!gm.wallManager.checkIfHWall(x,z)&&!gm.doorManager.checkIfHDoor(x,z)||gm.doorManager.checkIfOpenHDoor(x,z))||controlled.currentX==x&&controlled.currentZ>z&&(!gm.wallManager.checkIfHWall(controlled.currentX,controlled.currentZ)&&!gm.doorManager.checkIfHDoor(controlled.currentX,controlled.currentZ)||gm.doorManager.checkIfOpenHDoor(controlled.currentX,controlled.currentZ))
                ||controlled.currentZ==z&&controlled.currentX<x&&(!gm.wallManager.checkIfVWall(x,z)&&!gm.doorManager.checkIfVDoor(x,z)||gm.doorManager.checkIfOpenVDoor(x,z))||controlled.currentZ==z&&controlled.currentX>x&&(!gm.wallManager.checkIfVWall(controlled.currentX,controlled.currentZ)&&!gm.doorManager.checkIfVDoor(controlled.currentX,controlled.currentZ)||gm.doorManager.checkIfOpenVDoor(controlled.currentX,controlled.currentZ)))){
                    moveTo1=true;
                }

                if(gm.tileMap.tiles[x,z]==2&&(controlled.carryingVictim||controlled.leadingVictim)) moveTo1=false;

                int requiredAP=1;
                if(gm.tileMap.tiles[x,z]==2||controlled.carryingVictim){
                    requiredAP=2;
                }
                if(controlled.role==Role.Dog&&controlled.carryingVictim){
                    requiredAP=4;
                }
                if(moveTo1){
                    if(controlled.role==Role.CAFS&&this.commandMoves+gm.fireman.FreeAP>=requiredAP){
                        Operation op=new Operation(this,OperationType.Move);
                        possibleOp.Add(op);
                    }else{
                        if(controlled.role!=Role.CAFS&&gm.fireman.FreeAP+gm.fireman.remainingSpecAp>=requiredAP){
                            Operation op=new Operation(this,OperationType.Move);
                            possibleOp.Add(op);
                        }
                    }
                }
                
            }else{
                if (gm.fireman.role == Role.Captain)
                {
                    foreach(JSONObject o in gm.players.Values)
                    {
                        if (o["Location"].Equals( "\""+x*6 + "," + z*6+"\""))
                        {
                            Operation op = new Operation(this, OperationType.Command);
                            possibleOp.Add(op);
                        }
                    }
                }
            }

        }
        else // not neighboring 
        {
            int[] key = new int[] { x, z };
            Fireman fireman = gm.tileMap.selectedUnit;
            if (fireman.role == Role.ImagingTech&&fireman.FreeAP>=1&&gm.pOIManager.containsKey(key[0],key[1],gm.pOIManager.placedPOI)&&gm.pOIManager.getPOI(key[0],key[1], gm.pOIManager.placedPOI).status==POIStatus.Hidden)
            {
                Operation op = new Operation(this, OperationType.Imaging);
                possibleOp.Add(op);
            }

            //command
            if (fireman.role == Role.Captain)
            {
                foreach(JSONObject o in gm.players.Values)
                {
                    if (o["Location"] .Equals( "\""+x*6 + "," + z*6+"\""))
                    {
                        Operation op = new Operation(this, OperationType.Command);
                        possibleOp.Add(op);
                    }
                }
                if (inCommand)
                {
                    int distantX = Math.Abs(controlled.currentX/6 - x);
                    int distantZ = Math.Abs(controlled.currentZ/6-z);

                    Boolean moveTo = false;
                    int[] keyM = new int[2];
                    if (distantX + distantZ == 1)
                    {
                        if (controlled.currentX/6 == x) // same column
                        {
                            if (controlled.currentZ/6 < z) // below the target
                            {
                                keyM[0] = x;
                                keyM[1] = z;
                            }
                            else // above
                            {
                                keyM[0] = controlled.currentX/6;
                                keyM[1] = controlled.currentZ/6;
                            }
                            if (!gm.wallManager.checkIfHWall(keyM[0], keyM[1])&&!gm.doorManager.checkIfHDoor(keyM[0],keyM[1])||gm.doorManager.checkIfOpenHDoor(keyM[0],keyM[1]))
                            {
                                moveTo = true;
                            }
                        }
                        else // same row
                        {
                            if (controlled.currentX/6 < x) // left
                            {
                                keyM[0] = x;
                                keyM[1] = z;
                            }
                            else
                            {
                                keyM[0] = controlled.currentX/6;
                                keyM[1] = controlled.currentZ/6;
                            }
                            if (!gm.wallManager.checkIfVWall(keyM[0], keyM[1])&&!gm.doorManager.checkIfVDoor(keyM[0],keyM[1])||gm.doorManager.checkIfOpenVDoor(keyM[0],keyM[1]))
                            {
                                moveTo = true;
                            }
                        }
                        if (gm.tileMap.tiles[x, z] == 2 && (controlled.carryingVictim||controlled.leadingVictim)) {
                            moveTo = false;
                        }
                        //if (controlled.driving) moveTo = false;
                        int ap = fireman.remainingSpecAp;
                        int requiredAP = 1;
                        if(gm.tileMap.tiles[x, z] == 2 || controlled.carryingVictim)
                        {
                            requiredAP = 2;
                        }
                        if(controlled.role==Role.Dog&&controlled.carryingVictim){
                            requiredAP=4;
                        }
                        if (controlled.role == Role.CAFS&&commandMoves+fireman.FreeAP>=requiredAP) // not commanded before
                        {
                            if (moveTo)
                            {
                                Operation op = new Operation(this, OperationType.Move);
                                possibleOp.Add(op);
                            }
                        }
                        else
                        {
                            if (moveTo && fireman.remainingSpecAp+fireman.FreeAP >= requiredAP&&controlled.role!=Role.CAFS)
                            {
                                Operation op = new Operation(this, OperationType.Move);
                                possibleOp.Add(op);
                            }
                        }

                    }

                }
            }
            

            if (gm.tileMap.tiles[x, z] == 3) // engine
            {
                double vx = (double)gm.enG.x / 6;
                double vz = (double)gm.enG.z / 6;

                Debug.Log(vx);
                Debug.Log(vz);

                if (gm.tileMap.tiles[fireman.currentX/6, fireman.currentZ/6] == 3&&(Math.Abs(currentX-vx)<1&&(Math.Abs(currentZ-vz)<1))&&!StaticInfo.level.Equals("Family"))
                {
                    if (fireman.FreeAP >= 2 && (Math.Abs(x - vx) !=9 && Math.Abs(vz- z) !=7 )&&(Math.Abs(vx-x)>1||Math.Abs(vz-z)>1))
                    {
                        Operation op = new Operation(this, OperationType.Drive);
                        possibleOp.Add(op);
                    }

                    if (fireman.FreeAP >= 4 && (Math.Abs(x - vx) ==9 || Math.Abs(z - vz) ==7)&&(Math.Abs(vx-x)>1||Math.Abs(vz-z)>1))
                    {
                        Operation op = new Operation(this, OperationType.Drive);
                        possibleOp.Add(op);
                    }

                }
            }

            if (gm.tileMap.tiles[x, z] == 4) // ambulance
            {
                double vx = (double)gm.amB.x / 6;
                double vz = (double)gm.amB.z / 6;

                if (fireman.FreeAP >= 2 && (Math.Abs(x - vx) != 9 && Math.Abs(vz - z) != 7)&&(Math.Abs(vx-x)>1||Math.Abs(vz-z)>1)&&!StaticInfo.level.Equals("Family"))
                {
                    Debug.Log("before remoting, my AP is" + " " + fireman.FreeAP);
                    if ((Math.Abs(currentX - vx) <= 0.5 && (Math.Abs(currentZ - vz) <= 0.5)))
                    {
                        Operation op = new Operation(this, OperationType.Drive);
                        possibleOp.Add(op);
                    }

                    Operation op1 = new Operation(this, OperationType.Remote);
                    possibleOp.Add(op1);
                }

                if (fireman.FreeAP >= 4 && (Math.Abs(x - vx) == 9 || Math.Abs(z - vz) == 7)&&(Math.Abs(vx-x)>1||Math.Abs(vz-z)>1)&&!StaticInfo.level.Equals("Family"))
                {
                    if ((Math.Abs(currentX - vx) <= 0.5 && (Math.Abs(currentZ - vz) <= 0.5)))
                    {
                        Operation op = new Operation(this, OperationType.Drive);
                        possibleOp.Add(op);
                    }
                    Operation op1 = new Operation(this, OperationType.Remote);
                    possibleOp.Add(op1);
                }
            }
        }


    }

    //void Start()
    //{
    //    opPanel.SetActive(false);


    //    //-------button click------------
    //    prefabs[0].onClick.AddListener(move);
    //    prefabs[1].onClick.AddListener(extingSmoke);
    //    prefabs[2].onClick.AddListener(extingFire);
    //    prefabs[3].onClick.AddListener(treat);
    //    prefabs[4].onClick.AddListener(carryV);
    //    prefabs[5].onClick.AddListener(leadV);
    //    prefabs[6].onClick.AddListener(removeHazmat);
    //    prefabs[7].onClick.AddListener(carryHazmat);
    //    prefabs[8].onClick.AddListener(command);
    //    prefabs[9].onClick.AddListener(imaging);
    //    prefabs[10].onClick.AddListener(drive);
    //    prefabs[11].onClick.AddListener(remote);
    //    prefabs[12].onClick.AddListener(ride);
    //    prefabs[13].onClick.AddListener(deckGun);
    //    prefabs[14].onClick.AddListener(stopDrive);
    //    prefabs[15].onClick.AddListener(getOff);
    //    //prefabs[2].onClick.AddListener(extingFire);
    //    //prefabs[2].onClick.AddListener(extingFire);
    //    //prefabs[2].onClick.AddListener(extingFire);

    //}



    //----Operations---------------

    public void move()
    {
        Debug.Log("move");
        Fireman fireman = gm.tileMap.selectedUnit;
        if (inCommand)
        {
            if (controlled.role == Role.CAFS)
            {
                commandMoves -= 1;
            }
            if (controlled.driving)
            {
                controlled.driving = false;
                gm.stopDrive(controlled.name);
            }
            if(controlled.riding){
                controlled.riding=false;
                gm.stopRide(controlled.name);
            }
            gm.UpdateLocation(x, z, controlled.name);
            int requiredAP = 1;
            if (gm.tileMap.tiles[x, z] == 2||(controlled.carryingVictim))
            {
                requiredAP = 2;
            }
            if (controlled.role == Role.Paramedic || controlled.role == Role.RescueSpec)
            {
                requiredAP *= 2;
            }
            fireman.setSpecAP(fireman.remainingSpecAp - requiredAP);
        }
        else
        {
            if (fireman.driving)
            {
                fireman.driving = false;
                gm.stopDrive(StaticInfo.name);
            }
            if(fireman.riding){
                fireman.riding=false;
                gm.stopRide(StaticInfo.name);
            }
            fireman.move(x, z);
        }
        opPanel.SetActive(false);
        DestroyAll();

    }

    public void extingSmoke()
    {
        Debug.Log("extinguish smoke");
        Fireman fireman = gm.tileMap.selectedUnit;

        fireman.extingSmoke(x, z);
        opPanel.SetActive(false);
        DestroyAll();
    }

    public void extingFire()
    {
        Debug.Log("extinguish fire");
        Fireman fireman = gm.tileMap.selectedUnit;

        fireman.extingFire(x, z);
        opPanel.SetActive(false);
        DestroyAll();
    }

    public void treat()
    {
        Debug.Log("treat");
        Fireman fireman = gm.tileMap.selectedUnit;

        fireman.treat(x, z);

        opPanel.SetActive(false);
        DestroyAll();
    }

    public void carryV()
    {
        Debug.Log("carryV");
        Fireman fireman = gm.tileMap.selectedUnit;

        fireman.carryV(x, z);

        opPanel.SetActive(false); 
        DestroyAll();
    }

    public void leadV()
    {
        Debug.Log("leadV");
        Fireman fireman = gm.tileMap.selectedUnit;

        fireman.leadV(x, z);

        opPanel.SetActive(false); 
        DestroyAll();
    }

    public void removeHazmat()
    {
        Debug.Log("removeHazmat");
        Fireman fireman = gm.tileMap.selectedUnit;

        fireman.removeHazmet(x, z);

        opPanel.SetActive(false); 
        DestroyAll();
    }

    public void carryHazmat()
    {
        Debug.Log("carryHazmat");
        Fireman fireman = gm.tileMap.selectedUnit;

        fireman.carryHazmat(x, z);

        opPanel.SetActive(false); 
        DestroyAll();
    }

    public void imaging()
    {
        Debug.Log("imaging");
        Fireman fireman = gm.tileMap.selectedUnit;

        fireman.flipPOI(x, z);

        opPanel.SetActive(false); 
        DestroyAll();
    }

    public void command()
    {
        Debug.Log("command");

        this.inCommand = true;
        Role role = Role.None;
        int drive = 0;
        int ride=0;
        bool carrying = false;
        bool leading=false;
        string name = "";
        foreach (string o in gm.players.Keys)
        {
            if (gm.players[o]["Location"] .Equals( "\""+x * 6 + "," + z * 6+"\""))
            {
                role = (Role)Int32.Parse(gm.players[o].ToDictionary()["Role"]);
                if(!Int32.TryParse(gm.players[o].ToDictionary()["Driving"],out drive))
                {
                    drive = 0;
                }
                if(!Int32.TryParse(gm.players[o].ToDictionary()["Riding"],out ride)){
                    ride=0;
                }
                if (gm.players[o].ToDictionary()["Carrying"].Equals( "true"))
                {
                    carrying = true;
                }
                if(gm.players[o].ToDictionary()["Leading"].Equals("true")){
                    leading=true;
                }
                name = o;
            }
        }
        controlled = new Fireman(x * 6, z * 6, role, drive, ride ,carrying, leading ,name);

        opPanel.SetActive(false); 
        DestroyAll();
    }

    public void drive()
    {
        Ambulance amb = gm.tileMap.ambulance;
        Engine eng = gm.tileMap.engine;
        Fireman fireman = gm.tileMap.selectedUnit;
        if ((x==9&&z==4)||(x==9&&z==3)||(x==4&&z==0)||(x==3&&z==0)||(x==0&&z==4)||(x==0&&z==3)||(x==6&&z==7)||(x==5&&z==7))
        {
            int vx = gm.amB.x;
            int vz = gm.amB.z;
            gm.startDrive(1);
            gm.AskForRide(vx,vz);
            Debug.Log("gm.confirmed = " + gm.confirmed);
            Debug.Log("nPlayer" + StaticInfo.numberOfPlayer);
            // while(gm.confirmed!=Int32.Parse(StaticInfo.numberOfPlayer));

            amb.moveNextStation(x,z);
            gm.UpdateAmbulanceLocation(amb.x, amb.z,vx,vz);
            opPanel.SetActive(false);
            DestroyAll();
            int requiredAP = 2;
            Debug.Log("differece x:"+(vx/6-x));
            Debug.Log("differece z:"+(vz/6-z));
            if (Math.Abs(vx/6-x)==9||Math.Abs(vz/6-z)==7)
            {
                requiredAP = 4;
            }
            fireman.setAP(fireman.FreeAP - requiredAP);
        }
        else
        {
            int vx = gm.enG.x;
            int vz = gm.enG.z;
            gm.startDrive(2);
            gm.AskForRide(vx,vz);
            // while(gm.confirmed!=Int32.Parse(StaticInfo.numberOfPlayer));
            eng.moveNextStation(x,z);
            gm.UpdateEngineLocation(x, z);
            opPanel.SetActive(false);
            DestroyAll();
            int requiredAP = 2;
            Debug.Log("differece x:"+(vx/6-x));
            Debug.Log("differece z:"+(vz/6-z));
            if (Math.Abs(vx/6-x)==9||Math.Abs(vz/6-z)==7)
            {
                requiredAP = 4;
            }
            fireman.setAP(fireman.FreeAP - requiredAP);
        }
        fireman.s.transform.position = new Vector3(x*6, 0.2f, z*6);
        gm.UpdateLocation(x*6, z*6,StaticInfo.name);
        fireman.currentX=x*6;
        fireman.currentZ=z*6;
        gm.confirmed=0;
        opPanel.SetActive(false);
        DestroyAll();
        Debug.Log("fireman is at x:"+fireman.currentX);
        Debug.Log("fireman is at z:"+fireman.currentZ); 
        Debug.Log("engine is at x:"+eng.x);
        Debug.Log("engine is at z:"+eng.z);
        Debug.Log("confirmed 1: "+gm.confirmed);
    }

    public void remote()
    {
        Ambulance amb = gm.tileMap.ambulance;
        Fireman fireman = gm.tileMap.selectedUnit;
        int vx = gm.amB.x;
        int vz = gm.amB.z;
        int requiredAP = 2;
        Debug.Log((Math.Abs(vx/6-x)));
        Debug.Log((Math.Abs(vz/6-z)));
        if (Math.Abs(vx/6-x)==9||Math.Abs(vz/6-z)==7)
        {
            requiredAP = 4;
        }
        fireman.setAP(fireman.FreeAP - requiredAP);
        gm.startDrive(1);
        amb.moveNextStation(x,z);
        gm.UpdateAmbulanceLocation(amb.x, amb.z,vx,vz);
        opPanel.SetActive(false); 
        DestroyAll();

    }

    public void ride()
    {
        if(gm.tileMap.tiles[x, z] == 3){
            Fireman fireman = gm.tileMap.selectedUnit;
            fireman.riding = true;
            gm.startRide(1);
        }
        if(gm.tileMap.tiles[x, z] == 4){
            Fireman fireman = gm.tileMap.selectedUnit;
            fireman.riding = true;
            gm.startRide(2);
        }
        
        opPanel.SetActive(false); 
        DestroyAll();
    }

    public void deckGun()
    {
        int requiredAP = 4;
        if (gm.fireman.role == Role.Driver)
        {
            requiredAP = 2;
        }
        Fireman fireman = gm.tileMap.selectedUnit;
        fireman.setAP(fireman.FreeAP - requiredAP);

        Debug.Log("fire deckgunnnnnnnnnnnnnnnnnnnnnnnn");
        double vx = gm.enG.x / 6;
        double vz = gm.enG.z / 6;
        int rx1;
        int rx2;
        int rz1;
        int rz2;

        if (vz==0)
        {
            rx1=1;
            rx2=4;
            rz1=1;
            rz2=3;
        }
        else if (vx==0)
        {
            rx1=1;
            rx2=4;
            rz1=4;
            rz2=6;
        }
        else if (vz==7)
        {
            rx1=5;
            rx2=8;
            rz1=4;
            rz2=6;
        }
        else
        {
            rx1=5;
            rx2=8;
            rz1=1;
            rz2=3;
        }
        int rng_X = UnityEngine.Random.Range(rx1, rx2);
        int rng_Z = UnityEngine.Random.Range(rz1, rz2);
        Debug.Log("range"+rng_X+","+rng_Z);
        //need to ask player if he is satisfied with the extinguishing area
        gm.tileMap.buildNewTile(rng_X, rng_Z,0);
        gm.UpdateTile(rng_X, rng_Z, 0);
        if (!gm.wallManager.isVeritcalWall(rng_X, rng_Z)&&!gm.doorManager.isVerticalDoor(rng_X, rng_Z)){
            Debug.Log("im in 1");
            gm.tileMap.buildNewTile(rng_X-1, rng_Z,0);
            gm.UpdateTile(rng_X-1, rng_Z, 0);
        }
        if (!gm.wallManager.isVeritcalWall(rng_X+1, rng_Z)&&!gm.doorManager.isVerticalDoor(rng_X+1, rng_Z)){
            Debug.Log("im in 2");
            gm.tileMap.buildNewTile(rng_X+1, rng_Z,0);
            gm.UpdateTile(rng_X+1, rng_Z, 0);
        }
        if (!gm.wallManager.isHorizontalWall(rng_X, rng_Z+1)&&!gm.doorManager.isHorizontalDoor(rng_X, rng_Z+1)){
            Debug.Log("im in 3");
            gm.tileMap.buildNewTile(rng_X, rng_Z+1,0);
            gm.UpdateTile(rng_X, rng_Z+1, 0);
        }
        if (!gm.wallManager.isHorizontalWall(rng_X, rng_Z)&&!gm.doorManager.isHorizontalDoor(rng_X, rng_Z)){
            Debug.Log("im in 4");
            gm.tileMap.buildNewTile(rng_X, rng_Z-1,0);
            gm.UpdateTile(rng_X, rng_Z-1, 0);
        }       
        opPanel.SetActive(false); 
        DestroyAll();
    }

    public void stopDrive()
    {
        opPanel.SetActive(false); 
        DestroyAll();
    }

    public void getOff()
    {
        Fireman fireman = gm.tileMap.selectedUnit;
        fireman.riding = false;
        opPanel.SetActive(false); 
        DestroyAll();
    }

    public void dropeV()
    {
        Fireman fireman=gm.fireman;
        if(fireman.carriedPOI!=null){
            fireman.carryingVictim=false;
            gm.pOIManager.dropPOI(x,z);
            fireman.carriedPOI=null;
            gm.StopCarry(x,z);
            if(StaticInfo.level.Equals("Family")){
                if(x==0||x==7||z==0||z==9){
                    gm.pOIManager.rescueCarried(x,z);
                }
            }else{
                if(gm.tileMap.tiles[x,z]==4&&Math.Abs((double)gm.amB.x/6-x)<1&&Math.Abs((double)gm.amB.z/6-z)<1){
                    gm.pOIManager.rescueCarried(x,z);
                }
            }
        }else if(fireman.ledPOI!=null){
            gm.pOIManager.dropPOI(x,z);
            fireman.ledPOI=null;
            gm.StopLead(x,z);
            if(StaticInfo.level.Equals("Family")){
                if(x==0||x==7||z==0||z==9){
                    gm.pOIManager.rescueTreated(x,z);
                }
            }else{
                if(gm.tileMap.tiles[x,z]==4&&Math.Abs((double)gm.amB.x/6-x)<1&&Math.Abs((double)gm.amB.z/6-z)<1){
                    gm.pOIManager.rescueTreated(x,z);
                }
            }
        }
        
    }

    public void dropHazmat()
    {
        Fireman fireman=gm.fireman;
        fireman.carryingVictim=false;
        fireman.carriedHazmat=null;
        gm.hazmatManager.dropHazmat(x,z);
        gm.StopCarry(x,z);
    }

    public void stopCommand()
    {
        controlled = null;
        inCommand = false;
    }

    public void cancel()
    {
        if(askingForRide){
            gm.startRide(0);
        }
        opPanel.SetActive(false);
        DestroyAll();
        Debug.Log("cancel");
    }

    public void DestroyAll() {
        //opPanel.SetActive(false);
        Debug.Log("destroying");
        //for(int i = 0; i < gm.options.Count; i++) {
        //    foreach(Button b in gm.options[i].GetComponentsInChildren<Button>()) {
        //        gm.DestroyButton(b);
        //        Debug.Log(b);
        //    }
        //}
        gm.DestroyButtons();
        buttons.RemoveRange(0, buttons.Count);
    }
}

