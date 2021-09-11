using System.Collections;
using System.Collections.Generic;
using UnityEngine;


///<summary>
///管理角色脚底下那个血条的
///</summary>
public class ChaPie:MonoBehaviour{
    private ChaState chaState;
    private PieChartController chart;

    private void Start() {
        chaState = this.gameObject.GetComponent<ChaState>();
        chart = this.gameObject.GetComponentInChildren<PieChartController>();

        if (!chaState || !chart) return;
        chart.radius = chaState.property.bodyRadius;
    }

    private void FixedUpdate() {
        if (!chaState || !chart) return;

        chart.angleDegree = 360 * chaState.resource.hp / chaState.property.hp;
        chart.transform.localEulerAngles = new Vector3(
            chart.transform.localRotation.eulerAngles.x,
            -this.transform.eulerAngles.y,
            chart.transform.localRotation.eulerAngles.z
        );
    }
}