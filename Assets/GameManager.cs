using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

public class GameManager : NetworkBehaviour
{
    public TMP_Text scoretext, scoretext2;

    public Transform spawn1, spawn2;
    public static GameManager instancia;
    public NetworkVariable<uint> score1 = new NetworkVariable<uint>();//writePerm: NetworkVariableWritePermission.Owner);
        public NetworkVariable<uint> score2 = new NetworkVariable<uint>();//writePerm: NetworkVariableWritePermission.Owner);

    

    //public List<NetworkVariable<uint>> listscores = List<NetworkVariable<uint>>();>u
    public NetworkList<uint> listscore ;// EditorPrefsgetinit not allow from monobeahivour constructor = new NetworkList<uint>(){};



    //public List<PlayerMovement> listplayer = new List<PlayerMovement>();
    public List<ulong> listplayer = new List<ulong>();


    public GameObject obstacles1,obstacles2,obstacles3;

    // Start is called before the first frame update
    private void Awake() {
      
         instancia =this;
        

        //error: MUST INIT ON NETWORKSPAwn probably
       listscore =  new NetworkList<uint>(){};
       

    }

    public int addplayertolist(ulong player){

        listplayer.Add(player);
//        listscore.Add((uint)player);


        float residuo = listplayer.Count % 2;
        //Debug.Log("residuo " + residuo);

        if(residuo ==1 ){
            return 1;
        }else {
            return 2;
        }
    }

    public void removeplayertolist(ulong player){

        listplayer.Remove(player);
    }

    public override void OnNetworkSpawn(){

        if(IsServer){
             score1.Value =0;//k_InitialValue;
             score2.Value =0;
            //Destroy(gameObject);
        }else{
                if(score1.Value !=0 || score2.Value !=0){
                    Debug.Log("score is not sync");
                }else{
                    Debug.Log("scored is initiazlied in 0");
                }
            //instancia = this;
        }
            
        //listscore =  new NetworkList<uint>(){};
        //Actualizarscore();
    }

    public void Disparado(ulong owner){

        //int x = (int)OwnerClientId;
        Debug.Log("disparado");

        if(IsOwner)                     //only HOST is sending?
        FuedisparadoServerRpc(owner);
    }


    [ServerRpc]//(RequireOwnership =false)]
    void FuedisparadoServerRpc(ulong owner){//},ServerRpcParams serverRpcParams = default){

        //Debug.Log("Fue disparado " + owner);//, serverRpcParams.Receive.SenderClientId);

        if(owner ==2){
            score1.Value++;
           // listscore[0] = listscore[0].

        }else if( owner ==1){
            score2.Value++;
        }

        //Actualizarscore();
        NotificacionScoreClientRpc();
    }

    public void Fuedisparado(ulong owner){//},ServerRpcParams serverRpcParams = default){

        //Debug.Log("Fue disparado " + owner);//, serverRpcParams.Receive.SenderClientId);

        if(owner ==0){
            score1.Value++;
           // listscore[0] = listscore[0].

        }else if( owner ==1){
            score2.Value++;
        }

        //Actualizarscore();
        NotificacionScoreClientRpc();
    }



















    public void Gol1(){
         GolEquipo1ServerRpc();
    }

     public void Gol2(){
         GolEquipo2ServerRpc();
    }
     [ServerRpc]
    public void GolEquipo1ServerRpc(){
        Debug.Log("Equipo 1 Anotó gol");
            score1.Value ++;
           

        NotificacionScoreClientRpc();
    }

    [ServerRpc]
    public void GolEquipo2ServerRpc(){
        Debug.Log("Equipo 2 Anotó gol");
            score2.Value ++;

         NotificacionScoreClientRpc();
    }

    [ClientRpc]
    public void NotificacionScoreClientRpc(){

        Actualizarscore();
       
    }

    void Actualizarscore(){
         Debug.Log("Actualizado");
        scoretext.text =   score1.Value.ToString(); 
        scoretext2.text =   score2.Value.ToString(); 
    }


    public void selectmap(int x){

        Debug.Log("SELECT MAP" + x);
        switch(x){

            case 0:{

                    obstacles1.SetActive(true);
                    obstacles2.SetActive(false);
                    obstacles3.SetActive(false);
                break;
            }
             case 1:{

                    obstacles2.SetActive(true);
                    obstacles1.SetActive(false);
                    obstacles3.SetActive(false);
                break;
            }
             case 2:{

                    obstacles3.SetActive(true);
                    obstacles2.SetActive(false);
                    obstacles1.SetActive(false);
                break;
            }
            default:{
                     obstacles1.SetActive(true);
                    obstacles2.SetActive(false);
                    obstacles3.SetActive(false);
                break;


            }


        }

    }


    public void RESETgame(){
        //NetworkManager.Singleton.DisconnectClient(OwnerClientId);
        //List<ulong> clients = (List<ulong>)NetworkManager.Singleton.ConnectedClientsIds;// .DisconnectClient(OwnerClientId);
        NetworkManager.Singleton.Shutdown();

        StartCoroutine(shutingdown());//NetworkManager.Singleton.ShutdownInProgress
        /*
        foreach(ulong client in clients){


            NetworkManager.Singleton.DisconnectClient(client);/// .DisconnectClient 
            Debug.Log($"Client: {client} disconnected");
        }
        //SceneManager.LoadSceneAsync("gunfoot");
        NetworkManager.SceneManager.LoadScene("gunfoor",LoadSceneMode.Single);*/
    }
    IEnumerator shutingdown(){

        while(NetworkManager.Singleton.ShutdownInProgress){

            yield return null;
        }
          //NetworkManager.SceneManager.LoadScene("gunfoor",LoadSceneMode.Single);
            SceneManager.LoadScene("gunfoot");
    }
}
