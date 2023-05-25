using static Godot.Community.ControlBinding.ValidationChangedEventHandler;

namespace Godot.Community.ControlBinding.Interfaces
{
    public interface IObservableNode
    {
        void SetViewModelData(object viewModelData);
        void OnPropertyValidationFailed(Godot.Control control, string targetPropertyName, string message);
        void OnPropertyValidationSucceeded(Godot.Control control, string propertyName);
        bool HasErrors {get;}
        event Godot.Community.ControlBinding.ValidationChangedEventHandler ControlValidationChanged;
    }
}