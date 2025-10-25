namespace UnityCore.UI.MVP
{
    public class PresenterBase<V, M> where V : ViewBase where M : Model
    {
        protected V MainView;
        protected M Model;
        
        public PresenterBase(V mainView, M model)
        {
            MainView = mainView;
            Model = model;
            PreInit();
            Init();
        }

        private void Init()
        {
            Model.OnInitModel();
            SubscribeToModel();
            SubscribeViewEvents();
        }

        protected void PreInit()
        {
            OnPreInit();
        }
        
        protected virtual void OnPreInit()
        {
            
        }


        protected virtual void SubscribeToModel()
        {
            
        }
        
        protected virtual void SubscribeViewEvents()
        {
            
        }
    }
}