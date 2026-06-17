using System.Collections.Generic;
using UnityEngine;

public class WeaponManager : MonoBehaviour
{
    [Header("Inventory slots")]
    public List<GameObject> startingWeapons = new List<GameObject>();

    private readonly List<GameObject> weaponInventory = new List<GameObject>();
    private int currentWeaponIndex = -1;

    void Start()
    {
        foreach (GameObject gun in startingWeapons)
        {
            weaponInventory.Add(gun);
            gun.SetActive(false);
        }

        if (weaponInventory.Count > 0)
            SwitchWeapon(0);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1) && weaponInventory.Count >= 1) SwitchWeapon(0);
        if (Input.GetKeyDown(KeyCode.Alpha2) && weaponInventory.Count >= 2) SwitchWeapon(1);
    }

    public void SwitchWeapon(int index)
    {
        if (index < 0 || index >= weaponInventory.Count || index == currentWeaponIndex) return;

        if (currentWeaponIndex != -1)
            weaponInventory[currentWeaponIndex].SetActive(false);

        currentWeaponIndex = index;
        weaponInventory[currentWeaponIndex].SetActive(true);
    }

    public void AddWeaponToInventory(GameObject gunPrefabReference)
    {
        Camera playerCamera = GetComponentInChildren<Camera>();
        if (playerCamera == null) return;

        foreach (Transform child in playerCamera.transform)
        {
            if (child.name == gunPrefabReference.name && !weaponInventory.Contains(child.gameObject))
            {
                weaponInventory.Add(child.gameObject);
                SwitchWeapon(weaponInventory.Count - 1);
                return;
            }
        }
    }
}
