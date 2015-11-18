using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Services;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Threading;
using System.ComponentModel;
using System.Drawing; 

namespace ChartFTP
{
    public partial class index : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!this.IsPostBack)
            {
                cbPeriods.Items.Insert(0, new ListItem("200日平均"));
                cbPeriods.Items.Insert(0, new ListItem("150日平均"));
                cbPeriods.Items.Insert(0, new ListItem("75日平均"));
                cbPeriods.Items.Insert(0, new ListItem("25日平均"));
                cbPeriods.Items.Insert(0, new ListItem("5日平均"));

                //dvleftColumn.Visible = false;
                //dvrightColumn.Visible = false;

                //ClientScript.RegisterStartupScript(this.GetType(), "fun", "ReportStatus();", true);

                GetReports();
                GetParameter();
                GetMarketMaster();

                spRSI.Visible = false;
                sp25SMA.Visible = false;
                spVR25.Visible = false;
                spPriceRange.Visible = false;
                txtRSIUpper.Visible = false;
                txtVR25Upper.Visible = false;
                txtPriceRangeUpper.Visible = false;
                txt25SMAUpper.Visible = false;
            }
        }

        public static int period1D = 0;
        public static int period5D = 0;
        public static int period25D = 0;
        public static int period75D = 0;
        public static int period150D = 0;
        public static int period200D = 0;

        public BackgroundWorker bwProcess = null;

        public static string chartData = string.Empty;
        public static string chartData5D = string.Empty;
        public static string chartData25D = string.Empty;
        public static string chartData75D = string.Empty;
        public static string chartData150D = string.Empty;
        public static string chartData200D = string.Empty;
        public static string chartDataVol = string.Empty;
        public static DataTable dtReport = null;
        public static DataTable dtParametersK2C = null;
        public static DataTable dtParametersW2D = null;
        public static DataTable dtParametersP = null;
        public static DataTable dtParameters = null;
        private const string ASCENDING = " ASC";
        private const string DESCENDING = " DESC";
        public System.Web.UI.WebControls.Image sortImage = new System.Web.UI.WebControls.Image();

        public SortDirection GridViewSortDirection
        {
            get
            {
                if (ViewState["sortDirection"] == null)
                    ViewState["sortDirection"] = SortDirection.Ascending;

                return (SortDirection)ViewState["sortDirection"];
            }
            set { ViewState["sortDirection"] = value; }
        }

        protected void btnScripList_Click(object sender, EventArgs e)
        {
            try
            {
                List<ListItem> selected = new List<ListItem>();
                foreach (ListItem item in cbPeriods.Items)
                {
                    if (item.Selected)
                        selected.Add(item);
                }

                if ((selected.Count >= 2) && (txtFluctn.Text.Length > 0))
                {


                    MySql.Data.MySqlClient.MySqlConnection conn = new MySqlConnection(GetConnectionString());
                    MySqlCommand cmd = null;
                    var netChhng = txtFluctn.Text.Length > 0 ? txtFluctn.Text : "0";
                    conn.Open();
                    cmd = new MySqlCommand();

                    cmd.Connection = conn;

                    cmd.CommandText = "SPSearchStockByfluctuationPercentage";
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.AddWithValue("i_5Days", period5D);
                    cmd.Parameters["i_5Days"].Direction = ParameterDirection.Input;
                    cmd.Parameters["i_5Days"].MySqlDbType = MySqlDbType.Int32;

                    cmd.Parameters.AddWithValue("i_25Days", period25D);
                    cmd.Parameters["i_25Days"].Direction = ParameterDirection.Input;
                    cmd.Parameters["i_25Days"].MySqlDbType = MySqlDbType.Int32;

                    cmd.Parameters.AddWithValue("i_75Days", period75D);
                    cmd.Parameters["i_75Days"].Direction = ParameterDirection.Input;
                    cmd.Parameters["i_75Days"].MySqlDbType = MySqlDbType.Int32;

                    cmd.Parameters.AddWithValue("i_150Days", period150D);
                    cmd.Parameters["i_150Days"].Direction = ParameterDirection.Input;
                    cmd.Parameters["i_150Days"].MySqlDbType = MySqlDbType.Int32;

                    cmd.Parameters.AddWithValue("i_200Days", period200D);
                    cmd.Parameters["i_200Days"].Direction = ParameterDirection.Input;
                    cmd.Parameters["i_200Days"].MySqlDbType = MySqlDbType.Int32;

                    cmd.Parameters.AddWithValue("i_Percentage", netChhng);
                    cmd.Parameters["i_Percentage"].Direction = ParameterDirection.Input;
                    cmd.Parameters["i_Percentage"].MySqlDbType = MySqlDbType.Decimal;

                    cmd.Parameters.Add(new MySqlParameter("o_Flag", MySqlDbType.String));
                    cmd.Parameters["o_Flag"].Direction = ParameterDirection.Output;

                    cmd.Parameters.Add(new MySqlParameter("o_ErrorCode", MySqlDbType.String));
                    cmd.Parameters["o_ErrorCode"].Direction = ParameterDirection.Output;

                    cmd.Parameters.Add(new MySqlParameter("o_ErrorDescription", MySqlDbType.String));
                    cmd.Parameters["o_ErrorDescription"].Direction = ParameterDirection.Output;

                    MySqlDataAdapter adp = new MySqlDataAdapter(cmd);
                    DataSet ds = new DataSet();
                    adp.Fill(ds);
                    dgScripList.DataSource = ds.Tables[0];
                    dgScripList.DataBind();

                    conn.Close();
                    //dvleftColumn.Visible = true;
                }
                else
                    ScriptManager.RegisterClientScriptBlock(this, this.GetType(), "alertMessage", "alert('乖離率平均の値を2つ以上を選択してください。')", true);

            }
            catch (MySql.Data.MySqlClient.MySqlException ex)
            {
            }
        }

        protected void Upload(object sender, EventArgs e)
        {
            if (FileUpload1.PostedFile.FileName.Length > 0)
            {
                //Upload and save the file
                string csvPath = Server.MapPath("~/Files/") + Path.GetFileName(FileUpload1.PostedFile.FileName);
                FileUpload1.SaveAs(csvPath);

                DataTable dt = new DataTable();
                dt.Columns.AddRange(new DataColumn[9] { 
            new DataColumn("MarketCode", typeof(string)),
            new DataColumn("StockCode", typeof(string)),
            new DataColumn("SockName",typeof(string)) ,
            new DataColumn("PriceOpen",typeof(string)) ,
            new DataColumn("PriceHigh",typeof(string)) ,
            new DataColumn("PriceLow",typeof(string)) ,
            new DataColumn("PriceClose",typeof(string)),
            new DataColumn("Volume",typeof(string)),
            new DataColumn("PriceDate",typeof(string)) });


                string csvData = File.ReadAllText(csvPath);
                if (myDateField.Value.Length > 0)
                {
                    if (ValidateCsv(csvData))
                    {

                        var rows = csvData.Split('\n');
                        var rowLength = rows.Length;
                        for (int j = 3; j < rowLength; j++)
                        {
                            if (!string.IsNullOrEmpty(rows[j]))
                            {
                                dt.Rows.Add();
                                
                                List<string> cells = new List<string>();
                                cells = rows[j].Split(',').ToList();
                                var cellLength = 0;

                                if (cells.Count == 8)
                                {
                                    cells.Add(myDateField.Value);
                                    cellLength = cells.Count;
                                }
                                else
                                {
                                    cells[8] = myDateField.Value;
                                    cellLength = cells.Count - 1;
                                }
                                
                                
                                for (int i = 0; i < cellLength; i++)
                                {
                                    if (!String.IsNullOrEmpty(cells[i]))
                                        dt.Rows[dt.Rows.Count - 1][i] = cells[i];
                                }
                            }
                        }

                        MySql.Data.MySqlClient.MySqlConnection conn = new MySqlConnection(GetConnectionString());
                        MySqlCommand cmd = null;

                        conn.Open();

                        try
                        {
                            foreach (DataRow row in dt.Rows)
                            {
                                //Console.WriteLine("Connecting to MySQL...");
                                cmd = new MySqlCommand();

                                cmd.Connection = conn;

                                cmd.CommandText = "SPInsertStockTransaction";
                                cmd.CommandType = CommandType.StoredProcedure;

                                if (!String.IsNullOrEmpty(row[0].ToString()) && !String.IsNullOrEmpty(row[1].ToString()))
                                {

                                    cmd.Parameters.AddWithValue("i_MarketCode", row[0]);
                                    cmd.Parameters["i_MarketCode"].Direction = ParameterDirection.Input;
                                    cmd.Parameters["i_MarketCode"].MySqlDbType = MySqlDbType.VarChar;

                                    cmd.Parameters.AddWithValue("i_StockCode", row[1]);
                                    cmd.Parameters["i_StockCode"].MySqlDbType = MySqlDbType.VarChar;
                                    cmd.Parameters["i_StockCode"].Direction = ParameterDirection.Input;

                                    cmd.Parameters.AddWithValue("i_PriceDate", DateTime.Parse(row[8].ToString()).ToString("yyyy-MM-dd"));
                                    cmd.Parameters["i_PriceDate"].MySqlDbType = MySqlDbType.Date;
                                    cmd.Parameters["i_PriceDate"].Direction = ParameterDirection.Input;

                                    cmd.Parameters.AddWithValue("i_PriceOpen", (row[3]));
                                    cmd.Parameters["i_PriceOpen"].MySqlDbType = MySqlDbType.Decimal;
                                    cmd.Parameters["i_PriceOpen"].Direction = ParameterDirection.Input;

                                    cmd.Parameters.AddWithValue("i_PriceHigh", (row[4]));
                                    cmd.Parameters["i_PriceHigh"].MySqlDbType = MySqlDbType.Decimal;
                                    cmd.Parameters["i_PriceHigh"].Direction = ParameterDirection.Input;

                                    cmd.Parameters.AddWithValue("i_PriceLow", (row[5]));
                                    cmd.Parameters["i_PriceLow"].MySqlDbType = MySqlDbType.Decimal;
                                    cmd.Parameters["i_PriceLow"].Direction = ParameterDirection.Input;

                                    cmd.Parameters.AddWithValue("i_PriceClose", (row[6]));
                                    cmd.Parameters["i_PriceClose"].MySqlDbType = MySqlDbType.Decimal;
                                    cmd.Parameters["i_PriceClose"].Direction = ParameterDirection.Input;

                                    cmd.Parameters.AddWithValue("i_PriceVolumn", (row[7]));
                                    cmd.Parameters["i_PriceVolumn"].MySqlDbType = MySqlDbType.Decimal;
                                    cmd.Parameters["i_PriceVolumn"].Direction = ParameterDirection.Input;

                                    //Add the output parameter to the command object

                                    cmd.Parameters.Add(new MySqlParameter("o_Flag", MySqlDbType.String));
                                    cmd.Parameters["o_Flag"].Direction = ParameterDirection.Output;

                                    cmd.Parameters.Add(new MySqlParameter("o_ErrorCode", MySqlDbType.String));
                                    cmd.Parameters["o_ErrorCode"].Direction = ParameterDirection.Output;

                                    cmd.Parameters.Add(new MySqlParameter("o_ErrorDescription", MySqlDbType.String));
                                    cmd.Parameters["o_ErrorDescription"].Direction = ParameterDirection.Output;
                                    cmd.ExecuteNonQuery();

                                   
                                }
                                else
                                {

                                }

                            }

                            try
                            {
                                // MySqlCommand cmd = null;
                                cmd = new MySqlCommand();
                                cmd.Connection = conn;
                                cmd.CommandText = "UpdateConfigurationTable";
                                cmd.CommandType = CommandType.StoredProcedure;

                                cmd.ExecuteNonQuery();
                            }
                            catch (MySql.Data.MySqlClient.MySqlException exi)
                            {
                                conn.Close();

                            }
                            //cmd.Dispose();
                            conn.Close();
                            errorMsg.InnerText = "ファイルをアップロードしました。";
                        }
                        catch (MySql.Data.MySqlClient.MySqlException ex)
                        {
                            if (ex.Message == "Market Code does not exists")
                            {
                                errorMsg.InnerText = "市場名が存在しません。";
                            }
                            else if (ex.Message == "Stock Code does not exists")
                            {
                                errorMsg.InnerText = "銘柄コードが存在しません。";
                            }
                            else if (ex.Message == "Column 'MarketCode' cannot be null")
                            {
                                errorMsg.InnerText = "未登録の市場が見つかりました。ファイルアップロードができません。";
                            }
                            else
                                errorMsg.InnerText = "エラーが発生しました。";// ex.Message;
                            //conn.Close();
                            //break;

                            //errorMsg.InnerText = "ファイルアップロードを成功しました。";

                            try
                            {
                                // MySqlCommand cmd = null;
                                cmd = new MySqlCommand();
                                cmd.Connection = conn;
                                cmd.CommandText = "SPDeleteStockTransaction";
                                cmd.CommandType = CommandType.StoredProcedure;

                                cmd.Parameters.AddWithValue("i_Pricedate", DateTime.Parse(myDateField.Value).ToString("yyyy-MM-dd"));
                                cmd.Parameters["i_Pricedate"].Direction = ParameterDirection.Input;
                                cmd.Parameters["i_Pricedate"].MySqlDbType = MySqlDbType.VarChar;

                                cmd.Parameters.Add(new MySqlParameter("o_Flag", MySqlDbType.String));
                                cmd.Parameters["o_Flag"].Direction = ParameterDirection.Output;

                                cmd.Parameters.Add(new MySqlParameter("o_ErrorCode", MySqlDbType.String));
                                cmd.Parameters["o_ErrorCode"].Direction = ParameterDirection.Output;

                                cmd.Parameters.Add(new MySqlParameter("o_ErrorDescription", MySqlDbType.String));
                                cmd.Parameters["o_ErrorDescription"].Direction = ParameterDirection.Output;

                                cmd.ExecuteNonQuery();
                                conn.Close();
                            }
                            catch (MySql.Data.MySqlClient.MySqlException exi)
                            {
                                conn.Close();
                            }
                        }

                    }
                    else
                    {
                        errorMsg.InnerText = "ファイルフォーマットは無効です。";
                    }
                }
                else
                {
                    errorMsg.InnerText = "正しい日付を指定してください。";
                }
            }
            else
            {
                errorMsg.InnerText = "ファイルを指定してください。";
            }

        }

        public static string GetConnectionString()
        {
            string connStr = String.Format("server={0};user id={1}; password={2};" +
              "database=ing; pooling=false;", "localhost",
              "root", "root");

            return connStr;
        }

        public static bool ValidateCsv(string fileContents)
        {
            var fileLines = fileContents.Split(new string[] { "\r\n", "\n" }, StringSplitOptions.None);

            if (fileLines.Count() < 2) { }
            //fail - no data row.
            var isValid = true;
            //isValid = ValidateColumnHeaders(fileLines[1]);

            //isValid = ValidateRows(fileLines.Skip(3));

            return isValid;
        }

        public static bool ValidateColumnHeaders(string header)
        {
            return header.Trim().Replace(" ", "").ToLower() == "\"市場\",\"銘柄コード\",\"銘柄名\",\"始値\",\"高値\",\"安値\",\"終値\",\"出来高\",\"\",\"\",\"\",\"\",\"\",\"\",\"\",\"\",\"\",\"\",\"\",\"\",\"\",\"\",\"\"";
        }

        public static bool ValidateRows(IEnumerable<string> rows)
        {
            var count = 0;
            foreach (var row in rows)
            {
                var cells = row.Split(',');

                //check if the number of cells is correct
                if (cells.Length != 23)
                    return false;
                count++;

                if (count > 2)
                    break;
                ////ensure gender is correct
                //if (cells[3] != "M" && cells[3] != "F")
                //    return false;

                //perform any additional row checks relevant to your domain

            }
            return true;
        }

        protected void datePicker_TextChanged(object sender, EventArgs e)
        {

        }

        public void GetReports()
        {
            int RSILimiter = 0, PriceRangeLimiter = 0, SMALimiter = 0, VR25Limiter = 0;
            bool isValid = true;
            string errMsg = string.Empty;

            PriceRangeLimiter = Convert.ToInt32(ddlPriceRange.SelectedValue);
            if (PriceRangeLimiter == 3) 
            {
                var low = Convert.ToDecimal(txtPriceRangeLower.Text);
                var high = Convert.ToDecimal(txtPriceRangeUpper.Text);
                if (low > high)
                {
                    errMsg = "値幅の値を正しく入力してください。";
                    isValid = false;
                }
            }

            SMALimiter = Convert.ToInt32(ddl25SMAFilter.SelectedValue);
            if (SMALimiter == 3)
            {
                var low = Convert.ToDecimal(txt25SMALower.Text);
                var high = Convert.ToDecimal(txt25SMAUpper.Text);
                if (low > high)
                {
                    errMsg = "25 SMAの値を正しく入力してください。";
                    isValid = false;
                }
            }
            
            VR25Limiter = Convert.ToInt32(ddlVR25Filter.SelectedValue);
            if (VR25Limiter == 3)
            {
                var low = Convert.ToDecimal(txtVR25Lower.Text);
                var high = Convert.ToDecimal(txtVR25Upper.Text);
                if (low > high)
                {
                    errMsg = "VR(25)の値を正しく入力してください。";
                    isValid = false;
                }
            }

            RSILimiter = Convert.ToInt32(ddlRSIFilter.SelectedValue);
            if (RSILimiter == 3)
            {
                var low = Convert.ToDecimal(txtRSILower.Text);
                var high = Convert.ToDecimal(txtRSIUpper.Text);
                if (low > high)
                {
                    errMsg = "RSI(14)の値を正しく入力してください。";
                    isValid = false;
                }
            }



            if (isValid)
            {
                MySql.Data.MySqlClient.MySqlConnection conn = new MySqlConnection(GetConnectionString());
                try
                {
                    MySqlCommand cmd = null;
                    conn.Open();
                    cmd = new MySqlCommand();
                    cmd.Connection = conn;
                    cmd.CommandText = "SPGetResultSet";
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.AddWithValue("i_Symbol", ddlFilter.SelectedItem);
                    cmd.Parameters["i_Symbol"].Direction = ParameterDirection.Input;
                    cmd.Parameters["i_Symbol"].MySqlDbType = MySqlDbType.VarChar;

                    cmd.Parameters.AddWithValue("i_RSI14LowerLimit", txtRSILower.Text);
                    cmd.Parameters["i_RSI14LowerLimit"].Direction = ParameterDirection.Input;
                    cmd.Parameters["i_RSI14LowerLimit"].MySqlDbType = MySqlDbType.VarChar;

                    cmd.Parameters.AddWithValue("i_RSI14UpperLimit", txtRSIUpper.Text);
                    cmd.Parameters["i_RSI14UpperLimit"].Direction = ParameterDirection.Input;
                    cmd.Parameters["i_RSI14UpperLimit"].MySqlDbType = MySqlDbType.VarChar;

                    cmd.Parameters.AddWithValue("i_RSI14_LimitFlag", RSILimiter);
                    cmd.Parameters["i_RSI14_LimitFlag"].Direction = ParameterDirection.Input;
                    cmd.Parameters["i_RSI14_LimitFlag"].MySqlDbType = MySqlDbType.Int32;

                    cmd.Parameters.AddWithValue("i_25SMALowerLimit", txt25SMALower.Text);
                    cmd.Parameters["i_25SMALowerLimit"].Direction = ParameterDirection.Input;
                    cmd.Parameters["i_25SMALowerLimit"].MySqlDbType = MySqlDbType.VarChar;

                    cmd.Parameters.AddWithValue("i_25SMAUpperLimit", txt25SMAUpper.Text);
                    cmd.Parameters["i_25SMAUpperLimit"].Direction = ParameterDirection.Input;
                    cmd.Parameters["i_25SMAUpperLimit"].MySqlDbType = MySqlDbType.VarChar;

                    cmd.Parameters.AddWithValue("i_25SMA_LimitFlag", SMALimiter);
                    cmd.Parameters["i_25SMA_LimitFlag"].Direction = ParameterDirection.Input;
                    cmd.Parameters["i_25SMA_LimitFlag"].MySqlDbType = MySqlDbType.Int32;

                    cmd.Parameters.AddWithValue("i_PriceRangeLowerLimit", txtPriceRangeLower.Text);
                    cmd.Parameters["i_PriceRangeLowerLimit"].Direction = ParameterDirection.Input;
                    cmd.Parameters["i_PriceRangeLowerLimit"].MySqlDbType = MySqlDbType.VarChar;

                    cmd.Parameters.AddWithValue("i_PriceRangeUpperLimit", txtPriceRangeUpper.Text);
                    cmd.Parameters["i_PriceRangeUpperLimit"].Direction = ParameterDirection.Input;
                    cmd.Parameters["i_PriceRangeUpperLimit"].MySqlDbType = MySqlDbType.VarChar;

                    cmd.Parameters.AddWithValue("i_PriceRange_LimitFlag", PriceRangeLimiter);
                    cmd.Parameters["i_PriceRange_LimitFlag"].Direction = ParameterDirection.Input;
                    cmd.Parameters["i_PriceRange_LimitFlag"].MySqlDbType = MySqlDbType.Int32;

                    cmd.Parameters.AddWithValue("i_VR25LowerLimit", txtVR25Lower.Text);
                    cmd.Parameters["i_VR25LowerLimit"].Direction = ParameterDirection.Input;
                    cmd.Parameters["i_VR25LowerLimit"].MySqlDbType = MySqlDbType.VarChar;

                    cmd.Parameters.AddWithValue("i_VR25UpperLimit", txtVR25Upper.Text);
                    cmd.Parameters["i_VR25UpperLimit"].Direction = ParameterDirection.Input;
                    cmd.Parameters["i_VR25UpperLimit"].MySqlDbType = MySqlDbType.VarChar;

                    cmd.Parameters.AddWithValue("i_VR25_LimitFlag", VR25Limiter);
                    cmd.Parameters["i_VR25_LimitFlag"].Direction = ParameterDirection.Input;
                    cmd.Parameters["i_VR25_LimitFlag"].MySqlDbType = MySqlDbType.Int32;

                    MySqlDataAdapter adp = new MySqlDataAdapter(cmd);
                    DataSet ds = new DataSet();
                    adp.Fill(ds);
                    dtReport = ds.Tables[0];
                    dgMW.DataSource = ds.Tables[0];
                    dgMW.DataBind();
                    conn.Close();
                }
                catch (MySql.Data.MySqlClient.MySqlException ex)
                {
                    conn.Close();
                }
            }
            else
            {
                ScriptManager.RegisterClientScriptBlock(this, this.GetType(), "alertMessage", "alert('" + errMsg + "')", true);
            }
        }

        public void ResetReportFilters() 
        {
            ddlFilter.SelectedIndex = 0;
            ddlRSIFilter.SelectedIndex = 0;
            ddlPriceRange.SelectedIndex = 0;
            ddl25SMAFilter.SelectedIndex = 0;
            ddlVR25Filter.SelectedIndex = 0;
            txtRSILower.Text = string.Empty;
            txtRSIUpper.Text = string.Empty;
            txtRSIUpper.Visible = false;
            spRSI.Visible = false;
            txtPriceRangeLower.Text = string.Empty;
            txtPriceRangeUpper.Text = string.Empty;
            txtPriceRangeUpper.Visible = false;
            spPriceRange.Visible = false;
            txt25SMALower.Text = string.Empty;
            txt25SMAUpper.Text = string.Empty;
            txt25SMAUpper.Visible = false;
            sp25SMA.Visible = false;
            txtVR25Lower.Text = string.Empty;
            txtVR25Upper.Text = string.Empty;
            txtVR25Upper.Visible = false;
            spVR25.Visible = false;
        }

        internal Dictionary<string, Decimal> GetDict(DataTable dt)
        {
            return dt.AsEnumerable().ToDictionary<DataRow, string, Decimal>(row => row.Field<string>(0), row => row.Field<Decimal>(1));
        }

        public void GetParameter() 
        {
            MySql.Data.MySqlClient.MySqlConnection conn = new MySqlConnection(GetConnectionString());
            try
            {
                MySqlCommand cmd = null;
                conn.Open();
                cmd = new MySqlCommand();
                cmd.Connection = conn;
                cmd.CommandText = "SPGetParameter";
                cmd.CommandType = CommandType.StoredProcedure;
                
                MySqlDataAdapter adp = new MySqlDataAdapter(cmd);
                DataSet ds = new DataSet();
                adp.Fill(ds);
                dtParametersK2C = ds.Tables[0];
                dtParametersW2D = ds.Tables[1];
                dtParametersP = ds.Tables[2];

                dtParameters = new DataTable();
                dtParameters.TableName = "FreshTable";
                dtParameters.Columns.Add("Name", typeof(string));
                dtParameters.Columns.Add("Value", typeof(Decimal));

                dtParameters.Merge(dtParametersK2C);
                dtParameters.Merge(dtParametersW2D);
                dtParameters.Merge(dtParametersP);

                dgParameterK2C.DataSource = ds.Tables[0];
                dgParameterW2D.DataSource = ds.Tables[1];
                dgParameterP.DataSource = ds.Tables[2];
                dgParameterK2C.DataBind();
                dgParameterW2D.DataBind();
                dgParameterP.DataBind();
                conn.Close();
            }
            catch (MySql.Data.MySqlClient.MySqlException ex)
            {
                conn.Close();
            }
            
        }

        public void GetMarketMaster()
        {
            MySql.Data.MySqlClient.MySqlConnection conn = new MySqlConnection(GetConnectionString());
            try
            {
                MySqlCommand cmd = null;
                conn.Open();
                cmd = new MySqlCommand();
                cmd.Connection = conn;
                cmd.CommandText = "SPGetMarketMaster";
                cmd.CommandType = CommandType.StoredProcedure;

                MySqlDataAdapter adp = new MySqlDataAdapter(cmd);
                DataSet ds = new DataSet();
                adp.Fill(ds);

                ddlMarketName.DataSource = ds.Tables[0];
                ddlMarketName.DataTextField = "MarketName";
                ddlMarketName.DataBind();
                conn.Close();
            }
            catch (MySql.Data.MySqlClient.MySqlException ex)
            {
                conn.Close();
            }
        }

        protected void dgMW_PageIndexChanging(object sender, GridViewPageEventArgs e)
        {
            dgMW.PageIndex = e.NewPageIndex;
            GetReports();
        }

        public static string ConvertDataTableToHTML(DataTable dt)
        {
            string html = "<table class=\"table table-striped table-hover \">";
            //add header row
            html += "<tr>";
            for (int i = 0; i < dt.Columns.Count; i++)
                html += "<td>" + dt.Columns[i].ColumnName + "</td>";
            html += "</tr>";
            //add rows
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                html += "<tr>";
                for (int j = 0; j < dt.Columns.Count; j++)
                    html += "<td>" + dt.Rows[i][j].ToString() + "</td>";
                html += "</tr>";
            }
            html += "</table>";
            return html;
        }

        protected void dgScripList_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            if (e.CommandName == "Select")
            {

            }
        }

        protected void lnkDownload_Click(object sender, EventArgs e)
        {

            Button lnkbtn = sender as Button;
            GridViewRow gvrow = lnkbtn.NamingContainer as GridViewRow;
            string username = dgScripList.DataKeys[gvrow.RowIndex].Values.ToString();

            var marketCode = dgScripList.DataKeys[gvrow.RowIndex].Values[0];
            var stockCode = dgScripList.DataKeys[gvrow.RowIndex].Values[1];
            var period = 20;
            
            MySql.Data.MySqlClient.MySqlConnection conn = new MySqlConnection(GetConnectionString());
            try
            {

                MySqlCommand cmd = null;

                conn.Open();
                cmd = new MySqlCommand();

                cmd.Connection = conn;

                cmd.CommandText = "SPGetStockTransaction";
                cmd.CommandType = CommandType.StoredProcedure;

                cmd.Parameters.AddWithValue("i_MarketCode", marketCode);
                cmd.Parameters["i_MarketCode"].Direction = ParameterDirection.Input;
                cmd.Parameters["i_MarketCode"].MySqlDbType = MySqlDbType.VarChar;

                cmd.Parameters.AddWithValue("i_Days", period);
                cmd.Parameters["i_Days"].Direction = ParameterDirection.Input;
                cmd.Parameters["i_Days"].MySqlDbType = MySqlDbType.Int32;

                cmd.Parameters.AddWithValue("i_StockCode", stockCode);
                cmd.Parameters["i_StockCode"].Direction = ParameterDirection.Input;
                cmd.Parameters["i_StockCode"].MySqlDbType = MySqlDbType.VarChar;

                cmd.Parameters.Add(new MySqlParameter("o_Flag", MySqlDbType.String));
                cmd.Parameters["o_Flag"].Direction = ParameterDirection.Output;

                cmd.Parameters.Add(new MySqlParameter("o_ErrorCode", MySqlDbType.String));
                cmd.Parameters["o_ErrorCode"].Direction = ParameterDirection.Output;

                cmd.Parameters.Add(new MySqlParameter("o_ErrorDescription", MySqlDbType.String));
                cmd.Parameters["o_ErrorDescription"].Direction = ParameterDirection.Output;

                MySqlDataAdapter adp = new MySqlDataAdapter(cmd);
                DataSet ds = new DataSet();
                adp.Fill(ds);
                conn.Close();

                
                //var data1 = JSON_DataTable(ds.Tables[0]);
                var data2 = CreateJsonParameters(ds.Tables[0]);
                if (data2 != null)
                {
                    chartData = data2[0];// +"|" + data2[1] + "|" + data2[2] + "|" + data2[3] + "|" + data2[4] + "|" + data2[5] + "|" + data2[6];
                    chartData5D = data2[1];
                    chartData25D = data2[2];
                    chartData75D = data2[3];
                    chartData150D = data2[4];
                    chartData200D = data2[5];
                    chartDataVol = data2[6];
                }
                ClientScript.RegisterStartupScript(this.GetType(), "fun", "fun();", true);
            }
            catch (MySql.Data.MySqlClient.MySqlException ex)
            {
                conn.Close();
            }
        }

        #region webmethods
        [System.Web.Services.WebMethod]
        public static string GetChartData()
        {
            return chartData;
        }

        [System.Web.Services.WebMethod]
        public static string GetChart5DData()
        {
            return chartData5D;
        }

        [System.Web.Services.WebMethod]
        public static string GetChart25DData()
        {
            return chartData25D;
        }

        [System.Web.Services.WebMethod]
        public static string GetChart75DData()
        {
            return chartData75D;
        }

        [System.Web.Services.WebMethod]
        public static string GetChart150DData()
        {
            return chartData150D;
        }

        [System.Web.Services.WebMethod]
        public static string GetChart200DData()
        {
            return chartData200D;
        }

        [System.Web.Services.WebMethod]
        public static string GetChartVolData()
        {
            return chartDataVol;
        }

        [System.Web.Services.WebMethod]
        public static int CheckReportStatus()
        {
            int i = 0;
            MySql.Data.MySqlClient.MySqlConnection conn = new MySqlConnection(GetConnectionString());
            try
            {
                MySqlCommand cmd = null;
                conn.Open();
                cmd = new MySqlCommand();
                cmd.Connection = conn;
                cmd.CommandText = "SPGetConfiguration";
                cmd.CommandType = CommandType.StoredProcedure;

                cmd.Parameters.Add(new MySqlParameter("o_Flag", MySqlDbType.String));
                cmd.Parameters["o_Flag"].Direction = ParameterDirection.Output;

                i = cmd.ExecuteNonQuery();
                conn.Close();
                
            }
            catch (MySql.Data.MySqlClient.MySqlException ex)
            {
                conn.Close();
            }
            return i;
        }

        [System.Web.Services.WebMethod]
        public static int GenerateReportStatus()
        {
            int i = 0;
            MySql.Data.MySqlClient.MySqlConnection conn = new MySqlConnection(GetConnectionString());
            try
            {
                MySqlCommand cmd = null;
                conn.Open();
                cmd = new MySqlCommand();
                cmd.Connection = conn;
                cmd.CommandText = "SPGetResultParent";
                cmd.CommandType = CommandType.StoredProcedure;

                i = cmd.ExecuteNonQuery();
                conn.Close();

            }
            catch (MySql.Data.MySqlClient.MySqlException ex)
            {
                conn.Close();
            }
            return i;
        }
        #endregion

        protected void txtFluctn_TextChanged(object sender, EventArgs e)
        {

        }

        protected void txtScripName_TextChanged(object sender, EventArgs e)
        {

        }

        protected void GridView_RowDataBound(object sender, GridViewRowEventArgs e)
        {
            for (int i = 0; i < e.Row.Cells.Count; i++)
            {
                e.Row.Cells[i].Attributes.Add("style", "white-space: nowrap;");
            }
        }

        public string JSON_DataTable(DataTable dt)
        {

            /****************************************************************************
            * Without goingin to the depth of the functioning
            * of this method, i will try to give an overview
            * As soon as this method gets a DataTable
            * it starts to convert it into JSON String,
            * it takes each row and ineach row it creates
            * an array of cells and in each cell is having its data
            * on the client side it is very usefull for direct binding of object to  TABLE.
            * Values Can be Access on clien in this way. OBJ.TABLE[0].ROW[0].CELL[0].DATA 
            * NOTE: One negative point. by this method user
            * will not be able to call any cell by its name.
            * *************************************************************************/

            StringBuilder JsonString = new StringBuilder();

            JsonString.Append("{ ");
            JsonString.Append("\"TABLE\":[{ ");
            JsonString.Append("\"ROW\":[ ");

            for (int i = 0; i < dt.Rows.Count; i++)
            {

                JsonString.Append("{ ");
                JsonString.Append("\"COL\":[ ");

                for (int j = 0; j < dt.Columns.Count; j++)
                {
                    if (j < dt.Columns.Count - 1)
                    {
                        JsonString.Append("{" + "\"DATA\":\"" +
                                          dt.Rows[i][j].ToString() + "\"},");
                    }
                    else if (j == dt.Columns.Count - 1)
                    {
                        JsonString.Append("{" + "\"DATA\":\"" +
                                          dt.Rows[i][j].ToString() + "\"}");
                    }
                }
                /*end Of String*/
                if (i == dt.Rows.Count - 1)
                {
                    JsonString.Append("]} ");
                }
                else
                {
                    JsonString.Append("]}, ");
                }
            }
            JsonString.Append("]}]}");
            return JsonString.ToString();
        }

        public List<string> CreateJsonParameters(DataTable dt)
        {
            /* /****************************************************************************
             * Without goingin to the depth of the functioning
             * of this method, i will try to give an overview
             * As soon as this method gets a DataTable it starts to convert it into JSON String,
             * it takes each row and in each row it grabs the cell name and its data.
             * This kind of JSON is very usefull when developer have to have Column name of the .
             * Values Can be Access on clien in this way. OBJ.HEAD[0].<ColumnName>
             * NOTE: One negative point. by this method user
             * will not be able to call any cell by its index.
             * *************************************************************************/

            StringBuilder JsonString = new StringBuilder();
            StringBuilder JsonString5D = new StringBuilder();
            StringBuilder JsonString25D = new StringBuilder();
            StringBuilder JsonString75D = new StringBuilder();
            StringBuilder JsonString150D = new StringBuilder();
            StringBuilder JsonString200D = new StringBuilder();
            StringBuilder JsonStringVol = new StringBuilder();

            //Exception Handling
            if (dt != null && dt.Rows.Count > 0)
            {
                JsonString.Append("[ ");
                JsonString5D.Append("[ ");
                JsonString25D.Append("[ ");
                JsonString75D.Append("[ ");
                JsonString150D.Append("[ ");
                JsonString200D.Append("[ ");
                JsonStringVol.Append("[ ");

                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    JsonString.Append("[ ");
                    JsonString5D.Append("[ ");
                    JsonString25D.Append("[ ");
                    JsonString75D.Append("[ ");
                    JsonString150D.Append("[ ");
                    JsonString200D.Append("[ ");
                    JsonStringVol.Append("[ ");

                    for (int j = 0; j <= dt.Columns.Count - 1; j++)
                    {
                        if (j == 0)
                        {
                            //TimeSpan ts = DateTime.Parse(dt.Rows[i][0].ToString()).Subtract(new DateTime(1970, 1, 1, 0, 0, 0));
                            //JsonString.Append(ts.TotalMilliseconds.ToString() + ",");
                            JsonString.Append(dt.Rows[i][0].ToString() + ",");
                        }
                        else if (j == 4)
                        {
                            JsonString.Append(dt.Rows[i][j].ToString());
                        }
                        else if (j == 6)
                        {
                            //TimeSpan ts = DateTime.Parse(dt.Rows[i][0].ToString()).Subtract(new DateTime(1970, 1, 1, 0, 0, 0));
                            //JsonString5D.Append(ts.TotalMilliseconds.ToString() + ",");
                            JsonString5D.Append(dt.Rows[i][0].ToString() + ",");
                            JsonString5D.Append(dt.Rows[i][j].ToString().Length > 0 ? dt.Rows[i][j].ToString() : "0");
                        }
                        else if (j == 7)
                        {
                            //TimeSpan ts = DateTime.Parse(dt.Rows[i][0].ToString()).Subtract(new DateTime(1970, 1, 1, 0, 0, 0));
                            //JsonString25D.Append(ts.TotalMilliseconds.ToString() + ",");
                            JsonString25D.Append(dt.Rows[i][0].ToString() + ",");
                            JsonString25D.Append(dt.Rows[i][j].ToString().Length > 0 ? dt.Rows[i][j].ToString() : "0");
                        }
                        else if (j == 8)
                        {
                            //TimeSpan ts = DateTime.Parse(dt.Rows[i][0].ToString()).Subtract(new DateTime(1970, 1, 1, 0, 0, 0));
                            //JsonString75D.Append(ts.TotalMilliseconds.ToString() + ",");
                            JsonString75D.Append(dt.Rows[i][0].ToString() + ",");
                            JsonString75D.Append(dt.Rows[i][j].ToString().Length > 0 ? dt.Rows[i][j].ToString() : "0");
                        }
                        else if (j == 9)
                        {
                            //TimeSpan ts = DateTime.Parse(dt.Rows[i][0].ToString()).Subtract(new DateTime(1970, 1, 1, 0, 0, 0));
                            //JsonString150D.Append(ts.TotalMilliseconds.ToString() + ",");
                            JsonString150D.Append(dt.Rows[i][0].ToString() + ",");
                            JsonString150D.Append(dt.Rows[i][j].ToString().Length > 0 ? dt.Rows[i][j].ToString() : "0");
                        }
                        else if (j == 10)
                        {
                            //TimeSpan ts = DateTime.Parse(dt.Rows[i][0].ToString()).Subtract(new DateTime(1970, 1, 1, 0, 0, 0));
                            //JsonString200D.Append(ts.TotalMilliseconds.ToString() + ",");
                            JsonString200D.Append(dt.Rows[i][0].ToString() + ",");
                            JsonString200D.Append(dt.Rows[i][j].ToString().Length > 0 ? dt.Rows[i][j].ToString() : "0");
                        }
                        else if (j == 5)
                        {
                            //TimeSpan ts = DateTime.Parse(dt.Rows[i][0].ToString()).Subtract(new DateTime(1970, 1, 1, 0, 0, 0));
                            //JsonStringVol.Append(ts.TotalMilliseconds.ToString() + ",");
                            JsonStringVol.Append(dt.Rows[i][0].ToString() + ",");
                            JsonStringVol.Append(dt.Rows[i][j].ToString());
                        }
                        else
                        {
                            JsonString.Append(dt.Rows[i][j].ToString() + ",");
                        }
                    }

                    /*end Of String*/
                    if (i == dt.Rows.Count - 1)
                    {
                        JsonString.Append("] ");
                        JsonString5D.Append("] ");
                        JsonString25D.Append("] ");
                        JsonString75D.Append("] ");
                        JsonString150D.Append("] ");
                        JsonString200D.Append("] ");
                        JsonStringVol.Append("] ");
                    }
                    else
                    {
                        JsonString.Append("], ");
                        JsonString5D.Append("], ");
                        JsonString25D.Append("], ");
                        JsonString75D.Append("], ");
                        JsonString150D.Append("], ");
                        JsonString200D.Append("], ");
                        JsonStringVol.Append("], ");
                    }
                }

                JsonString.Append("]");
                JsonString5D.Append("]");
                JsonString25D.Append("]");
                JsonString75D.Append("]");
                JsonString150D.Append("]");
                JsonString200D.Append("]");
                JsonStringVol.Append("]");

                List<string> lst = new List<string>();

                lst.Add(JsonString.ToString());
                lst.Add(JsonString5D.ToString());
                lst.Add(JsonString25D.ToString());
                lst.Add(JsonString75D.ToString());
                lst.Add(JsonString150D.ToString());
                lst.Add(JsonString200D.ToString());
                lst.Add(JsonStringVol.ToString());

                return lst;
            }
            else
            {
                return null;
            }
        }

        protected void cbPeriods_SelectedIndexChanged(object sender, EventArgs e)
        {
            var items = (sender as CheckBoxList).Items;
            foreach (var item in items)
            {
                switch (((System.Web.UI.WebControls.ListItem)(item)).Text)
                {
                    case "Daily":
                        if (((System.Web.UI.WebControls.ListItem)(item)).Selected)
                            period1D = 1;
                        else
                            period1D = 0;
                        break;
                    case "200日平均":
                        if (((System.Web.UI.WebControls.ListItem)(item)).Selected)
                            period200D = 1;
                        else
                            period200D = 0;
                        break;
                    case "150日平均":
                        if (((System.Web.UI.WebControls.ListItem)(item)).Selected)
                            period150D = 1;
                        else
                            period150D = 0;
                        break;
                    case "75日平均":
                        if (((System.Web.UI.WebControls.ListItem)(item)).Selected)
                            period75D = 1;
                        else
                            period75D = 0;
                        break;
                    case "25日平均":
                        if (((System.Web.UI.WebControls.ListItem)(item)).Selected)
                            period25D = 1;
                        else
                            period25D = 0;
                        break;
                    case "5日平均":
                        if (((System.Web.UI.WebControls.ListItem)(item)).Selected)
                            period5D = 1;
                        else
                            period5D = 0;
                        break;
                }
            }
        }

        protected void dgScripList_PageIndexChanging(object sender, GridViewPageEventArgs e)
        {
            dgScripList.PageIndex = e.NewPageIndex;
            MySql.Data.MySqlClient.MySqlConnection conn = new MySqlConnection(GetConnectionString());
            MySqlCommand cmd = null;
            var netChhng = txtFluctn.Text.Length > 0 ? txtFluctn.Text : "0";
            conn.Open();
            cmd = new MySqlCommand();

            cmd.Connection = conn;

            cmd.CommandText = "SPSearchStockByfluctuationPercentage";
            cmd.CommandType = CommandType.StoredProcedure;

            cmd.Parameters.AddWithValue("i_5Days", period5D);
            cmd.Parameters["i_5Days"].Direction = ParameterDirection.Input;
            cmd.Parameters["i_5Days"].MySqlDbType = MySqlDbType.Int32;

            cmd.Parameters.AddWithValue("i_25Days", period25D);
            cmd.Parameters["i_25Days"].Direction = ParameterDirection.Input;
            cmd.Parameters["i_25Days"].MySqlDbType = MySqlDbType.Int32;

            cmd.Parameters.AddWithValue("i_75Days", period75D);
            cmd.Parameters["i_75Days"].Direction = ParameterDirection.Input;
            cmd.Parameters["i_75Days"].MySqlDbType = MySqlDbType.Int32;

            cmd.Parameters.AddWithValue("i_150Days", period150D);
            cmd.Parameters["i_150Days"].Direction = ParameterDirection.Input;
            cmd.Parameters["i_150Days"].MySqlDbType = MySqlDbType.Int32;

            cmd.Parameters.AddWithValue("i_200Days", period200D);
            cmd.Parameters["i_200Days"].Direction = ParameterDirection.Input;
            cmd.Parameters["i_200Days"].MySqlDbType = MySqlDbType.Int32;

            cmd.Parameters.AddWithValue("i_Percentage", netChhng);
            cmd.Parameters["i_Percentage"].Direction = ParameterDirection.Input;
            cmd.Parameters["i_Percentage"].MySqlDbType = MySqlDbType.Decimal;

            cmd.Parameters.Add(new MySqlParameter("o_Flag", MySqlDbType.String));
            cmd.Parameters["o_Flag"].Direction = ParameterDirection.Output;

            cmd.Parameters.Add(new MySqlParameter("o_ErrorCode", MySqlDbType.String));
            cmd.Parameters["o_ErrorCode"].Direction = ParameterDirection.Output;

            cmd.Parameters.Add(new MySqlParameter("o_ErrorDescription", MySqlDbType.String));
            cmd.Parameters["o_ErrorDescription"].Direction = ParameterDirection.Output;

            MySqlDataAdapter adp = new MySqlDataAdapter(cmd);
            DataSet ds = new DataSet();
            adp.Fill(ds);
            dgScripList.DataSource = ds.Tables[0];
            //dgScripList.Columns[1].Visible = false;
            dgScripList.DataBind();
            //dgScripList.Columns[0].Visible = false;

            conn.Close();

        }

        protected void btnSearch_Click(object sender, EventArgs e)
        {
            try
            {
                MySql.Data.MySqlClient.MySqlConnection conn = new MySqlConnection(GetConnectionString());
                MySqlCommand cmd = null;
                var scripName = txtScripName.Text.Length > 0 ? txtScripName.Text : "";
                conn.Open();
                cmd = new MySqlCommand();

                cmd.Connection = conn;

                cmd.CommandText = "SPSearchByStockCode";
                cmd.CommandType = CommandType.StoredProcedure;


                cmd.Parameters.AddWithValue("i_MarketCode", scripName);
                cmd.Parameters["i_MarketCode"].Direction = ParameterDirection.Input;
                cmd.Parameters["i_MarketCode"].MySqlDbType = MySqlDbType.VarChar;

                cmd.Parameters.Add(new MySqlParameter("o_Flag", MySqlDbType.String));
                cmd.Parameters["o_Flag"].Direction = ParameterDirection.Output;

                cmd.Parameters.Add(new MySqlParameter("o_ErrorCode", MySqlDbType.String));
                cmd.Parameters["o_ErrorCode"].Direction = ParameterDirection.Output;

                cmd.Parameters.Add(new MySqlParameter("o_ErrorDescription", MySqlDbType.String));
                cmd.Parameters["o_ErrorDescription"].Direction = ParameterDirection.Output;

                MySqlDataAdapter adp = new MySqlDataAdapter(cmd);
                DataSet ds = new DataSet();
                adp.Fill(ds);
                dgScripList.DataSource = ds.Tables[0];
                dgScripList.DataBind();

                conn.Close();
                //dvleftColumn.Visible = true;
            }
            catch (MySql.Data.MySqlClient.MySqlException ex)
            {
            }
        }

        protected void cbPeriods_SelectedIndexChanged1(object sender, EventArgs e)
        {

        }

        protected void dgMW_RowDataBound(object sender, GridViewRowEventArgs e)
        {
            for (int i = 0; i < e.Row.Cells.Count; i++)
            {
                e.Row.Cells[i].Attributes.Add("style", "white-space: nowrap;");
                if ((i != 1) && (e.Row.RowType == DataControlRowType.DataRow))
                {
                    e.Row.Cells[i].HorizontalAlign = HorizontalAlign.Right;
                }
            }

        }    

        private void SortGridView(string sortExpression, string direction)
        {
            try
            {
                //  You can cache the DataTable for improving performance
                DataTable dt = dtReport;

                DataView dv = new DataView(dt);
                dv.Sort = sortExpression + direction;

                dgMW.DataSource = dv;
                dgMW.DataBind();
            }
            catch (MySql.Data.MySqlClient.MySqlException ex)
            {
                
            }
        }

        protected void dgMW_Sorting(object sender, GridViewSortEventArgs e)
        {
            string sortExpression = e.SortExpression;

            if (GridViewSortDirection == SortDirection.Ascending)
            {
                GridViewSortDirection = SortDirection.Descending;
                SortGridView(sortExpression, DESCENDING);
                sortImage.ImageUrl = "asc.gif";
            }
            else
            {
                GridViewSortDirection = SortDirection.Ascending;
                SortGridView(sortExpression, ASCENDING);
                sortImage.ImageUrl = "desc.gif";
            }
            int columnIndex = 0;
            foreach (DataControlFieldHeaderCell headerCell in dgMW.HeaderRow.Cells)
            {
                if (headerCell.ContainingField.SortExpression == e.SortExpression)
                {
                    columnIndex = dgMW.HeaderRow.Cells.GetCellIndex(headerCell);
                }
            }

            dgMW.HeaderRow.Cells[columnIndex].Controls.Add(sortImage);
        }

        protected void btnGetReport_Click(object sender, EventArgs e)
        {
            GetReports();
        }

        protected void btnSubmitStockMaster_Click(object sender, EventArgs e)
        {
            if (txtStockCode.Text.Length > 0)
            {
                if (txtStockName.Text.Length > 0)
                {
                    //if (txtStockType.Text.Length > 0)
                    //{
                        MySql.Data.MySqlClient.MySqlConnection conn = new MySqlConnection(GetConnectionString());
                        try
                        {
                            MySqlCommand cmd = null;
                            conn.Open();
                            cmd = new MySqlCommand();
                            cmd.Connection = conn;
                            cmd.CommandText = "SPInsertStockMaster";
                            cmd.CommandType = CommandType.StoredProcedure;

                            cmd.Parameters.AddWithValue("i_StockCode", txtStockCode.Text);
                            cmd.Parameters["i_StockCode"].Direction = ParameterDirection.Input;
                            cmd.Parameters["i_StockCode"].MySqlDbType = MySqlDbType.VarChar;

                            cmd.Parameters.AddWithValue("i_StockName", txtStockName.Text);
                            cmd.Parameters["i_StockName"].Direction = ParameterDirection.Input;
                            cmd.Parameters["i_StockName"].MySqlDbType = MySqlDbType.VarChar;

                            cmd.Parameters.AddWithValue("i_StockType", txtStockType.Text);
                            cmd.Parameters["i_StockType"].Direction = ParameterDirection.Input;
                            cmd.Parameters["i_StockType"].MySqlDbType = MySqlDbType.VarChar;

                            cmd.Parameters.AddWithValue("i_MarketName", ddlMarketName.SelectedItem.Text);
                            cmd.Parameters["i_MarketName"].Direction = ParameterDirection.Input;
                            cmd.Parameters["i_MarketName"].MySqlDbType = MySqlDbType.VarChar;

                            cmd.Parameters.Add(new MySqlParameter("o_Flag", MySqlDbType.String));
                            cmd.Parameters["o_Flag"].Direction = ParameterDirection.Output;

                            cmd.Parameters.Add(new MySqlParameter("o_ErrorCode", MySqlDbType.String));
                            cmd.Parameters["o_ErrorCode"].Direction = ParameterDirection.Output;

                            cmd.Parameters.Add(new MySqlParameter("o_ErrorDescription", MySqlDbType.String));
                            cmd.Parameters["o_ErrorDescription"].Direction = ParameterDirection.Output;

                            cmd.ExecuteNonQuery();
                            conn.Close();
                            lblStockMaster.InnerText = "銘柄を登録しました。";

                            txtStockCode.Text = "";
                            txtStockName.Text = "";
                            txtStockType.Text = "";
                            ddlMarketName.SelectedIndex = 1;
                        }
                        catch (MySql.Data.MySqlClient.MySqlException ex)
                        {
                            conn.Close();
                            if (ex.Message == "Stock Code exists")
                            {
                                lblStockMaster.InnerText = "銘柄が既に存在しています。";
                            }
                            else if (ex.Message == "Market Code is blank")
                            {
                                lblStockMaster.InnerText = " 市場名を入力してください。";
                            }
                            else
                                lblStockMaster.InnerText = ex.Message;
                        }
                    //}
                    //else
                    //{
                     //   lblStockMaster.InnerText = "Stock type cannot be blank";
                    //}
                }
                else
                {
                    lblStockMaster.InnerText = "銘柄名を入力してください。";
                }
            }
            else
            {
                lblStockMaster.InnerText = "銘柄コードを入力してください。";
            }
        }

        protected void btnSubmitMarketMaster_Click(object sender, EventArgs e)
        {
            if (txtMarketName.Text.Length > 0)
            {
                MySql.Data.MySqlClient.MySqlConnection conn = new MySqlConnection(GetConnectionString());
                try
                {
                    MySqlCommand cmd = null;
                    conn.Open();
                    cmd = new MySqlCommand();
                    cmd.Connection = conn;
                    cmd.CommandText = "SPInsertMarketMaster";
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.AddWithValue("i_MarketName", txtMarketName.Text);
                    cmd.Parameters["i_MarketName"].Direction = ParameterDirection.Input;
                    cmd.Parameters["i_MarketName"].MySqlDbType = MySqlDbType.VarChar;

                    cmd.Parameters.Add(new MySqlParameter("o_Flag", MySqlDbType.String));
                    cmd.Parameters["o_Flag"].Direction = ParameterDirection.Output;

                    cmd.Parameters.Add(new MySqlParameter("o_ErrorCode", MySqlDbType.String));
                    cmd.Parameters["o_ErrorCode"].Direction = ParameterDirection.Output;

                    cmd.Parameters.Add(new MySqlParameter("o_ErrorDescription", MySqlDbType.String));
                    cmd.Parameters["o_ErrorDescription"].Direction = ParameterDirection.Output;

                    cmd.ExecuteNonQuery();
                    conn.Close();
                    txtMarketName.Text = "";
                    lblMarketMaster.InnerText = "市場名を登録しました。";

                    GetMarketMaster();
                }
                catch (MySql.Data.MySqlClient.MySqlException ex)
                {
                    
                    //Market Code is exists
                    if (ex.Message == "Market Code exists")
                    {
                        lblMarketMaster.InnerText = "市場名が既に存在しています。";
                    }
                    else
                        lblMarketMaster.InnerText = ex.Message;
                }
            }
            else
            {
                lblMarketMaster.InnerText = "市場名を入力してください。";
            }
        }

        protected void btnDelete_Click(object sender, EventArgs e)
        {
            MySql.Data.MySqlClient.MySqlConnection conn = new MySqlConnection(GetConnectionString());
            try
            {
                MySqlCommand cmd = null;
                conn.Open();
                cmd = new MySqlCommand();
                cmd.Connection = conn;
                cmd.CommandText = "SPDeleteStockTransaction";
                cmd.CommandType = CommandType.StoredProcedure;

                cmd.Parameters.AddWithValue("i_Pricedate", DateTime.Parse(myDateField.Value).ToString("yyyy-MM-dd"));
                cmd.Parameters["i_Pricedate"].Direction = ParameterDirection.Input;
                cmd.Parameters["i_Pricedate"].MySqlDbType = MySqlDbType.VarChar;

                cmd.Parameters.Add(new MySqlParameter("o_Flag", MySqlDbType.String));
                cmd.Parameters["o_Flag"].Direction = ParameterDirection.Output;

                cmd.Parameters.Add(new MySqlParameter("o_ErrorCode", MySqlDbType.String));
                cmd.Parameters["o_ErrorCode"].Direction = ParameterDirection.Output;

                cmd.Parameters.Add(new MySqlParameter("o_ErrorDescription", MySqlDbType.String));
                cmd.Parameters["o_ErrorDescription"].Direction = ParameterDirection.Output;

                cmd.ExecuteNonQuery();
                conn.Close();
                errorMsg.InnerText = "Record deleted successfully";
            }
            catch (MySql.Data.MySqlClient.MySqlException ex)
            {
                conn.Close();
            }
        }

        protected void btnGenerateReport_Click(object sender, EventArgs e)
        {

        }

        protected void ddlVR25Filter_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (((System.Web.UI.WebControls.DropDownList)(sender)).SelectedIndex == 3)
            {
                spVR25.Visible = true;
                txtVR25Upper.Visible = true;
            }
            else
            {
                spVR25.Visible = false;
                txtVR25Upper.Visible = false;
            }
        }

        protected void ddl25SMAFilter_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (((System.Web.UI.WebControls.DropDownList)(sender)).SelectedIndex == 3)
            {
                sp25SMA.Visible = true;
                txt25SMAUpper.Visible = true;
            }
            else
            {
                sp25SMA.Visible = false;
                txt25SMAUpper.Visible = false;
            }
        }

        protected void ddlPriceRange_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (((System.Web.UI.WebControls.DropDownList)(sender)).SelectedIndex == 3)
            {
                spPriceRange.Visible = true;
                txtPriceRangeUpper.Visible = true;
            }
            else
            {
                spPriceRange.Visible = false;
                txtPriceRangeUpper.Visible = false;
            }
        }

        protected void ddlRSIFilter_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (((System.Web.UI.WebControls.DropDownList)(sender)).SelectedIndex == 3)
            {
                spRSI.Visible = true;
                txtRSIUpper.Visible = true;
            }
            else
            {
                spRSI.Visible = false;
                txtRSIUpper.Visible = false;
            }
        }

        protected void btnSubmitParameters_Click(object sender, EventArgs e)
        {
            DataTable table = new DataTable();
            table.TableName = "EditedTable";
            table.Columns.Add("Name", typeof(string));
            table.Columns.Add("Value", typeof(Decimal));

            foreach (ListViewItem item in dgParameterK2C.Items)
            {
                table.Rows.Add(((((System.Web.UI.Control)(item)).FindControl("lblName") as Label)).Text, Convert.ToDecimal((((System.Web.UI.Control)(item)).FindControl("txtColumnValue") as TextBox).Text));
            }
            foreach (ListViewItem item in dgParameterW2D.Items)
            {
                table.Rows.Add(((((System.Web.UI.Control)(item)).FindControl("lblName") as Label)).Text, Convert.ToDecimal((((System.Web.UI.Control)(item)).FindControl("txtColumnValue") as TextBox).Text));
            }
            foreach (ListViewItem item in dgParameterP.Items)
            {
                table.Rows.Add(((((System.Web.UI.Control)(item)).FindControl("lblName") as Label)).Text, Convert.ToDecimal((((System.Web.UI.Control)(item)).FindControl("txtColumnValue") as TextBox).Text));
            }

            var dtResult = getDifferentRecords(dtParameters, table);

            if (dtResult.Rows.Count > 0) 
            {
                MySql.Data.MySqlClient.MySqlConnection conn = new MySqlConnection(GetConnectionString());
                try
                {
                    MySqlCommand cmd = null;
                    conn.Open();
                    cmd = new MySqlCommand();
                    cmd.Connection = conn;
                    cmd.CommandText = "SPUpdateParameterTBL";
                    cmd.CommandType = CommandType.StoredProcedure;

                    foreach (ListViewItem item in dgParameterK2C.Items)
                    {
                        var name = "i_" + ((((System.Web.UI.Control)(item)).FindControl("lblName") as Label)).Text;
                        var value = Convert.ToDecimal((((System.Web.UI.Control)(item)).FindControl("txtColumnValue") as TextBox).Text);
                        cmd.Parameters.AddWithValue(name, value);
                        cmd.Parameters[name].Direction = ParameterDirection.Input;
                        cmd.Parameters[name].MySqlDbType = MySqlDbType.Decimal;
                    }
                    foreach (ListViewItem item in dgParameterW2D.Items)
                    {
                        var name = "i_" + ((((System.Web.UI.Control)(item)).FindControl("lblName") as Label)).Text;
                        var value = Convert.ToDecimal((((System.Web.UI.Control)(item)).FindControl("txtColumnValue") as TextBox).Text);
                        cmd.Parameters.AddWithValue(name, value);
                        cmd.Parameters[name].Direction = ParameterDirection.Input;
                        cmd.Parameters[name].MySqlDbType = MySqlDbType.Decimal;
                    }
                    foreach (ListViewItem item in dgParameterP.Items)
                    {
                        var name = "i_" + ((((System.Web.UI.Control)(item)).FindControl("lblName") as Label)).Text;
                        var value = Convert.ToDecimal((((System.Web.UI.Control)(item)).FindControl("txtColumnValue") as TextBox).Text);
                        cmd.Parameters.AddWithValue(name, value);
                        cmd.Parameters[name].Direction = ParameterDirection.Input;
                        cmd.Parameters[name].MySqlDbType = MySqlDbType.Decimal;
                    }
                    cmd.ExecuteNonQuery();
                    conn.Close();
                    ScriptManager.RegisterClientScriptBlock(this, this.GetType(), "alertMessage", "alert('値を更新しました。')", true);
                    GetParameter();
                }
                catch (MySql.Data.MySqlClient.MySqlException ex)
                {
                    conn.Close();
                }
            }
            else
                ScriptManager.RegisterClientScriptBlock(this, this.GetType(), "alertMessage", "alert('変更はありません。')", true);

        }

        public DataTable getDifferentRecords(DataTable FirstDataTable, DataTable SecondDataTable)
        {
            //Create Empty Table   
            DataTable ResultDataTable = new DataTable("ResultDataTable");

            //use a Dataset to make use of a DataRelation object   
            using (DataSet ds = new DataSet())
            {
                //Add tables   
                ds.Tables.AddRange(new DataTable[] { FirstDataTable.Copy(), SecondDataTable.Copy() });

                //Get Columns for DataRelation   
                DataColumn[] firstColumns = new DataColumn[ds.Tables[0].Columns.Count];
                for (int i = 0; i < firstColumns.Length; i++)
                {
                    firstColumns[i] = ds.Tables[0].Columns[i];
                }

                DataColumn[] secondColumns = new DataColumn[ds.Tables[1].Columns.Count];
                for (int i = 0; i < secondColumns.Length; i++)
                {
                    secondColumns[i] = ds.Tables[1].Columns[i];
                }

                //Create DataRelation   
                DataRelation r1 = new DataRelation(string.Empty, firstColumns, secondColumns, false);
                ds.Relations.Add(r1);

                DataRelation r2 = new DataRelation(string.Empty, secondColumns, firstColumns, false);
                ds.Relations.Add(r2);

                //Create columns for return table   
                for (int i = 0; i < FirstDataTable.Columns.Count; i++)
                {
                    ResultDataTable.Columns.Add(FirstDataTable.Columns[i].ColumnName, FirstDataTable.Columns[i].DataType);
                }

                //If FirstDataTable Row not in SecondDataTable, Add to ResultDataTable.   
                ResultDataTable.BeginLoadData();
                foreach (DataRow parentrow in ds.Tables[0].Rows)
                {
                    DataRow[] childrows = parentrow.GetChildRows(r1);
                    if (childrows == null || childrows.Length == 0)
                        ResultDataTable.LoadDataRow(parentrow.ItemArray, true);
                }

                //If SecondDataTable Row not in FirstDataTable, Add to ResultDataTable.   
                foreach (DataRow parentrow in ds.Tables[1].Rows)
                {
                    DataRow[] childrows = parentrow.GetChildRows(r2);
                    if (childrows == null || childrows.Length == 0)
                        ResultDataTable.LoadDataRow(parentrow.ItemArray, true);
                }
                ResultDataTable.EndLoadData();
            }

            return ResultDataTable;
        }  
        protected void btnEditParameters_Click(object sender, EventArgs e)
        {

        }

        protected void btnResetParameters_Click(object sender, EventArgs e)
        {
            GetParameter();
        }

        protected void txtColumnValue_TextChanged(object sender, EventArgs e)
        {
            ((System.Web.UI.WebControls.WebControl)(sender)).ForeColor = Color.Red;
            ((System.Web.UI.WebControls.WebControl)(sender)).BorderColor = Color.Red;
        }

        protected void btnResetFilter_Click(object sender, EventArgs e)
        {
            ResetReportFilters();
            GetReports();
        }

        protected void uploadStock_Click(object sender, EventArgs e)
        {
           
            if (FileUpload2.PostedFile.FileName.Length > 0)
            {
                //Upload and save the file
                string csvPath = Server.MapPath("~/Files/") + Path.GetFileName(FileUpload2.PostedFile.FileName);
                FileUpload2.SaveAs(csvPath);

                DataTable dt = new DataTable();
                dt.Columns.AddRange(new DataColumn[9] { 
                    new DataColumn("PriceDate",typeof(string)) ,
            new DataColumn("MarketCode", typeof(string)),
            new DataColumn("StockCode", typeof(string)),
            new DataColumn("SockName",typeof(string)) ,
            new DataColumn("PriceOpen",typeof(string)) ,
            new DataColumn("PriceHigh",typeof(string)) ,
            new DataColumn("PriceLow",typeof(string)) ,
            new DataColumn("PriceClose",typeof(string)),
            new DataColumn("Volume",typeof(string))
            });


                string csvData = File.ReadAllText(csvPath);
                if (txtUpdatedStockCode.Text.Length > 0)
                {
                    if (ValidateCsv(csvData))
                    {

                        var rows = csvData.Split('\n');
                        var rowLength = rows.Length;
                        for (int j = 3; j < rowLength; j++)
                        {
                            if (!string.IsNullOrEmpty(rows[j]))
                            {
                                dt.Rows.Add();

                                List<string> cells = new List<string>();
                                cells = rows[j].Split(',').ToList();
                                var cellLength = 0;

                                cells[8] = cells[8].Split('\r')[0];
                                cellLength = cells.Count;
                                
                                for (int i = 0; i < cellLength; i++)
                                {
                                    if (!String.IsNullOrEmpty(cells[i]))
                                        dt.Rows[dt.Rows.Count - 1][i] = cells[i];
                                }
                            }
                        }

                        MySql.Data.MySqlClient.MySqlConnection conn = new MySqlConnection(GetConnectionString());
                        MySqlCommand cmd = null;

                        conn.Open();

                        try
                        {
                            foreach (DataRow row in dt.Rows)
                            {
                                //Console.WriteLine("Connecting to MySQL...");
                                cmd = new MySqlCommand();

                                cmd.Connection = conn;

                                cmd.CommandText = "SPInsertStockTransaction";
                                cmd.CommandType = CommandType.StoredProcedure;

                                if (!String.IsNullOrEmpty(row[0].ToString()) && !String.IsNullOrEmpty(row[1].ToString()))
                                {
                                    cmd.Parameters.AddWithValue("i_PriceDate", DateTime.Parse(row[0].ToString()).ToString("yyyy-MM-dd"));
                                    cmd.Parameters["i_PriceDate"].MySqlDbType = MySqlDbType.Date;
                                    cmd.Parameters["i_PriceDate"].Direction = ParameterDirection.Input;

                                    cmd.Parameters.AddWithValue("i_MarketCode", row[1]);
                                    cmd.Parameters["i_MarketCode"].Direction = ParameterDirection.Input;
                                    cmd.Parameters["i_MarketCode"].MySqlDbType = MySqlDbType.VarChar;

                                    cmd.Parameters.AddWithValue("i_StockCode", row[2]);
                                    cmd.Parameters["i_StockCode"].MySqlDbType = MySqlDbType.VarChar;
                                    cmd.Parameters["i_StockCode"].Direction = ParameterDirection.Input;

                                    cmd.Parameters.AddWithValue("i_StockName", row[3]);
                                    cmd.Parameters["i_StockCode"].MySqlDbType = MySqlDbType.VarChar;
                                    cmd.Parameters["i_StockCode"].Direction = ParameterDirection.Input;

                                    cmd.Parameters.AddWithValue("i_PriceOpen", (row[4]));
                                    cmd.Parameters["i_PriceOpen"].MySqlDbType = MySqlDbType.Decimal;
                                    cmd.Parameters["i_PriceOpen"].Direction = ParameterDirection.Input;

                                    cmd.Parameters.AddWithValue("i_PriceHigh", (row[5]));
                                    cmd.Parameters["i_PriceHigh"].MySqlDbType = MySqlDbType.Decimal;
                                    cmd.Parameters["i_PriceHigh"].Direction = ParameterDirection.Input;

                                    cmd.Parameters.AddWithValue("i_PriceLow", (row[6]));
                                    cmd.Parameters["i_PriceLow"].MySqlDbType = MySqlDbType.Decimal;
                                    cmd.Parameters["i_PriceLow"].Direction = ParameterDirection.Input;

                                    cmd.Parameters.AddWithValue("i_PriceClose", (row[7]));
                                    cmd.Parameters["i_PriceClose"].MySqlDbType = MySqlDbType.Decimal;
                                    cmd.Parameters["i_PriceClose"].Direction = ParameterDirection.Input;

                                    cmd.Parameters.AddWithValue("i_PriceVolumn", (row[8]));
                                    cmd.Parameters["i_PriceVolumn"].MySqlDbType = MySqlDbType.Decimal;
                                    cmd.Parameters["i_PriceVolumn"].Direction = ParameterDirection.Input;

                                    //Add the output parameter to the command object

                                    cmd.Parameters.Add(new MySqlParameter("o_Flag", MySqlDbType.String));
                                    cmd.Parameters["o_Flag"].Direction = ParameterDirection.Output;

                                    cmd.Parameters.Add(new MySqlParameter("o_ErrorCode", MySqlDbType.String));
                                    cmd.Parameters["o_ErrorCode"].Direction = ParameterDirection.Output;

                                    cmd.Parameters.Add(new MySqlParameter("o_ErrorDescription", MySqlDbType.String));
                                    cmd.Parameters["o_ErrorDescription"].Direction = ParameterDirection.Output;
                                    cmd.ExecuteNonQuery();

                                   
                                }
                                else
                                {

                                }

                            }

                            try
                            {
                                // MySqlCommand cmd = null;
                                cmd = new MySqlCommand();
                                cmd.Connection = conn;
                                cmd.CommandText = "UpdateConfigurationTable";
                                cmd.CommandType = CommandType.StoredProcedure;

                                cmd.ExecuteNonQuery();
                            }
                            catch (MySql.Data.MySqlClient.MySqlException exi)
                            {
                                conn.Close();

                            }
                            //cmd.Dispose();
                            conn.Close();
                            errorMsgStock.InnerText = "ファイルをアップロードしました。";
                        }
                        catch (MySql.Data.MySqlClient.MySqlException ex)
                        {
                            if (ex.Message == "Market Code does not exists")
                            {
                                errorMsgStock.InnerText = "市場名が存在しません。";
                            }
                            else if (ex.Message == "Stock Code does not exists")
                            {
                                errorMsgStock.InnerText = "銘柄コードが存在しません。";
                            }
                            else if (ex.Message == "Column 'MarketCode' cannot be null")
                            {
                                errorMsgStock.InnerText = "未登録の市場が見つかりました。ファイルアップロードができません。";
                            }
                            else
                                errorMsgStock.InnerText = "エラーが発生しました。";// ex.Message;
                            
                            conn.Close();
                        }

                    }
                    else
                    {
                        errorMsgStock.InnerText = "ファイルフォーマットは無効です。";
                    }
                }
                else
                {
                    errorMsgStock.InnerText = "銘柄コードを入力してください。";
                }
            }
            else
            {
                errorMsgStock.InnerText = "ファイルを指定してください。";
            }

        
        }

    }


}