using System.Collections;
using System.Collections.Generic;
using UnityEngine;


///<summary>
///单个动画的信息
///</summary>
public class AnimInfo{
    ///<summary>
    ///这个动画的key，相当于一个id
    ///</summary>
    public string key;

    ///<summary>
    ///动画优先级，因为本质是个回合制游戏，所以播放动作有优先级高的优先播放一说。
    ///比如受伤通常就会低于攻击，因为角色很可能同时受伤和发动攻击，但是发动攻击并不会因为受伤终止，比如wow里面。
    ///</summary>
    public int priority;

    ///<summary>
    ///这个动画有哪些可能性。
    ///最常见的比如受伤动画可能有3种，但其实出哪一种没有逻辑讲究，就说明这个值有3条。
    ///但如果受伤动画是根据受到伤害数量来定的，说明有逻辑，应该有3条SingleAnimInfo，他们的key都只有1个受伤动作。
    ///key（SingleAnimInfo）是animator里面的动画信息
    ///value（uint）是权重
    ///</summary>
    public KeyValuePair<SingleAnimInfo, int>[] animations;


    public static AnimInfo Null = new AnimInfo("", null, 0);

    public AnimInfo(string key, KeyValuePair<SingleAnimInfo, int>[] animations, int priority = 0){
        this.animations = animations;
        this.priority = priority;
        this.key = key;
    }


    ///<summary>
    ///随机获得一个动画信息（animator里的动画名字等）
    ///<return>动画信息</return>
    ///</summary>
    public SingleAnimInfo RandomKey(){
        if (animations.Length <= 0) return SingleAnimInfo.Null;

        if (animations.Length == 1) return animations[0].Key;
        
        int totalV = 0;
        for (int i = 0; i < animations.Length; i++){
            totalV += animations[i].Value;
        }
        if (totalV <= 0) return SingleAnimInfo.Null;


        int rv = Random.Range(0, totalV);
        int rIndex = 0;
        while (rv > 0){
            rv -= animations[rIndex].Value;
            rIndex += 1;
        }
        rIndex = Mathf.Min(rIndex, animations.Length - 1);
        return animations[rIndex].Key;
    }

    ///<summary>
    ///是否存在一个动画的名字动画名字（animator里的动画名字）
    ///<param name="animName">animator中动画的名字</param>
    ///<return>如果是true就存在</return>
    ///</summary>
    public bool ContainsAnim(string animName){
        for (int i = 0; i < animations.Length; i++){
            if (animations[i].Key.animName == animName) return true;
        }
        return false;
    }
}

///<summary>
///单个动画信息，主要是在animator中的name，以及多久以后回到可以被改写的程度
///</summary>
public struct SingleAnimInfo{
    ///<summary>
    ///animator中的名称
    ///</summary>
    public string animName;

    ///<summary>
    ///在多久之后权重清0，单位秒
    ///</summary>
    public float duration;

    public SingleAnimInfo(string animName, float duration = 0){
        this.animName = animName;
        this.duration = duration;
    }

    public static SingleAnimInfo Null = new SingleAnimInfo("", 0);

}