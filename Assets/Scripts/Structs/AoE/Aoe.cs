using System.Collections;
using System.Collections.Generic;
using UnityEngine;

///<summary>
///AoE发射器，创建aoe依赖的数据都在这里了
///</summary>
public class AoeLauncher{
    ///<summary>
    ///要释放的aoe
    ///</summary>
    public AoeModel model;

    ///<summary>
    ///释放的中心坐标
    ///</summary>
    public Vector3 position;

    ///<summary>
    ///释放aoe的角色的GameObject，当然可能是null的
    ///</summary>
    public GameObject caster;

    ///<summary>
    ///aoe的半径，单位：米
    ///目前这游戏的设计中，aoe只有圆形，所以只有一个半径，也不存在角度一说，如果需要可以扩展
    ///</summary>
    public float radius;

    ///<summary>
    ///aoe存在的时间，单位：秒
    ///</summary>
    public float duration;

    ///<summary>
    ///aoe的角度
    ///</summary>
    public float degree;

    ///<summary>
    ///aoe移动轨迹函数
    ///</summary>
    public AoeTween tween;
    public object[] tweenParam = new object[0];

    ///<summary>
    ///aoe的传入参数，比如可以吸收次数之类的
    ///</summary>
    public Dictionary<string, object> param = new Dictionary<string, object>();

    public AoeLauncher(
        AoeModel model, GameObject caster, Vector3 position, float radius, float duration, float degree, 
        AoeTween tween = null, object[] tweenParam = null, Dictionary<string, object> aoeParam = null
    ){
        this.model = model;
        this.caster = caster;
        this.position = position;
        this.radius = radius;
        this.duration = duration;
        this.degree = degree;
        this.tween = tween;
        if (aoeParam != null) this.param = aoeParam;
        if (tweenParam != null) this.tweenParam = tweenParam;
    }

    public AoeLauncher Clone(){
        return new AoeLauncher(
            this.model,
            this.caster,
            this.position,
            this.radius,
            this.duration,
            this.degree,
            this.tween,
            this.tweenParam,
            this.param
        );
    }
}

///<summary>
///AoE的模板数据
///</summary>
public struct AoeModel{
    public string id;

    ///<summary>
    ///aoe的视觉特效，如果是空字符串，就不会添加视觉特效
    ///这里需要的是在Prefabs/下的路径，因为任何东西都可以是视觉特效
    ///</summary>
    public string prefab;

    ///<summary>
    ///aoe是否碰撞到阻挡就摧毁了（removed），如果不是，移动就是smooth的，如果移动的话……
    ///</summary>
    public bool removeOnObstacle;

    ///<summmary>
    ///aoe的tag
    ///</summary>
    public string[] tags;

    ///<summary>
    ///aoe每一跳的时间，单位：秒
    ///如果这个时间小于等于0，或者没有onTick，则不会执行aoe的onTick事件
    ///</summary>
    public float tickTime;

    ///<summary>
    ///aoe创建时的事件
    ///</summary>
    public AoeOnCreate onCreate;

    ///<summary>
    ///aoe创建的参数
    ///</summary>
    public object[] onCreateParams;

    ///<summary>
    ///aoe每一跳的事件，如果没有，就不会发生每一跳
    ///</summary>
    public AoeOnTick onTick;
    public object[] onTickParams;

    ///<summary>
    ///aoe结束时的事件
    ///</summary>
    public AoeOnRemoved onRemoved;
    public object[] onRemovedParams;

    ///<summary>
    ///有角色进入aoe时的事件，onCreate时候位于aoe范围内的人不会触发这个，但是在onCreate里面会已经存在
    ///</summary>
    public AoeOnCharacterEnter onChaEnter;
    public object[] onChaEnterParams;

    ///<summary>
    ///有角色离开aoe结束时的事件
    ///</summary>
    public AoeOnCharacterLeave onChaLeave;
    public object[] onChaLeaveParams;

    ///<summary>
    ///有子弹进入aoe时的事件，onCreate时候位于aoe范围内的子弹不会触发这个，但是在onCreate里面会已经存在
    ///</summary>
    public AoeOnBulletEnter onBulletEnter;
    public object[] onBulletEnterParams;

    ///<summary>
    ///有子弹离开aoe时的事件
    ///</summary>
    public AoeOnBulletLeave onBulletLeave;
    public object[] onBulletLeaveParams;

    public AoeModel(
        string id, string prefab, string[] tags, float tickTime, bool removeOnObstacle,
        string onCreate, object[] onCreateParam,
        string onRemoved, object[] onRemovedParam,
        string onTick, object[] onTickParam,
        string onChaEnter, object[] onChaEnterParam,
        string onChaLeave, object[] onChaLeaveParam,
        string onBulletEnter, object[] onBulletEnterParam,
        string onBulletLeave, object[] onBulletLeaveParam
    ){
        this.id = id;
        this.prefab = prefab;
        this.tags = tags;
        this.tickTime = tickTime;
        this.removeOnObstacle = removeOnObstacle;
        this.onCreate = onCreate == "" ? null : DesignerScripts.AoE.onCreateFunc[onCreate];//DesignerScripts.AoE.onCreateFunc[onCreate];
        this.onCreateParams = onCreateParam;
        this.onRemoved = onRemoved == "" ? null : DesignerScripts.AoE.onRemovedFunc[onRemoved];
        this.onRemovedParams = onRemovedParam;
        this.onTick = onTick == "" ? null : DesignerScripts.AoE.onTickFunc[onTick];
        this.onTickParams = onTickParam;
        this.onChaEnter = onChaEnter == "" ? null : DesignerScripts.AoE.onChaEnterFunc[onChaEnter];
        this.onChaEnterParams = onChaEnterParam;
        this.onChaLeave = onChaLeave == "" ? null : DesignerScripts.AoE.onChaLeaveFunc[onChaLeave];
        this.onChaLeaveParams = onChaLeaveParam;
        this.onBulletEnter = onBulletEnter == "" ? null : DesignerScripts.AoE.onBulletEnterFunc[onBulletEnter];
        this.onBulletEnterParams = onBulletEnterParam;
        this.onBulletLeave = onBulletLeave == "" ? null : DesignerScripts.AoE.onBulletLeaveFunc[onBulletLeave];
        this.onBulletLeaveParams = onBulletLeaveParam;
    }
}

///<summary>
///aoe的移动信息
///</summary>
public class AoeMoveInfo{
    ///<summary>
    ///此时此刻的移动方式
    ///</summary>
    public MoveType moveType;

    ///<summary>
    ///此时aoe移动的力量，在这个游戏里，y坐标依然无效，如果要做手雷一跳一跳的，请使用其他的component绑定到特效的gameobject上，而非aoe的
    ///</summary>
    public Vector3 velocity;

    ///<summary>
    ///aoe的角度变成这个值
    ///</summary>
    public float rotateToDegree;

    public AoeMoveInfo(MoveType moveType, Vector3 velocity, float rotateToDegree){
        this.moveType = moveType;
        this.velocity = velocity;
        this.rotateToDegree = rotateToDegree;
    }
}

///<summary>
///aoe创建时的事件
///<param name="aoe">被创建出来的aoe的gameObject</param>
///</summary>
public delegate void AoeOnCreate(GameObject aoe);

///<summary>
///aoe移除时候的事件
///<param name="aoe">被创建出来的aoe的gameObject</param>
///</summary>
public delegate void AoeOnRemoved(GameObject aoe);

///<summary>
///aoe每一跳的事件
///<param name="aoe">被创建出来的aoe的gameObject</param>
///</summary>
public delegate void AoeOnTick(GameObject aoe);

///<summary>
///当有角色进入aoe范围的时候触发
///<param name="aoe">被创建出来的aoe的gameObject</param>
///<param name="cha">进入aoe范围的那些角色，他们现在还不在aoeState的角色列表里</param>
///</summary>
public delegate void AoeOnCharacterEnter(GameObject aoe, List<GameObject> cha);

///<summary>
///当有角色离开aoe范围的时候
///<param name="aoe">离开aoe的gameObject</param>
///<param name="cha">离开aoe范围的那些角色，他们现在已经不在aoeState的角色列表里</param>
///</summary>
public delegate void AoeOnCharacterLeave(GameObject aoe, List<GameObject> cha);

///<summary>
///当有子弹进入aoe范围的时候
///<param name="aoe">被创建出来的aoe的gameObject</param>
///<param name="bullet">离开aoe范围的那些子弹，他们现在已经不在aoeState的子弹列表里</param>
///</summary>
public delegate void AoeOnBulletEnter(GameObject aoe, List<GameObject> bullet);

///<summary>
///当有子弹离开aoe范围的时候
///<param name="aoe">离开的aoe的gameObject</param>
///<param name="bullet">离开aoe范围的那些子弹，他们现在已经不在aoeState的子弹列表里</param>
///</summary>
public delegate void AoeOnBulletLeave(GameObject aoe, List<GameObject> bullet);

///<summary>
///aoe的移动轨迹函数
///<param name="aoe">要执行的aoeObj</param>
///<param name="t">这个tween在aoe中运行了多久了，单位：秒</param>
///<return>aoe在这时候的移动信息</param>
public delegate AoeMoveInfo AoeTween(GameObject aoe, float t); 