﻿using System;
using System.Collections.ObjectModel;
using Shapeshifter.UserInterface.WindowsDesktop.Services.Interfaces;
using Shapeshifter.Core.Actions;
using System.ComponentModel;
using Shapeshifter.UserInterface.WindowsDesktop.Data.Interfaces;
using System.Collections.Generic;

namespace Shapeshifter.UserInterface.WindowsDesktop.Windows.ViewModels
{
    class ClipboardListViewModel : INotifyPropertyChanged
    {
        private IClipboardControlDataPackage selectedElement;
        private IAction selectedAction;

        private readonly IEnumerable<IAction> allActions;

        public ObservableCollection<IClipboardControlDataPackage> Elements { get; private set; }
        public ObservableCollection<IAction> Actions { get; private set; }

        public IAction SelectedAction
        {
            get
            {
                return selectedAction;
            }
            set
            {
                selectedAction = value;
                if (PropertyChanged != null)
                {
                    PropertyChanged(this, new PropertyChangedEventArgs(nameof(SelectedAction)));
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public ClipboardListViewModel(
            IEnumerable<IAction> allActions,
            IClipboardUserInterfaceMediator service)
        {
            Elements = new ObservableCollection<IClipboardControlDataPackage>();
            Actions = new ObservableCollection<IAction>();

            this.allActions = allActions;

            service.ControlAdded += Service_ControlAdded;
            service.ControlHighlighted += Service_ControlHighlighted;
            service.ControlPinned += Service_ControlPinned;
            service.ControlRemoved += Service_ControlRemoved;
        }

        public IClipboardControlDataPackage SelectedElement
        {
            get
            {
                return selectedElement;
            }
            set
            {
                selectedElement = value;
                if (PropertyChanged != null)
                {
                    PropertyChanged(this, new PropertyChangedEventArgs(nameof(SelectedElement)));
                }

                SetActions();
            }
        }

        private void SetActions()
        {
            Actions.Clear();
            SelectedAction = null;

            if (selectedElement != null)
            {
                foreach (var data in selectedElement.Contents)
                {
                    foreach (var action in allActions)
                    {
                        if (action.CanPerform(data))
                        {
                            Actions.Add(action);
                            if (SelectedAction == null)
                            {
                                SelectedAction = action;
                            }
                        }
                    }
                }
            }
        }

        private void Service_ControlRemoved(object sender, Services.Events.ControlEventArgument e)
        {
            Elements.Remove(e.Package);
        }

        private void Service_ControlPinned(object sender, Services.Events.ControlEventArgument e)
        {
            throw new NotImplementedException();
        }

        private void Service_ControlHighlighted(object sender, Services.Events.ControlEventArgument e)
        {
            Elements.Remove(e.Package);
            Elements.Insert(0, e.Package);
        }

        private void Service_ControlAdded(object sender, Services.Events.ControlEventArgument e)
        {
            Elements.Insert(0, e.Package);
            SelectedElement = e.Package;
        }
    }
}