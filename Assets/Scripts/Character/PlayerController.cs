using System.Collections;
using System.Collections.Generic;
using UnityEngine;

///<summary>
///玩家操作的控件，理论上它只能被加在“主角身上”。
///但如果我们有类似wow牧师的精神控制之类的技能、或者是控制多个分身同步行动的，就需要给多目标添加了
///</summary>
public class PlayerController : MonoBehaviour
{
    public Camera mainCamera;

    private ChaState chaState;

    void Start() {
        chaState = this.gameObject.GetComponent<ChaState>();   
    }

    void FixedUpdate() {
        if (!chaState || chaState.dead == true) return;

        float ix = Input.GetAxis("Horizontal");
        float iz = Input.GetAxis("Vertical");
        bool[] sBtn = new bool[]{
            Input.GetButton("Fire3"),
            Input.GetButton("Fire2"),
            Input.GetButton("Fire1")
        };
        
        Vector2 cursorPos = Input.mousePosition;

        float rotateTo = transform.rotation.eulerAngles.y;
        //TODO，这里不应该直接给UnitMove UnitRotate发信息
        if (mainCamera){
            //先获得主角的屏幕坐标，然后对比鼠标坐标就知道转向了
            Vector2 mScreenPos = RectTransformUtility.WorldToScreenPoint(mainCamera, transform.position);
            rotateTo = Mathf.Atan2(cursorPos.x - mScreenPos.x, cursorPos.y - mScreenPos.y) * 180.00f / Mathf.PI;
            chaState.OrderRotateTo(rotateTo);
        }

        if (ix != 0 || iz != 0){
            float mSpd = chaState.moveSpeed;
            Vector3 mInfo = new Vector3(ix*mSpd, 0, iz*mSpd);
            chaState.OrderMove(mInfo);
        }

        string[] skillId = new string[]{
            "spaceMonkeyBall", "teleportBullet", "cloakBoomerang"
        }; //"fire", 

        bool btnHolding = false;
        for (int i = 0; i < sBtn.Length; i++){
            if (sBtn[i] == true){
                chaState.CastSkill(skillId[i]);
                btnHolding = true;
            }
        }
        chaState.charging = btnHolding;
    }
}