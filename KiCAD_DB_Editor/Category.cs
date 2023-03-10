﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Xml.Linq;

namespace KiCAD_DB_Editor
{
    public class Category : NotifyObject
    {
        public static bool CheckNameValid(string name)
        {
            string trimmedNewCategoryName = name.Trim();
            return trimmedNewCategoryName != "";
        }


        // ============================================================================================
        // ============================================================================================

        private string? _name = null;
        [JsonPropertyName("name")]
        public string Name
        {
            get { Debug.Assert(_name is not null); return _name; }
            set
            {
                if (_name != value)
                {
                    if (Category.CheckNameValid(value))
                    {
                        _name = value;

                        InvokePropertyChanged();
                    }
                    else
                    {
                        throw new ArgumentException("Must not be an empty string");
                    }
                }
            }
        }

        private string? _table_name = null;
        [JsonPropertyName("table_name")]
        public string TableName
        {
            get { Debug.Assert(_table_name is not null); return _table_name; }
            set
            {
                if (_table_name != value)
                {
                    _table_name = value;

                    InvokePropertyChanged();
                }
            }
        }

        private string? _keyTableColumnName = null;
        [JsonPropertyName("key_table_column_name")]
        public string KeyTableColumnName
        {
            get { Debug.Assert(_keyTableColumnName is not null); return _keyTableColumnName; }
            set
            {
                if (_keyTableColumnName != value)
                {
                    _keyTableColumnName = value;

                    InvokePropertyChanged();
                }
            }
        }

        private string? _symbolsTableColumnName = null;
        [JsonPropertyName("symbols_table_column_name")]
        public string SymbolsTableColumnName
        {
            get { Debug.Assert(_symbolsTableColumnName is not null); return _symbolsTableColumnName; }
            set
            {
                if (_symbolsTableColumnName != value)
                {
                    _symbolsTableColumnName = value;

                    InvokePropertyChanged();
                }
            }
        }

        private string? _footprintsTableColumnName = null;
        [JsonPropertyName("footprints_table_column_name")]
        public string FootprintsTableColumnName
        {
            get { Debug.Assert(_footprintsTableColumnName is not null); return _footprintsTableColumnName; }
            set
            {
                if (_footprintsTableColumnName != value)
                {
                    _footprintsTableColumnName = value;

                    InvokePropertyChanged();
                }
            }
        }



        private SymbolFieldMap? _selectedSymbolFieldMap = null;
        [JsonIgnore]
        public SymbolFieldMap? SelectedSymbolFieldMap
        {
            get { return _selectedSymbolFieldMap; }
            set
            {
                if (_selectedSymbolFieldMap != value)
                {
                    _selectedSymbolFieldMap = value;

                    InvokePropertyChanged();
                }
            }
        }

        private ObservableCollection<SymbolFieldMap>? _symbolFieldMaps = null;
        [JsonPropertyName("symbol_field_map")]
        public ObservableCollection<SymbolFieldMap> SymbolFieldMaps
        {
            get { Debug.Assert(_symbolFieldMaps is not null); return _symbolFieldMaps; }
            set
            {
                if (_symbolFieldMaps != value)
                {
                    if (_symbolFieldMaps is not null)
                        _symbolFieldMaps.CollectionChanged -= _symbolFieldMaps_CollectionChanged;
                    _symbolFieldMaps = value;
                    _symbolFieldMaps.CollectionChanged += _symbolFieldMaps_CollectionChanged;
                    _symbolFieldMaps_CollectionChanged(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));

                    InvokePropertyChanged();
                }
            }
        }

        private void _symbolFieldMaps_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
        }

        private string? _newSymbolFieldName = null;
        [JsonIgnore]
        public string NewSymbolFieldName
        {
            get { Debug.Assert(_newSymbolFieldName is not null); return _newSymbolFieldName; }
            set
            {
                if (_newSymbolFieldName != value)
                {
                    _newSymbolFieldName = value;

                    InvokePropertyChanged();
                }
            }
        }



        private SymbolBuiltInPropertiesMap? _symbolBuiltInPropertiesMap = null;
        [JsonPropertyName("symbol_built_in_properties_map")]
        public SymbolBuiltInPropertiesMap SymbolBuiltInPropertiesMap
        {
            get { Debug.Assert(_symbolBuiltInPropertiesMap is not null); return _symbolBuiltInPropertiesMap; }
            set
            {
                if (_symbolBuiltInPropertiesMap != value)
                {
                    _symbolBuiltInPropertiesMap = value;

                    InvokePropertyChanged();
                }
            }
        }

        /// <summary>
        /// Exists only to get the WPF designer to believe I can use this object as DataContext
        /// </summary>
        public Category()
        {
            Name = "<Category Name>";
            TableName = "";
            KeyTableColumnName = "";
            SymbolsTableColumnName = "";
            FootprintsTableColumnName = "";
            NewSymbolFieldName = ""; // Exists for binding to textbox so must start as not-null
            SymbolFieldMaps = new();
            SymbolBuiltInPropertiesMap = new();
        }

        public Category(string name) : this()
        {
            Name = name;
        }

        public Category(KiCADDBL_Library kiCADDBL_Library) : this()
        {
            if (kiCADDBL_Library.Name is not null) Name = kiCADDBL_Library.Name;
            if (kiCADDBL_Library.Table is not null) TableName = kiCADDBL_Library.Table;
            if (kiCADDBL_Library.Key is not null) KeyTableColumnName = kiCADDBL_Library.Key;
            if (kiCADDBL_Library.Symbols is not null) SymbolsTableColumnName = kiCADDBL_Library.Symbols;
            if (kiCADDBL_Library.Footprints is not null) FootprintsTableColumnName = kiCADDBL_Library.Footprints;
            if (kiCADDBL_Library.Fields is not null) SymbolFieldMaps = new(kiCADDBL_Library.Fields.Select(f => new SymbolFieldMap(f)));
            if (kiCADDBL_Library.Properties is not null) SymbolBuiltInPropertiesMap = new(kiCADDBL_Library.Properties);
        }

        public override string ToString()
        {
            return $"{Name}";
        }

        public void NewSymbolFieldMap()
        {
            string newSymbolFieldName;
            if (SymbolFieldMap.CheckNameValid(NewSymbolFieldName))
                newSymbolFieldName = NewSymbolFieldName;
            else
            {
                const string newSymbolFieldNamePrefix = $"Field ";
                const string regexPattern = @$"^{newSymbolFieldNamePrefix}\d+$";

                int currentMax = SymbolFieldMaps.Where(l => Regex.IsMatch(l.SymbolFieldName, regexPattern))
                    .Select(l => int.Parse(l.SymbolFieldName.Remove(0, newSymbolFieldNamePrefix.Length)))
                    .DefaultIfEmpty()
                    .Max();

                newSymbolFieldName = $"{newSymbolFieldNamePrefix}{currentMax + 1}";
            }

            int loc = 0;
            while (loc < SymbolFieldMaps.Count && SymbolFieldMaps[loc].SymbolFieldName.CompareTo(newSymbolFieldName) < 0) loc++;
            SymbolFieldMaps.Insert(loc, new(newSymbolFieldName));
        }

        public void DeleteSymbolFieldMap(SymbolFieldMap symbolFieldMap)
        {
            SymbolFieldMaps.Remove(symbolFieldMap);
        }
    }

    public class SymbolBuiltInPropertiesMap : NotifyObject
    {
        private bool? _useSymbolDescriptionTableColumnName = null;
        [JsonPropertyName("use_description_table_column_name")]
        public bool UseSymbolDescriptionTableColumnName
        {
            get { Debug.Assert(_useSymbolDescriptionTableColumnName is not null); return _useSymbolDescriptionTableColumnName.Value; }
            set
            {
                if (_useSymbolDescriptionTableColumnName != value)
                {
                    _useSymbolDescriptionTableColumnName = value;

                    InvokePropertyChanged();
                }
            }
        }


        private string? _symbolDescriptionTableColumnName = null;
        [JsonPropertyName("symbol_description_table_column_name")]
        public string SymbolDescriptionTableColumnName
        {
            get { Debug.Assert(_symbolDescriptionTableColumnName is not null); return _symbolDescriptionTableColumnName; }
            set
            {
                if (_symbolDescriptionTableColumnName != value)
                {
                    _symbolDescriptionTableColumnName = value;

                    InvokePropertyChanged();
                }
            }
        }


        private bool? _useSymbolKeywordsTableColumnName = null;
        [JsonPropertyName("use_symbol_keywords_table_column_name")]
        public bool UseSymbolKeywordsTableColumnName
        {
            get { Debug.Assert(_useSymbolKeywordsTableColumnName is not null); return _useSymbolKeywordsTableColumnName.Value; }
            set
            {
                if (_useSymbolKeywordsTableColumnName != value)
                {
                    _useSymbolKeywordsTableColumnName = value;

                    InvokePropertyChanged();
                }
            }
        }

        private string? _symbolKeywordsTableColumnName = null;
        [JsonPropertyName("symbol_keywords_table_column_name")]
        public string SymbolKeywordsTableColumnName
        {
            get { Debug.Assert(_symbolKeywordsTableColumnName is not null); return _symbolKeywordsTableColumnName; }
            set
            {
                if (_symbolKeywordsTableColumnName != value)
                {
                    _symbolKeywordsTableColumnName = value;

                    InvokePropertyChanged();
                }
            }
        }


        private bool? _useSymbolExcludeFromBomTableColumnName = null;
        [JsonPropertyName("use_symbol_exclude_from_bom_table_column_name")]
        public bool UseSymbolExcludeFromBomTableColumnName
        {
            get { Debug.Assert(_useSymbolExcludeFromBomTableColumnName is not null); return _useSymbolExcludeFromBomTableColumnName.Value; }
            set
            {
                if (_useSymbolExcludeFromBomTableColumnName != value)
                {
                    _useSymbolExcludeFromBomTableColumnName = value;

                    InvokePropertyChanged();
                }
            }
        }

        private string? _symbolExcludeFromBomTableColumnName = null;
        [JsonPropertyName("symbol_exclude_from_bom_table_column_name")]
        public string SymbolExcludeFromBomTableColumnName
        {
            get { Debug.Assert(_symbolExcludeFromBomTableColumnName is not null); return _symbolExcludeFromBomTableColumnName; }
            set
            {
                if (_symbolExcludeFromBomTableColumnName != value)
                {
                    _symbolExcludeFromBomTableColumnName = value;

                    InvokePropertyChanged();
                }
            }
        }


        private bool? _useSymbolExcludeFromBoardTableColumnName = null;
        [JsonPropertyName("use_symbol_exclude_from_board_table_column_name")]
        public bool UseSymbolExcludeFromBoardTableColumnName
        {
            get { Debug.Assert(_useSymbolExcludeFromBoardTableColumnName is not null); return _useSymbolExcludeFromBoardTableColumnName.Value; }
            set
            {
                if (_useSymbolExcludeFromBoardTableColumnName != value)
                {
                    _useSymbolExcludeFromBoardTableColumnName = value;

                    InvokePropertyChanged();
                }
            }
        }

        private string? _symbolExcludeFromBoardTableColumnName = null;
        [JsonPropertyName("symbol_exclude_from_board_table_column_name")]
        public string SymbolExcludeFromBoardTableColumnName
        {
            get { Debug.Assert(_symbolExcludeFromBoardTableColumnName is not null); return _symbolExcludeFromBoardTableColumnName; }
            set
            {
                if (_symbolExcludeFromBoardTableColumnName != value)
                {
                    _symbolExcludeFromBoardTableColumnName = value;

                    InvokePropertyChanged();
                }
            }
        }


        public SymbolBuiltInPropertiesMap()
        {
            UseSymbolDescriptionTableColumnName = true;
            SymbolDescriptionTableColumnName = "";
            UseSymbolKeywordsTableColumnName = false;
            SymbolKeywordsTableColumnName = "";
            UseSymbolExcludeFromBomTableColumnName = false;
            SymbolExcludeFromBomTableColumnName = "";
            UseSymbolExcludeFromBoardTableColumnName = false;
            SymbolExcludeFromBoardTableColumnName = "";
        }

        public SymbolBuiltInPropertiesMap(KiCADDBL_Library_Properties kiCADDBL_Library_Properties) : this()
        {
            if (kiCADDBL_Library_Properties.Description is not null)
            {
                UseSymbolDescriptionTableColumnName = true;
                SymbolDescriptionTableColumnName = kiCADDBL_Library_Properties.Description;
            }
            if (kiCADDBL_Library_Properties.Keywords is not null)
            {
                UseSymbolKeywordsTableColumnName = true;
                SymbolKeywordsTableColumnName = kiCADDBL_Library_Properties.Keywords;
            }
            if (kiCADDBL_Library_Properties.ExcludeFromBom is not null)
            {
                UseSymbolExcludeFromBomTableColumnName = true;
                SymbolExcludeFromBomTableColumnName = kiCADDBL_Library_Properties.ExcludeFromBom;
            }
            if (kiCADDBL_Library_Properties.ExcludeFromBoard is not null)
            {
                UseSymbolExcludeFromBoardTableColumnName = true;
                SymbolExcludeFromBoardTableColumnName = kiCADDBL_Library_Properties.ExcludeFromBoard;
            }
        }
    }
}
