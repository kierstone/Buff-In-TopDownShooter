using System.Collections;
using System.Collections.Generic;
using UnityEngine;

///<summary>
///角色的数值属性部分，比如最大hp、攻击力等等都在这里
///这个建一个结构是因为并非只有角色有这些属性，包括装备、buff、aoe、damageInfo等都会用上
///</summary>
public struct ChaProperty{
    ///<summary>
    ///最大生命，基本都得有，哪怕角色只有1，装备可以是0
    ///</summary>
    public int hp;

    ///<summary>
    ///攻击力
    ///</summary>
    public int attack;

    ///<summary>
    ///移动速度，他不是米/秒作为单位的，而是一个可以培养的数值。
    ///具体转化为米/秒，是需要一个规则的，所以是策划脚本 int SpeedToMoveSpeed(int speed)来返回
    ///</summary>
    public int moveSpeed;

    ///<summary>
    ///行动速度，和移动速度不同，他是增加角色行动速度，也就是变化timeline和动画播放的scale的，比如wow里面开嗜血就是加行动速度
    ///具体多少也不是一个0.2f（我这个游戏中规则设定的最快为正常速度的20%，你的游戏你自己定）到5.0f（我这个游戏设定了最慢是正常速度20%），和移动速度一样需要脚本接口返回策划公式
    ///</summary>
    public int actionSpeed;

    ///<summary>
    ///弹仓，其实相当于mp了，只是我是射击游戏所以题材需要换皮。
    ///玩家层面理解，跟普通mp上限的区别是角色这个值上限一般都是0，它来自于装备。
    ///</summary>
    public int ammo;

    ///<summary>
    ///体型圆形半径，用于移动碰撞的，单位：米
    ///这个属性因人而异，但是其实在玩法中几乎不可能经营它，只有buff可能会改变一下，所以直接用游戏中用的数据就行了，不需要转化了
    ///</summary>
    public float bodyRadius;

    ///<summary>
    ///挨打圆形半径，同体型圆形，只是用途不同，用在判断子弹是否命中的时候
    ///</summary>
    public float hitRadius;

    ///<summary>
    ///角色移动类型
    ///</summary>
    public MoveType moveType;

    public ChaProperty(
        int moveSpeed, int hp = 0, int ammo = 0, int attack = 0, int actionSpeed = 100, 
        float bodyRadius = 0.25f, float hitRadius = 0.25f, MoveType moveType = MoveType.ground
    ){
        this.moveSpeed = moveSpeed;
        this.hp = hp;
        this.ammo = ammo;
        this.attack = attack;
        this.actionSpeed = actionSpeed;
        this.bodyRadius = bodyRadius;
        this.hitRadius = hitRadius;
        this.moveType = moveType;
    }

    
    public static ChaProperty zero = new ChaProperty(0,0,0,0,0,0,0,0);

    ///<summary>
    ///将所有值清0
    ///<param name="moveType">移动类型设置为</param>
    ///</summary>
    public void Zero(MoveType moveType = MoveType.ground){
        this.hp = 0;
        this.moveSpeed = 0;
        this.ammo = 0;
        this.attack = 0;
        this.actionSpeed = 0;
        this.bodyRadius = 0;
        this.hitRadius = 0;
        this.moveType = moveType;
    }

    //定义加法和乘法的用法，其实这个应该走脚本函数返回，抛给脚本函数多个ChaProperty，由脚本函数运作他们的运算关系，并返回结果
    public static ChaProperty operator +(ChaProperty a, ChaProperty b){
        return new ChaProperty(
            a.moveSpeed + b.moveSpeed,
            a.hp + b.hp,
            a.ammo + b.ammo,
            a.attack + b.attack,
            a.actionSpeed + b.actionSpeed,
            a.bodyRadius + b.bodyRadius,
            a.hitRadius + b.hitRadius,
            a.moveType == MoveType.fly || b.moveType == MoveType.fly ? MoveType.fly : MoveType.ground
        );
    }
    public static ChaProperty operator *(ChaProperty a, ChaProperty b){
        return new ChaProperty(
            Mathf.RoundToInt(a.moveSpeed * (1.0000f + Mathf.Max(b.moveSpeed, -0.9999f))),
            Mathf.RoundToInt(a.hp * (1.0000f + Mathf.Max(b.hp, -0.9999f))),
            Mathf.RoundToInt(a.ammo * (1.0000f + Mathf.Max(b.ammo, -0.9999f))),
            Mathf.RoundToInt(a.attack * (1.0000f + Mathf.Max(b.attack, -0.9999f))),
            Mathf.RoundToInt(a.actionSpeed * (1.0000f + Mathf.Max(b.actionSpeed, -0.9999f))),
            a.bodyRadius * (1.0000f + Mathf.Max(b.bodyRadius, -0.9999f)),
            a.hitRadius * (1.0000f + Mathf.Max(b.hitRadius, -0.9999f)),
            a.moveType == MoveType.fly || b.moveType == MoveType.fly ? MoveType.fly : MoveType.ground
        );
    }
    public static ChaProperty operator *(ChaProperty a, float b){
        return new ChaProperty(
            Mathf.RoundToInt(a.moveSpeed * b),
            Mathf.RoundToInt(a.hp * b),
            Mathf.RoundToInt(a.ammo * b),
            Mathf.RoundToInt(a.attack * b),
            Mathf.RoundToInt(a.actionSpeed * b),
            a.bodyRadius * b,
            a.hitRadius * b,
            a.moveType
        );
    }
}

///<summary>
///角色的资源类属性，比如hp，mp等都属于这个
///</summary>
public class ChaResource{
    ///<summary>
    ///当前生命
    ///</summary>
    public int hp;

    ///<summary>
    ///当前弹药量，在这游戏里就相当于mp了
    ///</summary>
    public int ammo;

    ///<summary>
    ///当前耐力，耐力是一个百分比消耗，实时恢复的概念，所以上限按规则就是100了，这里是现有多少
    ///</summary>
    public int stamina;

    public ChaResource(int hp, int ammo = 0, int stamina = 0){
        this.hp = hp;
        this.ammo = ammo;
        this.stamina = stamina;
    }

    ///<summary>
    ///是否足够
    ///</summary>
    public bool Enough(ChaResource requirement){
        return (
            this.hp >= requirement.hp &&
            this.ammo >= requirement.ammo &&
            this.stamina >= requirement.stamina
        );
    }

    public static ChaResource operator +(ChaResource a, ChaResource b){
        return new ChaResource(
            a.hp + b.hp,
            a.ammo + b.ammo,
            a.stamina + b.stamina
        );
    }
    public static ChaResource operator *(ChaResource a, float b){
        return new ChaResource(
            Mathf.FloorToInt(a.hp * b),
            Mathf.FloorToInt(a.ammo * b),
            Mathf.FloorToInt(a.stamina * b)
        );
    }
    public static ChaResource operator *(float a, ChaResource b){
        return new ChaResource(
            Mathf.FloorToInt(b.hp * a),
            Mathf.FloorToInt(b.ammo * a),
            Mathf.FloorToInt(b.stamina * a)
        );
    }
    public static ChaResource operator -(ChaResource a, ChaResource b){
        return a + b * (-1);
    }

    public static ChaResource Null = new ChaResource(0);
}