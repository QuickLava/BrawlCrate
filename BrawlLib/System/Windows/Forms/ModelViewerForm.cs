﻿namespace System.Windows.Forms
{
    public partial class ModelViewerForm : ThemedForm
    {
        public ModelEditorBase _mainWindow;

        public ModelViewerForm(ModelEditorBase mainWindow)
        {
            InitializeComponent();
            TopMost = true;
        }
    }
}