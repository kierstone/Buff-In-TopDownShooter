using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DesingerTables
{
    
    ///<summary>
    ///角色使用的动画信息
    ///</summary>
    public class UnitAnimInfo{
        public static Dictionary<string, Dictionary<string, AnimInfo>> data = new Dictionary<string, Dictionary<string, AnimInfo>>(){
            {"Default_Gunner", new Dictionary<string, AnimInfo>(){
                {"Stand", new AnimInfo("Stand",new KeyValuePair<SingleAnimInfo, int>[]{new KeyValuePair<SingleAnimInfo, int>(new SingleAnimInfo("Stand"),1)}, 0)},
                {"MoveForward", new AnimInfo("MoveForward",new KeyValuePair<SingleAnimInfo, int>[]{new KeyValuePair<SingleAnimInfo, int>(new SingleAnimInfo("MoveForward"),1)}, 0)},
                {"MoveBack", new AnimInfo("MoveBack", new KeyValuePair<SingleAnimInfo, int>[]{new KeyValuePair<SingleAnimInfo, int>(new SingleAnimInfo("MoveBack"),1)}, 0)},
                {"MoveLeft", new AnimInfo("MoveLeft", new KeyValuePair<SingleAnimInfo, int>[]{new KeyValuePair<SingleAnimInfo, int>(new SingleAnimInfo("MoveLeft"),1)}, 0)},
                {"MoveRight", new AnimInfo("MoveRight", new KeyValuePair<SingleAnimInfo, int>[]{new KeyValuePair<SingleAnimInfo, int>(new SingleAnimInfo("MoveRight"),1)}, 0)},
                {"Hurt", new AnimInfo("Hurt",new KeyValuePair<SingleAnimInfo, int>[]{new KeyValuePair<SingleAnimInfo, int>(new SingleAnimInfo("Hurt0", 0.3f),5),new KeyValuePair<SingleAnimInfo, int>(new SingleAnimInfo("Hurt1", 0.3f),2)}, 1)},
                {"Happy", new AnimInfo("Happy",new KeyValuePair<SingleAnimInfo, int>[]{new KeyValuePair<SingleAnimInfo, int>(new SingleAnimInfo("Victory"),1)}, 2)},
                {"Power", new AnimInfo("Power",new KeyValuePair<SingleAnimInfo, int>[]{new KeyValuePair<SingleAnimInfo, int>(new SingleAnimInfo("PowerUp", 1.33f),1)}, 2)},
                {"Fire", new AnimInfo("Fire", new KeyValuePair<SingleAnimInfo, int>[]{new KeyValuePair<SingleAnimInfo, int>(new SingleAnimInfo("Fire",0.5f),1)}, 3)},
                {"Reload", new AnimInfo("Reload", new KeyValuePair<SingleAnimInfo, int>[]{new KeyValuePair<SingleAnimInfo, int>(new SingleAnimInfo("Reload",1.33f),1)}, 3)},
                {"JumpStart", new AnimInfo("JumpStart",new KeyValuePair<SingleAnimInfo, int>[]{new KeyValuePair<SingleAnimInfo, int>(new SingleAnimInfo("JumpStart", 0.21f),1)}, 3)},
                {"Flying", new AnimInfo("Flying",new KeyValuePair<SingleAnimInfo, int>[]{new KeyValuePair<SingleAnimInfo, int>(new SingleAnimInfo("JumpAir"),1)}, 3)},
                {"JumpEnd", new AnimInfo("JumpEnd",new KeyValuePair<SingleAnimInfo, int>[]{new KeyValuePair<SingleAnimInfo, int>(new SingleAnimInfo("JumpEnd",0.33f),1)}, 3)},
                {"RapidFire", new AnimInfo("RapidFire",new KeyValuePair<SingleAnimInfo, int>[]{new KeyValuePair<SingleAnimInfo, int>(new SingleAnimInfo("RapidFire"),1)}, 3)},
                {"RollForward", new AnimInfo("RollForward",new KeyValuePair<SingleAnimInfo, int>[]{new KeyValuePair<SingleAnimInfo, int>(new SingleAnimInfo("RollForward",1.0f),1)}, 3)},
                {"RollBack", new AnimInfo("RollBack",new KeyValuePair<SingleAnimInfo, int>[]{new KeyValuePair<SingleAnimInfo, int>(new SingleAnimInfo("RollBack",1.0f),1)}, 3)},
                {"RollLeft", new AnimInfo("RollLeft",new KeyValuePair<SingleAnimInfo, int>[]{new KeyValuePair<SingleAnimInfo, int>(new SingleAnimInfo("RollLeft",1.0f),1)}, 3)},
                {"RollRight", new AnimInfo("RollRight",new KeyValuePair<SingleAnimInfo, int>[]{new KeyValuePair<SingleAnimInfo, int>(new SingleAnimInfo("RollRight",1.0f),1)}, 3)},
                {"StepForward", new AnimInfo("StepForward", new KeyValuePair<SingleAnimInfo, int>[]{new KeyValuePair<SingleAnimInfo, int>(new SingleAnimInfo("StepForward",0.66f),1)}, 3)},
                {"StepBack", new AnimInfo("StepBack", new KeyValuePair<SingleAnimInfo, int>[]{new KeyValuePair<SingleAnimInfo, int>(new SingleAnimInfo("StepBack",0.66f),1)}, 3)},
                {"StepLeft", new AnimInfo("StepLeft",new KeyValuePair<SingleAnimInfo, int>[]{new KeyValuePair<SingleAnimInfo, int>(new SingleAnimInfo("StepLeft",0.66f),1)}, 3)},
                {"StepRight", new AnimInfo("StepRight", new KeyValuePair<SingleAnimInfo, int>[]{new KeyValuePair<SingleAnimInfo, int>(new SingleAnimInfo("StepRight",0.66f),1)}, 3)},
                {"Stun", new AnimInfo("Stun",new KeyValuePair<SingleAnimInfo, int>[]{new KeyValuePair<SingleAnimInfo, int>(new SingleAnimInfo("Stun"),1)}, 3)},
                {"Dead", new AnimInfo("Dead", new KeyValuePair<SingleAnimInfo, int>[]{new KeyValuePair<SingleAnimInfo, int>(new SingleAnimInfo("Dead"),1)}, 10)}
            }}
        };
    }
}