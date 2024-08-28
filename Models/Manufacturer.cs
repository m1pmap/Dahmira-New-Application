using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dahmira.Models
{
    public class Manufacturer
    {
        public string name { get; set; } //Имя страны

        public override bool Equals(object obj)
        {
            return Equals(obj as Manufacturer);
        }

        public bool Equals(Manufacturer other)
        {
            return other != null && name == other.name; //Сравниваем по имени
        }

        public override int GetHashCode()
        {
            return name.GetHashCode(); //Используем хэш-код имени
        }
    }
}
