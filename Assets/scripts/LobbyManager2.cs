using System.Collections;
using System.Collections.Generic;
using Unity.Services.Core;
using UnityEngine;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using Unity.Services.Authentication;
using System.Threading.Tasks;
using Unity.Services.Relay.Models;
using Unity.Services.Relay;
using System;
using TMPro;


using UnityEngine.UI;


#if UNITY_EDITOR
using ParrelSync;
#endif

public class LobbyManager2 : MonoBehaviour
{//
   public RelayManager relayManager;
       private Lobby hostlobby;
    float lobbytimer;
    float maxlobbytimer=15; //max lobbytime 30seg;
    string playername;
    private string _playerId;
    private Lobby _connectedlobby;
    private float lobbyPollTimer;
    private readonly float lobbyPollTimerMAX= 2f;      
    private bool ishost=false;
    bool isplaying=false;
    private  const string JoinCodeKey ="JOINCODE";

    public const string GameMode ="GAMEMODE";
   


    ///LOBBY UI

    public GameObject startbutton;
    public GameObject lobbypanel;

    public TMP_Text playertext1,playertext2;
    private string niveldejugador ="0";
    public Image  playerimage,playerimage2;

    async void Start()
    {
         relayManager = GetComponent<RelayManager>();
         lobbyPollTimer = lobbyPollTimerMAX;

       

       await Authenticate();

         //Debug.Log("Signed in " +  AuthenticationService.Instance.PlayerId);
    }

    private async Task Authenticate() {
        var options = new InitializationOptions();
                
#if UNITY_EDITOR
        // Remove this if you don't have ParrelSync installed. 
        // It's used to differentiate the clients, otherwise lobby will count them as the same
        options.SetProfile(ClonesManager.IsClone() ? ClonesManager.GetArgument() : "Primary");
#endif

        await UnityServices.InitializeAsync(options);

         AuthenticationService.Instance.SignedIn += () =>{
            Debug.Log("Signed in " +  AuthenticationService.Instance.PlayerId);
        };

        await AuthenticationService.Instance.SignInAnonymouslyAsync();
        _playerId = AuthenticationService.Instance.PlayerId;
         playername = "UCslp "+ UnityEngine.Random.Range(1,1000);
    }

    public async void CreateorJoin(){


        _connectedlobby = await QuickJoinLobby() ?? await CreateLobby();


        //startbutton.SetActive(true);

    }

    public async void CreateLobbyButton(){

        await CreateLobby();
    }
    
   public async Task<Lobby>  CreateLobby(){


        try{ 
        string lobbyName = "myLobby";
        int maxPlayers=4;

        CreateLobbyOptions options = new CreateLobbyOptions{
            IsPrivate =false,
             Player = new Player{
                 Data = new Dictionary<string, PlayerDataObject>{
                    {"PlayerName",new PlayerDataObject(PlayerDataObject.VisibilityOptions.Public, playername)},
                     {"Nivel",new PlayerDataObject(PlayerDataObject.VisibilityOptions.Public,niveldejugador)},
                     {"Skin",new PlayerDataObject(PlayerDataObject.VisibilityOptions.Public,"0")}
                }
            },
            Data = new Dictionary<string,DataObject>{ 
                { JoinCodeKey, new DataObject(DataObject.VisibilityOptions.Public,"0")},
            { GameMode, new DataObject(DataObject.VisibilityOptions.Public,"GameMode")  }
                }
        };



            //instant JOIN for HOST ?                
            //joinCode = realy.getjoincode
           // string joincode = "0"; //GET FROM RELAY

        /*
        //lobby options Tarodev
        var options = new CreateLobbyOptions{
            Data = new Dictionary<string,DataObject>{ { JoinCodeKey, new DataObject(DataObject.VisibilityOptions.Public,joincode)}}
        };*/
        


        Unity.Services.Lobbies.Models.Lobby lobby = await LobbyService.Instance.CreateLobbyAsync(lobbyName,maxPlayers,options);


        _connectedlobby = lobby;
        ishost = true;

        startbutton.SetActive(true);
        lobbypanel.SetActive(true);
        playertext1.text =playername;

        Debug.Log("Lobby createad :" + lobby + lobby.MaxPlayers + " "+ lobby.Id+ " " + lobby.LobbyCode);

        return lobby;

        }catch(LobbyServiceException e){
            Debug.Log(e);
            return null;
        }

    }


    public async  void ListLobbies(){

        try{
        QueryLobbiesOptions queryLobbiesOptions = new QueryLobbiesOptions{
            Count =25,
            Filters = new List<QueryFilter>{
                new QueryFilter(QueryFilter.FieldOptions.AvailableSlots,"1",QueryFilter.OpOptions.GE)
            },
            Order = new List<QueryOrder>{
                new QueryOrder(false, QueryOrder.FieldOptions.Created)
                            }
        };

        
       QueryResponse queryResponse = await Lobbies.Instance.QueryLobbiesAsync(queryLobbiesOptions);

        Debug.Log("Lobbies found "+ queryResponse.Results.Count);

        foreach(Lobby lobby in queryResponse.Results){
            Debug.Log(lobby.Name + " " + lobby.MaxPlayers + " " + lobby.Players);

        }
        }catch(LobbyServiceException e){
            Debug.Log(e);
        }


    }

   public async void JoinLobby(){
        try{
                
       QueryResponse queryResponse = await Lobbies.Instance.QueryLobbiesAsync( );


       _connectedlobby = await Lobbies.Instance.JoinLobbyByIdAsync(queryResponse.Results[0].Id);

        Debug.Log("JOINED LOBBY" + queryResponse.Results[0].Id);
        
        }catch(LobbyServiceException e){
            Debug.Log(e);
        }


    }

    async void JoinLobbyByCode(string LobbyCode){
        try{
      
        await Lobbies.Instance.JoinLobbyByCodeAsync(LobbyCode);

        Debug.Log ("join with CODe "+ LobbyCode);
        
        }catch(LobbyServiceException e){
            Debug.Log(e);
        }


    }

    private async Task<Lobby> QuickJoinLobby(){

        try{
      
       var lobby = await Lobbies.Instance.QuickJoinLobbyAsync();

       // var a = await RelayService.Instance.JoinAllocationAsync(lobby.Data[JoinCodeKey].Value);
            //StartCLient;

        Debug.Log ("Quick joined a lobby");


        lobbypanel.SetActive(true);
        playertext2.text =playername;

        return lobby;
        
        }catch(LobbyServiceException e){
            Debug.Log(e);
            return null;
        }
    }




    private void PrintPlayers(Lobby lobby){

        Debug.Log("players in lobby" + lobby.Name);
        foreach(Player player in lobby.Players){
            Debug.Log(player.Id);
        }
    }

            public async void ActualizarColor(int x){


                    var options = new UpdatePlayerOptions{

                        Data = new Dictionary<string, PlayerDataObject>{
                            {"Skin", new PlayerDataObject(PlayerDataObject.VisibilityOptions.Public,x.ToString())}
                        }

                    };

                   await Lobbies.Instance.UpdatePlayerAsync(_connectedlobby.Id,_connectedlobby.Players[0].Id,options);


                     playerimage.color = Color.red;
            }

    public void ActualizarLobby(Lobby lobby){

        playertext1.text = lobby.Players[0].Data["PlayerName"].Value;

        if(lobby.Players[0].Data["Skin"].Value =="0"){
            playerimage.color = Color.white;
        }else  if(lobby.Players[0].Data["Skin"].Value =="1"){
            playerimage.color = Color.red;
        }else  if(lobby.Players[0].Data["Skin"].Value =="2"){
            playerimage.color = Color.green;
        }
        //playertext1n.text = lobby.Players[0].Data["Nivel"].Value;


        if(lobby.Players.Count>1){
            playerimage2.gameObject.SetActive(true);
         playertext2.text = lobby.Players[1].Data["PlayerName"].Value;


             if(lobby.Players[0].Data["Skin"].Value =="0"){
            playerimage2.color = Color.white;
        }else  if(lobby.Players[0].Data["Skin"].Value =="1"){
            playerimage2.color = Color.red;
        }else  if(lobby.Players[0].Data["Skin"].Value =="2"){
            playerimage2.color = Color.green;
        }
        }


    }


    public async void StartGame(){

        try{
              string relayjoinCo = await relayManager.CreateGame();
              isplaying=true;


               var options = new UpdateLobbyOptions{
            Data = new Dictionary<string,DataObject>{ { JoinCodeKey, new DataObject(DataObject.VisibilityOptions.Public,relayjoinCo)}}
        };


                    await Lobbies.Instance.UpdateLobbyAsync(_connectedlobby.Id,options);




        }catch(Exception){
            Debug.Log("error starting game");
        }


    }


    private async void LobbyPolling(){

        if(_connectedlobby !=null && !isplaying){

            
            lobbyPollTimer -= Time.deltaTime;

            if(lobbyPollTimer <=0){
                //float lobbyRefreshrate  =3f;
                lobbyPollTimer = lobbyPollTimerMAX;

                   Debug.Log("loBBY POLLING");
                   Lobby thislobby = await LobbyService.Instance.GetLobbyAsync(_connectedlobby.Id);



                   if(thislobby !=null){


                    ActualizarLobby(thislobby);

                   if(thislobby.Data[JoinCodeKey].Value !="0"){ //sino es el valor que pusimos por default ya tiene un codigo actualizado del relay
                            Debug.Log(thislobby.Data[JoinCodeKey].Value);
                            Debug.Log("JoinKEyCode was updated");

                            //UI
                            lobbypanel.SetActive(false);


                            if(!ishost)
                            {   isplaying = true;
                                relayManager.JoinGame(thislobby.Data[JoinCodeKey].Value);

                            }
                   }
                }else{
                    Debug.Log("connectedlobby disconnected");
                    _connectedlobby = null;
                    ishost = false;
                }
            }
        }
    }

   private void OnDestroy() {
    try{
        StopAllCoroutines();
        // todo: check if you are host
        if(_connectedlobby != null){
            if(_connectedlobby.HostId == _playerId){
                Debug.Log("On destroy: YOU ARE HOST, delete lobby");
                Lobbies.Instance.DeleteLobbyAsync(_connectedlobby.Id);

            }else{
                     Debug.Log("On destroy: Client removed from lobby");
                Lobbies.Instance.RemovePlayerAsync(_connectedlobby.Id,_playerId);
            }
        }


    }catch(Exception){

        Debug.Log("Error, on destoy lobbymanager");
    }
        
    }


    void Update(){

        PingLobby();
        LobbyPolling();
    }

    async void PingLobby(){

        if(ishost){

        if(_connectedlobby != null){
            lobbytimer -= Time.deltaTime;
            if( lobbytimer<0){
                lobbytimer = maxlobbytimer;
                await  LobbyService.Instance.SendHeartbeatPingAsync(_connectedlobby.Id);
            }

        }
        }

    }



    public void CambiarCOlor(){


    }
}
