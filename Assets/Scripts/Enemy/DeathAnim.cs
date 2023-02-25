using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
public class DeathAnim : MonoBehaviour
{
    // Start is called before the first frame update
    public int DestroyTime;
    void Start()
    {
        Invoke("DestroyObject", DestroyTime);
    }

    [PunRPC]
    void DestroyObject()
    {
        Destroy(gameObject);
    }
}
