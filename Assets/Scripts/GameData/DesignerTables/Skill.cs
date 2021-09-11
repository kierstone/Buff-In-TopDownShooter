using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DesingerTables
{
    
    ///<summary>
    ///BulletModel
    ///</summary>
    public class Skill{
        public static Dictionary<string, SkillModel> data = new Dictionary<string, SkillModel>(){
            {"fire", new SkillModel("fire", new ChaResource(0, 1), ChaResource.Null, "skill_fire")}, //即使没有子弹也可以用这个技能，但是因为有buff会让他自动转向另一个reload的timeline
            {"spaceMonkeyBall", new SkillModel("spaceMonkeyBall", new ChaResource(0, 3), ChaResource.Null, "skill_spaceMonkeyBall")},
            {"homingMissle", new SkillModel("homingMissle", new ChaResource(0, 2), ChaResource.Null, "skill_followfire")},
            {"roll", new SkillModel("roll", ChaResource.Null, ChaResource.Null, "skill_roll")}
        };
    }
}