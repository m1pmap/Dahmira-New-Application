﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace Dahmira.Interfaces
{
    public interface IFolderPath
    {
        void SelectedFolderPathToTextBox(TextBox textBox); //Отображение выбранного пути в выбраном textBox
    }
}
