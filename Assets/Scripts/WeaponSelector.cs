using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class WeaponSelector : MonoBehaviour
{
    public GameObject OneHandedWeapon;

    private MeshRenderer meshRenderer;
    private MeshFilter meshFilter;

    [SerializeField] private Material[] materials;
    [SerializeField] private Mesh[] meshes;

    private int currentWeapon = 0;
    private List<Weapon> weapons;

    public struct Weapon
    {
        public Material material;
        public Mesh mesh;
    }

    private void Awake()
    {
        // empty weapons list
        weapons = new List<Weapon>();

        if (materials.Length == meshes.Length)
        {
            // load weapon mesh and materials
            for(int i = 0; i < materials.Length; i++)
            {
                Material mat = materials[i];
                Mesh mesh = meshes[i];

                Weapon wep = new Weapon();
                wep.material = mat;
                wep.mesh = mesh;

                weapons.Add(wep);
            }
            meshRenderer = OneHandedWeapon.GetComponent<MeshRenderer>();
            meshFilter = OneHandedWeapon.GetComponent<MeshFilter>();
        }
        else
        {
            // must be same length
            print("[Error] Materials and meshes not same length");
        }
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetKeyDown(KeyCode.Tab))
        {
            currentWeapon = (currentWeapon == weapons.Count - 1) ? 0 : currentWeapon + 1;

            // find the new weapon
            Weapon newWeapon = weapons.ElementAt(currentWeapon);
            if(newWeapon.material != null)
            {
                // some weapons may not have materials
                meshRenderer.materials = new Material[] { newWeapon.material };
            }

            meshFilter.mesh = newWeapon.mesh;
        }
    }
}
