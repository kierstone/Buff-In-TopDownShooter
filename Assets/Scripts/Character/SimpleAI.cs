using System.Collections;
using System.Collections.Generic;
using UnityEngine;


///<summary>
///顾名思义，就是为了凑效果的，并不是真的ai结构，只是让敌人看起来运动了
///</summary>
public class SimpleAI:MonoBehaviour{
    private float toNextFire = 3.0f;
    private float toNextRotate = 2.0f;

    private float moveDegree;

    private TimelineModel fire = new TimelineModel("", new TimelineNode[]{
        new TimelineNode(0.00f, "SetCasterControlState", new object[]{true, true, false}),
        new TimelineNode(0.00f, "CasterPlayAnim", new object[]{"Fire", false}),
        new TimelineNode(0.10f, "PlaySightEffectOnCaster", new object[]{"Muzzle","Effect/MuzzleFlash","",false}),
        new TimelineNode(0.10f, "FireBullet", new object[]{
            new BulletLauncher(
                DesingerTables.Bullet.data["normal1"], null, Vector3.zero, 0, 6.0f, 10.0f, 0, 
                null, null, false
            ), "Muzzle"
        }),
        new TimelineNode(0.50f, "SetCasterControlState", new object[]{true, true, true})
    }, 0.50f, TimelineGoTo.Null);

    private ChaState chaState;

    void Start() {
        chaState = this.gameObject.GetComponent<ChaState>();
        moveDegree = this.transform.rotation.eulerAngles.y;   
    }

    private void FixedUpdate() {
        if (!chaState || chaState.dead == true) return;

        float timePassed = Time.fixedDeltaTime;

        Vector3 faceVec = (SceneVariants.MainActor().transform.position - this.transform.position);
        float rotateTo = Mathf.Atan2(faceVec.x, faceVec.z) * 180.00f / Mathf.PI;
        toNextRotate -= timePassed;
        if (toNextRotate <= 0) {
           moveDegree += Random.Range(-90.00f, 90.00f);
           toNextRotate = Random.Range(1.60f, 3.20f);
        }
        chaState.OrderRotateTo(rotateTo);
        float rRadius = moveDegree * Mathf.PI / 180;

        float mSpd = chaState.moveSpeed;
        Vector3 mInfo = new Vector3(
            Mathf.Sin(rRadius) * mSpd,
            0,
            Mathf.Cos(rRadius) * mSpd
        );
        chaState.OrderMove(mInfo);

        toNextFire -= timePassed;
        if (toNextFire <= 0){
            SceneVariants.CreateTimeline(fire, this.gameObject, null);
            toNextFire = Random.Range(2.00f, 5.00f);
        }
    }
}