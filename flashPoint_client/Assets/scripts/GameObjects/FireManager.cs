﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class FireManager : MonoBehaviour
{

	//private Random rng;
	private int mapSizeX;
	private int mapSizeZ;
	public GameManager gm;
	public TileMap tileMap;
	public Boolean debugMode = true;        // Toggle this for more descriptive Debug.Log() output



	// Use this for initialization
	public FireManager(GameManager in_gm, TileMap in_tileMap, int in_X, int in_Z)
	{
		this.gm = in_gm;
		this.tileMap = in_tileMap;  //initFireTileStores(in_tileMap);
		this.mapSizeX = in_X;
		this.mapSizeZ = in_Z;
		//this.rng = new Random();
	}

	public void advanceFire(int in_x, int in_z)
	{
		// Places the new Smoke marker (n.b. might be 'more' than Smoke due to rule interactions)
		//Debug.Log("NewFire running:");
		//newFire(in_x, in_z);


		Debug.Log("flashover:");
		// Flashover step:
		flashover();

	}

	public void flashover()
	{
		bool canLeft = false;
		bool canUp = false;
		bool canDown = false;
		bool canRight = false;
		DoorManager dm = gm.doorManager;
		WallManager wm = gm.wallManager;
		bool adjOnFire = false;
		bool flashoverTriggered = false;

		// Until nothing triggers we keep looping (i.e. until no smoke can be transformed into a Fire marker
		while(true) {
		// Want to flip the smoke tile if a neighbouring tile can ignite
			for (int x_elem = 0; x_elem < mapSizeX; x_elem++) 
			{
				for (int z_elem = 0; z_elem < mapSizeZ; z_elem++)
				{
					if (tileMap.tiles[x_elem, z_elem] != 1) continue;
				
					// Prep for adjacent Fire check:
					adjOnFire = false;
					if (x_elem >= 1) { canLeft = true; }
					else { canLeft = false; }

					if (x_elem < mapSizeX - 1) { canRight = true; }
					else { canRight = false; }

					if (z_elem >= 1) { canDown = true; }
					else { canDown = false; }
				
					if (z_elem < mapSizeZ - 1) { canUp = true; }
					else { canUp = false; }

				
					// If the tile is next to a Fire marker; if there's an obstacle in the way then Fire isn't 'adjacent'
					if(canUp && tileMap.tiles[x_elem, z_elem + 1] == 2 && !adjOnFire &&
								!(dm.checkIfHDoor(x_elem, z_elem + 1) || wm.checkIfHWall(x_elem, z_elem + 1)))
					{
							adjOnFire = true;
					}
				
					if (canDown && tileMap.tiles[x_elem, z_elem - 1] == 2 && !adjOnFire &&
								!(dm.checkIfHDoor(x_elem, z_elem) || wm.checkIfHWall(x_elem, z_elem)))
					{
							adjOnFire = true;
					}
	
					if(canLeft && tileMap.tiles[x_elem - 1, z_elem] == 2 && !adjOnFire &&
								!(dm.checkIfVDoor(x_elem, z_elem) || wm.checkIfVWall(x_elem, z_elem)))
					{
							adjOnFire = true;
					}
				
					if(canRight && tileMap.tiles[x_elem + 1, z_elem] == 2 && !adjOnFire &&
								!(dm.checkIfVDoor(x_elem + 1, z_elem) || wm.checkIfVWall(x_elem + 1, z_elem)))
					{
							adjOnFire = true;
					}


					// If any adjacent tiles were on Fire ('legitimately') then we change our tile from Smoke to Fire
					if (adjOnFire == true)
					{
						// Sanity check:
						if (debugMode)
						{
							Debug.Log("");
							Debug.Log("canLeft: " + canLeft);
							Debug.Log("canUp: " + canUp);
							Debug.Log("canDown: " + canDown);
							Debug.Log("canRight: " + canRight);
							Debug.Log("(x,z):  " + x_elem + ", " + z_elem);
							Debug.Log("");
						}

						tileMap.buildNewTile(x_elem, z_elem, 2);
						tileMap.gm.UpdateTile(x_elem, z_elem, 2);
						flashoverTriggered = true;
					}
					
				}
			}

			// We reach here if, at no point in the loop, there was a flashover
			if(flashoverTriggered == true) {
				flashoverTriggered = false;
				continue;
			}
			else
			{
				break;
			}
			
		}
	}


	// Begin phase 1 of advanceFire: placing the new Smoke marker
	public void newFire(int in_x, int in_z)
	{
		// Coordinate to place the new Smoke marker
		int rng_X = in_x;	//UnityEngine.Random.Range(0, mapSizeX - 1);
		int rng_Z = in_z;	//UnityEngine.Random.Range(0, mapSizeZ - 1);
		int current_type = tileMap.tiles[rng_X, rng_Z];
		if (debugMode) Debug.Log("Type is: " + current_type);
		
		Boolean canLeft = false;
		Boolean canUp = false;
		Boolean canDown = false;
		Boolean canRight = false;
		
		DoorManager dm = gm.doorManager;
		WallManager wm = gm.wallManager;
		Boolean adjOnFire = false;

		// If the tile has only a Smoke
		if (current_type == 1)
		{
			// Flip Smoke marker to Fire
			if (debugMode) Debug.Log("Flipping Smoke -> Fire");
			tileMap.buildNewTile(rng_X, rng_Z, 2);
			tileMap.gm.UpdateTile(rng_X, rng_Z, 2);
			return;
		}
		else if (current_type == 2)
		{
			// Trigger explosion
			return;
		}

		// Prep for adjacent Fire check:
		if (rng_X >= 1)
		{
			canLeft = true;
		}
		if (rng_X < mapSizeX - 1)
		{
			canRight = true;
		}
		if (rng_Z >= 1)
		{
			canDown = true;
		}
		if (rng_Z < mapSizeZ - 1)
		{
			canUp = true;
		}
		// Sanity check:
		if (debugMode)
		{
			Debug.Log("canLeft: " + canLeft);
			Debug.Log("canUp: " + canUp);
			Debug.Log("canDown: " + canDown);
			Debug.Log("canRight: " + canRight);
		}


		// If the tile is next to a Fire marker; if there's an obstacle in the way then Fire isn't 'adjacent'
		if (canUp && tileMap.tiles[rng_X, rng_Z + 1] == 2 && !adjOnFire &&
				!(dm.checkIfHDoor(rng_X, rng_Z + 1) || wm.checkIfHWall(rng_X, rng_Z + 1)))
		{
			adjOnFire = true;
		}
		if (canDown && tileMap.tiles[rng_X, rng_Z - 1] == 2 && !adjOnFire &&
				!(dm.checkIfHDoor(rng_X, rng_Z) || wm.checkIfHWall(rng_X, rng_Z)))
		{
			adjOnFire = true;
		}
		if (canLeft && tileMap.tiles[rng_X - 1, rng_Z] == 2 && !adjOnFire &&
				!(dm.checkIfHDoor(rng_X, rng_Z) || wm.checkIfHWall(rng_X, rng_Z)))
		{
			adjOnFire = true;
		}
		if (canRight && tileMap.tiles[rng_X + 1, rng_Z] == 2 && !adjOnFire &&
				!(dm.checkIfHDoor(rng_X + 1, rng_Z) || wm.checkIfHWall(rng_X + 1, rng_Z)))
		{
			adjOnFire = true;
		}

		// If any adjacent tiles were on Fire ('legitimately') then we change our tile from Smoke to Fire
		if (adjOnFire == true)
		{
			tileMap.buildNewTile(rng_X, rng_Z, 2);
			tileMap.gm.UpdateTile(rng_X, rng_Z, 2);
			return;
		}







		// We reach here if the tile should simply have a Smoke marker
		tileMap.buildNewTile(rng_X, rng_Z, 1);
		tileMap.gm.UpdateTile(rng_X, rng_Z, 1);

	}



	// Update is called once per frame
	/*
	void Update () {
		
	}
	*/
}
