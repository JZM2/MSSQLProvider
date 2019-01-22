using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ERAD
{
    /// <summary>
    /// Class for work with database MS SQL
    /// </summary>
    public class MSSQLProvider : ERAD.Data.Interface.IDataProvider
    {
        readonly private string QsoSelectStandard = "SELECT * FROM QSOS WHERE QsoFolder = @QsoFolder ORDER BY QsoNum ASC, QsoDate DESC";

        public string DataFilePath { set; get; }

        public string GetDataFile { get { return DataFilePath; } }

        private SqlConnection m_Connection;
        private SqlDataAdapter m_DataAdapterStation;
        protected SqlDataAdapter m_DataAdapterFolder;
        private SqlDataAdapter m_DataAdapterQSO;
        private SqlDataAdapter m_DataAdapterCallbook;

        /// <summary>
        /// Constructor MSSQL provider
        /// </summary>
        /// <param name="DatabaseFilePath"></param>
        public MSSQLProvider(string DatabaseFilePath)
        {
            DataFilePath = DatabaseFilePath;
            string StrCon = string.Format("Data Source = (LocalDB)\\MSSQLLocalDB; AttachDbFilename = \"{0}\"; Integrated Security = True", DataFilePath);

            try
            {
                m_Connection = new SqlConnection(StrCon);
                m_Connection.Open();

                Initialize();
            }
            catch (SqlException Chyba)
            {
                MessageBox.Show(string.Format("Nepodařilo se otevřít databázi!\n{0}\n\n{1}", DataFilePath , Chyba.Message), "ERAD", MessageBoxButtons.OK, MessageBoxIcon.Stop);
                Application.Exit();
            }
        }

        /// <summary>
        /// Function for change database file
        /// </summary>
        public void ChangeDataFile()
        {
            try
            {
                m_Connection.Close();
                m_Connection = new SqlConnection(string.Format("Data Source = (LocalDB)\\MSSQLLocalDB; AttachDbFilename = \"{0}\"; Integrated Security = True", DataFilePath));
                m_Connection.Open();
            }
            catch (Exception Chyba)
            {
                MessageBox.Show("Nepodařilo se otevřít databázi!\n\n" + Chyba.Message, "ERAD - změna databáze", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// Function for init data
        /// </summary>
        private void Initialize()
        {
            m_DataAdapterStation = new SqlDataAdapter("SELECT * FROM [Stations] ORDER BY [StationCall]", m_Connection)
            {
                InsertCommand = new SqlCommand("INSERT INTO Stations (StationCall, StationOperator, StationStreet, StationCity, StationPostCode, StationEmail, StationTelephone, StationEcholink, StationDXCC, StationCQzone, StationIOTA, StationPassword) " +
                " VALUES(@StationCall, @StationOperator, @StationStreet, @StationCity, @StationPostCode, @StationEmail, @StationTelephone, @StationEcholink, @StationDXCC, @StationCQzone, @StationIOTA, @StationPassword);", m_Connection)
            };

            m_DataAdapterStation.InsertCommand.Parameters.Add("@StationCall", SqlDbType.VarChar, 10, "StationCall");
            m_DataAdapterStation.InsertCommand.Parameters.Add("@StationOperator", SqlDbType.VarChar, 50, "StationOperator");
            m_DataAdapterStation.InsertCommand.Parameters.Add("@StationStreet", SqlDbType.VarChar, 150, "StationStreet");
            m_DataAdapterStation.InsertCommand.Parameters.Add("@StationCity", SqlDbType.VarChar, 150, "StationCity");
            m_DataAdapterStation.InsertCommand.Parameters.Add("@StationPostCode", SqlDbType.VarChar, 10, "StationPostCode");
            m_DataAdapterStation.InsertCommand.Parameters.Add("@StationEmail", SqlDbType.VarChar, 150, "StationEmail");
            m_DataAdapterStation.InsertCommand.Parameters.Add("@StationTelephone", SqlDbType.VarChar, 16, "StationTelephone");
            m_DataAdapterStation.InsertCommand.Parameters.Add("@StationEcholink", SqlDbType.Int, 4, "StationEcholink");
            m_DataAdapterStation.InsertCommand.Parameters.Add("@StationDXCC", SqlDbType.NChar, 6, "StationDXCC");
            m_DataAdapterStation.InsertCommand.Parameters.Add("@StationCQzone", SqlDbType.Int, 4, "StationCQzone");
            m_DataAdapterStation.InsertCommand.Parameters.Add("@StationIOTA", SqlDbType.NChar, 6, "StationIOTA");
            m_DataAdapterStation.InsertCommand.Parameters.Add("@StationPassword", SqlDbType.VarChar, 12, "StationPassword");


            m_DataAdapterStation.UpdateCommand = new SqlCommand("UPDATE [Stations] SET StationCall = @StationCall, StationOperator = @operator, StationStreet = @street, StationCity = @city, StationPostCode = @post, StationEmail = @email, StationTelephone = @tel, StationEcholink = @echolink, StationDXCC = @dxcc, StationCQzone = @cq, StationIOTA = @iota, StationPassword = @password WHERE StationCall = @StationCall", m_Connection);

            m_DataAdapterStation.UpdateCommand.Parameters.Add("@StationCall", SqlDbType.VarChar, 10, "StationCall");
            m_DataAdapterStation.UpdateCommand.Parameters.Add("@operator", SqlDbType.VarChar, 50, "StationOperator");
            m_DataAdapterStation.UpdateCommand.Parameters.Add("@street", SqlDbType.VarChar, 150, "StationStreet");
            m_DataAdapterStation.UpdateCommand.Parameters.Add("@city", SqlDbType.VarChar, 150, "StationCity");
            m_DataAdapterStation.UpdateCommand.Parameters.Add("@post", SqlDbType.VarChar, 10, "StationPostCode");
            m_DataAdapterStation.UpdateCommand.Parameters.Add("@email", SqlDbType.VarChar, 150, "StationEmail");
            m_DataAdapterStation.UpdateCommand.Parameters.Add("@tel", SqlDbType.VarChar, 16, "StationTelephone");
            m_DataAdapterStation.UpdateCommand.Parameters.Add("@echolink", SqlDbType.Int, 4, "StationEcholink");
            m_DataAdapterStation.UpdateCommand.Parameters.Add("@dxcc", SqlDbType.NChar, 6, "StationDXCC");
            m_DataAdapterStation.UpdateCommand.Parameters.Add("@cq", SqlDbType.Int, 4, "StationCQzone");
            m_DataAdapterStation.UpdateCommand.Parameters.Add("@iota", SqlDbType.NChar, 6, "StationIOTA");
            m_DataAdapterStation.UpdateCommand.Parameters.Add("@password", SqlDbType.VarChar, 12, "StationPassword");
            //m_DataAdapterStation.SelectCommand



            m_DataAdapterFolder = new SqlDataAdapter("SELECT * FROM Folders WHERE FolderStation = @Station", m_Connection);
            m_DataAdapterFolder.SelectCommand.Parameters.Add("@Station", SqlDbType.VarChar, 12, "FolderStation");

            m_DataAdapterFolder.InsertCommand = new SqlCommand("INSERT INTO Folders (FolderName, FolderParent, FolderDateFrom, FolderDateTo, FolderQTH, FolderGrid, FolderGPS, FolderContestName, FolderContestCategory, FolderContestPoints, FolderTx, FolderPower, FolderAnt, FolderRemarks, FolderStation) VALUES (@FolderName, @FolderParent, @FolderDateFrom, @FolderDateTo, @FolderQTH, @FolderGrid, @FolderGPS, @FolderContestName, @FolderContestCategory, @FolderContestPoints, @FolderTx, @FolderPower, @FolderAnt, @FolderRemarks, @FolderStation)", m_Connection);
            m_DataAdapterFolder.InsertCommand.Parameters.Add("@FolderName", SqlDbType.VarChar, 50, "FolderName");
            m_DataAdapterFolder.InsertCommand.Parameters.Add("@FolderParent", SqlDbType.Int, 4, "FolderParent");
            m_DataAdapterFolder.InsertCommand.Parameters.Add("@FolderDateFrom", SqlDbType.DateTime, -1, "FolderDateFrom");
            m_DataAdapterFolder.InsertCommand.Parameters.Add("@FolderDateTo", SqlDbType.DateTime, -1, "FolderDateTo");
            m_DataAdapterFolder.InsertCommand.Parameters.Add("@FolderQTH", SqlDbType.VarChar, 100, "FolderQTH");
            m_DataAdapterFolder.InsertCommand.Parameters.Add("@FolderGrid", SqlDbType.VarChar, 10, "FolderGrid");
            m_DataAdapterFolder.InsertCommand.Parameters.Add("@FolderGPS", SqlDbType.VarChar, 25, "FolderGPS");
            m_DataAdapterFolder.InsertCommand.Parameters.Add("@FolderContestName", SqlDbType.VarChar, 50, "FolderContestName");
            m_DataAdapterFolder.InsertCommand.Parameters.Add("@FolderContestPoints", SqlDbType.Int, 4, "FolderContestPoints");
            m_DataAdapterFolder.InsertCommand.Parameters.Add("@FolderContestCategory", SqlDbType.VarChar, 50, "FolderContestCategory");
            m_DataAdapterFolder.InsertCommand.Parameters.Add("@FolderRemarks", SqlDbType.Text, -1, "FolderRemarks");
            m_DataAdapterFolder.InsertCommand.Parameters.Add("@FolderStation", SqlDbType.VarChar, 12, "FolderStation");
            m_DataAdapterFolder.InsertCommand.Parameters.Add("@FolderTx", SqlDbType.VarChar, 50, "FolderTx");
            m_DataAdapterFolder.InsertCommand.Parameters.Add("@FolderPower", SqlDbType.Decimal, -1, "FolderPower");
            m_DataAdapterFolder.InsertCommand.Parameters.Add("@FolderAnt", SqlDbType.VarChar, 50, "FolderAnt");

            m_DataAdapterFolder.UpdateCommand = new SqlCommand("UPDATE [Folders] SET FolderName = @FolderName, FolderParent = @FolderParent, FolderDateFrom = @FolderDateFrom, FolderDateTo = @FolderDateTo, FolderQTH = @FolderQTH, FolderGrid = @FolderGrid, FolderGPS = @FolderGPS, FolderContestName = @FolderContestName, FolderContestCategory = @FolderContestCategory, FolderContestPoints = @FolderContestPoints, FolderRemarks = @FolderRemarks, FolderStation = @FolderStation, FolderTx = @FolderTx, FolderPower = @FolderPower, FolderAnt = @FolderAnt WHERE FolderID = @FolderID", m_Connection);
            m_DataAdapterFolder.UpdateCommand.Parameters.Add("@FolderName", SqlDbType.VarChar, 50, "FolderName");
            m_DataAdapterFolder.UpdateCommand.Parameters.Add("@FolderParent", SqlDbType.Int, 4, "FolderParent");
            m_DataAdapterFolder.UpdateCommand.Parameters.Add("@FolderDateFrom", SqlDbType.DateTime, 4, "FolderDateFrom");
            m_DataAdapterFolder.UpdateCommand.Parameters.Add("@FolderDateTo", SqlDbType.DateTime, 4, "FolderDateTo");
            m_DataAdapterFolder.UpdateCommand.Parameters.Add("@FolderQTH", SqlDbType.VarChar, 100, "FolderQTH");
            m_DataAdapterFolder.UpdateCommand.Parameters.Add("@FolderGrid", SqlDbType.VarChar, 10, "FolderGrid");
            m_DataAdapterFolder.UpdateCommand.Parameters.Add("@FolderGPS", SqlDbType.VarChar, 25, "FolderGPS");
            m_DataAdapterFolder.UpdateCommand.Parameters.Add("@FolderContestName", SqlDbType.VarChar, 50, "FolderContestName");
            m_DataAdapterFolder.UpdateCommand.Parameters.Add("@FolderContestCategory", SqlDbType.VarChar, 50, "FolderContestCategory");
            m_DataAdapterFolder.UpdateCommand.Parameters.Add("@FolderContestPoints", SqlDbType.Int, 4, "FolderContestPoints");
            m_DataAdapterFolder.UpdateCommand.Parameters.Add("@FolderRemarks", SqlDbType.Text, -1, "FolderRemarks");
            m_DataAdapterFolder.UpdateCommand.Parameters.Add("@FolderStation", SqlDbType.VarChar, 12, "FolderStation");
            m_DataAdapterFolder.UpdateCommand.Parameters.Add("@FolderTx", SqlDbType.VarChar, 50, "FolderTx");
            m_DataAdapterFolder.UpdateCommand.Parameters.Add("@FolderPower", SqlDbType.Decimal, -1, "FolderPower");
            m_DataAdapterFolder.UpdateCommand.Parameters.Add("@FolderAnt", SqlDbType.VarChar, 50, "FolderAnt");
            m_DataAdapterFolder.UpdateCommand.Parameters.Add("@FolderID", SqlDbType.Int, 4, "FolderID");


            m_DataAdapterQSO = new SqlDataAdapter(QsoSelectStandard, m_Connection);
            m_DataAdapterQSO.SelectCommand.Parameters.Add("@QsoFolder", SqlDbType.Int, -1, "QsoFolder");

            m_DataAdapterQSO.InsertCommand = new SqlCommand("INSERT INTO QSOS (QsoNum, QsoFolder, QsoDate, QsoFrequency, QsoMode, QsoCall, QsoYouGrid, QsoYouQTH, QsoYouReport, QsoYouQsl, QsoYouQslManager, QsoYouContestCode, QsoMyOperator, QsoMyReport, QsoMyQsl, QsoMyContestCode, QsoPoints, QsoPower, QsoTx, QsoDXCC, QsoCQZone, QsoIOTA, QsoRemarks) VALUES (@QsoNum, @QsoFolder, @QsoDate, @QsoFrequency, @QsoMode, @QsoCall, @QsoYouGrid, @QsoYouQTH, @QsoYouReport, @QsoYouQsl, @QsoYouQslManager, @QsoYouContestCode, @QsoMyOperator, @QsoMyReport, @QsoMyQsl, @QsoMyContestCode, @QsoPoints, @QsoPower, @QsoTx, @QsoDXCC, @QsoCQZone, @QsoIOTA, @QsoRemarks)", m_Connection);
            m_DataAdapterQSO.InsertCommand.Parameters.Add("@QsoNum", SqlDbType.Int, 4, "QsoNum");
            m_DataAdapterQSO.InsertCommand.Parameters.Add("@QsoFolder", SqlDbType.Int, 4, "QsoFolder");
            m_DataAdapterQSO.InsertCommand.Parameters.Add("@QsoDate", SqlDbType.DateTime, -1, "QsoDate");
            m_DataAdapterQSO.InsertCommand.Parameters.Add("@QsoFrequency", SqlDbType.Decimal, -1, "QsoFrequency");
            m_DataAdapterQSO.InsertCommand.Parameters.Add("@QsoMode", SqlDbType.VarChar, 10, "QsoMode");
            m_DataAdapterQSO.InsertCommand.Parameters.Add("@QsoCall", SqlDbType.VarChar, 10, "QsoCall");
            m_DataAdapterQSO.InsertCommand.Parameters.Add("@QsoYouGrid", SqlDbType.VarChar, 10, "QsoYouGrid");
            m_DataAdapterQSO.InsertCommand.Parameters.Add("@QsoYouQTH", SqlDbType.VarChar, 100, "QsoYouQTH");
            m_DataAdapterQSO.InsertCommand.Parameters.Add("@QsoYouReport", SqlDbType.Int, -1, "QsoYouReport");
            m_DataAdapterQSO.InsertCommand.Parameters.Add("@QsoYouQsl", SqlDbType.VarChar, 20, "QsoYouQsl");
            m_DataAdapterQSO.InsertCommand.Parameters.Add("@QsoYouQslManager", SqlDbType.VarChar, 50, "QsoYouQslManager");
            m_DataAdapterQSO.InsertCommand.Parameters.Add("@QsoYouContestCode", SqlDbType.VarChar, 50, "QsoYouContestCode");
            m_DataAdapterQSO.InsertCommand.Parameters.Add("@QsoMyOperator", SqlDbType.VarChar, 12, "QsoMyOperator");
            m_DataAdapterQSO.InsertCommand.Parameters.Add("@QsoMyReport", SqlDbType.Int, -1, "QsoMyReport");
            m_DataAdapterQSO.InsertCommand.Parameters.Add("@QsoMyQsl", SqlDbType.VarChar, 20, "QsoMyQsl");
            m_DataAdapterQSO.InsertCommand.Parameters.Add("@QsoMyContestCode", SqlDbType.VarChar, 50, "QsoMyContestCode");
            m_DataAdapterQSO.InsertCommand.Parameters.Add("@QsoPoints", SqlDbType.Int, -1, "QsoPoints");
            m_DataAdapterQSO.InsertCommand.Parameters.Add("@QsoPower", SqlDbType.Decimal, -1, "QsoPower");
            m_DataAdapterQSO.InsertCommand.Parameters.Add("@QsoTx", SqlDbType.VarChar, 50, "QsoTx");
            m_DataAdapterQSO.InsertCommand.Parameters.Add("@QsoDXCC", SqlDbType.NChar, 6, "QsoDXCC");
            m_DataAdapterQSO.InsertCommand.Parameters.Add("@QsoCQZone", SqlDbType.Int, -1, "QsoCQZone");
            m_DataAdapterQSO.InsertCommand.Parameters.Add("@QsoIOTA", SqlDbType.NChar, 6, "QsoIOTA");
            m_DataAdapterQSO.InsertCommand.Parameters.Add("@QsoRemarks", SqlDbType.Text, -1, "QsoRemarks");

            m_DataAdapterQSO.UpdateCommand = new SqlCommand("UPDATE [QSOS] SET QsoNum = @QsoNum, QsoDate = @QsoDate, QsoFrequency = @QsoFrequency, QsoMode = @QsoMode, QsoCall = @QsoCall, QsoYouGrid = @QsoYouGrid, QsoYouQTH = @QsoYouQTH, QsoYouReport = @QsoYouReport, QsoYouQsl = @QsoYouQsl, QsoYouQslManager = @QsoYouQslManager, QsoYouContestCode = @QsoYouContestCode, QsoMyOperator = @QsoMyOperator, QsoMyReport = @QsoMyReport, QsoMyQsl = @QsoMyQsl, QsoMyContestCode = @QsoMyContestCode, QsoPoints = @QsoPoints, QsoPower = @QsoPower, QsoTx = @QsoTx, QsoDXCC = @QsoDXCC, QsoCQZone = @QsoCQZone, QsoIOTA = @QsoIOTA, QsoRemarks = @QsoRemarks WHERE QsoID = @QsoID", m_Connection);
            m_DataAdapterQSO.UpdateCommand.Parameters.Add("@QsoNum", SqlDbType.Int, 4, "QsoNum");
            m_DataAdapterQSO.UpdateCommand.Parameters.Add("@QsoDate", SqlDbType.DateTime, -1, "QsoDate");
            m_DataAdapterQSO.UpdateCommand.Parameters.Add("@QsoFrequency", SqlDbType.Decimal, -1, "QsoFrequency");
            m_DataAdapterQSO.UpdateCommand.Parameters.Add("@QsoMode", SqlDbType.VarChar, 10, "QsoMode");
            m_DataAdapterQSO.UpdateCommand.Parameters.Add("@QsoCall", SqlDbType.VarChar, 10, "QsoCall");
            m_DataAdapterQSO.UpdateCommand.Parameters.Add("@QsoYouGrid", SqlDbType.VarChar, 10, "QsoYouGrid");
            m_DataAdapterQSO.UpdateCommand.Parameters.Add("@QsoYouQTH", SqlDbType.VarChar, 10, "QsoYouQTH");
            m_DataAdapterQSO.UpdateCommand.Parameters.Add("@QsoYouReport", SqlDbType.Int, -1, "QsoYouReport");
            m_DataAdapterQSO.UpdateCommand.Parameters.Add("@QsoYouQsl", SqlDbType.VarChar, 20, "QsoYouQsl");
            m_DataAdapterQSO.UpdateCommand.Parameters.Add("@QsoYouQslManager", SqlDbType.VarChar, 50, "QsoYouQslManager");
            m_DataAdapterQSO.UpdateCommand.Parameters.Add("@QsoYouContestCode", SqlDbType.VarChar, 50, "QsoYouContestCode");
            m_DataAdapterQSO.UpdateCommand.Parameters.Add("@QsoMyOperator", SqlDbType.VarChar, 12, "QsoMyOperator");
            m_DataAdapterQSO.UpdateCommand.Parameters.Add("@QsoMyReport", SqlDbType.Int, -1, "QsoMyReport");
            m_DataAdapterQSO.UpdateCommand.Parameters.Add("@QsoMyQsl", SqlDbType.VarChar, 20, "QsoMyQsl");
            m_DataAdapterQSO.UpdateCommand.Parameters.Add("@QsoMyContestCode", SqlDbType.VarChar, 50, "QsoMyContestCode");
            m_DataAdapterQSO.UpdateCommand.Parameters.Add("@QsoPoints", SqlDbType.Int, -1, "QsoPoints");
            m_DataAdapterQSO.UpdateCommand.Parameters.Add("@QsoPower", SqlDbType.Decimal, -1, "QsoPower");
            m_DataAdapterQSO.UpdateCommand.Parameters.Add("@QsoTx", SqlDbType.VarChar, 50, "QsoTx");
            m_DataAdapterQSO.UpdateCommand.Parameters.Add("@QsoDXCC", SqlDbType.NChar, 6, "QsoDXCC");
            m_DataAdapterQSO.UpdateCommand.Parameters.Add("@QsoCQZone", SqlDbType.Int, -1, "QsoCQZone");
            m_DataAdapterQSO.UpdateCommand.Parameters.Add("@QsoIOTA", SqlDbType.NChar, 6, "QsoIOTA");
            m_DataAdapterQSO.UpdateCommand.Parameters.Add("@QsoRemarks", SqlDbType.Text, -1, "QsoRemarks");
            m_DataAdapterQSO.UpdateCommand.Parameters.Add("@QsoID", SqlDbType.Int, -1, "QsoID");

            m_DataAdapterQSO.DeleteCommand = new SqlCommand("DELETE FROM QSOS WHERE QsoID = @QsoID", m_Connection);
            m_DataAdapterQSO.DeleteCommand.Parameters.Add("@QsoID", SqlDbType.Int, -1, "QsoID");


            m_DataAdapterCallbook = new SqlDataAdapter("SELECT * FROM Callbook WHERE [CALL] LIKE @CALL", m_Connection);

            m_DataAdapterCallbook.SelectCommand.Parameters.Add("@CALL", SqlDbType.VarChar, 12, "CALL");

            m_DataAdapterCallbook.InsertCommand = new SqlCommand("INSERT INTO Callbook ([CALL], [TITUL], [FAM_NAME], [NAME], [STREET], [CITY], [POST_CODE], [E_MAIL], [GRID], [CLASS], [REMARKS]) VALUES (@CALL, @TITUL, @FAM_NAME, @NAME, @STREET, @CITY, @POST_CODE, @E_MAIL, @GRID, @CLASS, @REMARKS)", m_Connection);
            m_DataAdapterCallbook.InsertCommand.Parameters.Add("@CALL", SqlDbType.VarChar, 12, "CALL");
            m_DataAdapterCallbook.InsertCommand.Parameters.Add("@TITUL", SqlDbType.VarChar, 12, "TITUL");
            m_DataAdapterCallbook.InsertCommand.Parameters.Add("@FAM_NAME", SqlDbType.VarChar, 50, "FAM_NAME");
            m_DataAdapterCallbook.InsertCommand.Parameters.Add("@NAME", SqlDbType.VarChar, 50, "NAME");
            m_DataAdapterCallbook.InsertCommand.Parameters.Add("@STREET", SqlDbType.VarChar, 150, "STREET");
            m_DataAdapterCallbook.InsertCommand.Parameters.Add("@CITY", SqlDbType.VarChar, 150, "CITY");
            m_DataAdapterCallbook.InsertCommand.Parameters.Add("@POST_CODE", SqlDbType.VarChar, 10, "POST_CODE");
            m_DataAdapterCallbook.InsertCommand.Parameters.Add("@E_MAIL", SqlDbType.VarChar, 150, "E_MAIL");
            m_DataAdapterCallbook.InsertCommand.Parameters.Add("@GRID", SqlDbType.VarChar, 10, "GRID");
            m_DataAdapterCallbook.InsertCommand.Parameters.Add("@CLASS", SqlDbType.VarChar, 5, "CLASS");
            m_DataAdapterCallbook.InsertCommand.Parameters.Add("@REMARKS", SqlDbType.VarChar, -1, "REMARKS");

            m_DataAdapterCallbook.DeleteCommand = new SqlCommand("DELETE FROM Callbook", m_Connection);

        }

        /// <summary>
        /// Obsolete
        /// </summary>
        /// <param name="Call"></param>
        /// <returns></returns>
        public ERAD.DataSetErad.StationsRow GetStationRowFromCall(string Call)
        {
            return null;
            /*
            SqlCommand Command = new SqlCommand("SELECT * FROM [Stations] WHERE StationCall = @StationCall", m_Connection);
            Command.Parameters.Add("@StationCall", System.Data.SqlDbType.VarChar, 10, "StationCall");
            Command.Parameters["@StationCall"].Value = Call;
            SqlDataReader DataReader = Command.ExecuteReader();
            DataReader.Read();

            return (ERAD_2018.DataSetErad.StationsRow)DataReader[;
            */
        }

        /// <summary>
        /// Fill to station table
        /// </summary>
        /// <param name="Stations"></param>
        public void StationFill(ERAD.DataSetErad.StationsDataTable Stations)
        {
            m_DataAdapterStation.Fill(Stations);
        }

        /// <summary>
        /// Save data Station table to database
        /// </summary>
        /// <param name="DataSet"></param>
        public void StationUpdate(ERAD.DataSetErad DataSet)
        {
            try
            {
                m_DataAdapterStation.Update(DataSet, "Stations");
            }
            catch (Exception Chyba)
            {
                MessageBox.Show("Nedošlo k uložení dat do databáze.\n\n" + Chyba.Message, "Stanice", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        /// <summary>
        /// Save data Station table to database
        /// </summary>
        /// <param name="StationTable"></param>
        public void StationUpdate(ERAD.DataSetErad.StationsDataTable StationTable)
        {
            try
            {
                m_DataAdapterStation.Update(StationTable);
            }
            catch (Exception Chyba)
            {
                MessageBox.Show("Nedošlo k uložení dat do databáze.\n\n" + Chyba.Message, "Stanice", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        /// <summary>
        /// Folder update
        /// </summary>
        /// <param name="Folders"></param>
        public void FolderUpdate(ERAD.DataSetErad.FoldersDataTable Folders)
        {
            try
            {
                m_DataAdapterFolder.Update(Folders);
            }
            catch (Exception Chyba)
            {
                MessageBox.Show("Nedošlo k uložení dat do databáze\n\n" + Chyba.Message, "Složky", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        /// <summary>
        /// Fill table folders
        /// </summary>
        /// <param name="Folders"></param>
        /// <param name="Station"></param>
        public void FolderFill(ERAD.DataSetErad.FoldersDataTable Folders, string Station)
        {
            Folders.Rows.Clear();
            m_DataAdapterFolder.SelectCommand.Parameters["@Station"].Value = Station;
            m_DataAdapterFolder.Fill(Folders);
        }

        /// <summary>
        /// Fill table Qsos
        /// </summary>
        /// <param name="QSOs"></param>
        /// <param name="FolderID"></param>
        public void QsoFill(ERAD.DataSetErad.QSOSDataTable QSOs, int FolderID)
        {
            try
            {
                m_DataAdapterQSO.SelectCommand.CommandText = QsoSelectStandard;
                m_DataAdapterQSO.SelectCommand.Parameters["@QsoFolder"].Value = FolderID;
                m_DataAdapterQSO.Fill(QSOs);
            }
            catch (InvalidCastException Chyba)
            {
                MessageBox.Show(Chyba.Message);
            }
        }

        /// <summary>
        /// Fill table Qsos
        /// </summary>
        /// <param name="QSOs"></param>
        /// <param name="FolderID"></param>
        /// <param name="filtr"></param>
        public void QsoFill(ERAD.DataSetErad.QSOSDataTable QSOs, int FolderID, string filtr)
        {
            try
            {
                m_DataAdapterQSO.SelectCommand.CommandText = String.Format("SELECT * FROM QSOS WHERE QsoFolder = @QsoFolder AND ( {0} ) ORDER BY QsoDate DESC", filtr);
                m_DataAdapterQSO.SelectCommand.Parameters["@QsoFolder"].Value = FolderID;
                m_DataAdapterQSO.Fill(QSOs);
            }
            catch (InvalidCastException Chyba)
            {
                MessageBox.Show(Chyba.Message);
            }
        }

        /// <summary>
        /// Fill table Qsos
        /// </summary>
        /// <param name="QSOs"></param>
        /// <param name="StationCall"></param>
        public void QsoFill(ERAD.DataSetErad.QSOSDataTable QSOs, string StationCall)
        {
            QSOs.Rows.Clear();
            m_DataAdapterQSO.SelectCommand.Parameters["@StationCall"].Value = StationCall;
            m_DataAdapterQSO.Fill(QSOs);
        }

        /// <summary>
        /// Qso Update
        /// </summary>
        /// <param name="QSOs"></param>
        public void QsoUpdate(ERAD.DataSetErad.QSOSDataTable QSOs)
        {
            try
            {
                m_DataAdapterQSO.Update(QSOs);
            }
            catch (Exception Chyba)
            {
                MessageBox.Show("Spojení nebyla uložena do databáze!\n\n" + Chyba.Message, "QSO", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        /// <summary>
        /// Qso sum points
        /// </summary>
        /// <param name="FolderID"></param>
        /// <returns></returns>
        public int QsoSumPoints(int FolderID)
        {
            int Points = 0;

            SqlCommand Command = new SqlCommand("SELECT Sum(QsoPoints) FROM QSOS WHERE QsoFolder = @FolderID", m_Connection);
            Command.Parameters.AddWithValue("@FolderID", FolderID);

            try
            {
                Points = (int)Command.ExecuteScalar();
            }
            catch (Exception)
            {
                Points = 0;
            }

            return Points;
        }

        /// <summary>
        /// Obsolete
        /// </summary>
        /// <param name="Callbook"></param>
        public void CallbookFill(ERAD.DataSetErad.CallbookDataTable Callbook)
        {
            try
            {
                /*
                m_DataAdapterCallbook.SelectCommand.Parameters["@CALL"].Value = "";
                m_DataAdapterCallbook.Fill(Callbook);
                */
            }
            catch (Exception Chyba)
            {
                MessageBox.Show("Callbook nebyl načten z databáze!\n\n" + Chyba.Message, "Callbook", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        /// <summary>
        /// Get info from table Callbook
        /// </summary>
        /// <param name="Call"></param>
        /// <returns></returns>
        public ERAD.Data.CallbookRadek GetInfoFromCall(string Call)
        {
            ERAD.Data.CallbookRadek CallInfo = new ERAD.Data.CallbookRadek();

            SqlCommand Command = new SqlCommand("SELECT * FROM Callbook WHERE [CALL] LIKE @CALL", m_Connection);
            Command.Parameters.Add("@CALL", SqlDbType.VarChar, 12, "CALL");
            Command.Parameters["@CALL"].Value = Call;

            SqlDataReader DataReader = Command.ExecuteReader();

            DataReader.Read();

            if (!DataReader.HasRows)
            {
                DataReader.Close();
                return null;
            }

            CallInfo.CALL = DataReader["CALL"].ToString();
            CallInfo.TITUL = DataReader["TITUL"].ToString();
            CallInfo.FAM_NAME = DataReader["FAM_NAME"].ToString();
            CallInfo.NAME = DataReader["NAME"].ToString();
            CallInfo.STREET = DataReader["STREET"].ToString();
            CallInfo.CITY = DataReader["CITY"].ToString();
            CallInfo.POST_CODE = DataReader["POST_CODE"].ToString();
            CallInfo.E_MAIL = DataReader["E_MAIL"].ToString();
            CallInfo.GRID = DataReader["GRID"].ToString();
            CallInfo.CLASS = DataReader["CLASS"].ToString();
            CallInfo.REMARKS = DataReader["REMARKS"].ToString();

            DataReader.Close();

            return CallInfo;
        }

        /// <summary>
        /// Callbook update data do database
        /// </summary>
        /// <param name="Callbook"></param>
        public void CallbookUpdate(ERAD.DataSetErad.CallbookDataTable Callbook)
        {
            try
            {
                int zaznamu = m_DataAdapterCallbook.Update(Callbook);

                MessageBox.Show(String.Format("Bylo importováno celkem {0}", zaznamu), "Callbook", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception Chyba)
            {
                MessageBox.Show("Callbook nebyl uložen do databáze!\n\n" + Chyba.Message, "Callbook", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        /// <summary>
        /// Delete all data from table Callbook
        /// </summary>
        public void CallbookDelete()
        {
            m_DataAdapterCallbook.DeleteCommand.ExecuteNonQuery();
        }
    }
}
