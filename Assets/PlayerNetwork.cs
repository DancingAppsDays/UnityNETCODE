using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class PlayerNetwork : NetworkBehaviour
{
    public float playerspeed =33;

    [SerializeField] private Transform rockprefab;
    // Start is called before the first frame update
    void Start()
    {
        
    }


    // Update is called once per frame
    void Update()
    {   

        if(!IsOwner) return;

      

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

            transform.position += mover;//*playerspeed*Time.deltaTime;


              if(Input.GetKey(KeyCode.T)){
                 Transform rockinstance =  Instantiate(rockprefab);
                 rockinstance.GetComponent<NetworkObject>().Spawn(true);
            
        }
    }



    
    public  override void OnNetworkSpawn()
    {
       // Debug.Log("Onnetworkspawn");
        StartCoroutine(sendmeteors());
    }
  
     IEnumerator sendmeteors(){

        while(true){
            Vector3 pos = new Vector3(UnityEngine.Random.Range(0,10),10,(UnityEngine.Random.Range(0,10)));
          Transform rockinstance =  Instantiate(rockprefab,pos,Quaternion.identity);
                 
                 rockinstance.GetComponent<NetworkObject>().Spawn(true);
                Destroy(rockinstance.gameObject,3);

        yield return new WaitForSeconds(2);
        }

    }
}
