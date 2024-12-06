﻿using KiCAD_DB_Editor.Model;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace KiCAD_DB_Editor.ViewModel
{
    public class PartVM : NotifyObject
    {
        public ParameterAccessor ParameterAccessor { get; }
        public FootprintLibraryNameAccessor FootprintLibraryNameAccessor { get; }
        public FootprintNameAccessor FootprintNameAccessor { get; }
        public readonly Model.Part Part;
        public readonly LibraryVM ParentLibraryVM;

        #region Notify Properties

        public string PartUID
        {
            get { return Part.PartUID; }
            set { if (Part.PartUID != value) { Part.PartUID = value; InvokePropertyChanged(); } }
        }
        public string Description
        {
            get { return Part.Description; }
            set { if (Part.Description != value) { Part.Description = value; InvokePropertyChanged(); } }
        }
        public string Manufacturer
        {
            get { return Part.Manufacturer; }
            set { if (Part.Manufacturer != value) { Part.Manufacturer = value; InvokePropertyChanged(); } }
        }
        public string MPN
        {
            get { return Part.MPN; }
            set { if (Part.MPN != value) { Part.MPN = value; InvokePropertyChanged(); } }
        }
        public string Value
        {
            get { return Part.Value; }
            set { if (Part.Value != value) { Part.Value = value; InvokePropertyChanged(); } }
        }
        public bool ExcludeFromBOM
        {
            get { return Part.ExcludeFromBOM; }
            set { if (Part.ExcludeFromBOM != value) { Part.ExcludeFromBOM = value; InvokePropertyChanged(); } }
        }
        public bool ExcludeFromBoard
        {
            get { return Part.ExcludeFromBoard; }
            set { if (Part.ExcludeFromBoard != value) { Part.ExcludeFromBoard = value; InvokePropertyChanged(); } }
        }
        public bool ExcludeFromSim
        {
            get { return Part.ExcludeFromSim; }
            set { if (Part.ExcludeFromSim != value) { Part.ExcludeFromSim = value; InvokePropertyChanged(); } }
        }
        public string SymbolLibraryName
        {
            get { return Part.SymbolLibraryName; }
            set
            {
                if (Part.SymbolLibraryName != value)
                {
                    Part.SymbolLibraryName = value;
                    InvokePropertyChanged();
                    InvokePropertyChanged(nameof(this.SelectedKiCADSymbolLibraryVM));
                }
            }
        }
        public string SymbolName
        {
            get { return Part.SymbolName; }
            set { if (Part.SymbolName != value) { Part.SymbolName = value; InvokePropertyChanged(); } }
        }

        public ParameterVM[] ParameterVMs
        {
            get
            {
                var keys = Part.ParameterValues.Keys;
                return ParentLibraryVM.ParameterVMs.Where(pVM => keys.Contains(pVM.Parameter)).ToArray();
            }
        }

        public int FootprintCount
        {
            get { return Part.FootprintLibraryNames.Count; }
        }

        // Included so the KiCAD symbol name drop down has a source
        public KiCADSymbolLibraryVM? SelectedKiCADSymbolLibraryVM
        {
            // Have to do ! as FirstOrDefault needs to think kSLVM could be null in order for me to return null
            get { return ParentLibraryVM.KiCADSymbolLibraryVMs.FirstOrDefault(kSLVM => kSLVM!.Nickname == SymbolLibraryName, null); }
        }

        // Included so I can get a category string for the all parts grid
        private CategoryVM? _parentCategoryVM;
        public CategoryVM? ParentCategoryVM
        {
            get { return _parentCategoryVM; }
            set { if (_parentCategoryVM != value) { _parentCategoryVM = value; InvokePropertyChanged(); } }
        }

        #endregion Notify Properties

        public PartVM(LibraryVM parentLibraryVM, Model.Part part)
        {
            ParentLibraryVM = parentLibraryVM;

            // Link model
            Part = part;

            ParameterAccessor = new(this);
            FootprintLibraryNameAccessor = new(this);
            FootprintNameAccessor = new(this);
        }

        public void AddParameterVM(ParameterVM pVM)
        {
            if (!Part.ParameterValues.ContainsKey(pVM.Parameter))
            {
                Part.ParameterValues[pVM.Parameter] = "";
                InvokePropertyChanged(nameof(ParameterVMs));
            }
        }

        public void RemoveParameterVM(ParameterVM pVM)
        {
            if (Part.ParameterValues.ContainsKey(pVM.Parameter))
            {
                Part.ParameterValues.Remove(pVM.Parameter);
                InvokePropertyChanged(nameof(ParameterVMs));
            }
        }

        public void AddFootprint()
        {
            // Always needs to be done in tandem
            Part.FootprintLibraryNames.Add("");
            Part.FootprintNames.Add("");
            InvokePropertyChanged(nameof(FootprintCount));
        }

        public void RemoveFootprint()
        {
            // Always needs to be done in tandem
            Part.FootprintLibraryNames.RemoveAt(Part.FootprintLibraryNames.Count - 1);
            Part.FootprintNames.RemoveAt(Part.FootprintNames.Count - 1);
            InvokePropertyChanged(nameof(FootprintCount));
        }

        #region Commands



        #endregion Commands

    }

    public class ParameterAccessor : NotifyObject
    {
        public readonly PartVM OwnerPartVM;

        #region Notify Properties

        public string? this[string parameterVMName]
        {
            get
            {
                var key = OwnerPartVM.ParentLibraryVM.ParameterVMs.First(pVM => pVM.Name == parameterVMName).Parameter;
                if (OwnerPartVM.Part.ParameterValues.TryGetValue(key, out string? val))
                    return val;
                else
                    return null;
            }
            set
            {
                if (value is not null)
                {
                    var key = OwnerPartVM.ParentLibraryVM.ParameterVMs.First(pVM => pVM.Name == parameterVMName).Parameter;
                    if (OwnerPartVM.Part.ParameterValues.TryGetValue(key, out string? s))
                    {
                        if (s != value)
                        {
                            OwnerPartVM.Part.ParameterValues[key] = value;
                            InvokePropertyChanged("Item[]");
                        }
                    }
                }
            }
        }

        #endregion Notify Properties

        public ParameterAccessor(PartVM ownerPVM)
        {
            OwnerPartVM = ownerPVM;
        }
    }

    public class FootprintLibraryNameAccessor : NotifyObject
    {
        public readonly PartVM OwnerPartVM;

        #region Notify Properties

        public string? this[int index]
        {
            get
            {
                if (OwnerPartVM.Part.FootprintLibraryNames.Count > index)
                    return OwnerPartVM.Part.FootprintLibraryNames[index];
                else
                    return null;
            }
            set
            {
                if (value is not null)
                {
                    if (OwnerPartVM.Part.FootprintLibraryNames.Count > index)
                        OwnerPartVM.Part.FootprintLibraryNames[index] = value;
                }
            }
        }

        #endregion Notify Properties

        public FootprintLibraryNameAccessor(PartVM ownerPVM)
        {
            OwnerPartVM = ownerPVM;
        }
    }

    public class FootprintNameAccessor : NotifyObject
    {
        public readonly PartVM OwnerPartVM;

        #region Notify Properties

        public string? this[int index]
        {
            get
            {
                if (OwnerPartVM.Part.FootprintNames.Count > index)
                    return OwnerPartVM.Part.FootprintNames[index];
                else
                    return null;
            }
            set
            {
                if (value is not null)
                {
                    if (OwnerPartVM.Part.FootprintNames.Count > index)
                        OwnerPartVM.Part.FootprintNames[index] = value;
                }
            }
        }

        #endregion Notify Properties

        public FootprintNameAccessor(PartVM ownerPVM)
        {
            OwnerPartVM = ownerPVM;
        }
    }
}
