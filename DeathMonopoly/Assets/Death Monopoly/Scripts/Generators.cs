using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Random = UnityEngine.Random;

public class Generators : MonoBehaviour
{
    public TileModule[] Tiles;
    public TileModule StartTile;
    public TileModule CornerTile;

    public int TileGenerations;
  
    public void Generate()
    {
        Debug.Log("GENERATING");
        //Start off by spawning the starting piece
        var start = Instantiate(StartTile, transform.position, transform.rotation);
        var pendExits = new List<TileConnector>(start.GetExitsForTile());
        Console.WriteLine("Starting Tile Placed");

        //Based on amount of tiles you want to spawn equivalent to generations value
        for (int gens = 1; gens <= TileGenerations; gens++)
        {
            var newExit = new List<TileConnector>();
            //TAGS ARE IMPORTANT TO THIS FUNCTIONALITY
            string newTag;
            TileModule newTilePrefab;
            TileModule newTile;
            TileConnector[] newTileExits;
            TileConnector exitMatch;

            foreach (var pendExit in pendExits)
            {
                //First corner spawn
                if (gens != TileGenerations && pendExits.Count > 0 && gens == Mathf.Round(0.25f * TileGenerations))
                {
                    newTile = Instantiate(CornerTile);
                    newTileExits = newTile.GetExitsForTile();
                    exitMatch = newTileExits.FirstOrDefault(x => x.IsDefault) ?? GetRandom(newTileExits);
                    MatchExit(pendExit, exitMatch);
                    newExit.AddRange(newTileExits.Where(e => e != exitMatch));
                    Debug.Log("Corner 1 Tile Placed " + Mathf.Round(0.25f * TileGenerations));
                }
                //Second corner spawn
                else if (gens != TileGenerations && pendExits.Count > 0 && gens == Mathf.Round(0.5f * TileGenerations))
                {
                    newTile = Instantiate(CornerTile);
                    newTileExits = newTile.GetExitsForTile();
                    exitMatch = newTileExits.FirstOrDefault(x => x.IsDefault) ?? GetRandom(newTileExits);
                    MatchExit(pendExit, exitMatch);
                    newExit.AddRange(newTileExits.Where(e => e != exitMatch));
                    Debug.Log("Corner 2 Tile Placed " + Mathf.Round(0.5f * TileGenerations));
                }
                //Third corner spawn
                else if (gens != TileGenerations && pendExits.Count > 0 && gens == Mathf.Round(0.75f * TileGenerations))
                {
                    newTile = Instantiate(CornerTile);
                    newTileExits = newTile.GetExitsForTile();
                    exitMatch = newTileExits.FirstOrDefault(x => x.IsDefault) ?? GetRandom(newTileExits);
                    MatchExit(pendExit, exitMatch);
                    newExit.AddRange(newTileExits.Where(e => e != exitMatch));
                    Debug.Log("Corner 3 Tile Placed " + Mathf.Round(0.25f * TileGenerations));
                }
                //The rest of the tiles
                else
                {
                    if (Tiles.Length > 0)
                    {
                        newTag = GetRandom(pendExit.connectorTags);
                        newTilePrefab = GetRandomTile(Tiles, newTag);

                        newTile = Instantiate(newTilePrefab);
                        newTileExits = newTile.GetExitsForTile();
                        exitMatch = newTileExits.FirstOrDefault(x => x.IsDefault) ?? GetRandom(newTileExits);
                        MatchExit(pendExit, exitMatch);
                        newExit.AddRange(newTileExits.Where(e => e != exitMatch));

                        Debug.Log("Tile Placed");
                    }
                    else
                    {
                        Debug.Log("Tiles not available");
                    }
                }
            }

            pendExits = newExit;

        }

    }

    //Full method that ensures that tiles match up correctly with others to make the board
    private void MatchExit(TileConnector oldExit, TileConnector newExit)
    {
        var newTile = newExit.transform.parent;
        var forwardVectToMatch = -oldExit.transform.forward;
        var correctRotate = Azimuth(forwardVectToMatch) - Azimuth(newExit.transform.forward);
        newTile.RotateAround(newExit.transform.position, Vector3.up, correctRotate);
        var correctTranslate = oldExit.transform.position - newExit.transform.position;
        newTile.transform.position += correctTranslate;
    }
    //GetRandom is used to take generic objects, allowing for multiple uses to generate rooms, obstacles, etc.
    private static TItem GetRandom<TItem>(TItem[] array)
    {
        return array[Random.Range(0, array.Length)];
    }

    //Used to get a random tile in the array of Tile objects
    private static TileModule GetRandomTile(IEnumerable<TileModule> Tiles, string tagMatch)
    {
        var matchingTiles = Tiles.Where(r => r.TileTags.Contains(tagMatch)).ToArray();
        return GetRandom(matchingTiles);
    }

    //Used to obtain a rotation of the objects vectors based off of objects connectors (exits)
    private static float Azimuth(Vector3 vector)
    {
        return Vector3.Angle(Vector3.forward, vector) * Mathf.Sign(vector.x);
    }
}
