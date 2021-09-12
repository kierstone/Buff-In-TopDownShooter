using System.Collections;
using System.Collections.Generic;
using UnityEngine;

///<summary>
///挂上这个的特效就会像球一样滚动，但这里有个特殊处理，就是他最深只关心自己所在父级的坐标变化，其他的一律就不管了
///</summary>
public class BallRolling : MonoBehaviour{
    private Vector3 pWasPos = new Vector3();

    private Renderer render;
    private void Start() {
        pWasPos = this.transform.parent.position;
        render =  this.gameObject.GetComponent<Renderer>();
        if (!render) render = this.gameObject.GetComponentInChildren<Renderer>();
    }
    
    private void Update() {
        if (!render) return;
        Vector3 mDis = this.transform.parent.position - pWasPos;
        float r = render.bounds.size.x / 2;  //既然是球，那么x,y,z应该都一样才对，都说了是“特殊处理”嘛
        float degreeX = mDis.x * 180.00f / (Mathf.PI * r) - this.transform.parent.eulerAngles.x; 
        float degreeZ = mDis.z * 180.00f / (Mathf.PI * r) - this.transform.parent.eulerAngles.z;
        transform.RotateAround(transform.position,Vector3.right,degreeX);
        transform.RotateAround(transform.position,Vector3.back,degreeZ);
        pWasPos = this.transform.parent.position;
    }
}