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
            {"normal0", new BulletModel("normal0", "BulletNormal0", "CommonBulletHit", new object[]{1.0f,0.05f,"HitEffect_A","Body"},"CommonBulletRemoved", new object[]{"HitEffect_A"})},
            {"normal1", new BulletModel("normal1", "BulletNormal1", "CommonBulletHit", new object[]{1.0f,0.05f,"HitEffect_A"}, "CommonBulletRemoved", new object[]{"HitEffect_A"})},
        };
    }
}