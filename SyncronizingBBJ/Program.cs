using System;
using System.Configuration;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SyncronizingBBJ
{
    class Program
    {
        private static DateTime businessDate;

        static void Main(string[] args)
        {
            GetBusinessDate();
            if (businessDate != null)
            {
                if(businessDate.ToString() != "")
                {
                    Thread thFinancialInfo = new Thread(new ThreadStart(SyncroFinancialInfo));
                    Thread thMemberInfo = new Thread(new ThreadStart(SyncroMemberInfo));
                    Thread thHiLoPrice = new Thread(new ThreadStart(SyncroHiLoPrice));
                    Thread thTradeFeed = new Thread(new ThreadStart(SyncroTradefeed));

                    thFinancialInfo.Start();
                    thMemberInfo.Start();
                    thHiLoPrice.Start();
                    thTradeFeed.Start();

                    while (true)
                    {
                        if (!thFinancialInfo.IsAlive)
                        {
                            thFinancialInfo = new Thread(new ThreadStart(SyncroFinancialInfo));
                            thFinancialInfo.Start();
                            GetBusinessDate();
                        }

                        if (!thMemberInfo.IsAlive)
                        {
                            thMemberInfo = new Thread(new ThreadStart(SyncroMemberInfo));
                            thMemberInfo.Start();
                        }

                        if (!thHiLoPrice.IsAlive)
                        {
                            thHiLoPrice = new Thread(new ThreadStart(SyncroHiLoPrice));
                            thHiLoPrice.Start();
                        }

                        if (!thTradeFeed.IsAlive)
                        {
                            thTradeFeed = new Thread(new ThreadStart(SyncroTradefeed));
                            thTradeFeed.Start();
                        }

                        Header();
                        DisplayData();
                        Footer();
                        Thread.Sleep(500);
                        Console.Clear();
                    }
                }
                else
                {
                    Console.WriteLine("Business Date is Not Ready, Please fill the Business Date Parameter");
                }
            }
            else
            {
                Console.WriteLine("Business Date is Not Ready, Please fill the Business Date Parameter");
            }
        }

        #region BusinessDate
        public static void GetBusinessDate()
        {
            SqlConnectionStringBuilder builder = new SqlConnectionStringBuilder();
            builder.DataSource = ConfigurationSettings.AppSettings["sqlserv_datasource"];
            builder.UserID = ConfigurationSettings.AppSettings["sqlserv_username"];
            builder.Password = ConfigurationSettings.AppSettings["sqlserv_password"];
            builder.InitialCatalog = ConfigurationSettings.AppSettings["sqlserv_database_kbi"];
            builder.ConnectTimeout = 0;
            builder.MultipleActiveResultSets = true;

            using (SqlConnection connection = new SqlConnection(builder.ConnectionString))
            {
                connection.Open();

                string sqlBusinessDate = "SELECT DISTINCT Code, DateValue FROM SKD.Parameter WHERE Code = 'BusinessDate'";

                using (SqlCommand command = new SqlCommand(sqlBusinessDate, connection))
                {
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            //businessDate = DateTime.Parse(reader["DateValue"].ToString());
                            businessDate = DateTime.Today;
                        }
                    }
                }
                connection.Close();
            }
        }
        #endregion

        #region Tradefeed
        public static void SyncroTradefeed()
        {
            SqlConnectionStringBuilder builder = new SqlConnectionStringBuilder();
            builder.DataSource = ConfigurationSettings.AppSettings["sqlserv_datasource"];
            builder.UserID = ConfigurationSettings.AppSettings["sqlserv_username"];
            builder.Password = ConfigurationSettings.AppSettings["sqlserv_password"];
            builder.InitialCatalog = ConfigurationSettings.AppSettings["sqlserv_database_kbi"];
            builder.ConnectTimeout = 0;
            builder.MultipleActiveResultSets = true;

            SqlConnectionStringBuilder builder2 = new SqlConnectionStringBuilder();
            builder2.DataSource = ConfigurationSettings.AppSettings["sqlserv_datasource"];
            builder2.UserID = ConfigurationSettings.AppSettings["sqlserv_username"];
            builder2.Password = ConfigurationSettings.AppSettings["sqlserv_password"];
            builder2.InitialCatalog = ConfigurationSettings.AppSettings["sqlserv_database_bbj"];
            builder2.ConnectTimeout = 0;
            builder2.MultipleActiveResultSets = true;
            
            using (SqlConnection connection2 = new SqlConnection(builder.ConnectionString))
            {
                connection2.Open();

                string sqlRawTradefeedInsert = "dbo.uspStagingRawTradeFeed";

                using (SqlCommand command = new SqlCommand(sqlRawTradefeedInsert, connection2))
                {
                    command.CommandType = CommandType.StoredProcedure;

                    command.Parameters.Add("@BusinessDate", SqlDbType.DateTime).Value = businessDate;

                    command.ExecuteNonQuery();
                }

                connection2.Close();
            }
        }
        #endregion

        #region FinancialInfo
        public static void SyncroFinancialInfo()
        {
            SqlConnectionStringBuilder builder = new SqlConnectionStringBuilder();
            builder.DataSource = ConfigurationSettings.AppSettings["sqlserv_datasource"];
            builder.UserID = ConfigurationSettings.AppSettings["sqlserv_username"];
            builder.Password = ConfigurationSettings.AppSettings["sqlserv_password"];
            builder.InitialCatalog = ConfigurationSettings.AppSettings["sqlserv_database_kbi"];
            builder.ConnectTimeout = 0;
            builder.MultipleActiveResultSets = true;

            SqlConnectionStringBuilder builder2 = new SqlConnectionStringBuilder();
            builder2.DataSource = ConfigurationSettings.AppSettings["sqlserv_datasource"];
            builder2.UserID = ConfigurationSettings.AppSettings["sqlserv_username"];
            builder2.Password = ConfigurationSettings.AppSettings["sqlserv_password"];
            builder2.InitialCatalog = ConfigurationSettings.AppSettings["sqlserv_database_bbj"];
            builder2.ConnectTimeout = 0;
            builder2.MultipleActiveResultSets = true;

            List<FinancialInfo> finInfoList = new List<FinancialInfo>();

            using (SqlConnection connection = new SqlConnection(builder.ConnectionString))
            {
                connection.Open();

                string sqlFinancialInfo = "SELECT BusinessDate,MessageCode,MessageCodeType,FinancialInfoTime, sequence " +
                                            ", ParticipantCode, AccountCode, Currency, SUM(Balance) as Balance, BondSerialNumber " +
                                            ", SpecialProductCode, Brand FROM SKD.FinancialInfo WHERE BusinessDate = '" + businessDate.ToString("yyyy-MM-dd") + "' AND (Flag is null OR Flag = 0) " +
                                            "GROUP BY BusinessDate,MessageCode,MessageCodeType,FinancialInfoTime " +
                                            ", ParticipantCode, AccountCode, Currency, BondSerialNumber, SpecialProductCode, Brand, sequence";


                using (SqlCommand command = new SqlCommand(sqlFinancialInfo, connection))
                {
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            FinancialInfo financial = new FinancialInfo();

                            financial.businessDate = DateTime.Parse(reader["BusinessDate"].ToString());
                            financial.codeType = reader["MessageCodeType"].ToString();
                            financial.sequence = int.Parse(reader["Sequence"].ToString());
                            financial.financialInfoTime = DateTime.Parse(reader["FinancialInfoTime"].ToString());
                            financial.participantCode = reader["ParticipantCode"].ToString();
                            financial.accountCode = reader["AccountCode"].ToString();
                            financial.bondSerialNumber = reader["BondSerialNumber"].ToString();
                            financial.specialProductCode = reader["SpecialProductCode"].ToString();
                            financial.currency = reader["Currency"].ToString();
                            financial.balance = decimal.Parse(reader["Balance"].ToString());
                            financial.brand = reader["Brand"].ToString();

                            finInfoList.Add(financial);
                        }
                    }
                }

                connection.Close();
            }

            using (SqlConnection connection2 = new SqlConnection(builder2.ConnectionString))
            {
                connection2.Open();

                string sqlUpsertFinancialInfo = "dbo.upsertFinancialInfo";

                foreach (FinancialInfo fininfo in finInfoList)
                {
                    using (SqlCommand command = new SqlCommand(sqlUpsertFinancialInfo, connection2))
                    {
                        command.CommandType = CommandType.StoredProcedure;
                        
                        command.Parameters.AddWithValue("@BusinessDate", fininfo.businessDate);
                        command.Parameters.AddWithValue("@MessageCodeType", fininfo.codeType);
                        command.Parameters.AddWithValue("@FinancialInfoTime", fininfo.financialInfoTime);
                        command.Parameters.AddWithValue("@ParticipantCode", fininfo.participantCode);
                        command.Parameters.AddWithValue("@AccountCode", fininfo.accountCode);
                        command.Parameters.AddWithValue("@BondSerialNumber", fininfo.bondSerialNumber);
                        command.Parameters.AddWithValue("@SpecialProductCode", fininfo.specialProductCode);
                        command.Parameters.AddWithValue("@Currency", fininfo.currency);
                        command.Parameters.AddWithValue("@Balance", fininfo.balance);
                        //command.Parameters.AddWithValue("@BondHaircutExpiredDate", fininfo.bondHaircutExpiredDate);
                        command.Parameters.AddWithValue("@Brand", fininfo.brand);

                        command.ExecuteNonQuery();
                    }
                }

                connection2.Close();
            }

            using (SqlConnection connection = new SqlConnection(builder.ConnectionString))
            {
                connection.Open();

                string sqlUpdateHilo = "UPDATE SKD.FinancialInfo SET Flag = 1 WHERE BusinessDate = @BusinessDate AND Sequence = @Sequence";

                foreach (FinancialInfo fininfo in finInfoList)
                {
                    using (SqlCommand command = new SqlCommand(sqlUpdateHilo, connection))
                    {
                        command.CommandType = CommandType.Text;
                        
                        command.Parameters.AddWithValue("@BusinessDate", fininfo.businessDate);
                        command.Parameters.AddWithValue("@Sequence", fininfo.sequence);

                        command.ExecuteNonQuery();
                    }
                }

                connection.Close();
            }
        }
        #endregion

        #region MemberInfo
        public static void SyncroMemberInfo()
        {
            SqlConnectionStringBuilder builder = new SqlConnectionStringBuilder();
            builder.DataSource = ConfigurationSettings.AppSettings["sqlserv_datasource"];
            builder.UserID = ConfigurationSettings.AppSettings["sqlserv_username"];
            builder.Password = ConfigurationSettings.AppSettings["sqlserv_password"];
            builder.InitialCatalog = ConfigurationSettings.AppSettings["sqlserv_database_kbi"];
            builder.ConnectTimeout = 0;
            builder.MultipleActiveResultSets = true;

            SqlConnectionStringBuilder builder2 = new SqlConnectionStringBuilder();
            builder2.DataSource = ConfigurationSettings.AppSettings["sqlserv_datasource"];
            builder2.UserID = ConfigurationSettings.AppSettings["sqlserv_username"];
            builder2.Password = ConfigurationSettings.AppSettings["sqlserv_password"];
            builder2.InitialCatalog = ConfigurationSettings.AppSettings["sqlserv_database_bbj"];
            builder2.ConnectTimeout = 0;
            builder2.MultipleActiveResultSets = true;

            List<MemberInfo> memberInfoList = new List<MemberInfo>();

            using (SqlConnection connection = new SqlConnection(builder.ConnectionString))
            {
                connection.Open();

                string sqlMemberInfo =  "SELECT TFMemberInfoId,BusinessDate,MemberSequence,RecordType,RegisteredTime,FixedParticipantCode,ParticipantCode,InvestorCode,AccountType,ProductGroup " +
                                        ", RegionCode, SpecialAccount, CustomerName, Email, CreatedBy, CreatedDate, LastUpdatedBy, LastUpdatedDate, AccountStatus, SuretyBondAccount " +
                                        "FROM SKD.TradefeedMemberInfo WHERE BusinessDate = '" + businessDate.ToString("yyyy-MM-dd") + "' AND (Flag is null OR Flag = 0)";

                using (SqlCommand command = new SqlCommand(sqlMemberInfo, connection))
                {
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            MemberInfo member = new MemberInfo();

                            member.TFMemberInfoId = int.Parse(reader["TFMemberInfoId"].ToString());
                            member.BusinessDate = DateTime.Parse(reader["BusinessDate"].ToString());
                            member.Sequence = int.Parse(reader["MemberSequence"].ToString());
                            member.FixParticipant = reader["FixedParticipantCode"].ToString();
                            member.ParticipantCode = reader["ParticipantCode"].ToString();
                            member.InvestorCode = reader["InvestorCode"].ToString();
                            member.AccountType = reader["AccountType"].ToString();
                            member.GroupProduct = reader["ProductGroup"].ToString();
                            member.Region = reader["RegionCode"].ToString();
                            member.Special = reader["SpecialAccount"].ToString();
                            member.CustomerName = reader["CustomerName"].ToString();
                            member.Email = reader["Email"].ToString();
                            member.AccountStatus = reader["AccountStatus"].ToString();
                            member.SuretyBondAccount = reader["SuretyBondAccount"].ToString();

                            memberInfoList.Add(member);
                        }
                    }
                }

                connection.Close();
            }

            using (SqlConnection connection2 = new SqlConnection(builder2.ConnectionString))
            {
                connection2.Open();

                string sqlUpsertFinancialInfo = "dbo.upsertMemberInfo";

                foreach (MemberInfo member in memberInfoList)
                {
                    using (SqlCommand command = new SqlCommand(sqlUpsertFinancialInfo, connection2))
                    {
                        command.CommandType = CommandType.StoredProcedure;
                        
                        command.Parameters.AddWithValue("@TFMemberInfoId", member.TFMemberInfoId);
                        command.Parameters.AddWithValue("@BusinessDate", member.BusinessDate);
                        command.Parameters.AddWithValue("@MemberSequence", member.Sequence);
                        command.Parameters.AddWithValue("@AccountStatus", member.AccountStatus);
                        command.Parameters.AddWithValue("@FixedParticipantCode", member.FixParticipant);
                        command.Parameters.AddWithValue("@ParticipantCode", member.ParticipantCode);
                        command.Parameters.AddWithValue("@InvestorCode", member.InvestorCode);
                        command.Parameters.AddWithValue("@AccountType", member.AccountType);
                        command.Parameters.AddWithValue("@ProductGroup", member.GroupProduct);
                        command.Parameters.AddWithValue("@RegionCode", member.Region);
                        command.Parameters.AddWithValue("@SpecialAccount", member.Special);
                        command.Parameters.AddWithValue("@CustomerName", member.CustomerName);
                        command.Parameters.AddWithValue("@Email", member.Email);
                        //command.Parameters.AddWithValue("@StatusMemberInfo", member.Status);
                        command.Parameters.AddWithValue("@SuretyBondAccount", member.SuretyBondAccount);

                        command.ExecuteNonQuery();
                    }
                }

                connection2.Close();
            }

            using (SqlConnection connection = new SqlConnection(builder.ConnectionString))
            {
                connection.Open();

                string sqlUpdateHilo = "UPDATE SKD.TradefeedMemberInfo SET Flag = 1 WHERE BusinessDate = @BusinessDate AND MemberSequence = @Sequence";

                foreach (MemberInfo member in memberInfoList)
                {
                    using (SqlCommand command = new SqlCommand(sqlUpdateHilo, connection))
                    {
                        command.CommandType = CommandType.Text;

                        command.Parameters.AddWithValue("@BusinessDate", member.BusinessDate);
                        command.Parameters.AddWithValue("@Sequence", member.Sequence);

                        command.ExecuteNonQuery();
                    }
                }

                connection.Close();
            }
        }
        #endregion

        #region HiLoPrice
        public static void SyncroHiLoPrice()
        {
            SqlConnectionStringBuilder builder = new SqlConnectionStringBuilder();
            builder.DataSource = ConfigurationSettings.AppSettings["sqlserv_datasource"];
            builder.UserID = ConfigurationSettings.AppSettings["sqlserv_username"];
            builder.Password = ConfigurationSettings.AppSettings["sqlserv_password"];
            builder.InitialCatalog = ConfigurationSettings.AppSettings["sqlserv_database_kbi"];
            builder.ConnectTimeout = 0;
            builder.MultipleActiveResultSets = true;

            SqlConnectionStringBuilder builder2 = new SqlConnectionStringBuilder();
            builder2.DataSource = ConfigurationSettings.AppSettings["sqlserv_datasource"];
            builder2.UserID = ConfigurationSettings.AppSettings["sqlserv_username"];
            builder2.Password = ConfigurationSettings.AppSettings["sqlserv_password"];
            builder2.InitialCatalog = ConfigurationSettings.AppSettings["sqlserv_database_bbj"];
            builder2.ConnectTimeout = 0;
            builder2.MultipleActiveResultSets = true;

            List<HiLoPrice> hiLoPriceList = new List<HiLoPrice>();

            using (SqlConnection connection = new SqlConnection(builder.ConnectionString))
            {
                connection.Open();

                string sqlMemberInfo =  "SELECT TOP 1 CONVERT(date, getdate()) as BusinessDate,CeilingPrice,FloorPrice " +
                                        "FROM SKD.CeilingPrice WHERE EffectiveStartDate <= '" + businessDate.ToString("yyyy-MM-dd") + "' AND (Flag is null OR Flag = 0)";

                using (SqlCommand command = new SqlCommand(sqlMemberInfo, connection))
                {
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            HiLoPrice hilo = new HiLoPrice();

                            hilo.BusinessDate = businessDate;
                            hilo.CeilingPrice = decimal.Parse(reader["CeilingPrice"].ToString());
                            hilo.FloorPrice = decimal.Parse(reader["FloorPrice"].ToString());

                            hiLoPriceList.Add(hilo);
                        }
                    }
                }

                connection.Close();
            }

            using (SqlConnection connection2 = new SqlConnection(builder2.ConnectionString))
            {
                connection2.Open();

                string sqlUpsertHiLoPrice = "dbo.upsertHiLoPrice";

                using (SqlCommand command = new SqlCommand(sqlUpsertHiLoPrice, connection2))
                {
                    command.CommandType = CommandType.StoredProcedure;

                    foreach (HiLoPrice hilo in hiLoPriceList)
                    {
                        command.Parameters.AddWithValue("@BusinessDate", hilo.BusinessDate);
                        command.Parameters.AddWithValue("@CeilingPrice", hilo.CeilingPrice);
                        command.Parameters.AddWithValue("@FloorPrice", hilo.FloorPrice);

                        command.ExecuteNonQuery();
                    }
                }

                connection2.Close();
            }
        }
        #endregion

        #region display
        public static void DisplayData()
        {
            SqlConnectionStringBuilder builder = new SqlConnectionStringBuilder();
            builder.DataSource = ConfigurationSettings.AppSettings["sqlserv_datasource"];
            builder.UserID = ConfigurationSettings.AppSettings["sqlserv_username"];
            builder.Password = ConfigurationSettings.AppSettings["sqlserv_password"];
            builder.InitialCatalog = ConfigurationSettings.AppSettings["sqlserv_database_kbi"];
            builder.ConnectTimeout = 0;
            builder.MultipleActiveResultSets = true;

            SqlConnectionStringBuilder builder2 = new SqlConnectionStringBuilder();
            builder2.DataSource = ConfigurationSettings.AppSettings["sqlserv_datasource"];
            builder2.UserID = ConfigurationSettings.AppSettings["sqlserv_username"];
            builder2.Password = ConfigurationSettings.AppSettings["sqlserv_password"];
            builder2.InitialCatalog = ConfigurationSettings.AppSettings["sqlserv_database_bbj"];
            builder2.ConnectTimeout = 0;
            builder2.MultipleActiveResultSets = true;

            int financialInfoKBI = 0, financialInfoBBJ = 0, memberInfoKBI = 0, memberInfoBBJ = 0, tradefeedKBI = 0, tradefeedBBJ = 0, hiLoPriceKBI = 0, hiLoPriceBBJ = 0;

            #region QueryKBI
            using (SqlConnection connection = new SqlConnection(builder.ConnectionString))
            {
                connection.Open();

                string sqlFinancialInfo = "SELECT COUNT(*) as total FROM SKD.FinancialInfo WHERE BusinessDate = '" + businessDate.ToString("yyyy-MM-dd") + "'";

                using (SqlCommand command1 = new SqlCommand(sqlFinancialInfo, connection))
                {
                    using (SqlDataReader reader1 = command1.ExecuteReader())
                    {
                        while (reader1.Read())
                        {
                            financialInfoKBI = int.Parse(reader1["total"].ToString());
                        }
                    }
                }

                string sqlMemberInfo = "SELECT COUNT(*) as total FROM SKD.TradefeedMemberInfo WHERE BusinessDate = '" + businessDate.ToString("yyyy-MM-dd") + "'";

                using (SqlCommand command2 = new SqlCommand(sqlMemberInfo, connection))
                {
                    using (SqlDataReader reader2 = command2.ExecuteReader())
                    {
                        while (reader2.Read())
                        {
                            memberInfoKBI = int.Parse(reader2["total"].ToString());
                        }
                    }
                }

                string sqlTradefeed = "SELECT COUNT(*) as total FROM SKD.RawTradefeed WHERE BusinessDate = '" + businessDate.ToString("yyyy-MM-dd") + "'";

                using (SqlCommand command3 = new SqlCommand(sqlTradefeed, connection))
                {
                    using (SqlDataReader reader3 = command3.ExecuteReader())
                    {
                        while (reader3.Read())
                        {
                            tradefeedKBI = int.Parse(reader3["total"].ToString());
                        }
                    }
                }

                string sqlHiLo = "SELECT COUNT(*) as total FROM SKD.CeilingPrice WHERE EffectiveStartDate <= '" + businessDate.ToString("yyyy-MM-dd") + "'";

                using (SqlCommand command4 = new SqlCommand(sqlHiLo, connection))
                {
                    using (SqlDataReader reader4 = command4.ExecuteReader())
                    {
                        while (reader4.Read())
                        {
                            hiLoPriceKBI = int.Parse(reader4["total"].ToString());
                        }
                    }
                }

                connection.Close();
            }
            #endregion

            #region QueryBBJ
            using (SqlConnection connection2 = new SqlConnection(builder2.ConnectionString))
            {
                connection2.Open();

                string sqlFinancialInfo = "SELECT COUNT(*) as total FROM FinancialInfo WHERE BusinessDate = '" + businessDate.ToString("yyyy-MM-dd") + "'";

                using (SqlCommand command1 = new SqlCommand(sqlFinancialInfo, connection2))
                {
                    using (SqlDataReader reader1 = command1.ExecuteReader())
                    {
                        while (reader1.Read())
                        {
                            financialInfoBBJ = int.Parse(reader1["total"].ToString());
                        }
                    }
                }

                string sqlMemberInfo = "SELECT COUNT(*) as total FROM MemberAccount WHERE BusinessDate = '" + businessDate.ToString("yyyy-MM-dd") + "'";

                using (SqlCommand command2 = new SqlCommand(sqlMemberInfo, connection2))
                {
                    using (SqlDataReader reader2 = command2.ExecuteReader())
                    {
                        while (reader2.Read())
                        {
                            memberInfoBBJ = int.Parse(reader2["total"].ToString());
                        }
                    }
                }

                string sqlTradefeed = "SELECT COUNT(*) as total FROM RawTradefeed WHERE BusinessDate = '" + businessDate.ToString("yyyy-MM-dd") + "'";

                using (SqlCommand command3 = new SqlCommand(sqlTradefeed, connection2))
                {
                    using (SqlDataReader reader3 = command3.ExecuteReader())
                    {
                        while (reader3.Read())
                        {
                            tradefeedBBJ = int.Parse(reader3["total"].ToString());
                        }
                    }
                }

                string sqlHiLo = "SELECT COUNT(*) as total FROM HiLoPrice WHERE BusinessDate = '" + businessDate.ToString("yyyy-MM-dd") + "'";

                using (SqlCommand command4 = new SqlCommand(sqlHiLo, connection2))
                {
                    using (SqlDataReader reader4 = command4.ExecuteReader())
                    {
                        while (reader4.Read())
                        {
                            hiLoPriceBBJ = int.Parse(reader4["total"].ToString());
                        }
                    }
                }

                connection2.Close();
            }
            #endregion

            Console.WriteLine();
            Console.WriteLine("Table Name\t\t\tData KBI\t\tData BBJ");
            Console.WriteLine(string.Format("HiLoPrice\t\t\t{0}\t\t\t{1}", hiLoPriceKBI, hiLoPriceBBJ));
            Console.WriteLine(string.Format("FinancialInfo\t\t\t{0}\t\t\t{1}", financialInfoKBI, financialInfoBBJ));
            Console.WriteLine(string.Format("TradefeedMemberInfo\t\t{0}\t\t\t{1}", memberInfoKBI, memberInfoBBJ));
            Console.WriteLine(string.Format("RawTradefeed\t\t\t{0}\t\t\t{1}", tradefeedKBI, tradefeedBBJ));
            Console.WriteLine();
        }
        #endregion

        public static void Header()
        {
            Console.WriteLine("##############################################################################");
            Console.WriteLine("                      SYNCRONIZING STAGING KBI - BGR");
            Console.WriteLine("##############################################################################");
            Console.WriteLine("Business Date : " + businessDate.ToString("yyyy-MM-dd"));
        }

        public static void Footer()
        {
            Console.WriteLine("##############################################################################");
            Console.WriteLine("          Copyright KBI 2019 - createdby : Hasto Gesang Wicaksono");
            Console.WriteLine("##############################################################################");
        }
    }
}
