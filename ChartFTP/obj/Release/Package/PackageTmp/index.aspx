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

        h3 {
            background-color: #1688D7;
            color: white;
            /*width: 365px;*/
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
        .right{
            float:right;
            text-align:right;
        }
    </style>
    <script>

        var chartDataList = [];
        var selectedRange = 0;

        Highcharts.setOptions({
            global: {
                useUTC: false
            }
        });

        function ShowProgress() {
            setTimeout(function () {
                var modal = $('<div />');
                modal.addClass("modal");
                $('body').append(modal);
                var loading = $(".loading");
                loading.show();
                var top = Math.max($(window).height() / 2 - loading[0].offsetHeight / 2, 0);
                var left = Math.max($(window).width() / 2 - loading[0].offsetWidth / 2, 0);
                loading.css({ top: top, left: left });
                $("#lblScreeningMessage").html("現在 " + $('#datePicker').val() + " データのスクリーニング中。しばらくお待ちください");
            }, 200);
        }

        function HideProgress() {
            $(".loading").hide();
        }

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
        var chartData1D = null;
        var chartData5D = null;
        var chartData25D = null;
        var chartData75D = null;
        var chartData150D = null;
        var chartData200D = null;
        var chartDataVolume = null;
        var count = 0;

        function ReportStatus() {
            PageMethods.CheckReportStatus(onSuccessStatus, onError);
        }

        function onSuccessStatus(result) {
            if (result === 0)
                $('#btnGenerateReport').prop('disabled', false);
            else
                $('#btnGenerateReport').prop('disabled', true);
        }

        function GenerateReport() {
            PageMethods.GenerateReportStatus(onReportSucceed, onError);
        }

        function onReportSucceed(result) {
            alert('Report generated');
        }

        function fun() {
            count = 0;
            PageMethods.GetChartData(onSucceed, onError);
            PageMethods.GetChart1DData(onSucceed1D, onError);
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
        function onSucceed1D(result) {
            chartData1D = JSON.parse(result);
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
            console.log(result);
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
                        <li id="tabHeader_4">市場・銘柄登録</li>
                        <li id="tabHeader_5">パラメータ設定</li>
                    </ul>
                </div>
                <div id="tabscontent">
                    <div class="tabpage" id="tabpage_1">
                        <label class="control-label marginRight" style="margin-left: 10px;">
                            <h3 style="width:365px;">ファイル登録</h3>
                        </label>
                        <br />
                        <b style="margin-left: 45px;" class="marginRight">日付:</b>
                        <asp:TextBox runat="server" ID="datePicker" CssClass="dtPicker" ViewStateMode="Enabled" OnTextChanged="datePicker_TextChanged" />
                        <asp:HiddenField ID="myDateField" runat="server" />
                        <div style="margin-top: 20px;">
                            <label class="control-label marginRight"><b>ファイル選択: </b></label>
                            <asp:FileUpload ID="FileUpload1" runat="server" CssClass="custom-file-input Cntrl1" />
                            <br />
                            <asp:Button Text="日付・ファイル登録" OnClientClick="ShowProgress();" Style="margin-left: 80px; margin-top: 20px;" ID="upload" OnClick="Upload" runat="server" class="btn" />
                            <%--<asp:Button Text="削除" style="margin-left: 10px;margin-top: 20px;" ID="btnDelete" OnClick="btnDelete_Click" runat="server" class="btn" />--%>
                            <br />
                            <span runat="server" id="errorMsg"></span>


                            <div class="loading" align="center">
                                Loading. Please wait.<br />
                                <br />
                                <img src="loader.gif" alt="" />
                                <br />
                                <span style="margin-top:7px;" runat="server" id="lblScreeningMessage"></span>
                            </div>
                        </div>
                        <div id="stockUpdatePlaceholder" style="margin-top: 30px;">
                            <label class="control-label marginRight" style="margin-left: 10px;"><h3 style="width:365px;">銘柄上書</h3></label>
                            <br />
                            <label class="control-label marginRight" style="margin-left: 10px;"><b>銘柄コード: </b></label>
                            <asp:TextBox ID="txtUpdatedStockCode" Width="85" runat="server" onkeypress="return CheckNumeric(this)" ></asp:TextBox>
                            <br />
                             <div style="margin-top: 20px;">
                            <label class="control-label marginRight"><b>ファイル選択: </b></label>
                            <asp:FileUpload ID="FileUpload2" runat="server" CssClass="custom-file-input Cntrl1" />
                                 </div>
                            <br />
                            <asp:Button Text="日付・ファイル登録" style="margin-left: 80px;margin-top: 0px;" ID="uploadStock" OnClick="uploadStock_Click" runat="server" class="btn" />
                            <br />
                            <span runat="server" id="errorMsgStock"></span>
                        </div>

                       
                            
                          
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

                                <div class="leftColumn" id="dvleftColumn" runat="server">
                                    <h3 style="margin-left: -13px;">銘柄一覧</h3>
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

                                <div class="rightColumn" >
                                    <div class="main-box">

                                        <div style="float: left">
                                            <input type="checkbox" class="Button" id="rdToday" value="1" name="現在値" /><span>現在値</span>
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
                        <%--<h2>SCReening</h2>--%>
                        <div id="scrip-list1" style="height: 420px; padding: 0px; position: relative;">
                            <div id="panel_placeholder" >
                                <span>記号:</span>
                                <asp:DropDownList ID="ddlFilter" runat="server">
                                    <asp:ListItem Value="0">全て</asp:ListItem>
                                    <asp:ListItem Value="1">△</asp:ListItem>
                                    <asp:ListItem Value="2">◎</asp:ListItem>
                                    <asp:ListItem Value="3">空白</asp:ListItem>
                                </asp:DropDownList>

                                <span style="margin-left:20px;">RSI(14):</span>
                                <asp:TextBox ID="txtRSILower" Width="50" runat="server" />
                                <span runat="server" id="spRSI">-</span>
                                <asp:TextBox ID="txtRSIUpper" Width="50" runat="server" />

                                <asp:DropDownList ID="ddlRSIFilter" AutoPostBack="true" OnSelectedIndexChanged="ddlRSIFilter_SelectedIndexChanged" runat="server">
                                    <asp:ListItem Value="0">全て</asp:ListItem>
                                    <asp:ListItem Value="1">以上</asp:ListItem>
                                    <asp:ListItem Value="2">以下</asp:ListItem>
                                    <asp:ListItem Value="3">範囲</asp:ListItem>
                                </asp:DropDownList>



                                 <span style="margin-left:20px;">値幅:</span>
                                <asp:TextBox ID="txtPriceRangeLower" Width="50" runat="server" />
                                  <span runat="server" id="spPriceRange">-</span>
                                <asp:TextBox ID="txtPriceRangeUpper" Width="50" runat="server" />

                                <asp:DropDownList ID="ddlPriceRange" AutoPostBack="true" OnSelectedIndexChanged="ddlPriceRange_SelectedIndexChanged" runat="server">
                                    <asp:ListItem Value="0">全て</asp:ListItem>
                                    <asp:ListItem Value="1">以上</asp:ListItem>
                                    <asp:ListItem Value="2">以下</asp:ListItem>
                                    <asp:ListItem Value="3">範囲</asp:ListItem>
                                </asp:DropDownList>

                                 <span style="margin-left:20px;">25 SMA:</span>
                                <asp:TextBox ID="txt25SMALower" Width="50" runat="server" />
                                  <span runat="server" id="sp25SMA">-</span>
                                <asp:TextBox ID="txt25SMAUpper" Width="50" runat="server" />

                                <asp:DropDownList ID="ddl25SMAFilter" AutoPostBack="true" OnSelectedIndexChanged="ddl25SMAFilter_SelectedIndexChanged" runat="server">
                                    <asp:ListItem Value="0">全て</asp:ListItem>
                                    <asp:ListItem Value="1">以上</asp:ListItem>
                                    <asp:ListItem Value="2">以下</asp:ListItem>
                                    <asp:ListItem Value="3">範囲</asp:ListItem>
                                </asp:DropDownList>

                                 <span style="margin-left:20px;">VR(25):</span>
                                <asp:TextBox ID="txtVR25Lower" Width="50" runat="server" />
                                  <span runat="server" id="spVR25">-</span>
                                <asp:TextBox ID="txtVR25Upper" Width="50" runat="server" />

                                <asp:DropDownList ID="ddlVR25Filter" AutoPostBack="true" OnSelectedIndexChanged="ddlVR25Filter_SelectedIndexChanged" runat="server">
                                    <asp:ListItem Value="0">全て</asp:ListItem>
                                    <asp:ListItem Value="1">以上</asp:ListItem>
                                    <asp:ListItem Value="2">以下</asp:ListItem>
                                    <asp:ListItem Value="3">範囲</asp:ListItem>
                                </asp:DropDownList>

                                <asp:Button class="btn" ID="btnGetReport" runat="server" OnClick="btnGetReport_Click" Text="検索" />
                                <asp:Button class="btn" ID="btnResetFilter" runat="server" OnClick="btnResetFilter_Click" Text="リセット" />
                                <%--<asp:Button ID="btnGenerateReport" runat="server" OnClick="btnGenerateReport_Click" Text="Resultテーブル生成" />--%>
                                <br />
                               <b style="color:darkblue;"> <asp:Label runat="server" ID="lblUpdateMessage" ></asp:Label></b>
                            </div>
                            <div id="placeholder123" style="overflow: overlay;" runat="server">

                                <asp:GridView PageSize="20" OnRowDataBound="dgMW_RowDataBound" AutoGenerateColumns="true" OnPageIndexChanging="dgMW_PageIndexChanging"
                                    ID="dgMW" AllowSorting="true" OnSorting="dgMW_Sorting" CssClass="Grid" PagerStyle-CssClass="pgr" AlternatingRowStyle-CssClass="alt" runat="server" AllowPaging="true">
                                    <SelectedRowStyle ForeColor="White" Font-Bold="True" BackColor="#9471DE"></SelectedRowStyle>
                                    <RowStyle ForeColor="Black" BackColor="#DEDFDE"></RowStyle>
                                    <HeaderStyle CssClass="HeaderRowStyle" />

                                </asp:GridView>

                                <%--<div id="jsGrid"></div>--%>

                            </div>
                        </div>
                    </div>

                    <div class="tabpage" id="tabpage_4">
                        <div id="dvMarketMaster">
                            <span><h3 style="width:365px;">市場情報入力</h3></span>
                            <br />
                            <br />
                            <span style="margin-left: 20px;">市場名:</span>
                            <asp:TextBox runat="server" Width="150" ID="txtMarketName" />
                            <br />
                            <br />
                            <asp:Button Style="margin-left: 66px;" Width="150" class="btn" Text="市場登録" ID="btnSubmitMarketMaster" OnClick="btnSubmitMarketMaster_Click" runat="server" />
                            <br />
                            <br />
                            <span runat="server" id="lblMarketMaster" />
                            <br />
                            <br />
                            <br />
                            <br />
                        </div>
                        <br />
                        <div id="dvStockMaster">
                            <span><h3 style="width:365px;">銘柄情報入力</h3></span>
                            <br />
                            <br />
                            <span>銘柄コード:</span>
                            <asp:TextBox Width="150" ID="txtStockCode" runat="server" />
                            <br />
                            <br />
                            <span style="margin-left: 19px;">銘柄名:</span>
                            <asp:TextBox Width="150" ID="txtStockName" runat="server" />
                            <br />
                            <br />
                            <span>銘柄タイプ:</span>
                            <asp:TextBox Width="150" ID="txtStockType" runat="server" />
                            <br />
                            <br />
                            <span style="margin-left: 19px;">市場名:</span>
                            <asp:DropDownList Width="150" ID="ddlMarketName" runat="server" />
                            <br />
                            <br />
                            <asp:Button Style="margin-left: 66px;" Width="150" class="btn" Text="銘柄登録" ID="btnSubmitStockMaster" OnClick="btnSubmitStockMaster_Click" runat="server" />
                            <br />
                            <br />
                            <span runat="server" id="lblStockMaster" />
                            <br />
                            <br />
                            <br />
                            <br />
                        </div>
                    </div>

                    <div class="tabpage" id="tabpage_5">
                        <div> 
                            <asp:Button class="btn" Width="100" ID="btnSubmitParameters" runat="server" Text="保存" OnClick="btnSubmitParameters_Click" />
                            <asp:Button class="btn" Width="100" ID="btnResetParameters" runat="server" Text="リセット" OnClick="btnResetParameters_Click" />
                        </div>

                        <div style="width: 300px; float: left; height: 500px; margin: 5px; overflow-y: auto;">
                            <asp:ListView EnableTheming="true" ID="dgParameterK2C" runat="server">
                                <LayoutTemplate>
                                    <table class="Grid" runat="server" id="table1">
                                        <thead>
                                            <tr runat="server" id="headerRow">
                                                <th style="text-align:left;">パラメータ名</th>
                                                <th style="text-align:right;">値</th>
                                            </tr>
                                        </thead>
                                        <tbody>
                                            <tr runat="server" id="groupPlaceholder">
                                            </tr>
                                        </tbody>
                                    </table>
                                </LayoutTemplate>
                                <GroupTemplate>
                                    <tr runat="server" id="tableRow">
                                        <td runat="server" id="itemPlaceholder" />
                                    </tr>
                                </GroupTemplate>
                                <ItemTemplate>
                                    <td class="Gridtd" id="td1" runat="server">
                                        <asp:Label ID="lblName" runat="server" Text='<%# Eval("Name") %>'></asp:Label>
                                        </td>
                                    <td class="Gridtd" id="td2" runat="server">
                                        <asp:TextBox Width="100" CssClass="right" ID="txtColumnValue" onkeypress="return CheckNumeric(this)" OnTextChanged="txtColumnValue_TextChanged" AutoPostBack="true" runat="server" Text='<%# Eval("Value") %>'></asp:TextBox>
                                        <%--<asp:Label ID="lblValueName" CssClass="right" runat="server" Text='<%# Eval("Value") %>'></asp:Label>--%>
                                    </td>
                                </ItemTemplate>
                                <EmptyDataTemplate>
                                    <p>
                                        No parameters found.
                                    </p>
                                </EmptyDataTemplate>
                            </asp:ListView>
                            </div>
                        <div style="width:300px; float:left; height:500px; margin:5px; overflow-y:auto;">
                            <asp:ListView EnableTheming="true" ID="dgParameterW2D" runat="server">
                                <LayoutTemplate>
                                    <table class="Grid" runat="server" id="table1">
                                        <thead>
                                            <tr runat="server" id="headerRow">
                                                <th style="text-align:left;">パラメータ名</th>
                                                <th style="text-align:right;">値</th>
                                            </tr>
                                        </thead>
                                        <tbody>
                                            <tr runat="server" id="groupPlaceholder">
                                            </tr>
                                        </tbody>
                                    </table>
                                </LayoutTemplate>
                                <GroupTemplate>
                                    <tr runat="server" id="tableRow">
                                        <td class="Gridtd" runat="server" id="itemPlaceholder" />
                                    </tr>
                                </GroupTemplate>
                                <ItemTemplate>
                                    <td class="Gridtd" id="td1" runat="server">
                                        <asp:Label ID="lblName" runat="server" Text='<%# Eval("Name") %>'></asp:Label>
                                        </td>
                                    <td class="Gridtd" id="td2" runat="server">
                                        <asp:TextBox Width="100" CssClass="right" ID="txtColumnValue" onkeypress="return CheckNumeric(this)" OnTextChanged="txtColumnValue_TextChanged" AutoPostBack="true" runat="server" Text='<%# Eval("Value") %>'></asp:TextBox>
                                        <%--<asp:Label ID="lblValueName" CssClass="right" runat="server" Text='<%# Eval("Value") %>'></asp:Label>--%>
                                    </td>
                                </ItemTemplate>
                                <EmptyDataTemplate>
                                    <p>
                                        No parameters found.
                                    </p>
                                </EmptyDataTemplate>
                            </asp:ListView>
                            </div>
                       <div style="width:300px; float:left; height:500px; margin:5px; overflow-y:auto;">
                            <asp:ListView EnableTheming="true" ID="dgParameterP" runat="server">
                                <LayoutTemplate>
                                   <table class="Grid" runat="server" id="table1">
                                        <thead>
                                            <tr runat="server" id="headerRow">
                                                <th style="text-align:left;">パラメータ名</th>
                                                <th style="text-align:right;">値</th>
                                            </tr>
                                        </thead>
                                        <tbody>
                                            <tr runat="server" id="groupPlaceholder">
                                            </tr>
                                        </tbody>
                                    </table>
                                </LayoutTemplate>
                                <GroupTemplate>
                                    <tr runat="server" id="tableRow">
                                        <td runat="server" id="itemPlaceholder" />
                                    </tr>
                                </GroupTemplate>
                                <ItemTemplate>
                                    <td class="Gridtd" id="td1" runat="server">
                                        <asp:Label ID="lblName" runat="server" Text='<%# Eval("Name") %>'></asp:Label>
                                        </td>
                                    <td class="Gridtd" id="td2" runat="server">
                                        <asp:TextBox Width="100" CssClass="right" ID="txtColumnValue" onkeypress="return CheckNumeric(this)" OnTextChanged="txtColumnValue_TextChanged" AutoPostBack="true" runat="server" Text='<%# Eval("Value") %>'></asp:TextBox>
                                        <%--<asp:Label ID="lblValueName" CssClass="right" runat="server" Text='<%# Eval("Value") %>'></asp:Label>--%>
                                    </td>
                                </ItemTemplate>
                                <EmptyDataTemplate>
                                    <p>
                                        No parameters found.
                                    </p>
                                </EmptyDataTemplate>
                            </asp:ListView>
                            </div>
                    <%--<asp:Button ID="btnEditParameters" runat="server" Text="Edit" OnClick="btnEditParameters_Click" />--%>
                        
                    </div>

                </div>
                <asp:HiddenField ID="hidTAB" runat="server" Value="tabHeader_1" />
            </div>
        </form>
        <script>
            function CheckNumeric(evt) {
                var charCode = (evt.which) ? evt.which : event.keyCode
                if (charCode > 31 && (charCode < 48 || charCode > 57) && charCode != 46 && charCode != 45)
                    return false;
                
                if (charCode == 46) {
                    if (evt.value.indexOf('.') > -1)
                        return false;
                }

                if (charCode == 45) {
                    if (evt.value.indexOf('-') > -1)
                        return false;
                }
                return true;
            }

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
                        case "1":
                            if (event.target.checked)
                                chartDataList.push({ key: 1, value: chartData1D, name: event.target.name });
                            else
                                removeByAttr(chartDataList, 1);
                            break;
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

                $('#btnGenerateReport').click(function () {
                    GenerateReport();
                    return;
                });

            });

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
                document.getElementById("tabpage_" + 4).style.display = "none";
                document.getElementById("tabpage_" + 5).style.display = "none";

                var ident = navitem.id.split("_")[1];
                //add class of activetabheader to new active tab and show contents
                navitem.setAttribute("class", "tabActiveHeader");
                document.getElementById("tabpage_" + ident).style.display = "block";
                navitem.parentNode.setAttribute("data-current", ident);
                document.getElementById('<%=hidTAB.ClientID %>').value = document.getElementsByClassName("tabActiveHeader")[0].id;
            }
        </script>
       
    </div>

</body>

</html>

