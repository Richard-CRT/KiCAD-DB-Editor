﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace KiCad_DB_Editor.Model.Json
{
    public class JsonKiCadSymbolLibrary
    {
        [JsonPropertyName("nickname"), JsonPropertyOrder(1)]
        public string Nickname { get; set; } = "";

        [JsonPropertyName("relative_path"), JsonPropertyOrder(2)]
        public string RelativePath { get; set; } = "";

        [JsonConstructor]
        public JsonKiCadSymbolLibrary() { }

        public JsonKiCadSymbolLibrary(KiCadSymbolLibrary kiCadSymbolLibrary)
        {
            Nickname = kiCadSymbolLibrary.Nickname;
            RelativePath = kiCadSymbolLibrary.RelativePath;
        }
    }
}
