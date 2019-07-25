using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

public class UIHPGauge : MonoBehaviour
{
    [SerializeField]
    private Player player;

    [SerializeField]
    private Image HPGauge;
    [SerializeField]
    private Image LoveGauge;

    private void Awake()
    {
        player.HP.onNotifyDelta += HP_onNotifyDelta;
        player.LoveGauge.onNotifyDelta += LoveGauge_onNotifyDelta;
    }

    private void Start()
    {
        HPGauge.fillAmount = player.HP.value / Player.MAXHP;
        LoveGauge.fillAmount = player.LoveGauge.value / Player.MAXLOVEGAUGE;
    }

    private void LoveGauge_onNotifyDelta(float arg1, float arg2)
    {
        StartCoroutine(GaugeAnimation(LoveGauge, arg2 / Player.MAXLOVEGAUGE, 0.2f));
    }

    private void HP_onNotifyDelta(float arg1, float arg2)
    {
        StartCoroutine(GaugeAnimation(HPGauge, arg2 / Player.MAXHP, 0.2f));
    }


    private IEnumerator GaugeAnimation(Image gauge, float to, float time)
    {
        float form = gauge.fillAmount;
        float t = 0;
        while (t < time)
        {
            gauge.fillAmount = Mathf.Lerp(form, to, t / time);
            t += Time.deltaTime;
            yield return null;
        }

        gauge.fillAmount = to;
    }
}
