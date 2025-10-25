using UnityCore.GameModule.Battle.Config;
using UnityEngine;

namespace UnityCore.GameModule.Battle.View
{
    public class BaseObject : MonoBehaviour
    {
        protected int Id;
        protected string Name;
        protected UnitType ObjectType;
        protected int m_Uid;

        public virtual void Init(int uid)
        {
            m_Uid = uid;
            ObjectType = UnitType.None;
        }

        public virtual void Enable(Vector3 pos)
        {
            gameObject.transform.position = pos;
        }

        public virtual void Disable()
        {
        }

        public int GetId()
        {
            return this.Id;
        }

        public void SetName(string name)
        {
            Name = name;
            
            gameObject.name = name;
        }

        public string GetName()
        {
            return this.Name;
        }

        public UnitType GetObjType()
        {
            return ObjectType;
        }

        public int GetUid()
        {
            return this.m_Uid;
        }

        private void OnDestroy()
        {
            OnObjectDestroy();
        }

        public virtual void OnObjectDestroy()
        {
        }

        public void SetId(int id)
        {
            Id = id;
        }

        protected void Update()
        {
            UpdateObj(Time.deltaTime);
        }

        protected virtual void UpdateObj(float deltaTime)
        {
        }
    }
}