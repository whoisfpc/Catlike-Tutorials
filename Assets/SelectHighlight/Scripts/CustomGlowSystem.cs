using System.Collections.Generic;

namespace SelectHighlight
{
    public class CustomGlowSystem
    {
        private static CustomGlowSystem instance;
        public static CustomGlowSystem Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new CustomGlowSystem();
                }
                return instance;
            }
        }

        public HashSet<CustomGlowObj> glowObjSet = new HashSet<CustomGlowObj>();

        public void Add(CustomGlowObj glowObj)
        {
            if (!glowObjSet.Contains(glowObj))
                glowObjSet.Add(glowObj);
        }

        public void Remove(CustomGlowObj glowObj)
        {
            if (glowObjSet.Contains(glowObj))
                glowObjSet.Remove(glowObj);
        }

        public void PresentOnlyOne(CustomGlowObj glowObj)
        {
            glowObjSet.Clear();
            glowObjSet.Add(glowObj);
        }
    }
}
