using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Text;

namespace PrivateSchool.Data
{
    abstract class DataConnection : Queries
    {
        public static bool DebugOnly = false;

        //Method to get a list of a class given the connection and command (data can come filtered or by order).
        //Class field must be public and same number and name as the database table. Better bring all columns with query
        protected static List<T> GetData<T>(string cmd, string conn) where T : class, new()
        {
            List<T> list = new List<T>();
            
                try
                {
                DataTable dataTable = GetDataTable(cmd, conn);

                    foreach (DataRow dataRow in dataTable.Rows)
                    {
                        list.Add(GetobjectFromDataTableRow<T>(dataRow));
                    }
                }
                catch (SqlException)
                {
                    //Console.WriteLine(ex.Message);
                }
                catch (Exception)
                {
                    //Console.WriteLine(ex.Message);
                }
          
            return list;
        }

        //Converts datatable row to object type given as long as field of object are public and same name and type with the ones in datatable row
        private static T GetobjectFromDataTableRow<T>(DataRow dataRow) where T : class, new()
        {
            T obj = new T();

            try
            {
                foreach (var field in obj.GetType().GetFields())
                {
                    if (!DBNull.Value.Equals(dataRow[field.Name]))
                    {
                        field.SetValue(obj, Convert.ChangeType(dataRow[field.Name], field.FieldType));
                    }
                }
            }
            catch (Exception)
            {
                //Console.WriteLine(ex.Message);
            }

            return obj;
        }


        //Returns data from sql database given the query and connection in Datatable object
        protected static DataTable GetDataTable(string cmd, string conn)
        {
            DataTable dataTable = new DataTable();
            using (SqlConnection connection = new SqlConnection(conn))
            {
                try
                {
                    connection.Open();
                    using (SqlCommand command = new SqlCommand(cmd, connection))
                    {
                        
                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            dataTable.Load(reader);
                        }
                    }
                }
                catch (SqlException)
                {
                    //Console.WriteLine(ex.Message);
                }
                catch (Exception)
                {
                    //Console.WriteLine(ex.Message);
                }
            }
            return dataTable;
        }

        //Insert values to database given an object as long as object and database table have same name of fields and in same order. Fields of object must be public
        public static void InsertData<T>(T obj, string conn, string tableName) where T : class
        {
            //string objName = obj.GetType().Name;
            int len = obj.GetType().GetFields().Length;

            //Here we build the query to be passed as command and store sqlParameters in an array
            List<SqlParameter> sqlParameters = new List<SqlParameter>();
            StringBuilder query = new StringBuilder();
            query.Append("INSERT INTO ");
            query.Append(tableName);
            query.Append(" VALUES (");

            for (int i = 0; i < len; i++)
            {
                string fieldName = obj.GetType().GetFields()[i].Name;
                var fieldValue = obj.GetType().GetFields()[i].GetValue(obj);
                sqlParameters.Add(new SqlParameter("@" + fieldName, fieldValue));              
                query.Append(i == len - 1 ? "@" + fieldName + ")" : "@" + fieldName + ", ");
            }

            using (SqlConnection connection = new SqlConnection(conn))
            {
                try
                {
                    connection.Open();
                    using (SqlCommand command = new SqlCommand(query.ToString(), connection))
                    {
                        command.Parameters.AddRange(sqlParameters.ToArray());
                        command.ExecuteNonQuery();
                    }
                }
                catch (SqlException ex)
                {
                    Console.WriteLine(ex.Message);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }
        }

        //Delete value with ID number given the object with objectID as name in database column
        public static void DeleteData<T>(T obj, string conn, string tableName) where T : class
        {
            int len = obj.GetType().GetFields().Length;
            string objName = obj.GetType().Name;

            //Here we build the query to be passed as command and store sqlParameters in an array
            StringBuilder query = new StringBuilder();
            query.Append("DELETE FROM ");
            query.Append(tableName);
            query.Append(" WHERE ");

            for (int i = 0; i < len; i++)
            {
                string fieldName = obj.GetType().GetFields()[i].Name;
                var fieldValue = obj.GetType().GetFields()[i].GetValue(obj);
                if (fieldName == obj.GetType().Name.ToString() + "ID")
                {
                    query.Append(fieldName + " = " + fieldValue.ToString());
                }               
            }

            using (SqlConnection connection = new SqlConnection(conn))
            {
                try
                {
                    connection.Open();
                    using (SqlCommand command = new SqlCommand(query.ToString(), connection))
                    {
                        command.ExecuteNonQuery();
                        Console.WriteLine(objName + " deleted");
                    }
                }
                catch (SqlException ex)
                {
                    Console.WriteLine(ex.Message);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }
        }

        //Update values given an object and values. All fields must be public and same name and order as in database 
        //We update all fields, so we need to make sure fields that are not updated have to be taken from the initial object!!!
        public static void UpdateData<T>(T obj, string conn, string tableName) where T : class
        {
            int len = obj.GetType().GetFields().Length;
            string objName = obj.GetType().Name;
            string objectID ="";

            //Here we build the query to be passed as command and store sqlParameters in an array
            List<SqlParameter> sqlParameters = new List<SqlParameter>();
            StringBuilder query = new StringBuilder();
            query.Append("UPDATE ");
            query.Append(tableName);
            query.Append(" SET ");

            for (int i = 0; i < len; i++)
            {
                string fieldName = obj.GetType().GetFields()[i].Name;
                var fieldValue = obj.GetType().GetFields()[i].GetValue(obj);
                if (fieldName == objName + "ID")
                {
                    objectID = fieldName + " = " + fieldValue.ToString();
                }
                else if (fieldValue != null)
                {
                    sqlParameters.Add(new SqlParameter("@" + fieldName, fieldValue));
                    query.Append(i == len - 1 ? fieldName + " = " + "@" + fieldName : fieldName + " = " + "@" + fieldName + ", ");
                }
            }

            query.Append(" WHERE " + objectID);

            using (SqlConnection connection = new SqlConnection(conn))
            {
                try
                {
                    connection.Open();
                    using (SqlCommand command = new SqlCommand(query.ToString(), connection))
                    {
                        command.Parameters.AddRange(sqlParameters.ToArray());
                        command.ExecuteNonQuery();
                        Console.WriteLine(objName + " updated");
                    }
                }
                catch (SqlException ex)
                {
                    Console.WriteLine(ex.Message);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }
        }
    }
}
