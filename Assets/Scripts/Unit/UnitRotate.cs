using System.Collections;
using System.Collections.Generic;
using UnityEngine;


///<summary>
///单位旋转控件，如果一个单位要通过游戏逻辑来进行旋转，就应该用它，不论是角色还是aoe还是bullet什么的
///</summary>
public class UnitRotate : MonoBehaviour
{
    ///<summary>
    ///单位当前是否可以旋转角度
    ///</summary>
    private bool canRotate = true;

    ///<summary>
    ///旋转的速度，1秒只能转这么多度（角度）
    ///每帧转动的角度上限是这个数字 * Time.fixedDeltaTime得来的。
    ///</summary>
    ///[Tooltip("旋转的速度，1秒只能转这么多度（角度），每帧转动的角度上限是这个数字*Time.fixedDeltaTime得来的。")]
    public float rotateSpeed;

    private float targetDegree = 0.00f;  //目标转到多少度，因为旋转发生在围绕y轴旋转，所以只有y就足够了

    void FixedUpdate() {
        if (this.canRotate == false || DoneRotate() == true) return;

        float sDeg = transform.rotation.eulerAngles.y;
        if (sDeg > 180.00f) sDeg -= 360.00f;
        float degDis = targetDegree - sDeg;
        float nagDis = targetDegree > sDeg ? (targetDegree - 360.00f - sDeg) : (targetDegree + 360.00f - sDeg);
        bool nagDegree = Mathf.Abs(degDis) < Mathf.Abs(nagDis) ? (degDis < 0) : (nagDis < 0);   //方向依据更短距离的那个来决定
        float rotSpeed = Mathf.Min(rotateSpeed * Time.fixedDeltaTime, Mathf.Abs(degDis), Mathf.Abs(nagDis));  //选择其中最短的一个，作为一个移动角度
        if (nagDegree) rotSpeed *= -1;
        
        transform.Rotate(new Vector3(0, rotSpeed, 0));
    }

    //判断是否完成了旋转
    private bool DoneRotate(){
        float rotSpeed = this.rotateSpeed * Time.fixedDeltaTime;
        return Mathf.Abs(transform.rotation.eulerAngles.y - targetDegree) < Mathf.Min(0.01f, rotSpeed); //允许一定的误差也当是达成了。
    }

    ///<summary>
    ///旋转到指定角度
    ///<param name="degree">需要旋转到的角度</param>
    ///</summary>
    public void RotateTo(float degree){
        targetDegree = degree;
    }

    ///<summary>
    ///指定两个点，旋转到对应角度
    ///<param name="x">目标点x-起点x</param>
    ///<param name="z">目标点z-起点z</param>
    ///</summary>
    public void RotateTo(float x, float z){
        targetDegree = Mathf.Atan2(x, z) * 180.00f / Mathf.PI;   
    }

    ///<summary>
    ///旋转指定角度
    ///<param name="degree">需要旋转到的角度</param>
    ///</summary>
    public void RotateBy(float degree){
        targetDegree = transform.rotation.eulerAngles.y + degree;
    }

    ///<summary>
    ///禁止单位可以旋转的能力，这会终止当前正在进行的旋转
    ///终止当前的旋转看起来是一个side-effect，但是依照游戏规则设计来说，他只是“配套功能”所以严格的说并不是side-effect
    ///</summary>
    public void DisableRotate(){
        canRotate = false;
        targetDegree = transform.rotation.eulerAngles.y;
    }

    ///<summary>
    ///开启单位可以旋转的能力
    ///</summary>
    public void EnableRotate(){
        canRotate = true;
    }
}
