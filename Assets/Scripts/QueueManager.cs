using Unity.Netcode;
using UnityEngine;

public class QueueManager : NetworkBehaviour
{

    [SerializeField] private Transform spawnLocation;

    public void Queue(GameObject menuCharacter, CharacterCreator.Character character)
    {
        Destroy(menuCharacter);

        if (character.name.Equals("Brennan"))
        {
            StartServer(character.prefabHash);
        }
        else
        {
            // client
            NetworkManager.Singleton.StartClient();
        }
    }

    private void StartServer(uint prefabHash)
    {
        NetworkManager manager = NetworkManager.Singleton;
        manager.NetworkConfig.ConnectionApproval = true;

        // host
        manager.ConnectionApprovalCallback = (request, response) =>
        {
            // The client identifier to be authenticated
            var clientId = request.ClientNetworkId;
            var connectionData = request.Payload;

            // Your approval logic determines the following values
            response.Approved = true;
            response.CreatePlayerObject = true;
            response.PlayerPrefabHash = prefabHash;
            response.Position = spawnLocation.position;
            response.Rotation = spawnLocation.rotation;
            response.Pending = false;
        };

        manager.StartHost();
    }
}
