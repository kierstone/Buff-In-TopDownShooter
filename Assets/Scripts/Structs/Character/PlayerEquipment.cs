using System.Collections;
using System.Collections.Generic;
using UnityEngine;

///<summary>
///游戏中的装备，其实道具实只有玩家有的，而不是某个角色有的，所以道具和角色本质上没有什么直接关系
///我们通常以为装备是挂在角色身上的，但实际上只有玩这个游戏的主玩家（最多一起热座的几个）才有装备的概念
///而通常因为我们看到了一个角色的外貌变化、数值变化，也猜得到他肯定穿了装备（其他玩家角色），我们就认为装备和角色是直接相关的
///这都是幻术，实际上我们只是看到了一个“属性被hack”了的角色而已。
///</summary>
public struct EquipmentObj{
    ///<summary>
    ///Model是什么
    ///</summary>
    public EquipmentModel model;

    //由于没有其他的内容，所以只有一个属性，但是为了扩展性和结构本身，obj还是需要的，万一要强化升星打孔镶钻了呢？

    public EquipmentObj(EquipmentModel model){
        this.model = model;
    }
}

///<summary>
///装备的模板属性，策划填表数据
///我们通常因为在一个背包内看到了道具和装备，就认为他们是一样的东西
///但实际上他们是存在于2个不同的容器中的不同数据，只是这两个容器的“上限之和”就是我们肉眼看到的“背包容量”
///</summary>
public struct EquipmentModel{
    ///<summary>
    ///装备id
    ///</summary>
    public string id;

    ///<summary>
    ///装备的icon
    ///</summary>
    public string icon;

    ///<summary>
    ///装备名称
    ///</summary>
    public string name;

    ///<summary>
    ///装备Tag
    ///</summary>
    public string[] tags;

    ///<summary>
    ///装备的部位
    ///</summary>
    public EqupmentSlot slot;

    ///<summary>
    ///对于装备而言，装上以后可以获得的属性
    ///而对于使用类的道具，则不该依赖于这个属性做事，因为给人增加临时属性的是使用效果timeline里面创建的buff
    ///</summary>
    public ChaProperty equipmentProperty;

    ///<summary>
    ///对于装备，装上之后可以临时学会的技能
    ///这也是这个游戏的设定——角色的一部分技能绑定在装备上，就像激战2一样，什么类型武器，用什么武器技能
    ///</summary>
    public SkillModel[] skills;

    ///<summary>
    ///如果是装备，则在装备之后会有buff，移除之后去掉buff，但是使用效果的buff不应该在这里
    ///</summary>
    public AddBuffInfo[] buffs;

    public EquipmentModel(
        string id, string icon, string name, string[] tags, 
        ChaProperty equipment,
        SkillModel[] skills,
        AddBuffInfo[] buffs,
        EqupmentSlot slot = EqupmentSlot.weapon
    ){
        this.id = id;
        this.name = name;
        this.icon = icon;
        this.tags = tags;
        this.slot = slot;
        this.equipmentProperty = equipment;
        this.skills = skills;
        this.buffs = buffs;
    }
}

///<summary>
///道具根据部位所区分的类别枚举
///</summary>
public enum EqupmentSlot{
    weapon = 1, //武器
    helm = 2,   //头盔
    armor = 3,  //盔甲
    trinket = 4   //饰品

    //没了，我这demo就这点了，你的游戏你可以自己定
}