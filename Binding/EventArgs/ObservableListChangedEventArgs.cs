using Godot;
using System;
using System.Collections.Generic;

namespace Godot.Community.ControlBinding.EventArgs;
public enum ObservableListChangeType
{
    Add,
    Remove,
    Replace,
    Insert,
    Clear
}

public partial class ObservableListChangedEventArgs
{
    public int Index { get; set; }
    public IList<Object> ChangedEntries { get; set; }
    public ObservableListChangeType ChangeType { get; set; }
}
