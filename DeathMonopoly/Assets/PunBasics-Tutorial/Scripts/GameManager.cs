// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Launcher.cs" company="Exit Games GmbH">
//   Part of: Photon Unity Networking Demos
// </copyright>
// <summary>
//  Used in "PUN Basic tutorial" to handle typical game management requirements
// </summary>
// <author>developer@exitgames.com</author>
// --------------------------------------------------------------------------------------------------------------------

using UnityEngine;
using UnityEngine.SceneManagement;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Random = UnityEngine.Random;

using Photon.Realtime;

namespace Photon.Pun.Demo.PunBasics
{
#pragma warning disable 649

	/// <summary>
	/// Game manager.
	/// Connects and watch Photon Status, Instantiate Player
	/// Deals with quiting the room and the game
	/// Deals with level loading (outside the in room synchronization)
	/// </summary>
	public class GameManager : MonoBehaviourPunCallbacks
	{

		#region Public Fields

		static public GameManager Instance;
		#endregion
		public TileModule[] Tiles;
		public TileModule StartTile;
		public TileModule CornerTile;
		public int TileGenerations;

		#region Private Fields

		private GameObject instance;

		[Tooltip("The prefab to use for representing the player")]
		[SerializeField]
		private GameObject player1Prefab;
		[SerializeField]
		private GameObject player2Prefab;
		[SerializeField]
		private GameObject player3Prefab;
		[SerializeField]
		private GameObject player4Prefab;
		[SerializeField]
		private GameObject player5Prefab;

		private GameObject playerPrefab;
		private string playerprefStr;


		#endregion

		#region MonoBehaviour CallBacks

		/// <summary>
		/// MonoBehaviour method called on GameObject by Unity during initialization phase.
		/// </summary>
		void Start()
		{

			playerprefStr = PlayerPrefs.GetString("Player");
			Debug.Log(playerprefStr);

			switch (playerprefStr)
			{
				case "Player1":
					playerPrefab = player1Prefab;
					break;
				case "Player2":
					playerPrefab = player2Prefab;
					break;
				case "Player3":
					playerPrefab = player3Prefab;
					break;
				case "Player4":
					playerPrefab = player4Prefab;
					break;
				case "Player5":
					playerPrefab = player5Prefab;
					break;
				default:
					playerPrefab = player1Prefab;
					break;
			}
			Instance = this;
		
				
			
			// in case we started this demo with the wrong scene being active, simply load the menu scene
			if (!PhotonNetwork.IsConnected)
			{
				SceneManager.LoadScene("PunBasics-Launcher");

				return;
			}

			if (playerPrefab == null)
			{ // #Tip Never assume public properties of Components are filled up properly, always check and inform the developer of it.

				Debug.LogError("<Color=Red><b>Missing</b></Color> playerPrefab Reference. Please set it up in GameObject 'Game Manager'", this);
			}
			else
			{


				if (PlayerManager.LocalPlayerInstance == null)
				{
					Debug.LogFormat("We are Instantiating LocalPlayer from {0}", SceneManagerHelper.ActiveSceneName);

					// we're in a room. spawn a character for the local player. it gets synced by using PhotonNetwork.Instantiate
			
					
					PhotonNetwork.Instantiate(this.playerPrefab.name, new Vector3(0f, 5f, 0f), Quaternion.identity, 0);
					Generate();

					if (PhotonNetwork.CurrentRoom.PlayerCount == 1)
					{
						Debug.Log("Called Generate()");
						//photonView.RPC
					}
				}
				else
				{
                   
					Debug.LogFormat("Ignoring scene load for {0}", SceneManagerHelper.ActiveSceneName);
				}


			}

		}

		/// <summary>
		/// MonoBehaviour method called on GameObject by Unity on every frame.
		/// </summary>
		void Update()
		{
			// "back" button of phone equals "Escape". quit app if that's pressed
			if (Input.GetKeyDown(KeyCode.Escape))
			{
				QuitApplication();
			}
		}

		#endregion

		#region Photon Callbacks

		/// <summary>
		/// Called when a Photon Player got connected. We need to then load a bigger scene.
		/// </summary>
		/// <param name="other">Other.</param>
		public override void OnPlayerEnteredRoom(Player other)
		{
			Debug.Log("OnPlayerEnteredRoom() " + other.NickName); // not seen if you're the player connecting

			if (PhotonNetwork.IsMasterClient)
			{
				Debug.LogFormat("OnPlayerEnteredRoom IsMasterClient {0}", PhotonNetwork.IsMasterClient); // called before OnPlayerLeftRoom

				//LoadArena();
			}
		}

		/// <summary>
		/// Called when a Photon Player got disconnected. We need to load a smaller scene.
		/// </summary>
		/// <param name="other">Other.</param>
		public override void OnPlayerLeftRoom(Player other)
		{
			Debug.Log("OnPlayerLeftRoom() " + other.NickName); // seen when other disconnects

			if (PhotonNetwork.IsMasterClient)
			{
				Debug.LogFormat("OnPlayerEnteredRoom IsMasterClient {0}", PhotonNetwork.IsMasterClient); // called before OnPlayerLeftRoom

				//LoadArena(); 
			}
		}

		/// <summary>
		/// Called when the local player left the room. We need to load the launcher scene.
		/// </summary>
		public override void OnLeftRoom()
		{
			SceneManager.LoadScene("PunBasics-Launcher");
		}

		#endregion

		#region Public Methods

		public void LeaveRoom()
		{
			PhotonNetwork.LeaveRoom();
		}

		public void QuitApplication()
		{
			Application.Quit();
		}

		#endregion

		#region Private Methods

		void LoadArena()
		{
			if (!PhotonNetwork.IsMasterClient)
			{
				Debug.LogError("PhotonNetwork : Trying to Load a level but we are not the master Client");
			}

			//Debug.LogFormat( "PhotonNetwork : Loading Level : {0}", PhotonNetwork.CurrentRoom.PlayerCount );
			PhotonNetwork.LoadLevel("PunBasics-Room for 1");


		}


		#endregion
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

}