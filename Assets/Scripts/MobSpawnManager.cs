using System.Collections;
using System.Collections.Generic;
using UnityEngine;

///<summary>
///我专门管刷怪的
///</summary>
public class MobSpawnManager : MonoBehaviour{
    [Tooltip("保持最多有多少个怪物的数量，一旦超过就不刷了\n如果数字太大，一切后果概不负责……")]
    public int maxMob;

    [Tooltip("刷怪的周期，现在肯定是均匀的，不高兴取随机数了，就这么地了")]
    public float spawnPeriod = 10.0f;   //10秒检查一次

    private float timePassed = 0;
    private bool justCreated = true;
    private int spawned = 0;    //已经刷过多少个了

    private static int mobSide = 2;

    private void FixedUpdate() {
        if (justCreated == true && maxMob > 0){
            Spawn();
        }
        timePassed += Time.fixedDeltaTime;
        if (timePassed >= spawnPeriod){
            timePassed = 0;
            Spawn();
        }
    }

    private void Spawn(){
        GameObject[] cha = GameObject.FindGameObjectsWithTag("Character");
        int toSpawn = maxMob;
        for (int i = 0; i < cha.Length; i++){
            ChaState cs = cha[i].GetComponent<ChaState>();
            if (cs != null && cs.dead == false && cs.side == mobSide){
                toSpawn -= 1;
            }
        }
        for (int i = 0; i < toSpawn; i++){
            GameObject enemy = SceneVariants.CreateCharacter(
                "MaleGunner", mobSide, 
                SceneVariants.map.GetRandomPosForCharacter(new RectInt(0, 0, SceneVariants.map.MapWidth(), SceneVariants.map.MapHeight())),
                new ChaProperty(Random.Range(50,70), 50 + spawned * 2, 0, Random.Range(15,30) + spawned, 100, 0.25f, 0.4f), Random.Range(0.00f, 359.99f)
            );
            enemy.AddComponent<SimpleAI>();
            spawned += 1;
        }
    }
}