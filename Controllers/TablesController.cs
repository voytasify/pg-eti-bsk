﻿using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using DAC.Models;

namespace DAC.Controllers
{
    public class TableDataContainer
    {
        public List<ColumnInfo> ColumnsInfo { get; set; }
        public List<List<object>> Values { get; set; }
        public string TableName { get; set; }

        public TableDataContainer(List<ColumnInfo> columnsInfo, List<List<object>> values, string tableName)
        {
            this.ColumnsInfo = columnsInfo;
            this.Values = values;
            this.TableName = tableName;
        }
    }

    public class TablesController : Controller
    {
        //command to retrieve all table names
        private string getTableNamesCmdText = @"SELECT T.[NAME] AS [table_name] FROM sys.[tables] AS T WHERE t.[is_ms_shipped] = 0";

        //command to retrieve columns and datatypes of given table
        private string getColumnsInfoCmdText = @"SELECT AC.[Name] AS [column_name], TY.[Name] AS system_data_type, AC.[max_length], AC.[is_nullable]
            FROM sys.[tables] AS T   
            INNER JOIN sys.[all_columns] AC ON T.[object_id] = AC.[object_id]  
            INNER JOIN sys.[types] TY ON AC.[system_type_id] = TY.[system_type_id] AND AC.[user_type_id] = TY.[user_type_id] 
            WHERE T.[Name] = @Name 
            ORDER BY AC.[column_id];";

        //command to retrieve rows
        private string getRowsCmdText = @"SELECT * FROM ";

        //connection string
        private string connectionStr = ConfigurationManager.ConnectionStrings["MainDbConnectionString"].ConnectionString;

        //get names of all available tables
        private List<string> GetTableNames()
        {
            List<string> tableNames = new List<string>();

            using (SqlConnection connection = new SqlConnection(connectionStr))
            {
                SqlCommand cmd = new SqlCommand(getTableNamesCmdText, connection);
                SqlDataReader reader;

                try
                {
                    cmd.Connection.Open();
                    reader = cmd.ExecuteReader();
                }
                catch (SqlException)
                {
                    //TO DO: handle errors
                    return tableNames;
                }

                //populate list of table names
                if (reader.HasRows)
                {
                    while (reader.Read())
                    {
                        tableNames.Add(reader.GetString(0));
                    }
                }

                reader.Close();
            }

            return tableNames;
        }

        //get column names and datatypes of given table
        private List<ColumnInfo> GetColumnsInfo(string tableName)
        {
            List<ColumnInfo> columnsInfo = new List<ColumnInfo>();

            using (SqlConnection connection = new SqlConnection(connectionStr))
            {
                SqlCommand cmd = new SqlCommand(getColumnsInfoCmdText, connection);
                cmd.Parameters.AddWithValue("@Name", tableName);
                SqlDataReader reader;

                try
                {
                    cmd.Connection.Open();
                    reader = cmd.ExecuteReader();
                }
                catch (SqlException)
                {
                    //TO DO: handle errors
                    return columnsInfo;
                }

                //populate list of table names
                if (reader.HasRows)
                {
                    while (reader.Read())
                    {
                        columnsInfo.Add(new ColumnInfo(reader.GetString(0), reader.GetString(1), reader.GetInt16(2), Convert.ToBoolean(reader.GetValue(3))));
                    }
                }

                reader.Close();
            }

            return columnsInfo;
        }

        //list all available tables
        [HttpGet]
        public ActionResult Index()
        {
            return View(GetTableNames());
        }

        //select
        [HttpGet]
        public ActionResult View(string Name)
        {
            //if the table does not exist
            if (!GetTableNames().Contains(Name))
                return RedirectToAction("Index");

            //fetch data from the table
            List<ColumnInfo> columnsInfo = GetColumnsInfo(Name);

            //each list == values by columns
            List<List<object>> values = new List<List<object>>();

            //add as many lists as many columns there is
            for (int i = 0; i < columnsInfo.Count; i++)
                values.Add(new List<object>());

            //populate lists
            using (SqlConnection connection = new SqlConnection(connectionStr))
            {
                SqlCommand cmd = new SqlCommand(getRowsCmdText + Name, connection);
                SqlDataReader reader;

                try
                {
                    cmd.Connection.Open();
                    reader = cmd.ExecuteReader();
                }
                catch (SqlException)
                {
                    //TO DO: handle errors
                    return View(new TableDataContainer(columnsInfo, values, Name));
                }

                //populate list of table names
                if (reader.HasRows)
                {
                    while (reader.Read())
                    {
                        for(int i = 0; i < values.Count; i++)
                        {
                            values[i].Add(reader.GetValue(i));
                        }
                    }
                }

                reader.Close();
            }

            return View(new TableDataContainer(columnsInfo, values, Name));
        }

        //create
        [HttpGet]
        public ActionResult Create(string Name)
        {
            List<string> tableNames = GetTableNames();

            return View();
        }

        //create
        [HttpPost]
        public ActionResult Create(FormCollection forms)
        {
            return View();
        }

        //update
        [HttpGet]
        public ActionResult Update(string tableName)
        {
            List<string> tableNames = GetTableNames();

            return View();
        }

        //delete
        [HttpGet]
        public ActionResult Delete(string tableName)
        {
            List<string> tableNames = GetTableNames();

            return View();
        }

    }
}