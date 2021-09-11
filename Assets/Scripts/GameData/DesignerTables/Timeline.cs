using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DesingerTables
{
    public class Timeline{
        public static Dictionary<string, TimelineModel> data = new Dictionary<string, TimelineModel>(){
            //发射普通子弹
            { "skill_fire", new TimelineModel("skill_fire", new TimelineNode[]{
                new TimelineNode(0.00f, "SetCasterControlState", new object[]{true, true, false}),
                new TimelineNode(0.00f, "CasterPlayAnim", new object[]{"Fire", false}),
                new TimelineNode(0.10f, "PlaySightEffectOnCaster", new object[]{"Muzzle","MuzzleFlash","",false}),
                new TimelineNode(0.10f, "FireBullet", new object[]{
                    new BulletLauncher(
                        Bullet.data["normal0"], null, Vector3.zero, 0, 6.0f, 10.0f
                    ), "Muzzle"
                }),
                new TimelineNode(0.50f, "SetCasterControlState", new object[]{true, true, true})
            }, 0.50f, TimelineGoTo.Null)},

            //填装子弹
            { "skill_reload", new TimelineModel("skill_reload", new TimelineNode[]{
                new TimelineNode(0.00f, "SetCasterControlState", new object[]{true, true, false}),
                new TimelineNode(0.00f, "CasterPlayAnim", new object[]{"Reload", false}),
                new TimelineNode(1.10f, "CasterAddAmmo", new object[]{99999}),
                new TimelineNode(1.12f, "SetCasterControlState", new object[]{true, true, true})
            }, 1.15f, TimelineGoTo.Null)},

            //发射跟踪子弹
            { "skill_followfire", new TimelineModel("skill_followfire", new TimelineNode[]{
                new TimelineNode(0.00f, "SetCasterControlState", new object[]{true, true, false}),
                new TimelineNode(0.00f, "CasterPlayAnim", new object[]{"Fire", false}),
                new TimelineNode(0.10f, "PlaySightEffectOnCaster", new object[]{"Muzzle","MuzzleFlash","",false}),
                new TimelineNode(0.10f, "FireBullet", new object[]{
                    new BulletLauncher(
                        Bullet.data["normal0"], null, Vector3.zero, 0, 3.0f, 100.0f, 0,  
                            DesignerScripts.Bullet.bulletTween["FollowingTarget"], 
                            DesignerScripts.Bullet.targettingFunc["GetNearestEnemy"], false
                        ), "Muzzle"
                    }
                ),
                new TimelineNode(0.50f, "SetCasterControlState", new object[]{true, true, true})
            }, 0.50f, TimelineGoTo.Null)},

            //发射小猴球
            { "skill_spaceMonkeyBall", new TimelineModel("skill_spaceMonkeyBall", new TimelineNode[]{
                new TimelineNode(0.00f, "SetCasterControlState", new object[]{true, true, false}),
                new TimelineNode(0.00f, "CasterPlayAnim", new object[]{"Fire", false}),
                new TimelineNode(0.10f, "PlaySightEffectOnCaster", new object[]{"Muzzle","MuzzleFlash","",false}),
                new TimelineNode(0.10f, "CreateAoE", new object[]{
                    new AoeLauncher(
                        AoE.data["SpaceMonkeyBall"], null, Vector3.zero, 0.25f, 100, 0,
                        DesignerScripts.AoE.aoeTweenFunc["SpaceMonkeyBallRolling"], new object[]{Vector3.forward * 0.3f} //小猴球原始滚动速度0.3米/秒
                    ), true
                }),
                new TimelineNode(0.50f, "SetCasterControlState", new object[]{true, true, true})
            }, 0.50f, TimelineGoTo.Null)},

            //角色向移动方向打滚的技能效果
            { "skill_roll", new TimelineModel("skill_roll", new TimelineNode[]{
                new TimelineNode(0.00f, "CasterPlayAnim", new object[]{"Roll", true}),
                new TimelineNode(0.00f, "PlaySightEffectOnCaster", new object[]{"Body","Fire_B","fire_following",true}),
                new TimelineNode(0.01f, "SetCasterControlState", new object[]{false, false, false}),
                new TimelineNode(0.10f, "CasterImmune", new object[]{0.70f}),
                new TimelineNode(0.20f, "CasterForceMove", new object[]{2.0f, 0.5f, 0.00f, true, false}),
                new TimelineNode(0.80f, "StopSightEffectOnCaster", new object[]{"Body", "fire_following"}),
                new TimelineNode(0.80f, "PlaySightEffectOnCaster", new object[]{"Body","ShockWave","ShockWave",false}),
                new TimelineNode(0.80f, "SetCasterControlState", new object[]{true, true, true})    //早0.1秒恢复操作状态手感好点
            }, 0.90f, TimelineGoTo.Null) },

            //角色跳跃(工作尚不正常)
            { "skill_jump", new TimelineModel("skill_jump", new TimelineNode[]{
                new TimelineNode(0.00f, "CasterPlayAnim", new object[]{"JumpStart", false}),
                new TimelineNode(0.00f, "SetCasterControlState", new object[]{false, false, false}),
                new TimelineNode(0.20f, "CasterForceMove", new object[]{0.5f, 0.33f, 0.00f, true, false}),   //y方向要新起一个了
                new TimelineNode(0.33f, "CasterPlayAnim", new object[]{"Flying", false}),
                new TimelineNode(0.33f, "CasterForceMove", new object[]{1.0f, 0.33f, 0.00f, true, false}),
                new TimelineNode(0.66f, "CasterPlayAnim", new object[]{"JumpEnd", false}),
                new TimelineNode(0.66f, "CasterForceMove", new object[]{0.5f, 0.33f, 0.00f, true, false}),
                new TimelineNode(0.90f, "SetCasterControlState", new object[]{true, true, true})    //早0.1秒恢复操作状态手感好点
            }, 0.90f, new TimelineGoTo(0.65f, 0.33f))}
        };

        
        //private static TimelineModel skill_roll = ;
    }    
}