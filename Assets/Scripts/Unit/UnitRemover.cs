using System.Collections;
using System.Collections.Generic;
using UnityEngine;

///<summary>
///我负责多久以后清理掉GameObject，比如人死了，特效时间到了，都可以用
///</summary>
public class UnitRemover : MonoBehaviour {
    [Tooltip("多久以后把我的gameObject干掉，单位：秒")]
    public float duration = 1.0f;

    private void FixedUpdate() {
        duration -= Time.fixedDeltaTime;
        if (duration <= 0){
            Destroy(this.gameObject);
        }
    }
}