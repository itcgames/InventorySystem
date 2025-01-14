﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Potion : MonoBehaviour
{
    InventoryItem _item;
    // Start is called before the first frame update
    void Awake()
    {
        _item = GetComponent<InventoryItem>();
        _item.useFunction += HealPlayer;
    }

    bool HealPlayer()
    {
        Debug.Log("Healing player");
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        PlayerScript script = player.GetComponent<PlayerScript>();
        return script.Heal(1.5f);
    }
}
