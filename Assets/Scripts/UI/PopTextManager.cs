using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

///<summary>
///PopUpText的管理器
///</summary>
public class PopTextManager : MonoBehaviour{
    private void Start() {
        
    }

    ///<summary>
    ///在指定角色身上跳一个伤害或者治疗的数字，要跳别的走别的函数
    ///<param name="cha">目标角色</param>
    ///<param name="value">伤害数字，或者治疗数字</param>
    ///<param name="asHeal">是否是治疗数字，如果是就用绿字，前面带+；如果不是，用红色前面-</param>
    ///<param name="asCritical">是否暴击，暴击数字会变大，并且加个感叹号</param>
    ///</summary>
    public void PopUpNumberOnCharacter(GameObject cha, int value, bool asHeal = false, bool asCritical = false){
        if (!cha) return;
        Vector2 mScreenPos = RectTransformUtility.WorldToScreenPoint(Camera.main, cha.transform.position);

        GameObject text = Instantiate<GameObject>(
            Resources.Load<GameObject>("Prefabs/UI/PopText"),
            mScreenPos,
            Quaternion.identity,
            this.gameObject.transform
        );

        text.GetComponent<UnitPopText>().target = cha;

        Text txt = text.GetComponent<Text>();
        txt.text = "<color="+(asHeal == true ? "green" : "red")+">" + (asHeal == true ? "+" : "-") + value.ToString() + (asCritical == true ? "!" : "") + "</color>";
        txt.fontSize = asCritical == false ? 30 : 40;
    }

    ///<summary>
    ///在指定角色身上跳一个文字
    ///<param name="cha">目标角色</param>
    ///<param name="text">要跳出来的文字，格式什么都靠这个了</param>
    ///<param name="size">字体大小</param>
    ///</summary>
    public void PopUpStringOnCharacter(GameObject cha, string text, int size = 30){
        if (!cha) return;
        Vector2 mScreenPos = RectTransformUtility.WorldToScreenPoint(Camera.main, cha.transform.position);

        GameObject textObj = Instantiate<GameObject>(
            Resources.Load<GameObject>("Prefabs/UI/PopText"),
            mScreenPos,
            Quaternion.identity,
            this.gameObject.transform
        );

        textObj.GetComponent<UnitPopText>().target = cha;

        Text txt = textObj.GetComponent<Text>();
        txt.text = text;
        txt.fontSize = size;
    }   
}