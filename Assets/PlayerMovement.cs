using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class PlayerMovement : NetworkBehaviour 
{
        public float playerspeed =100;
        public float rotationlerpspeed =.2f;
       private new  Rigidbody rigidbody;

       public Transform ballprefab;
       public float Bounceforce =777;
       public float bulletspeed =1100;


        public int teamnumber=0;


        float firecooldowntimer = 2f;
        bool canfire= true;

        public GameObject cooldownclock;


        public GameObject bullet;
    public float bulletradio=2;

    public List<PlayerMovement> listaplayers = new List<PlayerMovement>();

    public override void OnNetworkSpawn()
    {
          //if(!IsOwner) Destroy(this); //causa BUG en serverRPC!!!  //remueve el  componente de los otros jugadores

       teamnumber = GameManager.instancia.addplayertolist(OwnerClientId);

       if(teamnumber==1) transform.position = GameManager.instancia.spawn1.position + Vector3.up;
       else transform.position = GameManager.instancia.spawn2.position + Vector3.up;
       //Debug.Log("OnNetworkSpawn");
       //if(!IsOwner) Destroy

        listaplayers.Add(this);
    }

    public void OnDestroy() {
        GameManager.instancia.removeplayertolist(OwnerClientId);
    }

    //Start inicia despues de spawn
    void Start()
    {
            if(IsServer){
                 Debug.Log("Este es el server");
            }

        rigidbody = this.gameObject.GetComponent<Rigidbody>();
        Debug.Log("ONSTART");
    }

    void Update()
    {

                if(IsOwner){

         Vector3 mover = Vector3.zero;

            if(Input.GetKey(KeyCode.A)){
                mover.x =-1;
            }
             if(Input.GetKey(KeyCode.D)){
                mover.x =1;
            }
             if(Input.GetKey(KeyCode.W)){
                mover.z =1;
            }
             if(Input.GetKey(KeyCode.S)){
                mover.z =-1;
            }

            if(!canfire){
         firecooldowntimer -= Time.deltaTime;
            if(firecooldowntimer <=0){
                  firecooldowntimer = 2;
                  canfire = true;
                   cooldownclock.SetActive(false);
            }
            }
              




             if(Input.GetKeyDown(KeyCode.Y)){       //shoot
               
                    if(IsOwner && canfire){

                            


                         var ballinstance=  Instantiate(ballprefab);
                        ballinstance.transform.position = transform.position + (transform.forward*3);

                          //ballinstance.GetComponent<NetworkObject>().Spawn();

                          ballinstance.GetComponent<Rigidbody>().AddForce(transform.forward* bulletspeed);

                        canfire= false;
                        cooldownclock.SetActive(true);




                        DisparaPlayer(transform.position, transform.forward);



                        }
           
            }

            //transform.position += mover*playerspeed*Time.deltaTime;
            rigidbody.AddForce(10*mover*playerspeed*Time.deltaTime);

             if(mover != Vector3.zero){
      
                 Quaternion rota = Quaternion.LookRotation( mover, Vector3.up);
                  transform.rotation = Quaternion.RotateTowards(transform.rotation, rota,rotationlerpspeed);
            }


        if(Input.GetKeyDown(KeyCode.E)){

            Debug.Log("dispara");

           
            
                    RaycastHit hit;
           if(Physics.SphereCast(transform.position,bulletradio, transform.forward,out hit,110)){

                Debug.DrawRay(transform.position, transform.forward*20,Color.red,2);

                Debug.Log(hit.collider.gameObject);
                //for(int i=0;i<hit.point.)
               if( hit.collider.gameObject.tag =="Player"){
                Debug.Log("DIOO a otro player");
                    NotificarServerdegolpeServerRPC(hit.collider.gameObject.GetComponent<PlayerMovement>().OwnerClientId,hit.collider.gameObject);
                    
               }

           }

            var newbullet = Instantiate(bullet,transform.position+(transform.forward*3.5f),transform.rotation);
           newbullet.transform.GetComponent<Rigidbody>().AddForce(transform.forward*4444);//,ForceMode.Force);
             newbullet.transform.localScale*=2;
             newbullet.transform.Rotate(Vector3.right,90);
              Destroy(newbullet,2);

             NotificarServerDisparoServerRPC(transform.position+(transform.forward*3.5f),transform.forward);


            }

        }//si es owner
        }




        [ServerRpc]
        void NotificarServerDisparoServerRPC(Vector3 pos, Vector3 forward){

             DisparaPlayerClientRPC(pos, forward);

        }
    
    
             [ServerRpc]
    void   NotificarServerdegolpeServerRPC(ulong victimid,NetworkObjectReference playerhitted, ServerRpcParams serverRpcParams=default){


        Debug.Log("TO SERVER: alguien fue disparado  :" + victimid);

        GameManager.instancia.Fuedisparado(victimid);

        //victimid      
        NotificarClientdeGolpeClientRPC(victimid,playerhitted);

        //SOlo lo ve el server....
       // NetworkManager.Singleton.ConnectedClients[victimid].PlayerObject.transform.GetChild(2).GetComponent<ParticleSystem>().Play();
      
        //MOSTRAR en vas el hp del contrario o sumarse un punto
    }

      [ClientRpc]
    void NotificarClientdeGolpeClientRPC(ulong victimid,NetworkObjectReference playerhitted)
    {   
        //SOlo funciona en server
        // NetworkManager.Singleton.ConnectedClients[victimid].PlayerObject.transform.GetChild(2).GetComponent<ParticleSystem>().Play();
       
        ((GameObject)playerhitted).transform.GetChild(2).GetComponent<ParticleSystem>().Play();

       //TOdo get reference atraves de una lista
    }


    [ClientRpc]
    public void DisparaPlayerClientRPC(Vector3 pos, Vector3 forward){
           
           Debug.Log("DisparaplayerClinetRPC");
          if(!IsOwner){
             Debug.Log("DisparaplayerClinetRPC NOT owner");
                  var newbullet = Instantiate(bullet,pos,Quaternion.LookRotation(forward));
             newbullet.transform.GetComponent<Rigidbody>().AddForce(forward*4444);//was transform.forward...
            newbullet.transform.localScale*=2;
             newbullet.transform.Rotate(Vector3.right,90);  
             //newbullet.transform.Rotate(Vector3.right,90);
          }
        }










/////
        public void DisparaPlayer(Vector3 pos, Vector3 forward){
           
            DisparaClienteServerRpc(pos,forward);
             Debug.Log("dispara player");
        }

    [ServerRpc]//(RequireOwnership=false)]
        void DisparaClienteServerRpc(Vector3 pos, Vector3 forward,ServerRpcParams serverRpcParams=default){

            Debug.Log("Server notificado alguien dispara : "+serverRpcParams.Receive.SenderClientId);
            DisparadesdeClientRpc(pos,forward);

        }

    [ClientRpc]
    void  DisparadesdeClientRpc(Vector3 pos, Vector3 forward,ClientRpcParams clientRpcParams= default){

        Debug.Log("Server notifica que alguien dispara : "+ clientRpcParams.Send.TargetClientIds);
        if(!IsOwner){

             Debug.Log("No es el owner");
       
          var ballinstance=  Instantiate(ballprefab);
                        ballinstance.transform.position = pos + (forward*3);

                          //ballinstance.GetComponent<NetworkObject>().Spawn();

                          ballinstance.GetComponent<Rigidbody>().AddForce(forward* bulletspeed);
                  
        }

     }



    private void OnCollisionEnter(Collision other) {

     if(other.gameObject.tag =="ball"){

        Destroy(other.gameObject);

        GameManager.instancia.Disparado((ulong)teamnumber);// OwnerClientId);

        

     }

    }








        //patear pelota
     private void OnTriggerEnter(Collider other) {

     if(other.gameObject.tag =="ballnousado"){

        other.gameObject.GetComponent<NetworkObject>().ChangeOwnership(OwnerClientId);

        
         Vector3 direction = transform.position  -  other.ClosestPoint(transform.position);// .contacts[0].point;
         direction.Normalize();
         rigidbody.AddForce(direction*(Bounceforce/10));

         direction = new Vector3(direction.x,direction.y/5,direction.z);
         other.gameObject.GetComponent<Rigidbody>().AddForce(-direction *Bounceforce);
            
     }

    }
}
