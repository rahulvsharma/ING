<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="index.aspx.cs" Inherits="ChartFTP.index" %>

<!DOCTYPE html>
<html>
<head runat="server">
    <title>Chart FTP - ローソクチャート表示</title>
    <meta charset="utf-8">
    <link href="style.css" rel="stylesheet" type="text/css">
    <script src="jquery.js"></script>
    <script src="jquery-ui.min.js"></script>
    <link href="jquery-ui.min.css" rel="stylesheet" />
    <script src="HighStock.js"></script>
    <script src="exporting.js"></script>
    <%--<link href="jsgrid-theme.min.css" rel="stylesheet" />
    <link href="jsgrid.min.css" rel="stylesheet" />
    <script src="jsgrid.min.js"></script>--%>
    <style>
        .left {
            float: left;
        }

        .marginRight {
            margin-right: 5px;
        }

        .dtPicker {
            width: 85px;
            margin-right: 20px;
        }
        .HeaderRowStyle {
        
        }
    </style>
    <script>

        var chartDataList = [];
        var selectedRange = 0;
        function Draw(data, vol, chartSeries) {
            // split the data set into ohlc and volume
            var ohlc = [],
                volume = [],
                dataLength = data.length,
                // set the allowed units for data grouping
                groupingUnits = [[
                    'week',                         // unit name
                    [1]                             // allowed multiples
                ], [
                    'month',
                    [1, 2, 3, 4, 6]
                ]],

                i = 0;

            for (i; i < dataLength; i += 1) {
                ohlc.push([
                    data[i][0], // the date
                    data[i][1], // open
                    data[i][3], // high
                    data[i][4], // low
                    data[i][2] // close
                ]);

                volume.push([
                    vol[i][0], // the date
                    vol[i][1] // the volume
                ]);
            }


            var seriesArr = [];
            seriesArr.push(
           {
               type: 'candlestick',
               name: 'Data points',
               data: ohlc,
               dataGrouping: {
                   units: groupingUnits
               }
           });
            seriesArr.push({
                type: 'column',
                name: 'Volume',
                data: volume,
                yAxis: 1,
                dataGrouping: {
                    units: groupingUnits
                }
            });

            if (chartSeries.length > 0) {

                for (var j = 0; j < chartSeries.length; j++) {
                    seriesArr.push({
                        name: chartSeries[j].name,
                        data: chartSeries[j].value,
                        //yAxis: 1,
                        dataGrouping: {
                            units: groupingUnits
                        }
                    });
                }

            }

            // create the chart
            $('#graph-bar').highcharts('StockChart', {
                chart: {
                    // height: 500
                },
                rangeSelector: {
                    selected: selectedRange,
                    inputEnabled: true,
                    //inputStyle: {
                    //    disable:'disabled'
                    //}
                },
                xAxis: {
                    //    tickInterval: 24 * 3600 * 1000,
                    //    type: 'datetime',
                    //    labels: {
                    //        format: "{value:%Y-%m-%d}",
                    //        style: {
                    //            fontFamily: 'Tahoma'
                    //        },

                    //        rotation: -45,
                    //    }
                    events: {
                        setExtremes: function (e) {
                            if (typeof (e.rangeSelectorButton) !== 'undefined') {
                                var c = e.rangeSelectorButton.count;
                                var t = e.rangeSelectorButton.type;
                                var btn_index = null;
                                if (c == 1 && t == "month") {
                                    btn_index = 0;
                                } else if (c == 3 && t == "month") {
                                    btn_index = 1;
                                } else if (c == 6 && t == "month") {
                                    btn_index = 2;
                                } else if (t == "ytd") {
                                    btn_index = 3;
                                } else if (c == 1 && t == "year") {
                                    btn_index = 4;
                                } else if (t == "all") {
                                    btn_index = 5;
                                }
                                // Store btn_index in a cookie here and use it
                                // to initialise rangeSelector -> selected next
                                // time the chart is loaded
                                selectedRange = btn_index;
                                //alert(btn_index);
                            }
                        }
                    }
                },

                yAxis: [{
                    labels: {
                        align: 'right',
                        x: -3
                    },
                    title: {
                        text: 'OHLC'
                    },
                    height: '60%',
                    lineWidth: 2
                }, {
                    labels: {
                        align: 'right',
                        x: -3
                    },
                    title: {
                        text: 'Volume'
                    },
                    top: '65%',
                    height: '35%',
                    offset: 0,
                    lineWidth: 2
                }],

                series: seriesArr
            }, function (chart) {

                // apply the date pickers
                setTimeout(function () {
                    $('input.highcharts-range-selector', $(chart.container).parent())
                        .datepicker();
                }, 0);
            });

        }


        var chartData = null;
        var chartData5D = null;
        var chartData25D = null;
        var chartData75D = null;
        var chartData150D = null;
        var chartData200D = null;
        var chartDataVolume = null;
        var count = 0;

        function fun() {
            count = 0;
            PageMethods.GetChartData(onSucceed, onError);
            PageMethods.GetChart5DData(onSucceed5D, onError);
            PageMethods.GetChart25DData(onSucceed25D, onError);
            PageMethods.GetChart75DData(onSucceed75D, onError);
            PageMethods.GetChart150DData(onSucceed150D, onError);
            PageMethods.GetChart200DData(onSucceed200D, onError);
            PageMethods.GetChartVolData(onSucceedVol, onError);
            return false;
        }
        function onSucceed(result) {
            chartData = JSON.parse(result);
            count++;
            checkCountAndDraw(count);
        }
        function onSucceed5D(result) {
            chartData5D = JSON.parse(result);
            count++;
            checkCountAndDraw(count);
        }
        function onSucceed25D(result) {
            chartData25D = JSON.parse(result);
            count++;
            checkCountAndDraw(count);
        }
        function onSucceed75D(result) {
            chartData75D = JSON.parse(result);
            count++;
            checkCountAndDraw(count);
        }
        function onSucceed150D(result) {
            chartData150D = JSON.parse(result);
            count++;
            checkCountAndDraw(count);
        }
        function onSucceed200D(result) {
            chartData200D = JSON.parse(result);
            count++;
            checkCountAndDraw(count);
        }
        function onSucceedVol(result) {
            chartDataVolume = JSON.parse(result);
            count++;
            checkCountAndDraw(count);
        }

        function onError(result) {
            alert(result);
        }

        function checkCountAndDraw(chkCount) {
            if (chkCount == 7)
                Draw(chartData, chartDataVolume, []);
        }

        function GetReports() {
            PageMethods.GetReports(LoadReport, onReportError);
        }
    </script>
</head>
<body>
    <div id="wrapper">
        <form id="form1" runat="server">
            <div id="tabContainer">
                <div id="tabs">
                    <ul>
                        <li id="tabHeader_1">日付・ファイル登録</li>
                        <li id="tabHeader_2">計算</li>
                        <li id="tabHeader_3">SCReening</li>
                    </ul>
                </div>
                <div id="tabscontent">
                    <div class="tabpage" id="tabpage_1">
                        <h2></h2>

                        <b class="marginRight">日付:</b>
                        <asp:TextBox runat="server" ID="datePicker" CssClass="dtPicker" ViewStateMode="Enabled" OnTextChanged="datePicker_TextChanged" />
                        <asp:HiddenField ID="myDateField" runat="server" />

                        <label class="control-label marginRight"><b>ファイル選択: </b></label>
                        <asp:FileUpload ID="FileUpload1" runat="server" CssClass="custom-file-input Cntrl1" />
                        <asp:Button Text="日付・ファイル登録" ID="upload" OnClick="Upload" runat="server" class="btn" />
                        <span runat="server" id="errorMsg"></span>

                    </div>

                    <div class="tabpage" id="tabpage_2">
                        <div>


                            <div style="border-bottom: 1px solid grey; width: 100%; padding-bottom: 10px; height: 35px;">
                                <asp:Panel runat="server" CssClass="left" DefaultButton="btnScripList">
                                    <span class="left marginRight"><b>乖離率:</b></span>
                                    <asp:TextBox ID="txtFluctn" runat="server" Width="50" CssClass="left marginRight" OnTextChanged="txtFluctn_TextChanged"></asp:TextBox>
                                    <asp:CheckBoxList runat="server" RepeatDirection="Horizontal" ID="cbPeriods" CssClass="left" OnSelectedIndexChanged="cbPeriods_SelectedIndexChanged"></asp:CheckBoxList>
                                    <asp:Button ID="btnScripList" OnClick="btnScripList_Click" runat="server" CssClass="left btn" Text="検索" />
                                </asp:Panel>
                                <span class="left marginRight" style="margin-left: 20px;margin-right: 20px;">-OR-</span>
                                <asp:Panel runat="server" CssClass="left" DefaultButton="btnSearch">
                                    <span class="left marginRight"><b>該当する銘柄:</b></span>
                                    <asp:TextBox ID="txtScripName" CssClass="left marginRight" runat="server" Width="100" OnTextChanged="txtScripName_TextChanged"></asp:TextBox>
                                    <asp:Button ID="btnSearch" CssClass="left btn" OnClick="btnSearch_Click" runat="server" Text="検索" />
                                </asp:Panel>
                            </div>
                            <div class="columnsContainer">

                                <div class="leftColumn">
                                    <h2 style="margin-left: -13px;">銘柄一覧</h2>
                                    <div id="scrip-list" style="margin-left: -13px;">
                                        <div id="Div1" runat="server" style="border-right: 1px solid grey;">
                                            <asp:GridView PageSize="12" OnRowDataBound="GridView_RowDataBound" AutoGenerateColumns="false" OnPageIndexChanging="dgScripList_PageIndexChanging" ID="dgScripList"
                                                DataKeyNames="MarketName,StockCode,StockName" CssClass="Grid" PagerStyle-CssClass="pgr" AlternatingRowStyle-CssClass="alt"
                                                runat="server" OnRowCommand="dgScripList_RowCommand" AllowPaging="true">
                                                <Columns>
                                                    <asp:TemplateField>
                                                        <ItemTemplate>
                                                            <asp:Button ID="btnSelect" runat="server" CssClass="btn" OnClick="lnkDownload_Click" CommandName="Select" Text="/\/" />
                                                        </ItemTemplate>
                                                    </asp:TemplateField>
                                                </Columns>
                                                <Columns>
                                                    <asp:BoundField HeaderText="市場" InsertVisible="False" DataField="MarketName" SortExpression="MarketName"></asp:BoundField>
                                                    <asp:BoundField HeaderText="銘柄コード" InsertVisible="False" DataField="StockCode" SortExpression="StockCode"></asp:BoundField>
                                                    <asp:BoundField HeaderText="銘柄名" InsertVisible="False" DataField="StockName" SortExpression="StockName"></asp:BoundField>
                                                </Columns>
                                                <SelectedRowStyle ForeColor="White" Font-Bold="True" BackColor="#9471DE"></SelectedRowStyle>
                                                <RowStyle ForeColor="Black" BackColor="#DEDFDE"></RowStyle>
                                            </asp:GridView>
                                        </div>
                                    </div>
                                </div>

                                <div class="rightColumn">


                                    <div class="main-box">

                                        <div style="float: left">
                                            <input type="checkbox" class="Button" id="rd5day" value="5" name="5日平均" /><span>5日平均</span>
                                            <input type="checkbox" class="Button" id="rd25D" value="25" name="25日平均" /><span>25日平均</span>
                                            <input type="checkbox" class="Button" id="rd75D" value="75" name="75日平均" /><span>75日平均</span>
                                            <input type="checkbox" class="Button" id="rd100D" value="150" name="150日平均" /><span>150日平均</span>
                                            <input type="checkbox" class="Button" id="rd125D" value="200" name="200日平均" /><span>200日平均</span>
                                            <%--<asp:Button ID="btnShowChart" Text="表示" OnClientClick="return fun()" CssClass="btn" runat="server" />--%>
                                        </div>
                                        <div id="graph-bar"></div>
                                    </div>
                                    <asp:ScriptManager ID="ScriptManager1" runat="server" EnablePageMethods="True"></asp:ScriptManager>
                                </div>

                            </div>

                        </div>
                    </div>

                    <div class="tabpage" id="tabpage_3">
                        <h2>SCReening</h2>
                        <div id="scrip-list1" style="height: 420px; padding: 0px; position: relative;">
                            <div id="panel_placeholder" >
                                <span>Filter:</span>
                                <asp:DropDownList ID="ddlFilter" runat="server">
                                    <asp:ListItem>All</asp:ListItem>
                                    <asp:ListItem>△</asp:ListItem>
                                    <asp:ListItem>◎</asp:ListItem>
                                    <asp:ListItem>空白</asp:ListItem>
                                </asp:DropDownList>
                                <asp:Button ID="btnGetReport" runat="server" OnClick="btnGetReport_Click" Text="Fetch Report" />
                            </div>
                            <div id="placeholder123" style="overflow: overlay;" runat="server">

                                <asp:GridView PageSize="16" OnRowDataBound="dgMW_RowDataBound" AutoGenerateColumns="true" OnPageIndexChanging="dgMW_PageIndexChanging"
                                    ID="dgMW" AllowSorting="true" OnSorting="dgMW_Sorting" CssClass="Grid" PagerStyle-CssClass="pgr" AlternatingRowStyle-CssClass="alt" runat="server" AllowPaging="true">
                                    <SelectedRowStyle ForeColor="White" Font-Bold="True" BackColor="#9471DE"></SelectedRowStyle>
                                    <RowStyle ForeColor="Black" BackColor="#DEDFDE"></RowStyle>
                                    <HeaderStyle CssClass="HeaderRowStyle" />

                                </asp:GridView>

                                <%--<div id="jsGrid"></div>--%>

                            </div>
                        </div>
                    </div>
                </div>
                <asp:HiddenField ID="hidTAB" runat="server" Value="tabHeader_1" />
            </div>
        </form>
        <script>
            $(function () {
                $('#datePicker').datepicker({
                    defaultDate: new Date(),
                    onSelect: function (dateText, inst) {
                        var date = $(this).val();
                        document.getElementById('myDateField').value = date;

                    }

                });
                // get tab container
                var container = document.getElementById("tabContainer");
                var tabcon = document.getElementById("tabscontent");
                //alert(tabcon.childNodes.item(1));


                var tab = document.getElementById('<%=hidTAB.ClientID %>').value;
                // set current tab
                var navitem = document.getElementById(tab);

                //store which tab we are on
                var ident = navitem.id.split("_")[1];
                //alert(ident);
                navitem.parentNode.setAttribute("data-current", ident);
                //set current tab with class of activetabheader
                navitem.setAttribute("class", "tabActiveHeader");

                //hide two tab contents we don't need
                var pages = tabcon.getElementsByTagName("div");
                for (var i = 1; i < pages.length; i++) {
                    if (pages.item(i).getAttribute('class') == "tabpage")
                        pages.item(i).style.display = "none";
                };

                //this adds click event to tabs
                var tabs = container.getElementsByTagName("li");
                for (var i = 0; i < tabs.length; i++) {
                    tabs[i].onclick = displayPage;
                }

                displayPage(null, navitem);

                function removeByAttr(arr, attr) {
                    var i = arr.length;
                    while (i--) {
                        if (arr[i] && arr[i].key == attr) {
                            arr.splice(i, 1);
                        }
                    }
                    return arr;
                }

                $('.Button').click(function () {
                    //debugger;
                    switch (event.target.value) {

                        case "5":
                            if (event.target.checked)
                                chartDataList.push({ key: 5, value: chartData5D, name: event.target.name });
                            else
                                removeByAttr(chartDataList, 5);
                            break;
                        case "25":
                            if (event.target.checked)
                                chartDataList.push({ key: 25, value: chartData25D, name: event.target.name });
                            else
                                removeByAttr(chartDataList, 25);
                            break;
                        case "75":
                            if (event.target.checked)
                                chartDataList.push({ key: 75, value: chartData75D, name: event.target.name });
                            else
                                removeByAttr(chartDataList, 75);
                            break;
                        case "150":
                            if (event.target.checked)
                                chartDataList.push({ key: 150, value: chartData150D, name: event.target.name });
                            else
                                removeByAttr(chartDataList, 150);
                            break;
                        case "200":
                            if (event.target.checked)
                                chartDataList.push({ key: 200, value: chartData200D, name: event.target.name });
                            else
                                removeByAttr(chartDataList, 200);
                            break;
                        case "500":
                            if (event.target.checked)
                                chartDataList.push({ key: 500, value: chartDataVolume, name: event.target.name });
                            else
                                removeByAttr(chartDataList, 500);
                            break;
                        default:
                            chartDataList = [];
                            Draw(chartData, chartDataVolume, []);
                    }
                    if (chartDataList.length > 0)
                        Draw(chartData, chartDataVolume, chartDataList);
                    else
                        Draw(chartData, chartDataVolume, []);
                });

               

                //GetReports();
            });

            //function LoadReport(reportData) {
            //    debugger;
            //    $("#jsGrid").jsGrid({
            //        height: "500",
            //        width: "100%",

            //        //filtering: true,
            //        //editing: true,
            //        sorting: true,
            //        paging: true,
            //        autoload: true,

            //        pageIndex: 1,
            //        pageSize: 20,
            //        pageButtonCount: 15,
            //        pagerFormat: "Pages: {first} {prev} {pages} {next} {last}    {pageIndex} of {pageCount}",
            //        pagePrevText: "Prev",
            //        pageNextText: "Next",
            //        pageFirstText: "First",
            //        pageLastText: "Last",
            //        pageNavigatorNextText: "...",
            //        pageNavigatorPrevText: "...",

            //        loadIndication: true,
            //        loadIndicationDelay: 500,
            //        loadMessage: "Please, wait...",
            //        loadShading: true,

            //        updateOnResize: true,
            //        //controller: {
            //        //    loadData: function () {
            //        //        return data;
            //        //    }
            //        //},
            //        //url: 'index.aspx/GetReports',

            //        data:reportData,

            //        fields: [
            //            { name: "銘柄コード", type: "number", width: 70 },
            //            { name: "市場名", type: "text", width: 100 },
            //            { name: "調整後終値", type: "number", width: 100 },
            //            { name: "値幅", type: "number", width: 100 },
            //            { name: "上回り期間", type: "number", width: 100 },
            //            { name: "深さ 乖離率", type: "number", width: 100 },
            //            { name: "5SMA Volume", type: "number", width: 100 },
            //            { name: "Critical Price", type: "number", width: 100 },
            //            { name: "Range Low", type: "number", width: 100 },
            //            { name: "Range High", type: "number", width: 100 },
            //            { name: "Term", type: "number", width: 100 },
            //            { name: "Below Re-term", type: "number", width: 100 },
            //            { name: "レンジ＋150日", type: "number", width: 100 },
            //            { name: "記号", type: "number", width: 100 },
            //            { name: "RSI(14)", type: "number", width: 100 },
            //            { name: "25 SMA", type: "number", width: 100 },
            //            { name: "VR(25)", type: "number", width: 100 }//,
            //            //{ name: "Country", type: "select", items: db.countries, valueField: "Id", textField: "Name" },
            //            //{ name: "Married", type: "checkbox", title: "Is Married" }
            //        ]
            //    });
            //}

            function onReportError() {
                alert();
            }

            // on click of one of tabs
            function displayPage(e, navitem) {

                if (navitem == undefined)
                    navitem = this;
                var current = navitem.parentNode.getAttribute("data-current");
                //remove class of activetabheader and hide old contents
                document.getElementById("tabHeader_" + current).removeAttribute("class");
                document.getElementById("tabpage_" + 1).style.display = "none";
                document.getElementById("tabpage_" + 2).style.display = "none";
                document.getElementById("tabpage_" + 3).style.display = "none";

                var ident = navitem.id.split("_")[1];
                //add class of activetabheader to new active tab and show contents
                navitem.setAttribute("class", "tabActiveHeader");
                document.getElementById("tabpage_" + ident).style.display = "block";
                navitem.parentNode.setAttribute("data-current", ident);
                document.getElementById('<%=hidTAB.ClientID %>').value = document.getElementsByClassName("tabActiveHeader")[0].id;
            }
        </script>
        <script src="tabs_old.js"></script>

    </div>

</body>

</html>

