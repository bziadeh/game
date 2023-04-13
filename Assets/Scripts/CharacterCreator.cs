using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;
using TMPro;
using Unity.Netcode;
using UnityEngine.UI;

public class CharacterCreator : MonoBehaviour
{

    private List<Character> characters = new();
    private Dictionary<Button, int> objToSlot = new();

    private int selectedCharacter = 0;
    private GameObject character; 

    [SerializeField] private Transform characterLocation;
    [SerializeField] private GameObject characterSettingsPanel;
    [SerializeField] private Transform characterSettingsParent;
    [SerializeField] private GameObject[] characterSlots;
    [SerializeField] private TMP_Dropdown role;
    [SerializeField] private TMP_InputField characterName;
    [SerializeField] private GameObject knightPrefab;
    [SerializeField] private GameObject archerPrefab;

    public struct Character
    {
        public GameObject prefab;

        public FixedString32Bytes name;
        public FixedString32Bytes role;

        public int currentLevel;
        public float currentExperience;
    }

    private AudioSource clickSound;
    private void Start()
    {
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
        }    
        else
        {
            characterName.text = "Character Name";
            role.value = 0;
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

            TMP_Text text = characterSlot.transform.GetComponentInChildren<TMP_Text>();
            text.text = name + " Level 1 " + className;

            Character character = new();
            character.currentLevel = 0;
            character.currentExperience = 0.0f;
            character.name = characterName.text;
            character.role = className;
            character.prefab = Instantiate(this.character);
            character.prefab.SetActive(false);
            characters.Add(character);
            characterSettingsPanel.SetActive(false);
        }
    }

    public void PlaySound()
    {
        clickSound.Play();
    }
}
