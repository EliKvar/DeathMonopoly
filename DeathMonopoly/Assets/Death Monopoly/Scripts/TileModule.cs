using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TileModule : MonoBehaviour
{
    //TAGS ARE IMPORTANT FOR FUNCTIONALITY
    public string[] TileTags;

    public TileConnector[] GetExitsForTile()
    {
        return GetComponentsInChildren<TileConnector>();
    }
}
