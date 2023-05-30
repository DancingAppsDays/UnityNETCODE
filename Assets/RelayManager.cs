using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using Unity.Services;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;


using TMPro;
using Unity.Netcode.Transports.UTP;
using Unity.Networking.Transport.Relay;
using System.Threading.Tasks;

public class RelayManager : MonoBehaviour
{
    //
       public  TMP_Text joincodetext;

       string joincode;
        int maxplayers=4;
    // Start is called before the first frame update
    void Start()
    {
        //Autenticar();
    }
    async void Autenticar(){

       await UnityServices.InitializeAsync();
       await AuthenticationService.Instance.SignInAnonymouslyAsync();
       Debug.Log("Signed in " +  AuthenticationService.Instance.PlayerId);
    }


    public async Task<string> CreateGame(){


        try{
        Allocation a = await RelayService.Instance.CreateAllocationAsync(maxplayers);//,region:"US-west"); //hardcoded closest Region;
       joincode = await  RelayService.Instance.GetJoinCodeAsync(a.AllocationId);
        //joincodetext.text = joincode;

        RelayServerData relayServerData = new RelayServerData(a,"dtls");
       NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(relayServerData);

        /*
        NetworkManager.Singleton.GetComponent<UnityTransport>().SetHostRelayData(
            a.RelayServer.IpV4,
            (ushort)a.RelayServer.Port,
            a.AllocationIdBytes,
            a.Key,
            a.ConnectionData
        );*/



       
        NetworkManager.Singleton.StartHost();
        return joincode;

          }catch(RelayServiceException e){
                Debug.Log(e);
                return null;
            }
    }


        public void actualizarjoincode(string cadena){

            joincode = cadena;//joincodetext.text;
        }

    public async void JoinGame(string JoinCode){

            Debug.Log("La joincode es" + JoinCode);
            try{
          
        JoinAllocation a = await RelayService.Instance.JoinAllocationAsync(JoinCode);
      
        RelayServerData relayServerData = new RelayServerData(a,"dtls");
       NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(relayServerData);

        /*
        NetworkManager.Singleton.GetComponent<UnityTransport>().SetClientRelayData(
            a.RelayServer.IpV4,
            (ushort)a.RelayServer.Port,
            a.AllocationIdBytes,
            a.Key,
            a.ConnectionData,
            a.HostConnectionData
        );*/



       // Debug.Log()
        NetworkManager.Singleton.StartClient();

            }catch(RelayServiceException e){
                Debug.Log(e);
            }
    }


}
