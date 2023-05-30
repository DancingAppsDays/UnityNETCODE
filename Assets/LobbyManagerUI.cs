using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LobbyManagerUI : MonoBehaviour
{


    public GameObject playbutton;
    public GameObject startbutton;
    public GameObject lobbypanel,inlobbypanel,listlobbypanel;
    public GameObject botonlobbyprefab;
    public Transform contenedorbotoneslobby;

    

    private void OnEnable() {
        
            LobbyManager.joinlobbyaction += JoinedLobby;
            LobbyManager.showlistaction += Showlist;
              LobbyManager.hidelobbyaction += HideLobby;
              LobbyManager.exitlobby += exitlobby;
    }

    private void OnDisable() {
              LobbyManager.joinlobbyaction -= JoinedLobby;
                 LobbyManager.showlistaction -= Showlist;
                  LobbyManager.hidelobbyaction -= HideLobby;
                  LobbyManager.exitlobby -= exitlobby;
    }


    void JoinedLobby(){

         startbutton.SetActive(true);

        inlobbypanel.SetActive(true);
        lobbypanel.SetActive(false);
        playbutton.SetActive(false);
    }

    void HideLobby(){

        inlobbypanel.SetActive(false);
        lobbypanel.SetActive(false);
    }

    void Showlist(){

        listlobbypanel.SetActive(true);

    }
    public void Hidelist(){
        listlobbypanel.SetActive(false);
    }

    void Start()
    {
           startbutton.SetActive(false);

        inlobbypanel.SetActive(false);
        lobbypanel.SetActive(true);
        playbutton.SetActive(true);
    }


    public void exitlobby(){

         startbutton.SetActive(false);

        inlobbypanel.SetActive(false);
        lobbypanel.SetActive(true);
        playbutton.SetActive(true);

    }

    // Update is called once per frame
    void Update()
    {
        
    }
    
}
