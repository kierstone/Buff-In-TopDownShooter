using System.Collections;
using System.Collections.Generic;
using UnityEngine;

///<summary>
///技能是角色拥有的东西，因为角色有技能，玩家或者ai才能操作角色释放技能
///</summary>
public class SkillObj{
    ///<summary>
    ///技能的模板，创建于skillModel，但运行中还是会允许改变
    ///</summary>
    public SkillModel model;

    ///<summary>
    ///技能等级
    ///</summary>
    public int level;

    public SkillObj(SkillModel model, int level = 1){
        this.model = model;
        this.level = level;
    }
}

///<summary>
///策划填表的技能
///</summary>
public struct SkillModel{
    ///<summary>
    ///技能的id
    ///</summary>
    public string id;

    ///<summary>
    ///技能使用的条件，这个游戏中只有资源需求，比如hp、ammo之类的
    ///</summary>
    public ChaResource condition;

    ///<summary>
    ///技能的消耗，成功之后会扣除这些资源
    ///</summary>
    public ChaResource cost;

    ///<summary>
    ///技能的效果，必然是一个timeline
    ///</summary>
    public TimelineModel effect;

    ///<summary>
    ///学会技能的时候，同时获得的buff，比如被动技能，就是学会的时候给buff就够了，没有timeline
    ///</summary>
    public AddBuffInfo[] buff;

    public SkillModel(string id, ChaResource cost, ChaResource condition, string effectTimeline, AddBuffInfo[] buff){
        this.id = id;
        this.cost = cost;
        this.condition = condition;
        this.effect = DesingerTables.Timeline.data[effectTimeline]; //SceneVariants.desingerTables.timeline.data[effectTimeline];
        this.buff = buff;
    }
}