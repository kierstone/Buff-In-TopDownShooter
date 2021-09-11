using System.Collections;
using System.Collections.Generic;
using UnityEngine;

///<summary>
///预约了多少时间内【匀速直线】移动往某个方向多远
///</summary>
public class MovePreorder{
    ///<summary>
    ///想要移动的方向和距离
    ///</summary>
    public Vector3 velocity;

    ///<summary>
    ///多久完成，单位秒
    ///</summary>
    private float inTime;

    ///<summary>
    ///还有多久移动完成，单位：秒，如果小于1帧的时间但还大于0，就会当做1帧来执行
    ///</summary>
    public float duration;
    public MovePreorder(Vector3 velocity, float duration){
        this.velocity = velocity;
        this.duration = duration;
        this.inTime = duration;
    }

    ///<summary>
    ///运行了一段时间，返回这段时间内的移动力
    ///<param name="time">运行的时间，单位：秒</param>
    ///<return>移动力</return>
    public Vector3 VeloInTime(float time){
        if (time >= duration){
            this.duration = 0;
        }else{
            this.duration -= time;
        }
        return inTime <= 0 ? velocity : (velocity / inTime);
    }
}

public enum MoveType{
    ground = 0,
    fly = 1
}