using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class UIPlayerAmmo : MonoBehaviour
{
    [SerializeField]
    private Player player;

    [SerializeField]
    private Gradient AmmoGradient;

    [SerializeField]
    private List<Image> Cells;
    private List<Image> AvailableCells = new List<Image>();

    [SerializeField]
    private AnimationCurve PositionCurve;
    [SerializeField]
    private AnimationCurve ScaleCurve;

    private BaseWeapon Weapon;

    private void Awake()
    {
        player.OnWeaponChanged += Load;
    }

    public void Load(BaseWeapon weapon)
    {
        if (Weapon != null)
            Weapon.Capacity.onNotify -= Capacity_onNotify;

        AvailableCells.Clear();

        Weapon = weapon;
        foreach (var cell in Cells)
        {
            cell.gameObject.SetActive(false);
        }
        for (int i = 0; i < weapon.TotalCapacity; ++i)
        {
            AvailableCells.Add(Cells[i]);
            Cells[i].gameObject.SetActive(true);
        }

        for (int i = 0; i < AvailableCells.Count; ++i)
        {
            AvailableCells[i].color = AmmoGradient.Evaluate((float)i / AvailableCells.Count);
        }

        Weapon.Capacity.onNotify += Capacity_onNotify;
    }

    private void Capacity_onNotify(int obj)
    {
        if(obj == Weapon.TotalCapacity)
        {
            for (int i = 0; i < AvailableCells.Count; ++i)
            {
                AvailableCells[i].color = AmmoGradient.Evaluate((float)i / AvailableCells.Count);
            }
        }
        else
        {
            StartCoroutine(CellAnimation(AvailableCells[obj], Weapon.CoolDown));
            AvailableCells[obj].color = Color.gray;
        }
    }

    private IEnumerator CellAnimation(Image obj, float time)
    {
        var defaultPos = obj.transform.localPosition;
        var defaultScale = obj.transform.localScale;

        float t = 0;
        while (t < time)
        {
            obj.transform.localPosition = Vector3.Lerp(defaultPos + new Vector3(20, 30, 0), defaultPos, PositionCurve.Evaluate(t / time));
            obj.transform.localScale = Vector3.one * ScaleCurve.Evaluate(t / time);

            t += Time.deltaTime;
            yield return null;
        }

        obj.transform.localPosition = Vector3.Lerp(defaultPos + new Vector3(20, 30, 0), defaultPos, PositionCurve.Evaluate(1));
        obj.transform.localScale = Vector3.one * ScaleCurve.Evaluate(1);
    }

}
