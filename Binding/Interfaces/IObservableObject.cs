namespace Godot.Community.ControlBinding.Interfaces;

internal interface IObservableObject
{
    void OnPropertyChanged(string name);
    event PropertyChangedEventHandler PropertyChanged;
}