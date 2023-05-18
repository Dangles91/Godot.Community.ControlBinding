namespace ControlBinding.Interfaces;

internal interface IObservableObject
{
    void OnPropertyChanged(string name);
    void SetViewModelData(object viewModelData);
}