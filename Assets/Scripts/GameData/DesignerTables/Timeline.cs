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
                new TimelineNode(0.10f, "PlaySightEffectOnCaster", new object[]{"Muzzle","Effect/MuzzleFlash","",false}),
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

            //发射氪漏氪回力标
            { "skill_cloakBoomerang", new TimelineModel("skill_cloakBoomerang", new TimelineNode[]{
                new TimelineNode(0.00f, "SetCasterControlState", new object[]{true, true, false}),
                new TimelineNode(0.00f, "CasterPlayAnim", new object[]{"Fire", false}),
                new TimelineNode(0.10f, "PlaySightEffectOnCaster", new object[]{"Head","Effect/Heart","",false}),
                new TimelineNode(0.10f, "FireBullet", new object[]{
                    new BulletLauncher(
                        Bullet.data["cloakBoomerang"], null, Vector3.zero, 0, 5.0f, 10.0f, 0,
                        DesignerScripts.Bullet.bulletTween["CloakBoomerangTween"],
                        DesignerScripts.Bullet.targettingFunc["BulletCasterSelf"],
                        true, new Dictionary<string, object>(){{"backTime", 1.0f}}
                    ), "Muzzle"
                }),
                new TimelineNode(0.50f, "SetCasterControlState", new object[]{true, true, true})
            }, 0.50f, TimelineGoTo.Null)},

            //发射传送子弹
            { "skill_teleportBullet_fire", new TimelineModel("skill_teleportBullet_fire", new TimelineNode[]{
                new TimelineNode(0.00f, "SetCasterControlState", new object[]{true, true, false}),
                new TimelineNode(0.00f, "CasterPlayAnim", new object[]{"Fire", false}),
                new TimelineNode(0.10f, "PlaySightEffectOnCaster", new object[]{"Muzzle","Effect/MuzzleFlash","",false}),
                new TimelineNode(0.10f, "FireBullet", new object[]{
                    new BulletLauncher(
                        Bullet.data["teleportBullet"], null, Vector3.zero, 0, 6.0f, 3.0f, 0,
                        DesignerScripts.Bullet.bulletTween["SlowlyFaster"]
                    ), "Muzzle"
                }),
                new TimelineNode(0.50f, "SetCasterControlState", new object[]{true, true, true})
            }, 0.50f, TimelineGoTo.Null)},
            //闪现过去吃掉传送子弹(直接交给buff去办)
            { "skill_teleportBullet_tele", new TimelineModel("skill_teleportBullet_tele", new TimelineNode[]{
                new TimelineNode(0.00f, "AddBuffToCaster", new object[]{
                    new AddBuffInfo(DesingerTables.Buff.data["TeleportTo"], null, null, 1, 0.0f, true, false)
                })
            }, 0.10f, TimelineGoTo.Null)},

            //发射跟踪子弹
            { "skill_followfire", new TimelineModel("skill_followfire", new TimelineNode[]{
                new TimelineNode(0.00f, "SetCasterControlState", new object[]{true, true, false}),
                new TimelineNode(0.00f, "CasterPlayAnim", new object[]{"Fire", false}),
                new TimelineNode(0.10f, "PlaySightEffectOnCaster", new object[]{"Muzzle","Effect/MuzzleFlash","",false}),
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

            //丢手雷
            { "skill_grenade", new TimelineModel("skill_grenade", new TimelineNode[]{
                new TimelineNode(0.00f, "SetCasterControlState", new object[]{true, true, false}),
                new TimelineNode(0.00f, "CasterPlayAnim", new object[]{"Fire", false}),
                new TimelineNode(0.10f, "FireBullet", new object[]{
                    new BulletLauncher(
                        Bullet.data["boomball"], null, Vector3.zero, 0, 3.0f, 2.0f, 0,  
                            DesignerScripts.Bullet.bulletTween["BoomBallRolling"]
                        ), "Muzzle"
                    }
                ),
                new TimelineNode(0.50f, "SetCasterControlState", new object[]{true, true, true})
            }, 0.50f, TimelineGoTo.Null)},

            //召唤炸药桶
            { "skill_exploseBarrel", new TimelineModel("skill_exploseBarrel", new TimelineNode[]{
                new TimelineNode(0.00f, "SetCasterControlState", new object[]{true, true, false}),
                new TimelineNode(0.00f, "CasterPlayAnim", new object[]{"Fire", false}),
                new TimelineNode(0.10f, "SummonCharacter", new object[]{
                    "Barrel", new ChaProperty(0, 5), 0f, "",
                    new string[]{"Barrel"},
                    new AddBuffInfo[]{
                        new AddBuffInfo(DesingerTables.Buff.data["ExplosionBarrel"], null, null, 1, 10, true, true)
                    }
                }),
                new TimelineNode(0.50f, "SetCasterControlState", new object[]{true, true, true})
            }, 0.50f, TimelineGoTo.Null)},

            //发射小猴球
            { "skill_spaceMonkeyBall", new TimelineModel("skill_spaceMonkeyBall", new TimelineNode[]{
                new TimelineNode(0.00f, "SetCasterControlState", new object[]{true, true, false}),
                new TimelineNode(0.00f, "CasterPlayAnim", new object[]{"Fire", false}),
                new TimelineNode(0.10f, "PlaySightEffectOnCaster", new object[]{"Muzzle","Effect/MuzzleFlash","",false}),
                new TimelineNode(0.10f, "CreateAoE", new object[]{
                    new AoeLauncher(
                        AoE.data["SpaceMonkeyBall"], null, Vector3.zero, 0.25f, 100, 0,
                        DesignerScripts.AoE.aoeTweenFunc["SpaceMonkeyBallRolling"], new object[]{Vector3.forward * 0.1f} //小猴球原始滚动速度0.1米/秒
                    ), true
                }),
                new TimelineNode(0.50f, "SetCasterControlState", new object[]{true, true, true})
            }, 0.50f, TimelineGoTo.Null)},

            //角色向移动方向打滚的技能效果
            { "skill_roll", new TimelineModel("skill_roll", new TimelineNode[]{
                new TimelineNode(0.00f, "CasterPlayAnim", new object[]{"Roll", true}),
                new TimelineNode(0.00f, "PlaySightEffectOnCaster", new object[]{"Body","Effect/Fire_B","fire_following",true}),
                new TimelineNode(0.01f, "SetCasterControlState", new object[]{false, false, false}),
                new TimelineNode(0.10f, "CasterImmune", new object[]{0.70f}),
                new TimelineNode(0.20f, "CasterForceMove", new object[]{2.0f, 0.5f, 0.00f, true, false}),
                new TimelineNode(0.80f, "StopSightEffectOnCaster", new object[]{"Body", "fire_following"}),
                new TimelineNode(0.80f, "PlaySightEffectOnCaster", new object[]{"Body","Effect/ShockWave","shockWave",false}),
                new TimelineNode(0.80f, "SetCasterControlState", new object[]{true, true, true})    //早0.1秒恢复操作状态手感好点
            }, 0.90f, TimelineGoTo.Null) }

        };

        
        //private static TimelineModel skill_roll = ;
    }    
}