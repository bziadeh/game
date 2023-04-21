using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Netcode;
using Unity.Netcode.Components;
using UnityEngine;

// Handles Creating Client & Server
public class PlayerNetwork : MonoBehaviour
{

    private NetworkVariable<bool> inGame = new(false, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
    private NetworkVariable<bool> startingGame = new(false, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner); // in queue when starting by default

    public SceneLoader sceneLoader;
    public CharacterCreator creator;

    public Transform spawnLocation;
    public NetworkManager manager;

    private void Start()
    {
        manager = NetworkManager.Singleton;
    }

    // Creates a client or starts the server depending on some conditions...
    public void CreateConnection(GameObject menuCharacter, uint prefabHash, bool server)
    {
        Debug.Log("Creating new Instance...");

        manager.NetworkConfig.ConnectionData = System.BitConverter.GetBytes(prefabHash); // prefab setup
        manager.NetworkConfig.ConnectionApproval = true;
        
        if (server)
        {
            SetupServer();

            // manager.StartHost()
            manager.StartServer();
        }
        else
        {
            // Destroy(menuCharacter);
            manager.StartClient();
        }
    }

    // Sets up our server with some connection approval callback
    private void SetupServer()
    {
        // host
        manager.ConnectionApprovalCallback = (request, response) =>
        {
            // The client identifier to be authenticated
            var clientId = request.ClientNetworkId;
            var connectionData = request.Payload;

            // Your approval logic determines the following values
            response.Approved = true;
            response.CreatePlayerObject = false;
            response.PlayerPrefabHash = System.BitConverter.ToUInt32(request.Payload);
            response.Position = spawnLocation.position;
            response.Rotation = spawnLocation.rotation;
            response.Pending = false;
        };


    }

    public void StopConnection()
    {
        if(manager.IsServer)
        {
            // stop server
            StopServer();
        } else if(manager.IsConnectedClient)
        {
            // stop local client
            manager.Shutdown();
        }
    }

    public void StopServer()
    {
        manager.Shutdown();

        // At this point we must use the UnityEngine's SceneManager to switch back to the MainMenu
        // UnityEngine.SceneManagement.SceneManager.LoadScene("MainMenu");
    }

    [ServerRpc]
    public void StopClientServerRpc(ulong clientId)
    {
        manager.DisconnectClient(clientId);
    }

    private void Update()
    {
        if(manager.IsServer)
        {
            UpdateServer();

        } else if(manager.IsConnectedClient) {

            UpdateClient();
        }
    }

    private void UpdateClient()
    {
        // Inside this code block is being executed by all connected
        // clients on the network
        if (startingGame.Value)
        {

            // Update the players status
            startingGame.Value = false;
            inGame.Value = true;


            // Load the Scene
            LoadScene();
        }
    }

    private void UpdateServer()
    {
        IReadOnlyList<NetworkClient> clientList = manager.ConnectedClientsList;
        int clientCount = clientList.Count;

        // Keep track of the connected clients for debug purposes
        Debug.Log("Connected Clients: " + clientCount);

        if (clientCount >= 2)
        {
            for (int i = 0; i < clientCount; i++)
            {
                NetworkClient client = clientList[i];

                // create parameters to send only to this client id
                ClientRpcParams clientRpcParams = new()
                {
                    Send = new ClientRpcSendParams
                    {
                        TargetClientIds = new ulong[] { client.ClientId }
                    }
                };

                StartQueue(clientRpcParams);
            }
        }
    }

    [ClientRpc]
    public void StartQueue(ClientRpcParams param = default)
    {
        // Determine if this player has been moved to a Game scene yet...
        if (!startingGame.Value)
        {
            startingGame.Value = true;
        }
    }

    private void LoadScene()
    {
        sceneLoader.LoadSceneAsync("Arena");
    }
}
