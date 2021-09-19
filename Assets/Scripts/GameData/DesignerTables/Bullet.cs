using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DesingerTables
{
    
    ///<summary>
    ///BulletModel
    ///</summary>
    public class Bullet{
        public static Dictionary<string, BulletModel> data = new Dictionary<string, BulletModel>(){
            {"normal0", new BulletModel(
                "normal0", "BulletNormal0", 
                "", new object[0],
                "CommonBulletHit", new object[]{1.0f,0.05f,"Effect/HitEffect_A","Body"},
                "CommonBulletRemoved", new object[]{"Effect/HitEffect_A"}
            )},
            {"normal1", new BulletModel(
                "normal1", "BulletNormal1", 
                "", new object[0],
                "CommonBulletHit", new object[]{1.0f,0.05f,"Effect/HitEffect_A"}, 
                "CommonBulletRemoved", new object[]{"Effect/HitEffect_A"}
            )},
            {"cloakBoomerang", new BulletModel(
                "cloakBoomerang", "Boomerang", 
                "", new object[0],
                "CloakBoomerangHit", new object[]{1.0f,0.05f,"Effect/HitEffect_A"}, 
                "", new object[0],
                MoveType.fly, false, 0.5f, 99999, 0.5f, true, true)
            },
            {"teleportBullet", new BulletModel(
                "teleportBullet", "TeleportBulletEff",
                "RecordBullet", new object[0],
                "CommonBulletHit", new object[]{0.6f, 0.0f, "Effect/Star_B"},
                "CommonBulletRemoved", new object[]{"Effect/Star_B"}, 
                MoveType.fly, true
            )},
            {"boomball", new BulletModel(
                "boomball", "BombBall",
                "SetBombBouncing", new object[0],
                "CreateAoEOnHit", new object[]{new AoeLauncher(DesingerTables.AoE.data["BoomExplosive"], null, Vector3.zero, 1.5f, 0, 0)},
                "CreateAoEOnRemoved", new object[]{
                    new AoeLauncher(DesingerTables.AoE.data["StayingBoom"], null, Vector3.zero, 0.1f, 3.0f, 0),  //反正碰撞也没用，不如就直接…………
                    new AoeLauncher(DesingerTables.AoE.data["BoomExplosive"], null, Vector3.zero, 1.5f, 0, 0)                    
                }
            )}
        };
    }
}