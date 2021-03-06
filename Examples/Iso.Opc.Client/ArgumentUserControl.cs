﻿using System.Windows.Forms;
using Opc.Ua;

namespace Iso.Opc.Client
{
    public partial class ArgumentUserControl : UserControl
    {
        #region Properties
        /// <summary>
        /// Casts a value to the specified target type and return the object value
        /// </summary>
        public object ValueInput
        {
            get => string.IsNullOrEmpty(valueInputTextBox.Text)
                ? TypeInfo.Cast("0", TypeInfo.BuiltInType)
                : TypeInfo.Cast(valueInputTextBox.Text, TypeInfo.BuiltInType);
            set => valueInputTextBox.SetTextThreadSafe(value.ToString());
        }
        public TypeInfo TypeInfo { get; private set; }
        #endregion

        public ArgumentUserControl()
        {
            InitializeComponent();
        }

        #region Methods
        public void Initialise(string value, string description, string name, TypeInfo typeInfo)
        {
            TypeInfo = typeInfo;
            valueInputTextBox.Text = string.IsNullOrEmpty(value)?"0":value;
            inputArgumentDescriptionLabel.Text = $"Description: {description}";
            inputArgumentNameLabel.Text = $"{name}:";
            inputArgumentTypeLabel.Text = $"{typeInfo.BuiltInType.ToString()}";
        }
        #endregion
    }
}
