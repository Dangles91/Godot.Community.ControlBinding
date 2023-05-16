using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Godot;

namespace ControlBinding.Binding
{
    public class ListItem
    {
        public string DisplayValue { get; set; }
        public int Id { get; set; } = -1;
        public Texture2D Icon { get; set; }
        public ViewModel SceneViewModel { get; set; }
        public ObservableObject ViewModelData { get; set; }
        public string ScenePath { get; set; }
        public string Tooltip { get; set; }
        public Variant Metadata { get; set; }
        public bool? Disabled { get; set; }

        // Background color is not stateful. If it is set it will not be automatically reset.
        // NOTE: This would be possible if a ListItem backing is stored in cache and re-referenced. 
        // However, ItemList doesn't support getting the Godot default colors, only the custom colors.
        public Color? BackgroundColor { get; set; }

        // Background color is not stateful. If it is set it will not be automatically reset.
        public Color? ForegroundColor { get; set; }
        public string Language { get; set; }
        public Rect2? IconRegion { get; set; }
        public Color? IconModulate { get; set; }
        public bool? Selectable { get; set; }        
        public Godot.Control.TextDirection TextDirection { get; set; }        
        public bool? IconTransposed { get; set; }
    }
}