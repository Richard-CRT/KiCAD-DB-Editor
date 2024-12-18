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
        // Duplicated between LibraryVM and CategoryVM so we move the checks here
        public static bool AddFootprintCommandCanExecute(IEnumerable<PartVM> partVMsToAddFootprintTo)
        {
            return partVMsToAddFootprintTo.Count() > 0;
        }

        // Duplicated between LibraryVM and CategoryVM so we move the checks here
        public static void AddFootprintCommandExecuted(IEnumerable<PartVM> partVMsToAddFootprintTo)
        {
            Debug.Assert(partVMsToAddFootprintTo.Count() > 0);
            foreach (PartVM pVM in partVMsToAddFootprintTo)
                pVM.AddFootprint();
        }

        // Duplicated between LibraryVM and CategoryVM so we move the checks here
        public static bool RemoveFootprintCommandCanExecute(IEnumerable<PartVM> partVMsToRemoveFootprintsFrom)
        {
            return partVMsToRemoveFootprintsFrom.Count() > 0 && partVMsToRemoveFootprintsFrom.All(pVM => pVM.FootprintCount > 0);
        }

        // Duplicated between LibraryVM and CategoryVM so we move the checks here
        public static void RemoveFootprintCommandExecuted(IEnumerable<PartVM> partVMsToRemoveFootprintsFrom)
        {
            Debug.Assert(partVMsToRemoveFootprintsFrom.Count() > 0);
            foreach (PartVM pVM in partVMsToRemoveFootprintsFrom)
                pVM.RemoveFootprint();
        }

        // ======================================================================

        public ParameterAccessor ParameterAccessor { get; }
        public FootprintLibraryNameAccessor FootprintLibraryNameAccessor { get; }
        public FootprintNameAccessor FootprintNameAccessor { get; }
        public SelectedFootprintLibraryVMAccessor SelectedFootprintLibraryVMAccessor { get; }
        public readonly Model.Part Part;
        public readonly LibraryVM ParentLibraryVM;

        // Included so I can get a category string for the all parts grid
        // It has to have a setter since I can't provide parentCategoryVM at instantiation time, but this should never be changed
        // For that reason does not InvokePropertyChanged
        public CategoryVM? ParentCategoryVM { get; set; }

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
        public string Datasheet
        {
            get { return Part.Datasheet; }
            set { if (Part.Datasheet != value) { Part.Datasheet = value; InvokePropertyChanged(); } }
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

                    // Doesn't seem to be technically required as the bindings for the ComboBoxes I'm designing this for only load
                    // when the cells are edited, but if not then I'd need to do this to prompt the ComboBoxes to refetch the value
                    // On future investigation, it's clear that I can't switch to a system where the ComboBoxes are persistent. WPF is
                    // weird: when I clear the SelectedKiCADSymbolLibraryVM, the available items in the symbol name should be blank
                    // and it does do this, but if the current text is one of those items, it will get cleared, which is not what
                    // I want at all
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
                return ParentLibraryVM.ParameterVMs.Where(pVM => keys.Contains(pVM.UUID)).ToArray();
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

        #endregion Notify Properties

        public PartVM(LibraryVM parentLibraryVM, Model.Part part)
        {
            ParentLibraryVM = parentLibraryVM;

            // Link model
            Part = part;

            ParameterAccessor = new(this);
            FootprintLibraryNameAccessor = new(this);
            FootprintNameAccessor = new(this);
            SelectedFootprintLibraryVMAccessor = new(this);
        }

        public void AddParameterVM(ParameterVM pVM)
        {
            if (!Part.ParameterValues.ContainsKey(pVM.UUID))
            {
                Part.ParameterValues[pVM.UUID] = "";
                InvokePropertyChanged(nameof(ParameterVMs));

                // This is needed to update the table's existing cells
                ParameterAccessor.InvokePropertyChanged("Item[]");
            }
        }

        public void RemoveParameterVM(ParameterVM pVM)
        {
            if (Part.ParameterValues.ContainsKey(pVM.UUID))
            {
                Part.ParameterValues.Remove(pVM.UUID);
                InvokePropertyChanged(nameof(ParameterVMs));

                // This is needed to update the table's existing cells
                ParameterAccessor.InvokePropertyChanged("Item[]");
            }
        }

        public void AddFootprint()
        {
            // Always needs to be done in tandem
            Part.FootprintLibraryNames.Add("");
            Part.FootprintNames.Add("");

            // Have to do this one to tell the table it might have to redo its columns
            InvokePropertyChanged(nameof(FootprintCount));

            // These 2 are needed to update the table's existing cells
            FootprintNameAccessor.InvokePropertyChanged("Item[]");
            FootprintLibraryNameAccessor.InvokePropertyChanged("Item[]");
        }

        public void RemoveFootprint()
        {
            // Always needs to be done in tandem
            Part.FootprintLibraryNames.RemoveAt(Part.FootprintLibraryNames.Count - 1);
            Part.FootprintNames.RemoveAt(Part.FootprintNames.Count - 1);

            // Have to do this one to tell the table it might have to redo its columns
            InvokePropertyChanged(nameof(FootprintCount));

            // These 2 are needed to update the table's existing cells
            FootprintNameAccessor.InvokePropertyChanged("Item[]");
            FootprintLibraryNameAccessor.InvokePropertyChanged("Item[]");
        }

        #region Commands



        #endregion Commands

    }

    public class ParameterAccessor : NotifyObject
    {
        public readonly PartVM OwnerPartVM;

        #region Notify Properties

        public string? this[string parameterUUID]
        {
            get
            {
                if (OwnerPartVM.Part.ParameterValues.TryGetValue(parameterUUID, out string? val))
                    return val;
                else
                    return null;
            }
            set
            {
                if (value is not null)
                {
                    if (OwnerPartVM.Part.ParameterValues.TryGetValue(parameterUUID, out string? s))
                    {
                        if (s != value)
                        {
                            OwnerPartVM.Part.ParameterValues[parameterUUID] = value;
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
                    {
                        OwnerPartVM.Part.FootprintLibraryNames[index] = value;
                        InvokePropertyChanged("Item[]");

                        // Doesn't seem to be technically required as the bindings for the ComboBoxes I'm designing this for only load
                        // when the cells are edited, but if not then I'd need to do this to prompt the ComboBoxes to refetch the value
                        // On future investigation, it's clear that I can't switch to a system where the ComboBoxes are persistent. WPF is
                        // weird: when I clear the SelectedKiCADFootprintLibraryVM, the available items in the footprint name should be blank
                        // and it does do this, but if the current text is one of those items, it will get cleared, which is not what
                        // I want at all
                        OwnerPartVM.SelectedFootprintLibraryVMAccessor.InvokePropertyChanged("Item[]");
                    }
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
                    {
                        OwnerPartVM.Part.FootprintNames[index] = value;
                        InvokePropertyChanged("Item[]");
                    }
                }
            }
        }

        #endregion Notify Properties

        public FootprintNameAccessor(PartVM ownerPVM)
        {
            OwnerPartVM = ownerPVM;
        }
    }

    public class SelectedFootprintLibraryVMAccessor : NotifyObject
    {
        public readonly PartVM OwnerPartVM;

        #region Notify Properties

        public KiCADFootprintLibraryVM? this[int index]
        {
            get
            {
                if (OwnerPartVM.Part.FootprintLibraryNames.Count > index)
                    // Have to do ! as FirstOrDefault needs to think kSLVM could be null in order for me to return null
                    return OwnerPartVM.ParentLibraryVM.KiCADFootprintLibraryVMs.FirstOrDefault(kFLVM => kFLVM!.Nickname == OwnerPartVM.Part.FootprintLibraryNames[index], null);
                else
                    return null;
            }
        }

        #endregion Notify Properties

        public SelectedFootprintLibraryVMAccessor(PartVM ownerPVM)
        {
            OwnerPartVM = ownerPVM;
        }
    }
}
