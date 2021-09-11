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
                ChaControlState.origin, null    //不改变任何属性和状态
            )}
        };
    }
}