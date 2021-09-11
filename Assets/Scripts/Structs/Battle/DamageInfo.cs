using System.Collections;
using System.Collections.Generic;
using UnityEngine;

///<summary>
///游戏中任何一次伤害、治疗等逻辑，都会产生一条damageInfo，由此开始正常的伤害流程，而不是直接改写hp
///值得一提的是，在类似“攻击时产生额外一次伤害”这种效果中，额外一次伤害也应该是一个damageInfo。
///</summary>
public class DamageInfo{
    ///<summary>
    ///造成伤害的攻击者，当然可以是null的
    ///</summary>
    public GameObject attacker;

    ///<summary>
    ///造成攻击伤害的受击者，这个必须有
    ///</summary>
    public GameObject defender;

    ///<summary>
    ///这次伤害的类型Tag，这个会被用于buff相关的逻辑，是一个极其重要的信息
    ///这里是策划根据游戏设计来定义的，比如游戏中可能存在"frozen" "fire"之类的伤害类型，还会存在"directDamage" "period" "reflect"之类的类型伤害
    ///根据这些伤害类型，逻辑处理可能会有所不同，典型的比如"reflect"，来自反伤的，那本身一个buff的作用就是受到伤害的时候反弹伤害，如果双方都有这个buff
    ///并且这个buff没有判断damageInfo.tags里面有reflect，则可能造成“短路”，最终有一下有一方就秒了。
    ///</summary>
    public DamageInfoTag[] tags;

    ///<summary>
    ///伤害值，其实伤害值是多元的，通常游戏都会有多个属性伤害，所以会用一个struct，否则就会是一个int
    ///尽管起名叫Damage，但实际上治疗也是这个，只是负数叫做治疗量，这个起名看似不严谨，对于游戏（这个特殊的业务）而言却又是严谨的
    ///</summary>
    public Damage damage;

    ///<summary>
    ///是否暴击，这是游戏设计了有暴击的可能性存在。
    ///这里记录一个总暴击率，随着buff的不断改写，最后这个暴击率会得到一个0-1的值，代表0%-100%。
    ///最终处理的时候，会根据这个值来进行抉择，可以理解为，当这个值超过1的时候，buff就可以认为这次攻击暴击了。
    ///</summary>
    public float criticalRate;

    ///<summary>
    ///是否命中，是否命中与是否暴击并不直接相关，都是单独的算法
    ///作为一个射击游戏，子弹命中敌人是一种技巧，所以在这里设计命中了还会miss是愚蠢的
    ///因此这里的hitRate始终是2，就是必定命中的，之所以把这个属性放着，也是为了说明问题，而不是这个属性真的对这个游戏有用。
    ///要不要这个属性还是取决于游戏设计，比如当前游戏，本不该有这个属性。
    ///</summary>
    public float hitRate = 2.00f;

    public DamageInfo(GameObject attacker, GameObject defender, Damage damage, float baseCriticalRate, DamageInfoTag[] tags){
        this.attacker = attacker;
        this.defender = defender;
        this.damage = damage;
        this.criticalRate = baseCriticalRate;
        this.tags = new DamageInfoTag[tags.Length];
        for (int i = 0; i < tags.Length; i++){
            this.tags[i] = tags[i];
        }
    }

    ///<summary>
    ///从策划脚本获得最终的伤害值
    ///</summary>
    public int DamageValue(){
        return DesignerScripts.CommonScripts.DamageValue(this);
    }

    
}

///<summary>
///游戏中伤害值的struct，这游戏的伤害类型包括子弹伤害（治疗）、爆破伤害（治疗）、精神伤害（治疗）3种，这两种的概念更像是类似物理伤害、金木水火土属性伤害等等这种元素伤害的概念
///但是游戏的逻辑可能会依赖于这个伤害做一些文章，比如“受到子弹伤害减少90%”之类的
///</summary>
public struct Damage{
    public int bullet;
    public int explosion;
    public int mental;

    public Damage(int bullet, int explosion = 0, int mental = 0){
        this.bullet = bullet;
        this.explosion = explosion;
        this.mental = mental;
    }

    ///<summary>
    ///统计规则，在这个游戏里伤害和治疗不能共存在一个结果里，作为抵消用
    ///<param name="asHeal">是否当做治疗来统计</name>
    ///</summary>
    public int Overall(bool asHeal = false){
        return (asHeal == false) ? 
            (Mathf.Max(0, bullet) + Mathf.Max(0, explosion) + Mathf.Max(0, mental)):
            (Mathf.Min(0, bullet) + Mathf.Min(0, explosion) + Mathf.Min(0, mental));
    }

    public static Damage operator +(Damage a, Damage b){
        return new Damage(
            a.bullet + b.bullet,
            a.explosion + b.explosion,
            a.mental + b.mental
        );
    }
    public static Damage operator *(Damage a, float b){
        return new Damage(
            Mathf.RoundToInt(a.bullet * b),
            Mathf.RoundToInt(a.explosion * b),
            Mathf.RoundToInt(a.mental * b)
        );
    }
}

///<summary>
///伤害类型的Tag元素，因为DamageInfo的逻辑需要的严谨性远高于其他的元素，所以伤害类型应该是枚举数组的
///这个伤害类型不应该是类似 火伤害、水伤害、毒伤害之类的，如果是这种元素伤害，那么应该是在damage做文章，即damange不是int而是一个struct或者array或者dictionary，然后DamageValue函数里面去改最终值算法
///这里的伤害类型，指的还是比如直接伤害、反弹伤害、dot伤害等等，一些在逻辑处理流程会有不同待遇的东西，比如dot伤害可能不会触发一些效果等，当然这最终还是取决于策划设计的规则。
///</summary>
public enum DamageInfoTag{
    directDamage = 0,   //直接伤害
    periodDamage = 1,   //间歇性伤害
    reflectDamage = 2,  //反噬伤害
    directHeal = 10,    //直接治疗
    periodHeal = 11,    //间歇性治疗
    monkeyDamage = 9999    //这个类型的伤害在目前这个demo中没有意义，只是告诉你可以随意扩展，仅仅比string严肃些。
}