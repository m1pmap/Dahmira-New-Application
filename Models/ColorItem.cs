using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dahmira.Models
{
    public class ColorItem
    {
        public string Name { get; set; } //Название цвета
        public string ColorHex { get; set; } //HEX

        public ColorItem() { }
        public ColorItem(string name, Color color)
        {
            Name = name;
            ColorHex = ColorToHex(color);
           
        }

        public Color GetColor() //Получение цвета из HEX
        {
            return ColorTranslator.FromHtml(ColorHex);
        }

        private string ColorToHex(Color color) //Цвет в HEX
        {
            return $"#{color.R:X2}{color.G:X2}{color.B:X2}";
        }
    }
}
