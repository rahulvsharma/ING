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

namespace ChartFTP
{
    public partial class index : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            //GetReports();

            if (!this.IsPostBack)
            {
                cbPeriods.Items.Insert(0, new ListItem("200日平均"));
                cbPeriods.Items.Insert(0, new ListItem("150日平均"));
                cbPeriods.Items.Insert(0, new ListItem("75日平均"));
                cbPeriods.Items.Insert(0, new ListItem("25日平均"));
                cbPeriods.Items.Insert(0, new ListItem("5日平均"));
            }

        }

        public static int period1D = 0;
        public static int period5D = 0;
        public static int period25D = 0;
        public static int period75D = 0;
        public static int period150D = 0;
        public static int period200D = 0;
        public static string chartData = string.Empty;
        public static string chartData5D = string.Empty;
        public static string chartData25D = string.Empty;
        public static string chartData75D = string.Empty;
        public static string chartData150D = string.Empty;
        public static string chartData200D = string.Empty;
        public static string chartDataVol = string.Empty;
        public static DataTable dtReport = null;
        private const string ASCENDING = " ASC";
        private const string DESCENDING = " DESC";
        public Image sortImage = new Image();
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
                                //int i = 0;
                                var cells = rows[j].Split(',');
                                cells[8] = myDateField.Value;
                                var cellLength = cells.Length - 1;
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
                        foreach (DataRow row in dt.Rows)
                        {
                            try
                            {

                                //Console.WriteLine("Connecting to MySQL...");
                                cmd = new MySqlCommand();

                                cmd.Connection = conn;

                                cmd.CommandText = "SPInsertStockTransaction";
                                cmd.CommandType = CommandType.StoredProcedure;

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
                            catch (MySql.Data.MySqlClient.MySqlException ex)
                            {
                                errorMsg.InnerText = ex.Message;
                                conn.Close();
                                break;
                            }
                            errorMsg.InnerText = "ファイルアップロードを成功しました。";

                        }
                        //cmd.Dispose();
                        conn.Close();
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
              "database=ing; pooling=false", "localhost",
              "root", "root");

            return connStr;
        }

        public static bool ValidateCsv(string fileContents)
        {
            var fileLines = fileContents.Split(new string[] { "\r\n", "\n" }, StringSplitOptions.None);

            if (fileLines.Count() < 2) { }
            //fail - no data row.
            var isValid = true;
            isValid = ValidateColumnHeaders(fileLines[1]);

            isValid = ValidateRows(fileLines.Skip(3));

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
            MySql.Data.MySqlClient.MySqlConnection conn = new MySqlConnection(GetConnectionString());
            try
            {
                MySqlCommand cmd = null;
                conn.Open();
                cmd = new MySqlCommand();
                cmd.Connection = conn;
                cmd.CommandText = "SPGetResultSet";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("i_Symbol", ddlFilter.SelectedValue);
                cmd.Parameters["i_Symbol"].Direction = ParameterDirection.Input;
                cmd.Parameters["i_Symbol"].MySqlDbType = MySqlDbType.VarChar;
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

    }


}