using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using UnityEngine.UI;

//using MLAPI.
using Unity.Netcode.Transports.UNET;
using Unity.Netcode.Transports.UTP;
using TMPro;

public class Interfaz : MonoBehaviour
{
    public Button server, host, client;
   
    public TMP_InputField iptex;


    public NetworkManager net;


    //Relay relay;
        

    public string ip = "";
    public string relaycode ="";
    
    [SerializeField]
     TMP_Text textdebug;
     [SerializeField]
     GameObject scroll;

    void Start()
    {

        //relay = GetComponent<Relay>();

    //          textdebug = GameObject.Find("TEXTDEBUG").GetComponent<TMP_Text>();
      //  scroll = GameObject.Find("Scroll");
        
        server.onClick.AddListener(() =>
        {
            NetworkManager.Singleton.StartServer();
        });

        host.onClick.AddListener(() =>
        {
            NetworkManager.Singleton.StartHost();
           // relay.CreateRelay();

        });

        client.onClick.AddListener(() =>
        {
            NetworkManager.Singleton.StartClient();
        });
    }

   
    void Update()
    {
        
    }

    public void textdebugger(string texto){

        textdebug.text += "\n "+ texto;
            scroll.GetComponent<ScrollRect>().verticalNormalizedPosition = 0f;
        
    }
    public void UpdateIP(){
           ip = iptex.text ;
                  //textdebugger("adress cambiada a" + ip.ToString());
        //net.GetComponent<UNetTransport>().ConnectAddress = ip.ToString();// "192.168.1.100";
            NetworkManager.Singleton.GetComponent<UnityTransport>().ConnectionData.Address = ip.ToString();
          
    }
}
