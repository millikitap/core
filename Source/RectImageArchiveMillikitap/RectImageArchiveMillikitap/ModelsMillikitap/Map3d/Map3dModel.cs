using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

/// <summary>
/// Модель для визуализации карты LeafLet с ифнормационными слайдами и маркерами
/// </summary>
namespace RectImageArchiveMillikitap.ModelsMillikitap.Map3d
{
    public class Map3dModel
    {
        public class Location
        {
            public int LocationId { get; set; }
            public bool? use_custom_markers { get; set; }
            public string icon { get; set; }
            public double? lat { get; set; }
            public double? lon { get; set; }
            public int? zoom { get; set; }
            public bool? use_custom_marker { get; set; }
        }

        public class Media
        {
            public int MediaId { get; set; }
            public string caption { get; set; }
            public string credit { get; set; }
            public string url { get; set; }
        }

        public class Text
        {
            public int TextId { get; set; }
            public string headline { get; set; }
            public string text { get; set; }
        }

        public class Custom
        {
            public int id { get; set; }
        }

        public class Slide
        {
            public int SlideId { get; set; }
            public string date { get; set; }
            public Location location { get; set; }
            public Media media { get; set; }
            public Text text { get; set; }
            public string type { get; set; }
            public Custom custom { get; set; }
            public int? uniqueid { get; set; }
        }

        public class Storymap
        {
            public int StorymapId { get; set; }
            public bool call_to_action { get; set; } = true;
            public string call_to_action_text { get; set; } = string.Empty;
            public bool map_as_image { get; set; } = false;
            public string map_type { get; set; } = "zoomify";
            public List<Slide> slides { get; set; }
            public bool isVisible { get; set; } = false;
        }

        public class Root
        {
            public Storymap storymap { get; set; }
        }
    }
}