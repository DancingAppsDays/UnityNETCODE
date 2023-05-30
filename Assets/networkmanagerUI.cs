using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Unity.Netcode;

public class networkmanagerUI : MonoBehaviour
{
    public Button server,host,client;


    // Start is called before the first frame update
    void Start()
    {
        server.onClick.AddListener(() =>{
            NetworkManager.Singleton.StartServer(); 
        });

         host.onClick.AddListener(() =>{
            NetworkManager.Singleton.StartHost(); 
        });

         client.onClick.AddListener(() =>{
            NetworkManager.Singleton.StartClient(); 
        });
         
    }

    
}
