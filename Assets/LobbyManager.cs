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

public class LobbyManager : MonoBehaviour
{//

    public static LobbyManager singleton;
   public RelayManager relayManager;
       //private Lobby hostlobby;
    float lobbytimer;
    float maxlobbytimer=15; //max lobbytime 30seg;
    string playername;
    private string _playerId;
    public  Lobby _connectedlobby;  //Made public to reference in created player...
    private float lobbyPollTimer;
    private readonly float lobbyPollTimerMAX= 2f;      
    private bool ishost=false;
    bool isplaying=false;
    private  const string JoinCodeKey ="JOINCODE";
    public const string GameMode ="GAMEMODE";   
    public const string MAP ="mapa";

    ///LOBBY UI

    public GameObject playbutton;
    public GameObject startbutton, mapbutton;
   // public GameObject lobbypanel,inlobbypanel;
    public GameObject botonlobbyprefab;
    public Transform contenedorbotoneslobby;
    public TMP_Text playername1, playername2;
    public TMP_Text maptext;

    public Image playerimage1,playerimage2;
    public int playercolor1=0,playercolor2=0;

    public TMP_Text playerjob1, playerjob2;

    public GameObject butcolor1,butcolor2,butcolor3,butcolor21,butcolor22,butcolor23;


    public static Action joinlobbyaction,showlistaction, hidelobbyaction,exitlobby;


    public List<Lobby> listalobbies = new List<Lobby>();
    public List<Player> listaplayerslobby = new List<Player>();
    public GameObject panelplayers;
    public GameObject panelplayerprefab;
    public GameObject contenedorplayers;
    

    async void Start()
    {
        singleton = this;

         relayManager = GetComponent<RelayManager>();
         lobbyPollTimer = lobbyPollTimerMAX;
     playername = "UCslp "+ UnityEngine.Random.Range(1,1000);
       

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

            if(!AuthenticationService.Instance.IsSignedIn){

         AuthenticationService.Instance.SignedIn += () =>{
            Debug.Log("Signed in " +  AuthenticationService.Instance.PlayerId);
        };

        await AuthenticationService.Instance.SignInAnonymouslyAsync();
            }

        _playerId = AuthenticationService.Instance.PlayerId;        //causeblank if alaready authenticated!
        
    }

    public async void CreateorJoin(){

         _playerId = AuthenticationService.Instance.PlayerId;
         
        _connectedlobby = await QuickJoinLobby() ?? await CreateLobby();


        

    }

    public async void CreateLobbyButton(){

        await CreateLobby();
    }
    
   public async Task<Lobby>  CreateLobby(){

        int x = UnityEngine.Random.Range(0,1000);
        try{ 
        string lobbyName = "myLobby"+ x.ToString();
        int maxPlayers=2;

        CreateLobbyOptions options = new CreateLobbyOptions{
            IsPrivate =false,
             Player = Getplayer(),
            Data = new Dictionary<string,DataObject>{ 
                { JoinCodeKey, new DataObject(DataObject.VisibilityOptions.Public,"0")},
                 { GameMode, new DataObject(DataObject.VisibilityOptions.Public,"1")  },
                  { MAP, new DataObject(DataObject.VisibilityOptions.Public,"0")  }
                }
        };



        Unity.Services.Lobbies.Models.Lobby lobby = await LobbyService.Instance.CreateLobbyAsync(lobbyName,maxPlayers,options);


        _connectedlobby = lobby;
        ishost = true;
        listaplayerslobby.Clear();


        //UI
        //EN VEZ DE lobbymanagerscript.joinwindow();
           joinlobbyaction?.Invoke();

           playername1.text = lobby.Players[0].Data["PlayerName"].Value;
       // startbutton.SetActive(true);
       // inlobbypanel.SetActive(true);
       // playbutton.SetActive(false);

            listaplayerslobby.Add(lobby.Players[0]);



        Debug.Log("Lobby createad :" + lobby + lobby.MaxPlayers + " "+ lobby.Id+ " " + lobby.LobbyCode);

        return lobby;

        }catch(LobbyServiceException e){
            Debug.Log(e);
            return null;
        }

    }


    Player Getplayer(){
        var player = new Player{
                 Data = new Dictionary<string, PlayerDataObject>{
                    {"PlayerName",new PlayerDataObject(PlayerDataObject.VisibilityOptions.Public, playername)},
                    {"Skin",new PlayerDataObject(PlayerDataObject.VisibilityOptions.Member,"0")},
                    {"JOB" , new PlayerDataObject(PlayerDataObject.VisibilityOptions.Member,"0")}

                }
            };
        return player;

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

        listalobbies.Clear();

        foreach(Transform child in contenedorbotoneslobby.transform){

            DestroyImmediate(child.gameObject);

        }


        showlistaction?.Invoke();

        foreach(Lobby lobby in queryResponse.Results){
            Debug.Log(lobby.Name + " " + lobby.MaxPlayers + " " + lobby.Players);
                listalobbies.Add(lobby);


                //GameManager.instancia.AñadirBotonLobby(lobby);

                GameObject lobbybutton = Instantiate(botonlobbyprefab);
                lobbybutton.transform.SetParent(contenedorbotoneslobby);

                lobbybutton.transform.GetChild(0).GetComponent<TMP_Text>().text = lobby.Name + "  "  + lobby.Players.Count + "/"+ lobby.MaxPlayers ;

                lobbybutton.GetComponent<Button>().onClick.AddListener(() =>
                {
                   int x = contenedorbotoneslobby.transform.childCount-1;
                  
                        JoinLobbybyList(x);
                     
                    
                });
        }
        }catch(LobbyServiceException e){
            Debug.Log(e);
        }


    }

      public async void  JoinLobbybyList(int listvalue){

           // Debug.Log(listalobbies.Count);
           /// Debug.Log(listalobbies[listvalue].Name);
           // Debug.Log(listalobbies[listvalue].Id);
           // Debug.Log(listalobbies[listvalue].LobbyCode); //muestra null, solo para miembros

           await JoinLobbyByIdAsync( listalobbies[listvalue].Id);

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
    async Task<Lobby> JoinLobbyByIdAsync(string LobbyID){

        try{
       JoinLobbyByIdOptions options = new JoinLobbyByIdOptions{            
             Player = Getplayer()            };

        var lobby = await Lobbies.Instance.JoinLobbyByIdAsync(LobbyID,options);
        Debug.Log ("join with button ID "+ LobbyID);
       
         joinlobbyaction?.Invoke();
        playername2.text = lobby.Players[1].Data["PlayerName"].Value;

        return lobby;
        
        }catch(LobbyServiceException e){
            Debug.Log(e);
            return null;
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
                
       QuickJoinLobbyOptions options = new QuickJoinLobbyOptions{
            
             Player = Getplayer()
            };


      
       var lobby = await Lobbies.Instance.QuickJoinLobbyAsync(options);

       // var a = await RelayService.Instance.JoinAllocationAsync(lobby.Data[JoinCodeKey].Value);
            //StartCLient;

        Debug.Log ("Quick joined a lobby");
         joinlobbyaction?.Invoke();

        playername2.text = lobby.Players[1].Data["PlayerName"].Value;
        return lobby;
        
        }catch(LobbyServiceException e){
            Debug.Log(e);
            return null;
        }
    }




    private void PrintPlayers(Lobby lobby){

        Debug.Log("players in lobby" + lobby.Name);
        foreach(Player player in lobby.Players){
            Debug.Log(player.Id +" "+ player.Data["playername"].Value);
        }
    }

    private void ListarPlayers(Lobby lobby){

        listaplayerslobby.Clear();
        foreach(Player player in lobby.Players){
            listaplayerslobby.Add(player);
        }
    }


    public async void StartGame(){

           
        try{
              string relayjoinCo = await relayManager.CreateGame();
              isplaying=true;

                hidelobbyaction?.Invoke();
              
                //inlobbypanel.SetActive(false);
                

               var options = new UpdateLobbyOptions{
            Data = new Dictionary<string,DataObject>{ { JoinCodeKey, new DataObject(DataObject.VisibilityOptions.Public,relayjoinCo)}}
                };


                Lobby lob = await Lobbies.Instance.UpdateLobbyAsync(_connectedlobby.Id,options);

                 // Debug.Log($"this connectedlobby mao is {lob.Data[MAP].Value}");
                 GameManager.instancia.selectmap(int.Parse(lob.Data[MAP].Value)); //outside...testing
                



        }catch(LobbyServiceException e){
            Debug.Log(e);
            Debug.Log("error starting game");
        }


    }




//// LOVVY POLLING
    private async void LobbyPolling(){

        if(_connectedlobby !=null && !isplaying){

            
            lobbyPollTimer -= Time.deltaTime;

            if(lobbyPollTimer <=0){
                //float lobbyRefreshrate  =3f;
                lobbyPollTimer = lobbyPollTimerMAX;

                   Debug.Log("loBBY POLLING");
                   Lobby thislobby = await LobbyService.Instance.GetLobbyAsync(_connectedlobby.Id); //TOMA actualizada

                   if(thislobby !=null){

                    UpdateLobbyUI(thislobby); //actualized data
                   // UpdateLobbyUIAbstract(thislobby);

                   if(thislobby.Data[JoinCodeKey].Value !="0"){ //sino es el valor que pusimos por default ya tiene un codigo actualizado del relay
                            Debug.Log(thislobby.Data[JoinCodeKey].Value);
                            Debug.Log("JoinKEyCode was updated");

                            //UI
                           // inlobbypanel.SetActive(false);
                            hidelobbyaction?.Invoke();


                            if(!ishost)
                            {   isplaying = true;
                                relayManager.JoinGame(thislobby.Data[JoinCodeKey].Value);
                                  //GameManager.instancia.selectmap(int.Parse(_connectedlobby.Data[MAP].Value)); 
                                  GameManager.instancia.selectmap(int.Parse(thislobby.Data[MAP].Value));   

                            }
                   }
                }else{
                    Debug.Log("connectedlobby disconnected");
                    _connectedlobby = null;
                    ishost = false;
                }
            }
        }else if(_connectedlobby == null){

               // Debug.Log()
                 //exitlobby?.Invoke();  

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
                ishost = false;
                _connectedlobby = null;

            }else{
                     Debug.Log("On destroy: Client removed from lobby");

                     //TODO CHECK IF LOBBY NOT FOUND???
                Lobbies.Instance.RemovePlayerAsync(_connectedlobby.Id,_playerId);
                _connectedlobby = null;
                    //TODO: regresar al menu principal

            }
        }


    }catch(Exception e ){
        Debug.Log(e);
        Debug.Log("Error, on destoy lobbymanager");
    }
        
    }



    public async void ExitLobbyButton(){

        try{
        StopAllCoroutines();
        // todo: check if you are host
        if(_connectedlobby != null){
            if(_connectedlobby.HostId == _playerId){
                Debug.Log(" YOU ARE HOST, delete lobby");

                 await Lobbies.Instance.RemovePlayerAsync(_connectedlobby.Id,_connectedlobby.Players[1].Id); //KICK player 2

               await  Lobbies.Instance.DeleteLobbyAsync(_connectedlobby.Id);        //TODO    LOBBY heredable a otro player?

               


               ishost= false;
               _connectedlobby = null;

                exitlobby?.Invoke();

            }else{
                     Debug.Log(" Client exited from lobby");

                     //TODO CHECK IF LOBBY NOT FOUND???
               await Lobbies.Instance.RemovePlayerAsync(_connectedlobby.Id,_playerId);
               
                _connectedlobby = null;
                exitlobby?.Invoke();

                    

            }


        }else{

                //ishost= false;
        }


    }catch(Exception e){
        Debug.Log(e);
        Debug.Log("Error, on exiting lobby");
    }

    }


    void Update(){

        PingLobby();
        LobbyPolling();
    }


public async void ActualizarJobPlayer(int jobnumber){     
        var options = new UpdatePlayerOptions{
          Data = new Dictionary<string,PlayerDataObject>{
             {"JOB", new PlayerDataObject(PlayerDataObject.VisibilityOptions.Member,jobnumber.ToString())},
                                                         }
                                             };        
       await Lobbies.Instance.UpdatePlayerAsync(_connectedlobby.Id,_connectedlobby.Players[0].Id,options);
}





    void UpdateLobbyUIAbstract(Lobby lobby){

             listaplayerslobby.Clear();
        foreach(Transform child in contenedorplayers.transform){
            DestroyImmediate(child.gameObject);
        }

         try{  

          for(int i=0;i<lobby.Players.Count;i++){
                
                var panelplayerinstance = Instantiate(panelplayerprefab);
                panelplayerinstance.transform.SetParent(contenedorplayers.transform);

                //HARDCODED: 0 image, 1 playertext,...
                panelplayerinstance.transform.GetChild(0).GetComponent<Image>().color = UpdatePlayerColors(Int32.Parse(lobby.Players[i].Data["Skin"].Value));
                panelplayerinstance.transform.GetChild(1).GetComponent<TMP_Text>().text = lobby.Players[i].Data["PlayerName"].Value;


            }

         }catch(Exception e){
            Debug.Log(e);
            Debug.Log("error updationg lobbyUIMultiplayer");
        }

    }

    void UpdateLobbyUI(Lobby lobby){

        try{
            if(lobby.Players[0].Data["JOB"].Value =="0"){
                 playerjob1.text ="SNIPER";
            }else   if(lobby.Players[0].Data["JOB"].Value =="1"){
                 playerjob1.text ="TANK";
            }else{
                  playerjob1.text ="WARRIOR";
            }



           
          

         playername1.text = lobby.Players[0].Data["PlayerName"].Value;

        if(lobby.Players[0].Data["Skin"].Value == "0") playerimage1.color = Color.white;
        else if(lobby.Players[0].Data["Skin"].Value == "1") playerimage1.color = Color.blue;
        else if(lobby.Players[0].Data["Skin"].Value == "2") playerimage1.color = Color.red;
        else if(lobby.Players[0].Data["Skin"].Value == "3") playerimage1.color = Color.green;

         if(lobby.Players.Count>1){
         playername2.text = lobby.Players[1].Data["PlayerName"].Value;
         playerimage2.gameObject.SetActive(true);

        if(lobby.Players[1].Data["Skin"].Value == "0") playerimage2.color = Color.white;
        else if(lobby.Players[1].Data["Skin"].Value == "1") playerimage2.color = Color.blue;
        else if(lobby.Players[1].Data["Skin"].Value == "2") playerimage2.color = Color.red;
        else if(lobby.Players[1].Data["Skin"].Value == "3") playerimage2.color = Color.green;

         }else {
             playerimage2.gameObject.SetActive(false);
             playername2.text = "Waiting for player";
         }

        if( lobby.Data[MAP].Value == "0")
         maptext.text = "MAPA 1";
         else if( lobby.Data[MAP].Value == "1")
         maptext.text = "CENTRAL";
         else if( lobby.Data[MAP].Value == "2")
         maptext.text = "CRUZ";
         
        
             if(_connectedlobby.HostId != _playerId){       //SI NO SOMOS EL HOST

                    startbutton.SetActive(false);
                    mapbutton.SetActive(false);

                    butcolor1.SetActive(false);
                   butcolor2.SetActive(false);
                   butcolor3.SetActive(false);
                    butcolor21.SetActive(true);
                   butcolor22.SetActive(true);
                   butcolor23.SetActive(true);
             }else{
                    // startbutton.SetActive(false);
                    mapbutton.SetActive(true);
                    butcolor1.SetActive(true);
                   butcolor2.SetActive(true);
                   butcolor3.SetActive(true);
                     butcolor21.SetActive(false);
                   butcolor22.SetActive(false);
                   butcolor23.SetActive(false);


             }

        }catch(Exception e){
            Debug.Log(e);
            Debug.Log("error updationg lobbyUI");
        }
        // Debug.Log(_connectedlobby.Data[MAP].Value);    //this dont updates, no actualizado...
        //  Debug.Log(lobby.Data[MAP].Value);             //this updates
    }

   

    public void UpdateName(string newname){

        Debug.Log("actuaizado name " + newname);
        playername = newname;

    }

    public async void  UpdateMap(int x){
            Debug.Log(x);
            try{
            var options = new UpdateLobbyOptions{
            Data = new Dictionary<string,DataObject>{ { MAP, new DataObject(DataObject.VisibilityOptions.Public,x.ToString())}}
                };


                  Lobby lob=  await Lobbies.Instance.UpdateLobbyAsync(_connectedlobby.Id,options);

                    Debug.Log($"Updated map of connectedlobby: {lob.Data[MAP].Value}"); //NOT ACTUALIZING;



        }catch(LobbyServiceException e){
            Debug.Log(e);
            Debug.Log("error updating map");
        }

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

    Color UpdatePlayerColors(int x){

          switch(x){
            case 1:{
              return Color.green; }
             case 2:{
                 return Color.red; }
             case 3:{
                 return Color.blue; }  
            default: return Color.white;

          }
    }

    public async void UpdatePlayerColor(int x){

         

            try{
           

                                                                                                                        //DIT IT failed because public visibility?
            var options = new UpdatePlayerOptions{
            Data = new Dictionary<string,PlayerDataObject>{ {"Skin", new PlayerDataObject(PlayerDataObject.VisibilityOptions.Member,x.ToString())}}
                };



                                                                                                    //DID _playerID failed??
                  Lobby lob=  await Lobbies.Instance.UpdatePlayerAsync(_connectedlobby.Id,_connectedlobby.Players[0].Id, options);

                  
           Debug.Log(lob.Players[0].Data["Skin"].Value);


        }catch(LobbyServiceException e){
            Debug.Log(e);
            Debug.Log("error updating player");
        }

        
        switch(x){
            case 1:{
                playerimage1.color = Color.blue;
                break;
            }
             case 2:{
                playerimage1.color = Color.red;
                break;
            }
             case 3:{
                playerimage1.color = Color.green;
                break;
            }        
        }
    }

      public async void UpdatePlayerColor2(int x){

          Debug.Log(x);
          
           switch(x){
            case 1:{
                playerimage2.color = Color.blue;
                break;
            }
             case 2:{
                playerimage2.color = Color.red;
                break;
            }
             case 3:{
                playerimage2.color = Color.green;
                break;
            }        
        }
            try{
            var options = new UpdatePlayerOptions{

            Data = new Dictionary<string,PlayerDataObject>{ {"Skin", new PlayerDataObject(PlayerDataObject.VisibilityOptions.Member,x.ToString())}}
                };


                //_connectedlobby.Players[1].Id

                  Lobby lob=  await Lobbies.Instance.UpdatePlayerAsync(_connectedlobby.Id,_connectedlobby.Players[1].Id, options);

                  



        }catch(LobbyServiceException e){
            Debug.Log(e);
            Debug.Log("error updating player2");
        }

       
    }

}
