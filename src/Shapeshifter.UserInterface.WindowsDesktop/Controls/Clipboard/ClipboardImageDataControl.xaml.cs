﻿#region

using System.Windows.Controls;
using Shapeshifter.UserInterface.WindowsDesktop.Controls.Clipboard.Interfaces;

#endregion

namespace Shapeshifter.UserInterface.WindowsDesktop.Controls.Clipboard
{
    /// <summary>
    ///     Interaction logic for ClipboardImageDataControl.xaml
    /// </summary>
    public partial class ClipboardImageDataControl : UserControl, IClipboardImageDataControl
    {
        public ClipboardImageDataControl()
        {
            InitializeComponent();
        }
    }
}