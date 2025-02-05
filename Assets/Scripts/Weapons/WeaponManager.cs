using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponManager : MonoBehaviour
{
    public static WeaponManager instance;
    public static WeaponMoveset equippedWeapon;

    public WeaponMoveset startingWeapon;
    public List<WeaponMoveset> ownedWeapons = new List<WeaponMoveset>();

    // Start is called before the first frame update
    void Start()
    {
        if (instance != null)
        {
            Destroy(this.gameObject);
            return;
        }

        //Debug.Log("Weapon manager setup");

        instance = this;
        transform.SetParent(null);
        DontDestroyOnLoad(this.gameObject);

        equippedWeapon = startingWeapon;

        if (!ownedWeapons.Contains(startingWeapon))
        {
            ownedWeapons.Add(startingWeapon);
        }

        equipDelegate += EquipWeapon;
    }

    void EquipWeapon(WeaponMoveset weapon)
    {
        equippedWeapon = weapon;
    }

    public delegate void EquipDelegate(WeaponMoveset weapon);
    public EquipDelegate equipDelegate;
}
