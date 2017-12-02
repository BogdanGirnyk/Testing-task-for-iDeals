using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data;
using System.Data.SqlClient;
using System.Configuration;
using TestTask_Bogdan_iDeals.ViewModels;

namespace TestTask_Bogdan_iDeals
{

    public enum ResultType
    {
        Success,
        Error
    }

    public class DBActionStatus<T>
    {
        public ResultType status;
        public string message;
        public T data;
    }

    public static class DBAccess
    {
        private static string dbName = "TestingTaskBogdan";
        private static string connString = ConfigurationManager.ConnectionStrings["MSSQLConnection"].ConnectionString;

        private static bool DatabaseExists()
        {
            using (var connection = new SqlConnection(connString))
            {
                using (var command = new SqlCommand($"SELECT db_id('{dbName}')", connection))
                {
                    connection.Open();
                    return (command.ExecuteScalar() != DBNull.Value);
                }
            }
        }

        public static void CheckAndCreateDB()
        {
            if (DatabaseExists())
                return;

            SqlConnection myConn = new SqlConnection(connString);
            SqlConnection myConnWithDB = new SqlConnection(connString + ";Database=" + dbName);

            String db_cr = "CREATE DATABASE " + dbName;
            String tbl_logs_cr = @"CREATE TABLE [dbo].[Logs](
	                                [ID] [int] IDENTITY(1,1) NOT NULL,
	                                [Type] [varchar](50) NULL,
	                                [Event] [varchar](500) NULL,
	                                [Date] [datetime] NULL,
                                    CONSTRAINT [PK_Logs] PRIMARY KEY CLUSTERED 
                                        (
	                                        [ID] ASC
                                        )WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
                                      ) ON [PRIMARY]";

            String tbl_users_cr = @"CREATE TABLE  [dbo].[Users](
	                                [Surname] [varchar](255) NULL,
	                                [Name] [varchar](255) NULL,
	                                [Email] [varchar](125) NOT NULL,
	                                [MobilePhone] [varchar](15) NULL,
                                    [Password] [varchar](255) NULL,
                                    CONSTRAINT [PK_Users] PRIMARY KEY CLUSTERED 
                                    (
	                                    [Email] ASC
                                    )WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
                                    ) ON [PRIMARY]";


            String prc_createuser_cr = @"CREATE PROCEDURE [dbo].[create_user] @Surname varchar(255), @Name varchar(255), 
                                                    @Email varchar(255), @MobilePhone varchar(30), @Password varchar(255)

                                        AS

                                        INSERT INTO [dbo].Users
                                                   ([Surname]
                                                   ,[Name]
                                                   ,[Email]
                                                   ,[MobilePhone]
                                                   ,[Password])
                                             VALUES
                                                   (@Surname
                                                   ,@Name
                                                   ,@Email
                                                   ,@MobilePhone
                                                   ,@Password)";
            String prc_getuser_cr = @"CREATE PROCEDURE [dbo].[get_user] @Email varchar(255)

                                        AS

                                        SELECT [Surname]
                                                   ,[Name]
                                                   ,[Email]
                                                   ,[MobilePhone] FROM [dbo].Users           
                                            WHERE Email=@Email";

            String prc_updateuser_cr = @"CREATE PROCEDURE [dbo].[update_user] @Email varchar(255), @Surname varchar(255), @Name varchar(255), @MobilePhone varchar(30)

                                        AS

                                        UPDATE  [dbo].Users
                                                SET  [Surname] = @Surname
                                                    ,[Name] = @Name
                                                    ,[MobilePhone] = @MobilePhone
                                             WHERE [Email]=@Email";

            String prc_writelog_cr = @"CREATE PROCEDURE [dbo].[write_log] (@Type VARCHAR(30),@Event VARCHAR(500), @Date dateTime)
                                        as
                                        INSERT INTO [dbo].Logs (Type,Event,Date) VALUES (@Type,@Event,@Date)";

            String prc_checkpw_cr = @"CREATE PROCEDURE [dbo].[check_password] @Email varchar(255) , @Password varchar(255)

                                        AS

                                        SELECT Count(*) as Checked FROM [dbo].Users           
                                            WHERE Email=@Email and Password=@Password";


            try
            {
                myConn.Open();
                SqlCommand myCommand = new SqlCommand(db_cr, myConn);
                myCommand.ExecuteNonQuery();
                myConnWithDB.Open();
                myCommand = new SqlCommand(tbl_logs_cr, myConnWithDB);
                myCommand.ExecuteNonQuery();
                myCommand = new SqlCommand(tbl_users_cr, myConnWithDB);
                myCommand.ExecuteNonQuery();

                myCommand = new SqlCommand(prc_createuser_cr, myConnWithDB);
                myCommand.ExecuteNonQuery();

                myCommand = new SqlCommand(prc_getuser_cr, myConnWithDB);
                myCommand.ExecuteNonQuery();

                myCommand = new SqlCommand(prc_updateuser_cr, myConnWithDB);
                myCommand.ExecuteNonQuery();

                myCommand = new SqlCommand(prc_writelog_cr, myConnWithDB);
                myCommand.ExecuteNonQuery();

                myCommand = new SqlCommand(prc_checkpw_cr, myConnWithDB);
                myCommand.ExecuteNonQuery();

                WriteLog("Event", "Database created", DateTime.Now);

            }
            catch (System.Exception ex)
            {
                //Console.WriteLine(ex.ToString());
            }
            finally
            {
                if (myConnWithDB.State == ConnectionState.Open)
                {
                    myConnWithDB.Close();
                }
                if (myConn.State == ConnectionState.Open)
                {
                    myConn.Close();
                }
            }

        }

        public static DBActionStatus<bool> CreateUser(UserCreate user)
        {
            try
            {

                using (var connection = new SqlConnection(connString + ";Database=" + dbName))
                {
                    using (var command = new SqlCommand("create_user", connection))
                    {
                        command.CommandType = CommandType.StoredProcedure;
                        command.Parameters.Add(new SqlParameter("@Surname", user.Surname));
                        command.Parameters.Add(new SqlParameter("@Name", user.Name));
                        command.Parameters.Add(new SqlParameter("@Email", user.Email));
                        command.Parameters.Add(new SqlParameter("@Password", user.Password));
                        command.Parameters.Add(new SqlParameter("@MobilePhone", user.Mobilephone));

                        connection.Open();

                        command.ExecuteNonQuery();
                        return new DBActionStatus<bool> { status = ResultType.Success, message = "User created: " + user.Email, data = true };

                    }
                }
            }

            catch (Exception e)
            {
                return new DBActionStatus<bool> { status = ResultType.Error, message = "Unable yo create user: " + user.Email + " Error: " + e.Message, data = false };
            }

        }

        public static DBActionStatus<bool> UpdateUser(string email,UserGetUpdate user)
        {
            try
            {

                using (var connection = new SqlConnection(connString + ";Database=" + dbName))
                {
                    using (var command = new SqlCommand("update_user", connection))
                    {
                        command.CommandType = CommandType.StoredProcedure;
                        command.Parameters.Add(new SqlParameter("@Surname", user.Surname));
                        command.Parameters.Add(new SqlParameter("@Name", user.Name));
                        command.Parameters.Add(new SqlParameter("@Email", email));
                        command.Parameters.Add(new SqlParameter("@MobilePhone", user.Mobilephone));

                        connection.Open();

                        command.ExecuteNonQuery();
                        return new DBActionStatus<bool> { status = ResultType.Success, message = "User updated: " + email, data = true };

                    }
                }
            }

            catch (Exception e)
            {
                return new DBActionStatus<bool> { status = ResultType.Error, message = "Unable yo update user: " + email + " Error: " + e.Message, data = false };
            }

        }

        public static DBActionStatus<UserGetUpdate> GetUser(string email)
        {
            try
            {

                using (var connection = new SqlConnection(connString + ";Database=" + dbName))
                {
                    using (var command = new SqlCommand("get_user", connection))
                    {
                        command.CommandType = CommandType.StoredProcedure;
                        command.Parameters.Add(new SqlParameter("@Email", email));
                        connection.Open();

                        var data = command.ExecuteReader();
                        if (data.Read())
                        {
                            UserGetUpdate user = new UserGetUpdate();
                            user.Surname = data.GetValue(0).ToString();
                            user.Name = data.GetValue(1).ToString();
                            user.Email = data.GetValue(2).ToString();
                            user.Mobilephone = data.GetValue(3).ToString();

                            return new DBActionStatus<UserGetUpdate> { status = ResultType.Success, message = "User found", data = user };
                        }
                        else
                            return new DBActionStatus<UserGetUpdate> { status = ResultType.Error, message = "User not found "+email, data = null };

                    }
                }

            }
            catch (Exception e)
            {
                return new DBActionStatus<UserGetUpdate> { status = ResultType.Error, message = "Error while searching user: " + email + " Error: " + e.Message, data = null };
            }
        }

        public static DBActionStatus<bool> WriteLog(string type, string message, DateTime date)
        {
            try
            {
                using (var connection = new SqlConnection(connString + ";Database=" + dbName))
                {
                    using (var command = new SqlCommand("write_log", connection))
                    {
                        command.CommandType = CommandType.StoredProcedure;
                        command.Parameters.Add(new SqlParameter("@Type", type));
                        command.Parameters.Add(new SqlParameter("@Event", message));
                        command.Parameters.Add(new SqlParameter("@Date", date));

                        connection.Open();

                        command.ExecuteNonQuery();
                        return new DBActionStatus<bool> { status = ResultType.Success, message = "Log written", data = true };

                    }
                }
            }
            catch (Exception e)
            {
                return new DBActionStatus<bool> { status = ResultType.Error, message = "Unable yo write log: " + e.Message, data = false };
            }
        }

        public static DBActionStatus<bool> CheckLogin(string email,string password)
        {
            try
            {

                using (var connection = new SqlConnection(connString + ";Database=" + dbName))
                {
                    using (var command = new SqlCommand("check_password", connection))
                    {
                        command.CommandType = CommandType.StoredProcedure;

                        command.Parameters.Add(new SqlParameter("@Email", email));
                        command.Parameters.Add(new SqlParameter("@Password", password));


                        connection.Open();

                        int pwchecked = (int)command.ExecuteScalar();
                        if (pwchecked==1)
                         return new DBActionStatus<bool> { status = ResultType.Success, message = "User checked" + email, data = true };
                        else
                         return new DBActionStatus<bool> { status = ResultType.Error, message = "Wrong login" + email, data = false };
                    }
                }
            }

            catch (Exception e)
            {
                return new DBActionStatus<bool> { status = ResultType.Error, message = "Error while checking login: " + email + " Error: " + e.Message, data = false };
            }

        }

    }
}