using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using System;
using System.Threading.Tasks;
using TMPro;
using UnityEngine.UI;

#if UNITY_EDITOR
using ParrelSync;
#endif

public class ManagerLobbyParcial : MonoBehaviour
{
            public RelayManager relayManager;
        private Lobby nuestrolobby;



        //INCLUIR EN otro script UI
        public GameObject canvaslobby;
        public TMP_Text lobbylisttext;



        

    void Start()
    {
            relayManager = GetComponent<RelayManager>();
        Autenticar();
    }
    async void Autenticar(){


           var options = new InitializationOptions();
                
#if UNITY_EDITOR
        // Remove this if you don't have ParrelSync installed. 
        // It's used to differentiate the clients, otherwise lobby will count them as the same
        options.SetProfile(ClonesManager.IsClone() ? ClonesManager.GetArgument() : "Primary");
#endif

        await UnityServices.InitializeAsync(options);

       //await UnityServices.InitializeAsync();
       await AuthenticationService.Instance.SignInAnonymouslyAsync();
       Debug.Log("Signed in " +  AuthenticationService.Instance.PlayerId);


       ListarLobbies();
    }


    public async void CreateOrJoin(){
            nuestrolobby = await QuickJoinLobby() ?? await CreateLobby();
    }


    public async Task<Lobby>  CreateLobby(){

            try{
            //TODO, almancenar nombres de jugador
        Lobby lobby = await LobbyService.Instance.CreateLobbyAsync("Lobby1",4);

        Debug.Log("LObby creado" + lobby +"id #"+ lobby.Id+ "lobbycode" +  lobby.LobbyCode);
    
               // relayManager.CreateGame();


             //   canvaslobby.SetActive(false);


            return lobby;
            }catch(LobbyServiceException e){

                Debug.Log("falló la creacion de lobhy");
                return null;
            }
    }


    public async void JoinLobbybyCode(string LobbyCode){

        await Lobbies.Instance.JoinLobbyByCodeAsync(LobbyCode);
        Debug.Log("nos unimos a looby by code");


    }
public async Task<Lobby>  QuickJoinLobby(){

            try{
              Lobby lobby =  await Lobbies.Instance.QuickJoinLobbyAsync();
             Debug.Log("nos unimos a looby RAPIDO");



              // canvaslobby.SetActive(false);

               // relayManager.JoinGame(nuestrolobby.LobbyCode);



             return lobby;

            }  catch(LobbyServiceException e){
                   
                Debug.Log("falló unirse de lobhy");
                 return null;
            }
}


public async void ListarLobbies(){


    QueryLobbiesOptions queryLobbiesOptions= new QueryLobbiesOptions{
        Count=25,
        Filters = new List<QueryFilter>{
            new QueryFilter(QueryFilter.FieldOptions.AvailableSlots,"1",QueryFilter.OpOptions.GE)
        }//,
        //Order = new List<QueryOrder>....
    };
          QueryResponse query =   await Lobbies.Instance.QueryLobbiesAsync(queryLobbiesOptions);
            Debug.Log("Lobbies :" + query.Results.Count);
              lobbylisttext.text ="";
            foreach (Lobby lobby in query.Results){

                Debug.Log(lobby.Name + " " + lobby.MaxPlayers + " " + lobby.Players);
                lobbylisttext.text += "\n" +lobby.Name.ToString() + " " + lobby.MaxPlayers.ToString()  + " " + lobby.Players.ToString() ;



                GameObject newbut = new GameObject();
                newbut.AddComponent<Button>();
                newbut.transform.SetParent(canvaslobby.transform);
            }

}
    


}
