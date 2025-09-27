namespace MisterGames.UI.Navigation {
    
    public interface IUiNavigationCallback {

        bool CanNavigateBack();
        
        void OnNavigateBack();
    }
    
}