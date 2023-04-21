using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;
using TMPro;
using Unity.Netcode;
using UnityEngine.UI;
using UnityEditor;

public class CharacterCreator : MonoBehaviour
{

    public List<Character> characters = new();
    private Dictionary<Button, int> objToSlot = new();

    public bool inQueue = false;
    public int selectedCharacter = 0;
    public GameObject[] characterSlots;
    public GameObject startQueuePanel;
    private GameObject character;
    public PlayerNetwork network;

    public GameObject leaveQueue;
    public GameObject queueTimer;

    [SerializeField] private Canvas canvas;
    [SerializeField] private Transform characterLocation;
    [SerializeField] private GameObject characterSettingsPanel;
    [SerializeField] private Transform characterSettingsParent;
    [SerializeField] private TMP_Dropdown role;
    [SerializeField] private TMP_InputField characterName;
    [SerializeField] private GameObject knightPrefab;
    [SerializeField] private GameObject archerPrefab;

    public struct Character
    {
        public GameObject prefab;
        public uint prefabHash;

        public FixedString32Bytes name;
        public FixedString32Bytes role;

        public int currentLevel;
        public float currentExperience;
    }

    private AudioSource clickSound;
    private void Start()
    {
        // todo: load character from database

        clickSound = GetComponent<AudioSource>();
        UpdateCharacter();
        
        // role change listener
        role.onValueChanged.AddListener(delegate { UpdateCharacter(); });

        // change character selection listener
        for (int i = 0; i < characterSlots.Length; i++)
        {
            GameObject characterSlot = characterSlots[i];
            Button btn = characterSlot.GetComponent<Button>();
            if(btn != null)
            {
                objToSlot.Add(btn, i);
            }
            btn.onClick.AddListener(delegate { SelectCharacter(btn); });
        }

        // Allow create character with enter key
        if (Input.GetKeyDown(KeyCode.Return) && characterName.isFocused)
        {
            CreateCharacter();
        }
    }

    public void QueueCharacter()
    {
        if (selectedCharacter < characters.Count)
        {
            Character character = characters[selectedCharacter];

            // hide other menu character during queue
            for (int i = 0; i < characterSlots.Length; i++)
            {
                if (selectedCharacter != i) characterSlots[i].SetActive(false); // disable them :)
            }

            startQueuePanel.SetActive(false); // remove start queue panel
            leaveQueue.SetActive(true); // add leave queue panel
            queueTimer.SetActive(true); // add queue timer

            Debug.Log("Prefab Hash: " + character.prefabHash);
            network.CreateConnection(this.character, character.prefabHash, character.name.Equals("Brennan"));
        }
    }

    public void LeaveQueue()
    {
        // re-enable the other menu characters again
        for (int i = 0; i < characterSlots.Length; i++)
        {
            if (selectedCharacter != i) characterSlots[i].SetActive(true);
        }

        leaveQueue.SetActive(false);
        queueTimer.SetActive(false);
        startQueuePanel.SetActive(true);

        network.StopConnection();

    }

    private void UpdateCharacter()
    {
        Debug.Log("Updating the character role...");
        if(character != null)
        {
            Destroy(character);
        }

        string text = role.options[role.value].text;
        switch (text)
        {
            case "Archer":
                character = Instantiate(archerPrefab, characterLocation);
                character.SetActive(true);
                break;
            case "Warrior":
                character = Instantiate(knightPrefab, characterLocation);
                character.SetActive(true);
                break;
            default: break;
        }
    }

    public void SelectCharacter(Button button)
    {
        int newCharacter = objToSlot[button];

        // do not update if already selected
        if (selectedCharacter == newCharacter) return;

        selectedCharacter = newCharacter;
        if (selectedCharacter < characters.Count)
        {
            Destroy(this.character);
            Character character = characters[selectedCharacter];
            this.character = Instantiate(character.prefab, characterLocation);
            this.character.SetActive(true);
            characterSettingsPanel.SetActive(false);
            startQueuePanel.SetActive(true);
        }    
        else
        {
            characterName.text = "Character Name";
            role.value = 0;
            startQueuePanel.SetActive(false);
            characterSettingsPanel.SetActive(true);
            UpdateCharacter();
        }
    }

    public void CreateCharacter()
    {

        if (characters.Count >= characterSlots.Length) return;    // check max characters reached
        if (selectedCharacter < characters.Count) return;         // check already on existing character

        if (character != null)
        {
            Debug.Log("Creating Character...");
            GameObject characterSlot = characterSlots[selectedCharacter];

            string name = characterName.text;
            string className = role.options[role.value].text;

            // update the slot text on the left side of the screen
            TMP_Text text = characterSlot.transform.GetComponentInChildren<TMP_Text>();
            text.text = name + " Level 1 " + className;

            Character character = new();
            // experience
            character.currentLevel = 0;
            character.currentExperience = 0.0f;

            // name and role
            character.name = characterName.text;
            character.role = className;

            // prefab data
            character.prefab = Instantiate(this.character);
            character.prefab.SetActive(false);
            character.prefabHash = character.prefab.GetComponent<CustomNetworkObject>().hashId;

            // old method (uint)new SerializedObject(character.prefab.GetComponent<NetworkObject>()).FindProperty("GlobalObjectIdHash").intValue;

            // add our character and save to database
            characters.Add(character);

            // update panels
            characterSettingsPanel.SetActive(false);
            startQueuePanel.SetActive(true);
        }
    }

    public void PlaySound()
    {
        clickSound.Play();
    }
}
