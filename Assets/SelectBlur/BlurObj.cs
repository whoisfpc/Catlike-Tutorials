using UnityEngine;


namespace SelectBlur
{
    public class BlurObj : MonoBehaviour
    {
        private void OnDisable()
        {
            BlurSystem.Remove(this);
        }

        private void OnMouseDown()
        {
            GetComponent<Renderer>().enabled = false;
            BlurSystem.PresentOnlyOne(this);
        }

        public void ToRemove()
        {
            GetComponent<Renderer>().enabled = true;
        }
    }
}
