using A2ParserTestTask.Data;
using System;

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
