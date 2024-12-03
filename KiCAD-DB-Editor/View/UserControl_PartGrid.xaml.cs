﻿using KiCAD_DB_Editor.ViewModel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Xml;

namespace KiCAD_DB_Editor.View
{
    /// <summary>
    /// Interaction logic for UserControl_PartGrid.xaml
    /// </summary>
    public partial class UserControl_PartGrid : UserControl
    {
        #region Dependency Properties

        public static readonly DependencyProperty ParameterVMsProperty = DependencyProperty.Register(
            nameof(ParameterVMs),
            typeof(ObservableCollectionEx<ParameterVM>),
            typeof(UserControl_PartGrid),
            new PropertyMetadata(new PropertyChangedCallback(ParameterVMsPropertyChangedCallback))
            );

        public ObservableCollectionEx<ParameterVM> ParameterVMs
        {
            get => (ObservableCollectionEx<ParameterVM>)GetValue(ParameterVMsProperty);
            set => SetValue(ParameterVMsProperty, value);
        }

        private static void ParameterVMsPropertyChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is UserControl_PartGrid uc_pg)
            {
                uc_pg.ParameterVMsPropertyChanged();
            }
        }

        public static readonly DependencyProperty PartVMsProperty = DependencyProperty.Register(
            nameof(PartVMs),
            typeof(ObservableCollectionEx<PartVM>),
            typeof(UserControl_PartGrid),
            new PropertyMetadata(new PropertyChangedCallback(PartVMsPropertyChangedCallback))
            );

        public ObservableCollectionEx<PartVM> PartVMs
        {
            get => (ObservableCollectionEx<PartVM>)GetValue(PartVMsProperty);
            set => SetValue(PartVMsProperty, value);
        }

        private static void PartVMsPropertyChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is UserControl_PartGrid uc_pg)
            {
                uc_pg.PartVMsPropertyChanged();
            }
        }

        private ObservableCollectionEx<ParameterVM>? oldParameterVMs = null;
        protected void ParameterVMsPropertyChanged()
        {
            if (oldParameterVMs is not null)
                oldParameterVMs.CollectionChanged -= ParameterVMs_CollectionChanged;
            oldParameterVMs = ParameterVMs;
            ParameterVMs.CollectionChanged += ParameterVMs_CollectionChanged;

            ParameterVMs_CollectionChanged(this, new(NotifyCollectionChangedAction.Reset));
        }

        private ObservableCollectionEx<ParameterVM>? oldParameterVMsCopy = null;
        private void ParameterVMs_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            if (oldParameterVMsCopy is not null)
            {
                foreach (ParameterVM pVM in oldParameterVMsCopy)
                    pVM.PropertyChanged -= ParameterVM_PropertyChanged;
            }
            oldParameterVMsCopy = new(ParameterVMs);
            foreach (ParameterVM pVM in oldParameterVMsCopy)
                pVM.PropertyChanged += ParameterVM_PropertyChanged;

            redoColumns();
        }

        private void ParameterVM_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            // TODO At some point should check that renaming the parameter doesn't actually break things
            if (e.PropertyName == nameof(ParameterVM.Name))
                redoColumns();
        }

        private ObservableCollectionEx<PartVM>? oldPartVMs = null;
        protected void PartVMsPropertyChanged()
        {
            if (oldPartVMs is not null)
                oldPartVMs.CollectionChanged -= PartVMs_CollectionChanged;
            PartVMs.CollectionChanged += PartVMs_CollectionChanged;
            oldPartVMs = PartVMs;

            PartVMs_CollectionChanged(this, new(NotifyCollectionChangedAction.Reset));
        }

        private ObservableCollectionEx<PartVM>? oldPartVMsCopy = null;
        private void PartVMs_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            if (oldPartVMsCopy is not null)
            {
                foreach (PartVM pVM in oldPartVMsCopy)
                    pVM.PropertyChanged -= PartVM_PropertyChanged;
            }
            oldPartVMsCopy = new(PartVMs);
            foreach (PartVM pVM in oldPartVMsCopy)
                pVM.PropertyChanged += PartVM_PropertyChanged;
            redoColumns();
        }

        private void PartVM_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "ParametersChanged")
                redoColumns();
        }

        #endregion

        private void redoColumns()
        {
            if (ParameterVMs is not null && PartVMs is not null)
            {
                while (dataGrid_Main.Columns.Count > 8)
                    dataGrid_Main.Columns.RemoveAt(8);

                DataGridTextColumn dataGridTextColumn;
                foreach (ParameterVM parameterVM in ParameterVMs)
                {
                    dataGridTextColumn = new();
                    dataGridTextColumn.Header = parameterVM.Name;
                    dataGridTextColumn.Binding = new Binding($"[{parameterVM.Name}]");
                    dataGridTextColumn.Binding.TargetNullValue = "\x7F";
                    dataGrid_Main.Columns.Add(dataGridTextColumn);
                }
            }
            else
            {
                while (dataGrid_Main.Columns.Count > 8)
                    dataGrid_Main.Columns.RemoveAt(8);
            }
        }

        public UserControl_PartGrid()
        {
            InitializeComponent();
        }
    }
}
