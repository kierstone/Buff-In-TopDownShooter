using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DesingerTables
{
    ///<summary>
    ///buff的效果
    ///</summary>
    public class Buff{
        public static Dictionary<string, BuffModel> data = new Dictionary<string, BuffModel>(){
            { "AutoCheckReload", new BuffModel( "AutoCheckReload", "自动填装", new string[]{"Passive"}, 0, 1, 0,
                "", new object[0],  //occur
                "", new object[0],  //remove
                "", new object[0],  //tick
                "ReloadAmmo", new object[0],  //cast
                "", new object[0],  //hit
                "", new object[0],  //hurt
                "", new object[0],  //kill
                "", new object[0],  //dead
                ChaControlState.origin, null
            )},
            { "TeleportBulletPassive", new BuffModel("TeleportBulletPassive", "传送弹技能被动效果", new string[]{"Passive"}, 0, 1, 0,
                "", new object[0],  //occur
                "", new object[0],  //remove
                "", new object[0],  //tick
                "FireTeleportBullet", new object[0],  //cast
                "", new object[0],  //hit
                "", new object[0],  //hurt
                "", new object[0],  //kill
                "", new object[0],  //dead
                ChaControlState.origin, null
            )},
            { "TeleportTo", new BuffModel("TeleportTo", "直接把GameObject传送到某个世界坐标（非常危险）", new string[]{"Dangerous"}, 0, 1, 0,
                "", new object[0],  //occur
                "TeleportCarrier", new object[0],  //remove
                "", new object[0],  //tick
                "", new object[0],  //cast
                "", new object[0],  //hit
                "", new object[0],  //hurt
                "", new object[0],  //kill
                "", new object[0],  //dead
                ChaControlState.stun, null 
            )},
            { "ExplosionBarrel", new BuffModel("ExplosionBarrel", "爆炸的桶子用的", new string[]{"Passive"}, -1, 1, 5.0f,
                "", new object[0],  //occur
                "", new object[0],  //remove
                "BarrelDurationLose", new object[0],  //tick
                "", new object[0],  //cast
                "", new object[0],  //hit
                "OnlyTakeOneDirectDamage", new object[0],  //hurt
                "", new object[0],  //kill
                "BarrelExplosed", new object[0],  //dead
                ChaControlState.stun, null  //桶子也是被昏迷的
            )}
        };
    }
}