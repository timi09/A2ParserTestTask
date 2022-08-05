using A2ParserTestTask.Data.Models;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static A2ParserTestTask.Parser;

namespace A2ParserTestTask.Data
{
    public class Database
    {
        private string ConnectionString = null;

        private SqlConnection SQLConnection = null;

        private string СontractorsTableName = "Сontractors";
        private string DealsTableName = "Deals";

        public Database(string ConnectionString)
        {
            this.ConnectionString = ConnectionString;
            Connect();

            List<string> tableNames = GetTables();

            if(!tableNames.Contains(СontractorsTableName))
                CreateСontractorsTable();

            if (!tableNames.Contains(DealsTableName))
                CreateDealsTable();
        }

        public void Connect()
        {
            SQLConnection = new SqlConnection(ConnectionString);
            SQLConnection.Open();
        }

        public void Disconnect()
        {
            SQLConnection = null;
            SQLConnection.Close(); ;
        }

        public int CreateDeal(DealModel deal)
        {
            string commandString = $"INSERT INTO {DealsTableName}(Number, Date, VolumeBuyer, VolumeSeller, BuyerId, SellerId) " +
                $"VALUES ('{deal.Number}', '{deal.Date}', {deal.VolumeBuyer.ToString(new CultureInfo("en-us", false))}, {deal.VolumeSeller.ToString(new CultureInfo("en-us", false))}, {deal.BuyerId}, {deal.SellerId})\n" +
                "SELECT NewId = SCOPE_IDENTITY();";

            var results = SelectCommand(commandString);

            return Convert.ToInt32(results[0]["NewId"]);
        }

        public DealModel ReadDeal(DealModel deal) 
        {
            string commandString = $"SELECT * FROM {DealsTableName} " +
                $"WHERE Number = '{deal.Number}' AND Date = '{deal.Date}'";

            var results = SelectCommand(commandString);

            if (results.Count == 0)
                return null;

            DealModel findedDeal = new DealModel()
            {
                Id = Convert.ToInt32(results[0]["Id"]),
                Number = Convert.ToString(results[0]["Number"]),
                Date = Convert.ToString(results[0]["Date"]),
                VolumeBuyer = Convert.ToDouble(results[0]["VolumeBuyer"]),
                VolumeSeller = Convert.ToDouble(results[0]["VolumeSeller"]),
                BuyerId = Convert.ToInt32(results[0]["BuyerId"]),
                SellerId = Convert.ToInt32(results[0]["SellerId"]),
            };

            return findedDeal;
        }

        public void UpdateDeal(int Id, DealModel deal)
        {
            string commandString = $"UPDATE {DealsTableName} " +
                $"SET VolumeBuyer = {deal.VolumeBuyer.ToString(new CultureInfo("en-us", false))}, VolumeSeller = {deal.VolumeSeller.ToString(new CultureInfo("en-us", false))} " +
                $"WHERE Id = {Id}";

            NonQueryCommand(commandString);
        }

        public int CreateСontractor(ContractorModel contractor) 
        {
            string commandString = $"INSERT INTO {СontractorsTableName}(Name, INN) " +
                                $"VALUES (N'{contractor.Name.Replace("'", "")}', '{contractor.INN}')\n" +
                                "SELECT NewId = SCOPE_IDENTITY();";

            var results = SelectCommand(commandString);

            return Convert.ToInt32(results[0]["NewId"]);
        }

        public ContractorModel ReadСontractor(ContractorModel contractor)
        {
            string commandString = $"SELECT * FROM {СontractorsTableName} " +
                $"WHERE INN = '{contractor.INN}'";

            var results = SelectCommand(commandString);

            if (results.Count == 0)
                return null;

            ContractorModel findedContractor = new ContractorModel()
            {
                Id = Convert.ToInt32(results[0]["Id"]),
                Name = Convert.ToString(results[0]["Name"]),
                INN = Convert.ToString(results[0]["INN"]),
            };

            return findedContractor;
        }

        public void UpdateContractor(int Id, ContractorModel contractor) 
        {
            string commandString = $"UPDATE {СontractorsTableName} SET Name = N'{contractor.Name.Replace("'", "")}' WHERE Id = {Id}";

            NonQueryCommand(commandString);
        }

        public List<string> GetTables()
        {
            string commandString = "SELECT name FROM sys.Tables";
            var tables = SelectCommand(commandString);
            return tables.Select(x => x["name"].ToString()).ToList();
        }

        private void CreateСontractorsTable() 
        {
            string commandString = $"CREATE TABLE [dbo].[{СontractorsTableName}]" +
                                    "(" +
                                        "[Id] INT NOT NULL IDENTITY PRIMARY KEY," +
                                        "[Name] NVARCHAR(300) NOT NULL,"+
                                        "[INN] NVARCHAR(12) NOT NULL,"+
                                        "CONSTRAINT [AK_INN] UNIQUE ([INN])" +
                                    ")";

            NonQueryCommand(commandString);
        }

        private void CreateDealsTable()
        {
            string commandString = $"CREATE TABLE [dbo].[{DealsTableName}]" +
                                    "(" +
                                        "[Id] INT NOT NULL IDENTITY PRIMARY KEY," +
                                        "[Number] NVARCHAR(50) NOT NULL," +
                                        "[Date] NVARCHAR(10) NOT NULL," +
                                        "[VolumeBuyer] FLOAT NOT NULL," +
                                        "[VolumeSeller] FLOAT NOT NULL," +
                                        "[BuyerId] INT NOT NULL," +
                                        "[SellerId] INT NOT NULL," +
                                        "CONSTRAINT [AK_Number_Date] UNIQUE ([Number], [Date])," +
                                        "CONSTRAINT [FK_Deal_Buyer] FOREIGN KEY([BuyerId]) REFERENCES[Сontractors]([Id])," +
                                        "CONSTRAINT [FK_Deal_Seller] FOREIGN KEY([SellerId]) REFERENCES[Сontractors]([Id])" +
                                    ")";

            NonQueryCommand(commandString);
        }

        private List<Dictionary<string, object>> SelectCommand(string selectCommandString)
        {
            SqlCommand Comand = new SqlCommand(selectCommandString, SQLConnection);

            SqlDataReader SQLReader = null;

            try
            {
                SQLReader = Comand.ExecuteReader();

                List<Dictionary<string, object>> tableRows = new List<Dictionary<string, object>>();
                if (SQLReader.HasRows)
                {
                    while (SQLReader.Read())
                    {
                        Dictionary<string, object> row = new Dictionary<string, object>();
                        for (int i = 0; i < SQLReader.FieldCount; i++)
                        {
                            object Value = SQLReader[i];
                            row.Add(SQLReader.GetName(i), Value);
                        }
                        tableRows.Add(row);
                    }
                }

                return tableRows;
            }
            catch (Exception exeption)
            {
                throw exeption;
            }
            finally
            {
                if (SQLReader != null)
                {
                    SQLReader.Close();
                }
            }
        }

        private void NonQueryCommand(string commandString) 
        {
            SqlCommand Comand = new SqlCommand(commandString, SQLConnection);
            try
            {
                Comand.ExecuteNonQuery();
            }
            catch (Exception exeption)
            {
                throw exeption;
            }

        }

        //public async Task<List<MSSQLObject>> CommitCommand(string CommandString, Dictionary<string, object> Parameters = null, Func<Exception, object> ExeptionFunction = null)
        //{
        //    SqlCommand Comand = new SqlCommand(CommandString, SQLConnection);

            //    if (Parameters != null)
            //    {
            //        foreach (var Key in Parameters.Keys)
            //        {
            //            Comand.Parameters.AddWithValue(Key, Parameters[Key]);
            //        }

            //    }

            //    if (CommandString.Contains("SELECT"))
            //    {
            //        if (CommandString.Contains("["))
            //        {
            //            string TableName = CommandString.Split(new string[] { "FROM" }, StringSplitOptions.None)[1].Split('[')[1].Split(']')[0];
            //        }


            //        SqlDataReader SQLReader = null;

            //        List<MSSQLObject> MSSQLObjectObjects = new List<MSSQLObject>();

            //        try
            //        {
            //            SQLReader = Comand.ExecuteReader();

            //            if (SQLReader.HasRows)
            //            {
            //                while (SQLReader.Read())
            //                {

            //                    MSSQLObject MSSQLObject = new MSSQLObject();

            //                    for (int i = 0; i < SQLReader.FieldCount; i++)
            //                    {
            //                        string Value = Convert.ToString(SQLReader[i]).Trim();
            //                        MSSQLObject.Properties.Add(SQLReader.GetName(i), Value);

            //                    }
            //                    MSSQLObjectObjects.Add(MSSQLObject);
            //                }
            //            }

            //        }
            //        catch (Exception Exeption)
            //        {
            //            if (ExeptionFunction == null)
            //            {
            //                Console.Write(Exeption.Message.ToString());
            //            }
            //            else
            //            {
            //                ExeptionFunction(Exeption);
            //            }

            //        }
            //        finally
            //        {
            //            if (SQLReader != null)
            //            {
            //                SQLReader.Close();
            //            }
            //        }

            //        return MSSQLObjectObjects;

            //    }
            //    else
            //    {
            //        try
            //        {
            //            Comand.ExecuteNonQuery();
            //        }
            //        catch (Exception e)
            //        {
            //            //MessageBox.Show("An error occurred: " + e.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            //        }

            //        return null;
            //    }

            //}


            //public async void DeleteLast(MSSQLObject Object, Func<Exception, object> ExeptionFunction = null)
            //{
            //    string DeleteCommand = "DELETE TOP 1 FROM [" + Object.Table + "] WHERE ";

            //    bool Flag = false;
            //    foreach (var Key in Object.Properties.Keys)
            //    {
            //        if (Flag)
            //        {
            //            DeleteCommand += " AND [" + Key + "] = @" + Key;
            //        }
            //        else
            //        {
            //            DeleteCommand += "[" + Key + "] = @" + Key;
            //            Flag = true;
            //        }
            //    }

            //    await CommitCommand(DeleteCommand, Object.Properties, ExeptionFunction);

            //}


    }
}
