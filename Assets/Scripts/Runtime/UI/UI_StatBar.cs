using UnityEngine;
using UnityEngine.UI;

namespace LZ
{
    public class UI_StatBar : MonoBehaviour
    {
        protected Slider slider;
        protected RectTransform rectTransform;

        [Header("Bar Options")]
        [SerializeField] protected bool scaleBarLengthWithStats = true;
        [SerializeField] protected float widthScaleMultiplier = 1;
        
        // 次要条形可能用于修饰效果（黄色条形显示动作/伤害从当前状态中扣除的量）

        protected virtual void Awake()
        {
            slider = GetComponent<Slider>();
            rectTransform = GetComponent<RectTransform>();
        }
        
        protected virtual void Start()
        {

        }

        public virtual void SetStat(int newValue)
        {
            slider.value = newValue;
        }

        public virtual void SetMaxStat(int maxValue)
        {
            slider.maxValue = maxValue;
            slider.value = maxValue;

            if (scaleBarLengthWithStats)
            {
                rectTransform.sizeDelta = new Vector2(maxValue * widthScaleMultiplier, rectTransform.sizeDelta.y);
                
                // 根据layout group的设置重置bar的位置
                PlayerUIManager.instance.playerUIHudManager.RefreshHUD();
            }
        }
    }
}