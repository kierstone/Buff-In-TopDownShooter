using System.Collections;
using System.Collections.Generic;
using UnityEngine;

///<summary>
///挂上这个的特效会绕y轴不断旋转
///</summary>
public class RotateY : MonoBehaviour{
    [Tooltip("每秒转多少度（角度）")]
    public float rotatePerSec = 360;

    private float cDeg = 0; //当前应该多少度

    private void Update() {
        cDeg = (cDeg + rotatePerSec * Time.deltaTime) % 360;
        float shouldRotate = cDeg - transform.eulerAngles.y;
        Transform t = this.transform;
        while(t.parent != null){
            shouldRotate -= t.parent.eulerAngles.y;
            t = t.parent;
        }
        this.transform.RotateAround(this.transform.position, Vector3.up, shouldRotate);
    } 
}