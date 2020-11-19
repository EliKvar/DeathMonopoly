// --------------------------------------------------------------------------------------------------------------------
// <copyright file="PlayerManager.cs" company="Exit Games GmbH">
//   Part of: Photon Unity Networking Demos
// </copyright>
// <summary>
//  Used in PUN Basics Tutorial to deal with the networked player instance
// </summary>
// <author>developer@exitgames.com</author>
// --------------------------------------------------------------------------------------------------------------------

using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Random = UnityEngine.Random;

namespace Photon.Pun.Demo.PunBasics
{
	#pragma warning disable 649

    /// <summary>
    /// Player manager.
    /// Handles fire Input and Beams.
    /// </summary>
    public class PlayerManager : MonoBehaviourPunCallbacks, IPunObservable
    {
        #region Public Fields
        public float turnSmoothTime = 2f;
        float turnspeed = 200.0f;
        public GameObject winText;
        public GameObject loseText;

        Rigidbody rb;
        float speed = 9f;
        // [Tooltip("The current Health of our player")]
        // public float Health = 1f;

        [Tooltip("The local player instance. Use this to know if the local player is represented in the Scene")]
        public static GameObject LocalPlayerInstance;

        #endregion

        #region Private Fields

        [Tooltip("The Player's UI GameObject Prefab")]
        [SerializeField]
        private GameObject playerUiPrefab;
        private GameObject currentCheckpoint;
        public GameObject startPoint;
         bool hasWon;
         bool hasLost;
        private bool hippoIsAtt;
        private float hippoTimer = 2.0f;

        //[Tooltip("The Beams GameObject to control")]
        //[SerializeField]
        //private GameObject beams;

        //True, when the user is firing
        //bool IsFiring;

        #endregion

        #region MonoBehaviour CallBacks

        /// <summary>
        /// MonoBehaviour method called on GameObject by Unity during early initialization phase.
        /// </summary>
        public void Awake()
        {
            //The first player sets to false, so new players get an error because they cant find the object. Put the ui objects setactive(false) in the gamemanager script. call gamemanager function that
            //wins or loses for player? Or do photon.isMine with UI
            currentCheckpoint = startPoint;

            winText = GameObject.Find("WinScreen");
            loseText = GameObject.Find("LoseScreen");

            // if (this.beams == null)
            //  {
            //      Debug.LogError("<Color=Red><b>Missing</b></Color> Beams Reference.", this);
            //  }
            //  else
            //  {
            //      this.beams.SetActive(false);
            //  }

            // #Important
            // used in GameManager.cs: we keep track of the localPlayer instance to prevent instanciation when levels are synchronized
            if (photonView.IsMine)
            {
                LocalPlayerInstance = gameObject;
            }

            // #Critical
            // we flag as don't destroy on load so that instance survives level synchronization, thus giving a seamless experience when levels load.
            DontDestroyOnLoad(gameObject);
        }

        /// <summary>
        /// MonoBehaviour method called on GameObject by Unity during initialization phase.
        /// </summary>
        public void Start()
        {
            CameraWork _cameraWork = gameObject.GetComponent<CameraWork>();
            rb = GetComponent<Rigidbody>();

            if (_cameraWork != null)
            {
                if (photonView.IsMine)
                {
                    _cameraWork.OnStartFollowing();
                }
            }
            else
            {
                Debug.LogError("<Color=Red><b>Missing</b></Color> CameraWork Component on player Prefab.", this);
            }

            // Create the UI
            if (this.playerUiPrefab != null)
            {
                GameObject _uiGo = Instantiate(this.playerUiPrefab);
                _uiGo.SendMessage("SetTarget", this, SendMessageOptions.RequireReceiver);
            }
            else
            {
                Debug.LogWarning("<Color=Red><b>Missing</b></Color> PlayerUiPrefab reference on player Prefab.", this);
            }

            #if UNITY_5_4_OR_NEWER
            // Unity 5.4 has a new scene management. register a method to call CalledOnLevelWasLoaded.
			UnityEngine.SceneManagement.SceneManager.sceneLoaded += OnSceneLoaded;
            #endif
        }


		public override void OnDisable()
		{
			// Always call the base to remove callbacks
			base.OnDisable ();

			#if UNITY_5_4_OR_NEWER
			UnityEngine.SceneManagement.SceneManager.sceneLoaded -= OnSceneLoaded;
			#endif
		}


        /// <summary>
        /// MonoBehaviour method called on GameObject by Unity on every frame.
        /// Process Inputs if local player.
        /// Show and hide the beams
        /// Watch for end of game, when local player health is 0.
        /// </summary>
        public void Update()
        {
            // we only process Inputs and check health if we are the local player
            if (photonView.IsMine)
            {
                this.ProcessInputs();

                if (hasWon == true)
                {
                    //show winner Ui
                    winText.GetComponentInChildren<Text>().text = "You have Won!";
                    //PhotonNetwork.NickName
                }
                // if (this.Health <= 0f)
                // {
                //     GameManager.Instance.LeaveRoom();
                // }
            }
            else if(hasLost == true)
            {
               // hasLost = true;
                loseText.GetComponentInChildren<Text>().text =  "You have Lost!";
            }

            // if (this.beams != null && this.IsFiring != this.beams.activeInHierarchy)
            // {
            //     this.beams.SetActive(this.IsFiring);
            // }
           
    
        }

        /// <summary>
        /// MonoBehaviour method called when the Collider 'other' enters the trigger.
        /// Affect Health of the Player if the collider is a beam
        /// Note: when jumping and firing at the same, you'll find that the player's own beam intersects with itself
        /// One could move the collider further away to prevent this or check if the beam belongs to the player.
        /// </summary>
        public void OnTriggerEnter(Collider other)
        {
            if (!photonView.IsMine)
            {
                return;
            }


            // We are only interested in Beamers
            // we should be using tags but for the sake of distribution, let's simply check by name.
            // if (!other.name.Contains("Beam"))
            // {
            //     return;
            // }
            if (other.tag.Contains("Checkpoint"))
            {
                currentCheckpoint = other.gameObject;
            }
           if (other.tag.Contains("Axe"))
            {
                Debug.Log("Axe Hit");
                //Renderer temp = other.GetComponent<Renderer>();
               // temp.enabled = true;
                Vector3 newDir = new Vector3(Random.Range(-5, 5), Random.Range(1, 7), Random.Range(-5,5));
                rb.AddForce(newDir * 2, ForceMode.Impulse);
              
            }
           
           if (other.tag.Contains("Springboard"))
           {
               Debug.Log("Springboard Hit");

               Vector3 newDir = new Vector3(Random.Range(-3, 3), Random.Range(5, 10), Random.Range(-3, 3));
               rb.AddForce(newDir * 1, ForceMode.Impulse);
                foreach (Transform child in other.gameObject.transform)
                {
                    if (child.name == "redChecker (1)")
                    {
                        Debug.Log("SPRING");

                        GameObject obj = child.gameObject;
                        obj.GetComponent<Renderer>().enabled = true;
                    }
                   
                }

            }
            if (other.tag.Contains("Void"))
            {
                Respawn(currentCheckpoint);
            }
            if (other.tag.Contains("End"))
            {
                Debug.Log("You won");
                hasWon = true;
            }
            if (other.tag.Contains("Hippo"))
            {
                Debug.Log("Hippo Hit");
                HippoAtt();
            }
            if (other.tag.Contains("RevealCollider"))
            {
                Debug.Log("Reveal");
                foreach (Transform child in other.gameObject.transform)
                { 
                    
                    foreach(Transform chil in child.gameObject.transform)
                    {
                        if (chil.name == "Axe")
                        {
                            GameObject ob = chil.gameObject;
                            ob.GetComponent<Renderer>().enabled = true;
                        }
                        foreach (Transform chi in chil.gameObject.transform)
                        {
                            if (chi.name == "pSphere6" || chi.name == "pCone10")
                            {
                                Debug.Log(chi.name);
                                GameObject obj = chi.gameObject;
                                obj.GetComponent<Renderer>().enabled = true;
                            }
                            foreach(Transform ch in chi.gameObject.transform)
                            {
                                if(ch.name == "pCylinder14")
                                {
                                    GameObject o = ch.gameObject;
                                    o.GetComponent<Renderer>().enabled = true;
                                }
                            }
                        }
                    }
                   
                    // Do things with obj
                }
            }
            // this.Health -= 0.1f;
        }
        public void HippoAtt()
        {
            hippoIsAtt = true;
        }
        public void Respawn(GameObject checkPoint)
        {
           
            Debug.Log("RESPAWN");
            this.transform.position = checkPoint.transform.position + new Vector3(0f,5f,0f);
        }
        /// <summary>
        /// MonoBehaviour method called once per frame for every Collider 'other' that is touching the trigger.
        /// We're going to affect health while the beams are interesting the player
        /// </summary>
        /// <param name="other">Other.</param>
        public void OnTriggerStay(Collider other)
        {
            // we dont' do anything if we are not the local player.
            if (!photonView.IsMine)
            {
                return;
            }

            // We are only interested in Beamers
            // we should be using tags but for the sake of distribution, let's simply check by name.
           // if (!other.name.Contains("Beam"))
           // {
           //     return;
           // }

            // we slowly affect health when beam is constantly hitting us, so player has to move to prevent death.
           // this.Health -= 0.1f*Time.deltaTime;
        }

        public void OnTriggerExit(Collider other)
        {
            if (!photonView.IsMine)
            {
                return;
            }
            if (other.tag.Contains("Springboard"))
            {
                foreach (Transform child in other.gameObject.transform)
                {
                    if (child.name == "redChecker (1)")
                    {
                        GameObject obj = child.gameObject;
                        obj.GetComponent<Renderer>().enabled = false;
                    }
                }
            }
            if (other.tag.Contains("RevealCollider"))
            {
                Debug.Log("Reveal");
                foreach (Transform child in other.gameObject.transform)
                {
                    foreach (Transform chil in child.gameObject.transform)
                    {
                        if (chil.name == "Axe")
                        {
                            GameObject ob = chil.gameObject;
                            ob.GetComponent<Renderer>().enabled = false;
                        }
                        foreach (Transform chi in chil.gameObject.transform)
                        {
                            if (chi.name == "pSphere6" || chi.name == "pCone10")
                            {
                                GameObject obj = chi.gameObject;
                                obj.GetComponent<Renderer>().enabled = false;
                            }
                            foreach (Transform ch in chi.gameObject.transform)
                            {
                                if (ch.name == "pCylinder14")
                                {
                                    GameObject o = ch.gameObject;
                                    o.GetComponent<Renderer>().enabled = false;
                                }
                            }
                        }
                    }
                }
            }
        }


#if !UNITY_5_4_OR_NEWER
        /// <summary>See CalledOnLevelWasLoaded. Outdated in Unity 5.4.</summary>
        void OnLevelWasLoaded(int level)
        {
            this.CalledOnLevelWasLoaded(level);
        }
        #endif


        /// <summary>
        /// MonoBehaviour method called after a new level of index 'level' was loaded.
        /// We recreate the Player UI because it was destroy when we switched level.
        /// Also reposition the player if outside the current arena.
        /// </summary>
        /// <param name="level">Level index loaded</param>
        void CalledOnLevelWasLoaded(int level)
        {
            // check if we are outside the Arena and if it's the case, spawn around the center of the arena in a safe zone
            if (!Physics.Raycast(transform.position, -Vector3.up, 5f))
            {
                transform.position = new Vector3(0f, 5f, 0f);
            }

            GameObject _uiGo = Instantiate(this.playerUiPrefab);
            _uiGo.SendMessage("SetTarget", this, SendMessageOptions.RequireReceiver);
        }

        #endregion

        #region Private Methods


		#if UNITY_5_4_OR_NEWER
		void OnSceneLoaded(UnityEngine.SceneManagement.Scene scene, UnityEngine.SceneManagement.LoadSceneMode loadingMode)
		{
			this.CalledOnLevelWasLoaded(scene.buildIndex);
		}
		#endif

        /// <summary>
        /// Processes the inputs. This MUST ONLY BE USED when the player has authority over this Networked GameObject (photonView.isMine == true)
        /// </summary>
        void ProcessInputs()
        {
            float x = Input.GetAxisRaw("Horizontal");
            float z = Input.GetAxisRaw("Vertical");
            

            transform.Rotate(Vector3.up * turnspeed * x * Time.deltaTime);
            transform.Translate(0f, 0f, speed * z * Time.deltaTime);
          
            if(hippoIsAtt == true)
            {
                speed = 0.3f;
                hippoTimer -= Time.deltaTime;
                if(hippoTimer <= 0)
                {
                    hippoIsAtt = false;
                    speed = 9f;
                    hippoTimer = 1.0f;
                }
            }

            // if (Input.GetButtonDown("Fire1"))
            // {
            // we don't want to fire when we interact with UI buttons for example. IsPointerOverGameObject really means IsPointerOver*UI*GameObject
            // notice we don't use on on GetbuttonUp() few lines down, because one can mouse down, move over a UI element and release, which would lead to not lower the isFiring Flag.
            //  if (EventSystem.current.IsPointerOverGameObject())
            //  {
            //   //	return;
            //  }

            // if (!this.IsFiring)
            //  {
            //     this.IsFiring = true;
            //  }
            //}

            // if (Input.GetButtonUp("Fire1"))
            // {
            //    if (this.IsFiring)
            //    {
            //        this.IsFiring = false;
            //   }
            // }
        }

        #endregion

        #region IPunObservable implementation

        public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
        {
            if (stream.IsWriting)
            {
                stream.SendNext(this.hasWon);
                // We own this player: send the others our data
               // stream.SendNext(this.IsFiring);
               // stream.SendNext(this.Health);
            }
            else
            {
                this.hasLost = (bool)stream.ReceiveNext();
                // Network player, receive data
               // this.IsFiring = (bool)stream.ReceiveNext();
               // this.Health = (float)stream.ReceiveNext();
            }
        }

        #endregion
    }
}