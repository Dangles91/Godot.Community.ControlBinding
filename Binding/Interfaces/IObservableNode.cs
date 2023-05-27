using static Godot.Community.ControlBinding.ValidationChangedEventHandler;

namespace Godot.Community.ControlBinding.Interfaces
{
    public interface IViewModel : IObservableObject
    {
        void SetViewModelData(object viewModelData);
    }
}