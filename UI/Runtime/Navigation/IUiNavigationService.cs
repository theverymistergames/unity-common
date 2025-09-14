using MisterGames.UI.Windows;

namespace MisterGames.UI.Navigation {
    
    public interface IUiNavigationService {

        void PerformCancel();

        void AddNavigationCallback(IUiWindow window, IUiNavigationCallback callback);
        void RemoveNavigationCallback(IUiWindow window, IUiNavigationCallback callback);
    }
    
}