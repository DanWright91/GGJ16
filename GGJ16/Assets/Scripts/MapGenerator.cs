﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System;

public class MapGenerator : MonoBehaviour {

    public int WidthSize = 100;

    public int HeightSize = 100;

    public List<GameObject> TileTypes = new List<GameObject>();

    public List<string> TileSets = new List<string>();

    public float Tilesize = 2.56f; 



	// Use this for initialization
	void Start () {
        //splits all the objects into something I can search with. 
        DualStore<TileDetails, GameObject> gameObjects = new DualStore<TileDetails, GameObject>(); 

        foreach(GameObject obj in TileTypes)
        {
            TileDetails details = obj.GetComponent<TileDetails>();
            gameObjects.Add(details, obj); 
        }


        gameObjects = GetTileset(gameObjects);

        Dictionary<TileSetType, TileLevel> tilesByTileSetType = new Dictionary<TileSetType, TileLevel>(); 

        foreach(TileSetType type in Enum.GetValues(typeof(TileSetType)))
        {
            TileLevel level = new TileLevel(UnityEngine.Random.seed);
            DualStore<TileDetails, GameObject> tileSetTiles = GetTileSetType(gameObjects, type); 

            foreach(TileType ttype in Enum.GetValues(typeof(TileType)))
            {
                level.AssignTiles(ttype, GetTileTypes(tileSetTiles, ttype)); 
            }

            tilesByTileSetType.Add(type, level); 
        }


        SpawnTiles(tilesByTileSetType); 
	}
	
	// Update is called once per frame
	void Update () {
	
	}

    /// <summary>
    /// Retrieves the tileset from all the different tilesets. 
    /// </summary>
    /// <param name="allTiles"></param>
    /// <returns></returns>
    protected virtual DualStore<TileDetails, GameObject> GetTileset(DualStore<TileDetails, GameObject> allTiles)
    {
        //choose a tileset to play on. 
        int tileSetNo = UnityEngine.Random.Range(0, TileSets.Count);
        string tileSet = TileSets[tileSetNo];

        DualStore<TileDetails, GameObject> thisTileset = new DualStore<TileDetails, GameObject>();

        foreach (KeyValuePair<TileDetails, GameObject> kv in allTiles.KeyValuePairs)
        {
            if (kv.Key.TileSet == tileSet)
            {
                thisTileset.Add(kv.Key, kv.Value);
            }
        }

        return thisTileset; 
    }

    /// <summary>
    /// Gets all tiles of a specific type. 
    /// </summary>
    /// <param name="allTiles"></param>
    /// <param name="type"></param>
    /// <returns></returns>
    protected virtual DualStore<TileDetails, GameObject> GetTileSetType (DualStore<TileDetails, GameObject> allTiles, TileSetType type)
    {
        DualStore<TileDetails, GameObject> thisTileset = new DualStore<TileDetails, GameObject>();

        foreach (KeyValuePair<TileDetails, GameObject> kv in allTiles.KeyValuePairs)
        {
            if (kv.Key.Type == type)
            {
                thisTileset.Add(kv.Key, kv.Value);
            }
        }

        return thisTileset;
    }

    protected virtual List<GameObject> GetTileTypes (DualStore<TileDetails, GameObject> tiles, TileType type)
    {
        List<GameObject> thetiles = new List<GameObject>(); 
        foreach (KeyValuePair<TileDetails, GameObject> kv in tiles.KeyValuePairs)
        {
            if (kv.Key.TileType == type)
            {
                thetiles.Add(kv.Value);
            }
        }

        return thetiles; 
    }

    protected virtual void SpawnTiles (Dictionary<TileSetType, TileLevel> availiableTile)
    {
        int dividerLevel = UnityEngine.Random.Range(0, HeightSize);

        int platforms = (int)UnityEngine.Random.Range(1f, HeightSize / Tilesize);  

        for (int idx = 0; idx < platforms; idx++)
        {
            float height = UnityEngine.Random.Range(0f, HeightSize / Tilesize);

            float width = UnityEngine.Random.Range(1f, WidthSize / Tilesize);

            TileSetType level = height > dividerLevel ? TileSetType.upperLevels : TileSetType.lowerLevels;

            for (int xdx = 1; xdx < width; xdx++)
            {
                GameObject obj = availiableTile[level].GetTileType(TileType.Top);
                Instantiate(obj, new Vector3(xdx * Tilesize, height * Tilesize), new Quaternion()); 
            }
        }
    }
}
