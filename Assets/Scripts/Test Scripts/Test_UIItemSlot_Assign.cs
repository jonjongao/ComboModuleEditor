using UnityEngine;
using System.Collections;

namespace UnityEngine.UI
{
    public class Test_UIItemSlot_Assign : MonoBehaviour
    {

        public UIItemSlot slot;
        public UIItemDatabase itemDatabase;
        public int assignItem;

        void Awake()
        {
                DestroyImmediate(this);

            if (this.slot == null)
                this.slot = this.GetComponent<UIItemSlot>();
        }

        void Start()
        {
            if (this.slot == null || this.itemDatabase == null)
            {
                this.Destruct();
                return;
            }

            if (this.slot.IsAssigned())
            {
                this.Destruct();
                return;
            }
            else
            {
                this.slot.Assign(this.itemDatabase.GetByID(this.assignItem));
                //Debug.LogWarning(gameObject.name + " Has Assign Item");
            }


            this.Destruct();
        }

        private void Destruct()
        {
            DestroyImmediate(this);
        }
    }
}