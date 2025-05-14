using System;
using UnityEngine;

public class TriggerAttack : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Enemy"))
        {
             
        }
    }
}
