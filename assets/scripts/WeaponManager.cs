//WeaponManager.cs

using UnityEngine;
using System.Collections.Generic;

public class WeaponManager : MonoBehaviour
{
    [Header("Inventory slots")]
    public List<GameObject> startingWeapons = new List<GameObject>(); // Drag weapon child objects here
    private List<GameObject> weaponInventory = new List<GameObject>();
    private int currentWeaponIndex = -1; // -1 means bare hands / melee weapon active

    void Start()
    {
        // Populate inventory with starting weapons (like your knife)
        foreach (GameObject gun in startingWeapons)
        {
            weaponInventory.Add(gun);
            gun.SetActive(false);
        }

        if (weaponInventory.Count > 0) SwitchWeapon(0); // Equip first slot
    }

    void Update()
    {
        if (weaponInventory.Count == 0 || currentWeaponIndex == -1) return;

        Firearm activeWeapon = weaponInventory[currentWeaponIndex].GetComponent<Firearm>();
        if (activeWeapon == null) return;

        // NOTE: Shooting input, fire rate, and ammo management are now handled 
        // automatically inside the Firearm script itself! No logic needed here.

        // Quick Weapon Swapping with number keys
        if (Input.GetKeyDown(KeyCode.Alpha1) && weaponInventory.Count >= 1) SwitchWeapon(0);
        if (Input.GetKeyDown(KeyCode.Alpha2) && weaponInventory.Count >= 2) SwitchWeapon(1);
    }

    public void SwitchWeapon(int index)
    {
        if (index < 0 || index >= weaponInventory.Count || index == currentWeaponIndex) return;

        // Hide previous gun
        if (currentWeaponIndex != -1) weaponInventory[currentWeaponIndex].SetActive(false);

        currentWeaponIndex = index;
        weaponInventory[currentWeaponIndex].SetActive(true);
        Debug.Log("Switched to weapon: " + weaponInventory[currentWeaponIndex].name);
    }

    // This public method handles adding picked up weapons directly to the player inventory!
    public void AddWeaponToInventory(GameObject gunPrefabReference)
    {
        // Search inside your camera/weapon holder for an matching deactivated gun object
        foreach (Transform child in GetComponentInChildren<Camera>().transform)
        {
            if (child.name == gunPrefabReference.name && !weaponInventory.Contains(child.gameObject))
            {
                weaponInventory.Add(child.gameObject);
                SwitchWeapon(weaponInventory.Count - 1); // Auto-equip the fresh pickup!
                return;
            }
        }
    }
}