using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Domain;

namespace DataBaseInitializer
{
    class Program
    {
        static void Main(string[] args)
        {
            ITagDataBase tagDB = new TagDataBase();

            // Initializes DataBase
            tagDB.SetUpDataBase();
        }
    }
}
