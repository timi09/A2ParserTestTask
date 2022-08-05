using A2ParserTestTask.Data;
using A2ParserTestTask.Data.Models;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace A2ParserTestTask
{
    internal class Program
    {
        static void Main(string[] args)
        {
            string connectionString = @"Data Source=(LocalDB)\MSSQLLocalDB;AttachDbFilename=C:\Users\Evgeniy\source\repos\A2ParserTestTask\Data\Database.mdf;Integrated Security=True";

            Database database = new Database(connectionString);

            Parser parser = new Parser(database);

            parser.Start();

            Console.ReadLine();

            parser.Stop();

        }
    }
}
