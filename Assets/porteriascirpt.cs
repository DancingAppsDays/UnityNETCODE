using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class porteriascirpt : MonoBehaviour
{
    public int numeroporteria;
    private void OnTriggerEnter(Collider other) {
        
        if(other.gameObject.tag == "ball"){

            other.gameObject.GetComponent<NetworkObject>().Despawn();
            Destroy(other.gameObject);


            if(numeroporteria ==1)
            GameManager.instancia.Gol1();

            if(numeroporteria ==2)
             GameManager.instancia.Gol2();
        }

    }
}
