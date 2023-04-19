using Unity.Netcode;
using UnityEngine;

public class QueueManager : MonoBehaviour
{

    [SerializeField] private Transform spawnLocation;

    private void Start()
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
            response.PlayerPrefabHash = System.BitConverter.ToUInt32(request.Payload);
            response.Position = spawnLocation.position;
            response.Rotation = spawnLocation.rotation;
            response.Pending = false;
        };
    }

    public void Queue(GameObject menuCharacter, CharacterCreator.Character character)
    {
        Destroy(menuCharacter);

        NetworkManager.Singleton.NetworkConfig.ConnectionData = System.BitConverter.GetBytes(character.prefabHash);

        if (character.name.Equals("Brennan"))
        {
            NetworkManager.Singleton.StartHost();
        }
        else
        {
            // client
            NetworkManager.Singleton.StartClient();
        }
    }
}
