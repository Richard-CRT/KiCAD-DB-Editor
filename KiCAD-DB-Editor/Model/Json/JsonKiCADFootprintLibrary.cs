﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace KiCAD_DB_Editor.Model.Json
{
    public class JsonKiCADFootprintLibrary
    {
        [JsonPropertyName("nickname"), JsonPropertyOrder(1)]
        public string Nickname { get; set; } = "";

        [JsonPropertyName("relative_path"), JsonPropertyOrder(2)]
        public string RelativePath { get; set; } = "";

        [JsonConstructor]
        public JsonKiCADFootprintLibrary() { }

        public JsonKiCADFootprintLibrary(KiCADFootprintLibrary kicadFootprintLibrary)
        {
            Nickname = kicadFootprintLibrary.Nickname;
            RelativePath = kicadFootprintLibrary.RelativePath;
        }
    }
}
