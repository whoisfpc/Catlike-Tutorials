using UnityEngine;


namespace SelectHighlight
{
    public class CustomGlowObj : MonoBehaviour
    {
        public Material glowMaterial;

        private void OnEnable()
        {
            //CustomGlowSystem.Instance.Add(this);
        }

        private void OnDisable()
        {
            CustomGlowSystem.Instance.Remove(this);
        }

        private void OnMouseDown()
        {
            CustomGlowSystem.Instance.PresentOnlyOne(this);
        }
    }
}
