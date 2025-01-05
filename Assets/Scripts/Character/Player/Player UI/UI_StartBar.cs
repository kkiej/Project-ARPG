using UnityEngine;
using UnityEngine.UI;

namespace LZ
{
    public class UI_StartBar : MonoBehaviour
    {
        private Slider slider;
        // 根据状态调整条形大小的变量（状态越高，屏幕上的条形越长）
        // 次要条形可能用于修饰效果（黄色条形显示动作/伤害从当前状态中扣除的量）

        protected virtual void Awake()
        {
            slider = GetComponent<Slider>();
        }

        public virtual void SetStat(int newValue)
        {
            slider.value = newValue;
        }

        public virtual void SetMaxStat(int maxValue)
        {
            slider.maxValue = maxValue;
            slider.value = maxValue;
        }
    }
}