using UnityCore.Base;

namespace UnityCore.Entities.Core
{
    public abstract class SystemBase: ISystem
    {
        protected EntityComponent World => Game.World;
        
        public abstract void Update();
    }
}