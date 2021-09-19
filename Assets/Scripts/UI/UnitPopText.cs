using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

///<summary>
///PopUp的text挂上这个以后，就有个相当于数字的管理器了
///</summary>
public class UnitPopText : MonoBehaviour{
    //生命周期还剩下多久
    private float duration = totalDuration;

    [Tooltip("文字最终飘多高")]
    public float popHeight = 10.000f;

    [Tooltip("在谁头上跳")]
    public GameObject target;

    //总生命周期
    private static float totalDuration = 1.50f;


    private void Update() {
        if (!target) return;

        float timePassed = Time.deltaTime;

        Vector2 pos = RectTransformUtility.WorldToScreenPoint(Camera.main, target.transform.position);
        this.transform.position = pos + Vector2.up * ease((totalDuration - duration) / totalDuration) * popHeight;

        duration -= timePassed;
        if (duration <= 0) Destroy(this.gameObject);
    }

    //缓动函数，传入0-1的数字，返回0-1的数字
    private float ease(float t){
        t = Mathf.Clamp(t, 0.000f, 1.000f);
        return Mathf.Sqrt(t);
    }
}