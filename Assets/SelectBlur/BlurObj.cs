using UnityEngine;


namespace SelectBlur
{
    public class BlurObj : MonoBehaviour
    {
        public bool addDefault;
        private void OnEnable()
        {
            if (addDefault)
            {
                BlurSystem.Add(this);
            }
        }
        private void OnDisable()
        {
            BlurSystem.Remove(this);
        }

        private void OnMouseDown()
        {
            if (!BlurSystem.blurObjSet.Contains(this))
            {
                BlurSystem.PresentOnlyOne(this);
            }
        }

        public void ToAdd()
        {
            GetComponent<Renderer>().enabled = false;
        }

        public void ToRemove()
        {
            GetComponent<Renderer>().enabled = true;
        }
    }
}
