using Godot;
using System;
using System.Collections.Generic;

namespace ControlBinding.EventArgs;
public enum ObservableListChangeType
{
    Add,
    Remove,
    Replace,
    Insert,
    Clear
}

public partial class ObservableListChangedEventArgs : GodotObject
{
    public int Index { get; set; }
    public IList<Object> ChangedEntries { get; set; }
    public ObservableListChangeType ChangeType { get; set; }
}
