using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DesignerScripts
{
    ///<summary>
    ///这里的函数都是程序暴露给策划的脚本，这些脚本是游戏中一些“规则级”的，比如升级经验等，都是流程中一些关键的函数
    ///
    ///</summary>
    public class CommonScripts{
        ///<summary>
        ///根据暴击等信息获得最终伤害
        ///<param name="damageInfo">伤害信息</param>
        ///<param name="asHeal">是否当做治疗</param>
        ///<return>伤害数值</return>
        ///</summary>
        public static int DamageValue(DamageInfo damageInfo, bool asHeal = false){
            bool isCrit = Random.Range(0.00f, 1.00f) <= damageInfo.criticalRate;
            return Mathf.CeilToInt(damageInfo.damage.Overall(asHeal) * (isCrit == true ? 1.80f:1.00f));  //暴击1.8倍（就这么设定的别问为啥，我是数值策划我说了算）
        }
    }
}