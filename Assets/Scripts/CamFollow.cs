using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CamFollow : MonoBehaviour
{
    //镜头跟随的偏移
    private Vector3 offset;

    //镜头要跟随的角色，没有角色镜头就停止跟随了
    private GameObject followCharacter;
    
    void LateUpdate()
    {
        if (!this.followCharacter) return;
        this.transform.position = this.offset + this.followCharacter.transform.position;
    }

    public void SetFollowCharacter(GameObject cha){
        followCharacter = cha;
        this.offset = new Vector3(
            transform.position.x - cha.transform.position.x,
            -transform.position.z / Mathf.Cos(transform.rotation.eulerAngles.x * Mathf.PI / 180) - cha.transform.position.y,
            transform.position.z - cha.transform.position.z
        );
    }
}
