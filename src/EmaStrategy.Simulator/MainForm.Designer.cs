#nullable disable

namespace EmaStrategy.Simulator;

partial class MainForm
{
    private System.ComponentModel.IContainer components = null;

    private TableLayoutPanel _tableRoot;
    private TableLayoutPanel _tableOptions;
    private TableLayoutPanel _tableBottom;
    private FlowLayoutPanel _panelRun;

    private Label _lblSymbolFolder;
    private Label _lblEmaPeriodsComma;
    private Label _lblWarmupBars;
    private Label _lblMaxOpen;
    private Label _lblMaxClosed;
    private Label _lblOrderQty;
    private Label _lblPriorCandle;
    private Label _lblChaseEma;
    private Label _lblMinDist;
    private Label _lblTpMode;
    private Label _lblTpPts;
    private Label _lblTpUsd;
    private Label _lblSlMode;
    private Label _lblSlPts;
    private Label _lblSlUsd;
    private Label _lblSlAnchorEma;
    private Label _lblDollarsPerPoint;
    private Label _lblEvalSec;
    private Label _lblCdOrder;
    private Label _lblCdExit;
    private Label _lblFillMode;
    private Label _lblSyntheticBars;
    private Label _lblSyntheticPrice;

    private ComboBox _cmbSymbol;
    private TextBox _txtEmaPeriods;
    private NumericUpDown _numWarmup;
    private NumericUpDown _numMaxOpen;
    private NumericUpDown _numMaxClosed;
    private NumericUpDown _numOrderQty;
    private ComboBox _cmbPriorField;
    private NumericUpDown _numChaseEma;
    private NumericUpDown _numMinDist;
    private ComboBox _cmbTpMode;
    private NumericUpDown _numTpPts;
    private NumericUpDown _numTpUsd;
    private ComboBox _cmbSlMode;
    private NumericUpDown _numSlPts;
    private NumericUpDown _numSlUsd;
    private NumericUpDown _numSlAnchorEma;
    private NumericUpDown _numDollarsPerPoint;
    private NumericUpDown _numEvalSec;
    private NumericUpDown _numCdOrder;
    private NumericUpDown _numCdExit;
    private ComboBox _cmbFill;
    private CheckBox _chkAvgDown;
    private NumericUpDown _numSyntheticCount;
    private NumericUpDown _numSyntheticPrice;
    private TextBox _txtLog;
    private Button _btnRun;

    /// <summary>
    /// Required for Designer support. Edit layout via the WinForms designer or by adjusting properties here.
    /// </summary>
    private void InitializeComponent()
    {
        this._tableRoot = new TableLayoutPanel();
        this._tableBottom = new TableLayoutPanel();
        this._panelRun = new FlowLayoutPanel();
        this._btnRun = new Button();
        this._txtLog = new TextBox();
        this._tableOptions = new TableLayoutPanel();
        this._lblSymbolFolder = new Label();
        this._cmbSymbol = new ComboBox();
        this._lblEmaPeriodsComma = new Label();
        this._txtEmaPeriods = new TextBox();
        this._lblWarmupBars = new Label();
        this._numWarmup = new NumericUpDown();
        this._lblMaxOpen = new Label();
        this._numMaxOpen = new NumericUpDown();
        this._lblMaxClosed = new Label();
        this._numMaxClosed = new NumericUpDown();
        this._lblOrderQty = new Label();
        this._numOrderQty = new NumericUpDown();
        this._lblPriorCandle = new Label();
        this._cmbPriorField = new ComboBox();
        this._lblChaseEma = new Label();
        this._numChaseEma = new NumericUpDown();
        this._lblMinDist = new Label();
        this._numMinDist = new NumericUpDown();
        this._lblTpMode = new Label();
        this._cmbTpMode = new ComboBox();
        this._lblTpPts = new Label();
        this._numTpPts = new NumericUpDown();
        this._lblTpUsd = new Label();
        this._numTpUsd = new NumericUpDown();
        this._lblSlMode = new Label();
        this._cmbSlMode = new ComboBox();
        this._lblSlPts = new Label();
        this._numSlPts = new NumericUpDown();
        this._lblSlUsd = new Label();
        this._numSlUsd = new NumericUpDown();
        this._lblSlAnchorEma = new Label();
        this._numSlAnchorEma = new NumericUpDown();
        this._lblDollarsPerPoint = new Label();
        this._numDollarsPerPoint = new NumericUpDown();
        this._lblEvalSec = new Label();
        this._numEvalSec = new NumericUpDown();
        this._lblCdOrder = new Label();
        this._numCdOrder = new NumericUpDown();
        this._lblCdExit = new Label();
        this._numCdExit = new NumericUpDown();
        this._lblFillMode = new Label();
        this._cmbFill = new ComboBox();
        this._lblSyntheticBars = new Label();
        this._numSyntheticCount = new NumericUpDown();
        this._lblSyntheticPrice = new Label();
        this._numSyntheticPrice = new NumericUpDown();
        this._chkAvgDown = new CheckBox();
        this._tableRoot.SuspendLayout();
        this._tableBottom.SuspendLayout();
        this._panelRun.SuspendLayout();
        this._tableOptions.SuspendLayout();
        ((System.ComponentModel.ISupportInitialize)this._numWarmup).BeginInit();
        ((System.ComponentModel.ISupportInitialize)this._numMaxOpen).BeginInit();
        ((System.ComponentModel.ISupportInitialize)this._numMaxClosed).BeginInit();
        ((System.ComponentModel.ISupportInitialize)this._numOrderQty).BeginInit();
        ((System.ComponentModel.ISupportInitialize)this._numChaseEma).BeginInit();
        ((System.ComponentModel.ISupportInitialize)this._numMinDist).BeginInit();
        ((System.ComponentModel.ISupportInitialize)this._numTpPts).BeginInit();
        ((System.ComponentModel.ISupportInitialize)this._numTpUsd).BeginInit();
        ((System.ComponentModel.ISupportInitialize)this._numSlPts).BeginInit();
        ((System.ComponentModel.ISupportInitialize)this._numSlUsd).BeginInit();
        ((System.ComponentModel.ISupportInitialize)this._numSlAnchorEma).BeginInit();
        ((System.ComponentModel.ISupportInitialize)this._numDollarsPerPoint).BeginInit();
        ((System.ComponentModel.ISupportInitialize)this._numEvalSec).BeginInit();
        ((System.ComponentModel.ISupportInitialize)this._numCdOrder).BeginInit();
        ((System.ComponentModel.ISupportInitialize)this._numCdExit).BeginInit();
        ((System.ComponentModel.ISupportInitialize)this._numSyntheticCount).BeginInit();
        ((System.ComponentModel.ISupportInitialize)this._numSyntheticPrice).BeginInit();
        this.SuspendLayout();
        // 
        // _tableRoot
        // 
        this._tableRoot.ColumnCount = 1;
        this._tableRoot.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 20F));
        this._tableRoot.Controls.Add(this._tableBottom, 0, 1);
        this._tableRoot.Controls.Add(this._tableOptions, 0, 0);
        this._tableRoot.Dock = DockStyle.Fill;
        this._tableRoot.Location = new Point(0, 0);
        this._tableRoot.Name = "_tableRoot";
        this._tableRoot.RowCount = 2;
        this._tableRoot.RowStyles.Add(new RowStyle());
        this._tableRoot.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
        this._tableRoot.Size = new Size(1138, 831);
        this._tableRoot.TabIndex = 0;
        // 
        // _tableBottom
        // 
        this._tableBottom.ColumnCount = 1;
        this._tableBottom.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 20F));
        this._tableBottom.Controls.Add(this._panelRun, 0, 0);
        this._tableBottom.Controls.Add(this._txtLog, 0, 1);
        this._tableBottom.Dock = DockStyle.Fill;
        this._tableBottom.Location = new Point(3, 417);
        this._tableBottom.Name = "_tableBottom";
        this._tableBottom.Padding = new Padding(8);
        this._tableBottom.RowCount = 2;
        this._tableBottom.RowStyles.Add(new RowStyle(SizeType.Absolute, 40F));
        this._tableBottom.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
        this._tableBottom.Size = new Size(1132, 411);
        this._tableBottom.TabIndex = 1;
        // 
        // _panelRun
        // 
        this._panelRun.AutoSize = true;
        this._panelRun.Controls.Add(this._btnRun);
        this._panelRun.Location = new Point(11, 11);
        this._panelRun.Name = "_panelRun";
        this._panelRun.Size = new Size(103, 31);
        this._panelRun.TabIndex = 0;
        // 
        // _btnRun
        // 
        this._btnRun.AutoSize = true;
        this._btnRun.Location = new Point(3, 3);
        this._btnRun.Name = "_btnRun";
        this._btnRun.Size = new Size(97, 25);
        this._btnRun.TabIndex = 0;
        this._btnRun.Text = "Run simulation";
        this._btnRun.UseVisualStyleBackColor = true;
        this._btnRun.Click += this.OnRunClick;
        // 
        // _txtLog
        // 
        this._txtLog.Dock = DockStyle.Fill;
        this._txtLog.Font = new Font("Consolas", 9F);
        this._txtLog.Location = new Point(11, 51);
        this._txtLog.Multiline = true;
        this._txtLog.Name = "_txtLog";
        this._txtLog.ReadOnly = true;
        this._txtLog.ScrollBars = ScrollBars.Both;
        this._txtLog.Size = new Size(1110, 349);
        this._txtLog.TabIndex = 1;
        this._txtLog.WordWrap = false;
        // 
        // _tableOptions
        // 
        this._tableOptions.AutoSize = true;
        this._tableOptions.AutoSizeMode = AutoSizeMode.GrowAndShrink;
        this._tableOptions.ColumnCount = 4;
        this._tableOptions.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 160F));
        this._tableOptions.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
        this._tableOptions.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 170F));
        this._tableOptions.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
        this._tableOptions.Controls.Add(this._lblSymbolFolder, 0, 0);
        this._tableOptions.Controls.Add(this._cmbSymbol, 1, 0);
        this._tableOptions.Controls.Add(this._lblEmaPeriodsComma, 2, 0);
        this._tableOptions.Controls.Add(this._txtEmaPeriods, 3, 0);
        this._tableOptions.Controls.Add(this._lblWarmupBars, 0, 1);
        this._tableOptions.Controls.Add(this._numWarmup, 1, 1);
        this._tableOptions.SetColumnSpan(this._numWarmup, 3);
        this._tableOptions.Controls.Add(this._lblMaxOpen, 0, 2);
        this._tableOptions.Controls.Add(this._numMaxOpen, 1, 2);
        this._tableOptions.Controls.Add(this._lblMaxClosed, 2, 2);
        this._tableOptions.Controls.Add(this._numMaxClosed, 3, 2);
        this._tableOptions.Controls.Add(this._lblOrderQty, 0, 3);
        this._tableOptions.Controls.Add(this._numOrderQty, 1, 3);
        this._tableOptions.Controls.Add(this._lblPriorCandle, 2, 3);
        this._tableOptions.Controls.Add(this._cmbPriorField, 3, 3);
        this._tableOptions.Controls.Add(this._lblChaseEma, 0, 4);
        this._tableOptions.Controls.Add(this._numChaseEma, 1, 4);
        this._tableOptions.Controls.Add(this._lblMinDist, 2, 4);
        this._tableOptions.Controls.Add(this._numMinDist, 3, 4);
        this._tableOptions.Controls.Add(this._lblTpMode, 0, 5);
        this._tableOptions.Controls.Add(this._cmbTpMode, 1, 5);
        this._tableOptions.Controls.Add(this._lblTpPts, 2, 5);
        this._tableOptions.Controls.Add(this._numTpPts, 3, 5);
        this._tableOptions.Controls.Add(this._lblTpUsd, 0, 6);
        this._tableOptions.Controls.Add(this._numTpUsd, 1, 6);
        this._tableOptions.Controls.Add(this._lblSlMode, 2, 6);
        this._tableOptions.Controls.Add(this._cmbSlMode, 3, 6);
        this._tableOptions.Controls.Add(this._lblSlPts, 0, 7);
        this._tableOptions.Controls.Add(this._numSlPts, 1, 7);
        this._tableOptions.Controls.Add(this._lblSlUsd, 2, 7);
        this._tableOptions.Controls.Add(this._numSlUsd, 3, 7);
        this._tableOptions.Controls.Add(this._lblSlAnchorEma, 0, 8);
        this._tableOptions.Controls.Add(this._numSlAnchorEma, 1, 8);
        this._tableOptions.Controls.Add(this._lblDollarsPerPoint, 2, 8);
        this._tableOptions.Controls.Add(this._numDollarsPerPoint, 3, 8);
        this._tableOptions.Controls.Add(this._lblEvalSec, 0, 9);
        this._tableOptions.Controls.Add(this._numEvalSec, 1, 9);
        this._tableOptions.Controls.Add(this._lblCdOrder, 2, 9);
        this._tableOptions.Controls.Add(this._numCdOrder, 3, 9);
        this._tableOptions.Controls.Add(this._lblCdExit, 0, 10);
        this._tableOptions.Controls.Add(this._numCdExit, 1, 10);
        this._tableOptions.Controls.Add(this._lblFillMode, 2, 10);
        this._tableOptions.Controls.Add(this._cmbFill, 3, 10);
        this._tableOptions.Controls.Add(this._lblSyntheticBars, 0, 11);
        this._tableOptions.Controls.Add(this._numSyntheticCount, 1, 11);
        this._tableOptions.Controls.Add(this._lblSyntheticPrice, 2, 11);
        this._tableOptions.Controls.Add(this._numSyntheticPrice, 3, 11);
        this._tableOptions.Controls.Add(this._chkAvgDown, 0, 12);
        this._tableOptions.SetColumnSpan(this._chkAvgDown, 4);
        this._tableOptions.Location = new Point(3, 3);
        this._tableOptions.Name = "_tableOptions";
        this._tableOptions.Padding = new Padding(8);
        this._tableOptions.RowCount = 13;
        this._tableOptions.RowStyles.Add(new RowStyle());
        this._tableOptions.RowStyles.Add(new RowStyle());
        this._tableOptions.RowStyles.Add(new RowStyle());
        this._tableOptions.RowStyles.Add(new RowStyle());
        this._tableOptions.RowStyles.Add(new RowStyle());
        this._tableOptions.RowStyles.Add(new RowStyle());
        this._tableOptions.RowStyles.Add(new RowStyle());
        this._tableOptions.RowStyles.Add(new RowStyle());
        this._tableOptions.RowStyles.Add(new RowStyle());
        this._tableOptions.RowStyles.Add(new RowStyle());
        this._tableOptions.RowStyles.Add(new RowStyle());
        this._tableOptions.RowStyles.Add(new RowStyle());
        this._tableOptions.RowStyles.Add(new RowStyle());
        this._tableOptions.Size = new Size(1132, 408);
        this._tableOptions.TabIndex = 0;
        // 
        // _lblSymbolFolder
        // 
        this._lblSymbolFolder.Anchor = AnchorStyles.Left;
        this._lblSymbolFolder.AutoSize = true;
        this._lblSymbolFolder.Location = new Point(11, 15);
        this._lblSymbolFolder.Name = "_lblSymbolFolder";
        this._lblSymbolFolder.Size = new Size(89, 15);
        this._lblSymbolFolder.TabIndex = 0;
        this._lblSymbolFolder.Text = "Symbol (folder)";
        // 
        // _cmbSymbol
        // 
        this._cmbSymbol.Anchor = AnchorStyles.Left | AnchorStyles.Right;
        this._cmbSymbol.DropDownStyle = ComboBoxStyle.DropDownList;
        this._cmbSymbol.Items.AddRange(new object[] { "MNQ", "NQ", "ES", "MES" });
        this._cmbSymbol.Location = new Point(171, 11);
        this._cmbSymbol.Name = "_cmbSymbol";
        this._cmbSymbol.Size = new Size(387, 23);
        this._cmbSymbol.TabIndex = 1;
        // 
        // _lblEmaPeriodsComma
        // 
        this._lblEmaPeriodsComma.Anchor = AnchorStyles.Left;
        this._lblEmaPeriodsComma.AutoSize = true;
        this._lblEmaPeriodsComma.Location = new Point(564, 15);
        this._lblEmaPeriodsComma.Name = "_lblEmaPeriodsComma";
        this._lblEmaPeriodsComma.Size = new Size(126, 15);
        this._lblEmaPeriodsComma.TabIndex = 2;
        this._lblEmaPeriodsComma.Text = "EMA periods (comma)";
        // 
        // _txtEmaPeriods
        // 
        this._txtEmaPeriods.Anchor = AnchorStyles.Left | AnchorStyles.Right;
        this._txtEmaPeriods.Location = new Point(734, 11);
        this._txtEmaPeriods.Name = "_txtEmaPeriods";
        this._txtEmaPeriods.Size = new Size(387, 23);
        this._txtEmaPeriods.TabIndex = 3;
        // 
        // _lblWarmupBars
        // 
        this._lblWarmupBars.Anchor = AnchorStyles.Left;
        this._lblWarmupBars.AutoSize = true;
        this._lblWarmupBars.Location = new Point(11, 48);
        this._lblWarmupBars.Name = "_lblWarmupBars";
        this._lblWarmupBars.Size = new Size(78, 15);
        this._lblWarmupBars.TabIndex = 6;
        this._lblWarmupBars.Text = "Warmup bars";
        // 
        // _numWarmup
        // 
        this._numWarmup.Anchor = AnchorStyles.Left | AnchorStyles.Right;
        this._numWarmup.Location = new Point(171, 44);
        this._numWarmup.Maximum = new decimal(new int[] { 100000, 0, 0, 0 });
        this._numWarmup.Minimum = new decimal(new int[] { 1, 0, 0, 0 });
        this._numWarmup.Name = "_numWarmup";
        this._numWarmup.Size = new Size(387, 23);
        this._numWarmup.TabIndex = 7;
        this._numWarmup.Value = new decimal(new int[] { 120, 0, 0, 0 });
        // 
        // _lblMaxOpen
        // 
        this._lblMaxOpen.Anchor = AnchorStyles.Left;
        this._lblMaxOpen.AutoSize = true;
        this._lblMaxOpen.Location = new Point(11, 81);
        this._lblMaxOpen.Name = "_lblMaxOpen";
        this._lblMaxOpen.Size = new Size(112, 15);
        this._lblMaxOpen.TabIndex = 8;
        this._lblMaxOpen.Text = "Max open contracts";
        // 
        // _numMaxOpen
        // 
        this._numMaxOpen.Anchor = AnchorStyles.Left | AnchorStyles.Right;
        this._numMaxOpen.Location = new Point(171, 77);
        this._numMaxOpen.Maximum = new decimal(new int[] { 1000, 0, 0, 0 });
        this._numMaxOpen.Minimum = new decimal(new int[] { 1, 0, 0, 0 });
        this._numMaxOpen.Name = "_numMaxOpen";
        this._numMaxOpen.Size = new Size(387, 23);
        this._numMaxOpen.TabIndex = 9;
        this._numMaxOpen.Value = new decimal(new int[] { 1, 0, 0, 0 });
        // 
        // _lblMaxClosed
        // 
        this._lblMaxClosed.Anchor = AnchorStyles.Left;
        this._lblMaxClosed.AutoSize = true;
        this._lblMaxClosed.Location = new Point(564, 81);
        this._lblMaxClosed.Name = "_lblMaxClosed";
        this._lblMaxClosed.Size = new Size(116, 15);
        this._lblMaxClosed.TabIndex = 10;
        this._lblMaxClosed.Text = "Max closed (session)";
        // 
        // _numMaxClosed
        // 
        this._numMaxClosed.Anchor = AnchorStyles.Left | AnchorStyles.Right;
        this._numMaxClosed.Location = new Point(734, 77);
        this._numMaxClosed.Maximum = new decimal(new int[] { 10000000, 0, 0, 0 });
        this._numMaxClosed.Name = "_numMaxClosed";
        this._numMaxClosed.Size = new Size(387, 23);
        this._numMaxClosed.TabIndex = 11;
        this._numMaxClosed.Value = new decimal(new int[] { 10000000, 0, 0, 0 });
        // 
        // _lblOrderQty
        // 
        this._lblOrderQty.Anchor = AnchorStyles.Left;
        this._lblOrderQty.AutoSize = true;
        this._lblOrderQty.Location = new Point(11, 110);
        this._lblOrderQty.Name = "_lblOrderQty";
        this._lblOrderQty.Size = new Size(117, 15);
        this._lblOrderQty.TabIndex = 12;
        this._lblOrderQty.Text = "Order qty (contracts)";
        // 
        // _numOrderQty
        // 
        this._numOrderQty.Anchor = AnchorStyles.Left | AnchorStyles.Right;
        this._numOrderQty.DecimalPlaces = 2;
        this._numOrderQty.Increment = new decimal(new int[] { 25, 0, 0, 131072 });
        this._numOrderQty.Location = new Point(171, 106);
        this._numOrderQty.Maximum = new decimal(new int[] { 1000, 0, 0, 0 });
        this._numOrderQty.Minimum = new decimal(new int[] { 1, 0, 0, 0 });
        this._numOrderQty.Name = "_numOrderQty";
        this._numOrderQty.Size = new Size(387, 23);
        this._numOrderQty.TabIndex = 13;
        this._numOrderQty.Value = new decimal(new int[] { 1, 0, 0, 0 });
        // 
        // _lblPriorCandle
        // 
        this._lblPriorCandle.Anchor = AnchorStyles.Left;
        this._lblPriorCandle.AutoSize = true;
        this._lblPriorCandle.Location = new Point(564, 110);
        this._lblPriorCandle.Name = "_lblPriorCandle";
        this._lblPriorCandle.Size = new Size(99, 15);
        this._lblPriorCandle.TabIndex = 14;
        this._lblPriorCandle.Text = "Prior candle price";
        // 
        // _cmbPriorField
        // 
        this._cmbPriorField.Anchor = AnchorStyles.Left | AnchorStyles.Right;
        this._cmbPriorField.DropDownStyle = ComboBoxStyle.DropDownList;
        this._cmbPriorField.Items.AddRange(new object[] { "Open", "Close", "High", "Low", "Mid" });
        this._cmbPriorField.Location = new Point(734, 106);
        this._cmbPriorField.Name = "_cmbPriorField";
        this._cmbPriorField.Size = new Size(387, 23);
        this._cmbPriorField.TabIndex = 15;
        // 
        // _lblChaseEma
        // 
        this._lblChaseEma.Anchor = AnchorStyles.Left;
        this._lblChaseEma.AutoSize = true;
        this._lblChaseEma.Location = new Point(11, 139);
        this._lblChaseEma.Name = "_lblChaseEma";
        this._lblChaseEma.Size = new Size(121, 15);
        this._lblChaseEma.TabIndex = 16;
        this._lblChaseEma.Text = "Chase ref EMA period";
        // 
        // _numChaseEma
        // 
        this._numChaseEma.Anchor = AnchorStyles.Left | AnchorStyles.Right;
        this._numChaseEma.Location = new Point(171, 135);
        this._numChaseEma.Maximum = new decimal(new int[] { 500, 0, 0, 0 });
        this._numChaseEma.Minimum = new decimal(new int[] { 1, 0, 0, 0 });
        this._numChaseEma.Name = "_numChaseEma";
        this._numChaseEma.Size = new Size(387, 23);
        this._numChaseEma.TabIndex = 17;
        this._numChaseEma.Value = new decimal(new int[] { 20, 0, 0, 0 });
        // 
        // _lblMinDist
        // 
        this._lblMinDist.Anchor = AnchorStyles.Left;
        this._lblMinDist.AutoSize = true;
        this._lblMinDist.Location = new Point(564, 139);
        this._lblMinDist.Name = "_lblMinDist";
        this._lblMinDist.Size = new Size(134, 15);
        this._lblMinDist.TabIndex = 18;
        this._lblMinDist.Text = "Min dist from EMA (pts)";
        // 
        // _numMinDist
        // 
        this._numMinDist.Anchor = AnchorStyles.Left | AnchorStyles.Right;
        this._numMinDist.DecimalPlaces = 2;
        this._numMinDist.Increment = new decimal(new int[] { 25, 0, 0, 131072 });
        this._numMinDist.Location = new Point(734, 135);
        this._numMinDist.Maximum = new decimal(new int[] { 1000000, 0, 0, 0 });
        this._numMinDist.Name = "_numMinDist";
        this._numMinDist.Size = new Size(387, 23);
        this._numMinDist.TabIndex = 19;
        // 
        // _lblTpMode
        // 
        this._lblTpMode.Anchor = AnchorStyles.Left;
        this._lblTpMode.AutoSize = true;
        this._lblTpMode.Location = new Point(11, 168);
        this._lblTpMode.Name = "_lblTpMode";
        this._lblTpMode.Size = new Size(96, 15);
        this._lblTpMode.TabIndex = 20;
        this._lblTpMode.Text = "Take profit mode";
        // 
        // _cmbTpMode
        // 
        this._cmbTpMode.Anchor = AnchorStyles.Left | AnchorStyles.Right;
        this._cmbTpMode.DropDownStyle = ComboBoxStyle.DropDownList;
        this._cmbTpMode.Items.AddRange(new object[] { "Points", "Dollars" });
        this._cmbTpMode.Location = new Point(171, 164);
        this._cmbTpMode.Name = "_cmbTpMode";
        this._cmbTpMode.Size = new Size(387, 23);
        this._cmbTpMode.TabIndex = 21;
        // 
        // _lblTpPts
        // 
        this._lblTpPts.Anchor = AnchorStyles.Left;
        this._lblTpPts.AutoSize = true;
        this._lblTpPts.Location = new Point(564, 168);
        this._lblTpPts.Name = "_lblTpPts";
        this._lblTpPts.Size = new Size(56, 15);
        this._lblTpPts.TabIndex = 22;
        this._lblTpPts.Text = "TP points";
        // 
        // _numTpPts
        // 
        this._numTpPts.Anchor = AnchorStyles.Left | AnchorStyles.Right;
        this._numTpPts.DecimalPlaces = 2;
        this._numTpPts.Increment = new decimal(new int[] { 25, 0, 0, 131072 });
        this._numTpPts.Location = new Point(734, 164);
        this._numTpPts.Maximum = new decimal(new int[] { 1000000, 0, 0, 0 });
        this._numTpPts.Name = "_numTpPts";
        this._numTpPts.Size = new Size(387, 23);
        this._numTpPts.TabIndex = 23;
        this._numTpPts.Value = new decimal(new int[] { 10, 0, 0, 0 });
        // 
        // _lblTpUsd
        // 
        this._lblTpUsd.Anchor = AnchorStyles.Left;
        this._lblTpUsd.AutoSize = true;
        this._lblTpUsd.Location = new Point(11, 197);
        this._lblTpUsd.Name = "_lblTpUsd";
        this._lblTpUsd.Size = new Size(58, 15);
        this._lblTpUsd.TabIndex = 24;
        this._lblTpUsd.Text = "TP dollars";
        // 
        // _numTpUsd
        // 
        this._numTpUsd.Anchor = AnchorStyles.Left | AnchorStyles.Right;
        this._numTpUsd.DecimalPlaces = 2;
        this._numTpUsd.Location = new Point(171, 193);
        this._numTpUsd.Maximum = new decimal(new int[] { 100000000, 0, 0, 0 });
        this._numTpUsd.Name = "_numTpUsd";
        this._numTpUsd.Size = new Size(387, 23);
        this._numTpUsd.TabIndex = 25;
        this._numTpUsd.Value = new decimal(new int[] { 20, 0, 0, 0 });
        // 
        // _lblSlMode
        // 
        this._lblSlMode.Anchor = AnchorStyles.Left;
        this._lblSlMode.AutoSize = true;
        this._lblSlMode.Location = new Point(564, 197);
        this._lblSlMode.Name = "_lblSlMode";
        this._lblSlMode.Size = new Size(88, 15);
        this._lblSlMode.TabIndex = 26;
        this._lblSlMode.Text = "Stop loss mode";
        // 
        // _cmbSlMode
        // 
        this._cmbSlMode.Anchor = AnchorStyles.Left | AnchorStyles.Right;
        this._cmbSlMode.DropDownStyle = ComboBoxStyle.DropDownList;
        this._cmbSlMode.Items.AddRange(new object[] { "Points", "Dollars", "Ema" });
        this._cmbSlMode.Location = new Point(734, 193);
        this._cmbSlMode.Name = "_cmbSlMode";
        this._cmbSlMode.Size = new Size(387, 23);
        this._cmbSlMode.TabIndex = 27;
        // 
        // _lblSlPts
        // 
        this._lblSlPts.Anchor = AnchorStyles.Left;
        this._lblSlPts.AutoSize = true;
        this._lblSlPts.Location = new Point(11, 226);
        this._lblSlPts.Name = "_lblSlPts";
        this._lblSlPts.Size = new Size(55, 15);
        this._lblSlPts.TabIndex = 28;
        this._lblSlPts.Text = "SL points";
        // 
        // _numSlPts
        // 
        this._numSlPts.Anchor = AnchorStyles.Left | AnchorStyles.Right;
        this._numSlPts.DecimalPlaces = 2;
        this._numSlPts.Increment = new decimal(new int[] { 25, 0, 0, 131072 });
        this._numSlPts.Location = new Point(171, 222);
        this._numSlPts.Maximum = new decimal(new int[] { 1000000, 0, 0, 0 });
        this._numSlPts.Name = "_numSlPts";
        this._numSlPts.Size = new Size(387, 23);
        this._numSlPts.TabIndex = 29;
        this._numSlPts.Value = new decimal(new int[] { 10, 0, 0, 0 });
        // 
        // _lblSlUsd
        // 
        this._lblSlUsd.Anchor = AnchorStyles.Left;
        this._lblSlUsd.AutoSize = true;
        this._lblSlUsd.Location = new Point(564, 226);
        this._lblSlUsd.Name = "_lblSlUsd";
        this._lblSlUsd.Size = new Size(57, 15);
        this._lblSlUsd.TabIndex = 30;
        this._lblSlUsd.Text = "SL dollars";
        // 
        // _numSlUsd
        // 
        this._numSlUsd.Anchor = AnchorStyles.Left | AnchorStyles.Right;
        this._numSlUsd.DecimalPlaces = 2;
        this._numSlUsd.Location = new Point(734, 222);
        this._numSlUsd.Maximum = new decimal(new int[] { 100000000, 0, 0, 0 });
        this._numSlUsd.Name = "_numSlUsd";
        this._numSlUsd.Size = new Size(387, 23);
        this._numSlUsd.TabIndex = 31;
        this._numSlUsd.Value = new decimal(new int[] { 20, 0, 0, 0 });
        // 
        // _lblSlAnchorEma
        // 
        this._lblSlAnchorEma.Anchor = AnchorStyles.Left;
        this._lblSlAnchorEma.AutoSize = true;
        this._lblSlAnchorEma.Location = new Point(11, 255);
        this._lblSlAnchorEma.Name = "_lblSlAnchorEma";
        this._lblSlAnchorEma.Size = new Size(136, 15);
        this._lblSlAnchorEma.TabIndex = 32;
        this._lblSlAnchorEma.Text = "SL anchor EMA (Ema SL)";
        // 
        // _numSlAnchorEma
        // 
        this._numSlAnchorEma.Anchor = AnchorStyles.Left | AnchorStyles.Right;
        this._numSlAnchorEma.Location = new Point(171, 251);
        this._numSlAnchorEma.Maximum = new decimal(new int[] { 500, 0, 0, 0 });
        this._numSlAnchorEma.Minimum = new decimal(new int[] { 1, 0, 0, 0 });
        this._numSlAnchorEma.Name = "_numSlAnchorEma";
        this._numSlAnchorEma.Size = new Size(387, 23);
        this._numSlAnchorEma.TabIndex = 33;
        this._numSlAnchorEma.Value = new decimal(new int[] { 50, 0, 0, 0 });
        // 
        // _lblDollarsPerPoint
        // 
        this._lblDollarsPerPoint.Anchor = AnchorStyles.Left;
        this._lblDollarsPerPoint.AutoSize = true;
        this._lblDollarsPerPoint.Location = new Point(564, 255);
        this._lblDollarsPerPoint.Name = "_lblDollarsPerPoint";
        this._lblDollarsPerPoint.Size = new Size(107, 15);
        this._lblDollarsPerPoint.TabIndex = 34;
        this._lblDollarsPerPoint.Text = "$ / point / contract";
        // 
        // _numDollarsPerPoint
        // 
        this._numDollarsPerPoint.Anchor = AnchorStyles.Left | AnchorStyles.Right;
        this._numDollarsPerPoint.DecimalPlaces = 2;
        this._numDollarsPerPoint.Increment = new decimal(new int[] { 25, 0, 0, 131072 });
        this._numDollarsPerPoint.Location = new Point(734, 251);
        this._numDollarsPerPoint.Maximum = new decimal(new int[] { 1000000, 0, 0, 0 });
        this._numDollarsPerPoint.Minimum = new decimal(new int[] { 1, 0, 0, 131072 });
        this._numDollarsPerPoint.Name = "_numDollarsPerPoint";
        this._numDollarsPerPoint.Size = new Size(387, 23);
        this._numDollarsPerPoint.TabIndex = 35;
        this._numDollarsPerPoint.Value = new decimal(new int[] { 2, 0, 0, 0 });
        // 
        // _lblEvalSec
        // 
        this._lblEvalSec.Anchor = AnchorStyles.Left;
        this._lblEvalSec.AutoSize = true;
        this._lblEvalSec.Location = new Point(11, 284);
        this._lblEvalSec.Name = "_lblEvalSec";
        this._lblEvalSec.Size = new Size(98, 15);
        this._lblEvalSec.TabIndex = 36;
        this._lblEvalSec.Text = "Eval interval (sec)";
        // 
        // _numEvalSec
        // 
        this._numEvalSec.Anchor = AnchorStyles.Left | AnchorStyles.Right;
        this._numEvalSec.Location = new Point(171, 280);
        this._numEvalSec.Maximum = new decimal(new int[] { 86400, 0, 0, 0 });
        this._numEvalSec.Minimum = new decimal(new int[] { 1, 0, 0, 0 });
        this._numEvalSec.Name = "_numEvalSec";
        this._numEvalSec.Size = new Size(387, 23);
        this._numEvalSec.TabIndex = 37;
        this._numEvalSec.Value = new decimal(new int[] { 30, 0, 0, 0 });
        // 
        // _lblCdOrder
        // 
        this._lblCdOrder.Anchor = AnchorStyles.Left;
        this._lblCdOrder.AutoSize = true;
        this._lblCdOrder.Location = new Point(564, 284);
        this._lblCdOrder.Name = "_lblCdOrder";
        this._lblCdOrder.Size = new Size(148, 15);
        this._lblCdOrder.TabIndex = 38;
        this._lblCdOrder.Text = "Cooldown after order (sec)";
        // 
        // _numCdOrder
        // 
        this._numCdOrder.Anchor = AnchorStyles.Left | AnchorStyles.Right;
        this._numCdOrder.Location = new Point(734, 280);
        this._numCdOrder.Maximum = new decimal(new int[] { 86400, 0, 0, 0 });
        this._numCdOrder.Name = "_numCdOrder";
        this._numCdOrder.Size = new Size(387, 23);
        this._numCdOrder.TabIndex = 39;
        this._numCdOrder.Value = new decimal(new int[] { 30, 0, 0, 0 });
        // 
        // _lblCdExit
        // 
        this._lblCdExit.Anchor = AnchorStyles.Left;
        this._lblCdExit.AutoSize = true;
        this._lblCdExit.Location = new Point(11, 313);
        this._lblCdExit.Name = "_lblCdExit";
        this._lblCdExit.Size = new Size(139, 15);
        this._lblCdExit.TabIndex = 40;
        this._lblCdExit.Text = "Cooldown after exit (sec)";
        // 
        // _numCdExit
        // 
        this._numCdExit.Anchor = AnchorStyles.Left | AnchorStyles.Right;
        this._numCdExit.Location = new Point(171, 309);
        this._numCdExit.Maximum = new decimal(new int[] { 86400, 0, 0, 0 });
        this._numCdExit.Name = "_numCdExit";
        this._numCdExit.Size = new Size(387, 23);
        this._numCdExit.TabIndex = 41;
        this._numCdExit.Value = new decimal(new int[] { 30, 0, 0, 0 });
        // 
        // _lblFillMode
        // 
        this._lblFillMode.Anchor = AnchorStyles.Left;
        this._lblFillMode.AutoSize = true;
        this._lblFillMode.Location = new Point(564, 313);
        this._lblFillMode.Name = "_lblFillMode";
        this._lblFillMode.Size = new Size(56, 15);
        this._lblFillMode.TabIndex = 42;
        this._lblFillMode.Text = "Fill mode";
        // 
        // _cmbFill
        // 
        this._cmbFill.Anchor = AnchorStyles.Left | AnchorStyles.Right;
        this._cmbFill.DropDownStyle = ComboBoxStyle.DropDownList;
        this._cmbFill.Items.AddRange(new object[] { "NextBarOpen", "SignalBarClose" });
        this._cmbFill.Location = new Point(734, 309);
        this._cmbFill.Name = "_cmbFill";
        this._cmbFill.Size = new Size(387, 23);
        this._cmbFill.TabIndex = 43;
        // 
        // _lblSyntheticBars
        // 
        this._lblSyntheticBars.Anchor = AnchorStyles.Left;
        this._lblSyntheticBars.AutoSize = true;
        this._lblSyntheticBars.Location = new Point(11, 342);
        this._lblSyntheticBars.Name = "_lblSyntheticBars";
        this._lblSyntheticBars.Size = new Size(133, 15);
        this._lblSyntheticBars.TabIndex = 44;
        this._lblSyntheticBars.Text = "Synthetic bars (fallback)";
        // 
        // _numSyntheticCount
        // 
        this._numSyntheticCount.Anchor = AnchorStyles.Left | AnchorStyles.Right;
        this._numSyntheticCount.Location = new Point(171, 338);
        this._numSyntheticCount.Maximum = new decimal(new int[] { 1000000, 0, 0, 0 });
        this._numSyntheticCount.Minimum = new decimal(new int[] { 10, 0, 0, 0 });
        this._numSyntheticCount.Name = "_numSyntheticCount";
        this._numSyntheticCount.Size = new Size(387, 23);
        this._numSyntheticCount.TabIndex = 45;
        this._numSyntheticCount.Value = new decimal(new int[] { 600, 0, 0, 0 });
        // 
        // _lblSyntheticPrice
        // 
        this._lblSyntheticPrice.Anchor = AnchorStyles.Left;
        this._lblSyntheticPrice.AutoSize = true;
        this._lblSyntheticPrice.Location = new Point(564, 342);
        this._lblSyntheticPrice.Name = "_lblSyntheticPrice";
        this._lblSyntheticPrice.Size = new Size(111, 15);
        this._lblSyntheticPrice.TabIndex = 46;
        this._lblSyntheticPrice.Text = "Synthetic start price";
        // 
        // _numSyntheticPrice
        // 
        this._numSyntheticPrice.Anchor = AnchorStyles.Left | AnchorStyles.Right;
        this._numSyntheticPrice.DecimalPlaces = 2;
        this._numSyntheticPrice.Location = new Point(734, 338);
        this._numSyntheticPrice.Maximum = new decimal(new int[] { 1000000000, 0, 0, 0 });
        this._numSyntheticPrice.Minimum = new decimal(new int[] { 1, 0, 0, 0 });
        this._numSyntheticPrice.Name = "_numSyntheticPrice";
        this._numSyntheticPrice.Size = new Size(387, 23);
        this._numSyntheticPrice.TabIndex = 47;
        this._numSyntheticPrice.Value = new decimal(new int[] { 24000, 0, 0, 0 });
        // 
        // _chkAvgDown
        // 
        this._chkAvgDown.AutoSize = true;
        this._chkAvgDown.Location = new Point(11, 371);
        this._chkAvgDown.Name = "_chkAvgDown";
        this._chkAvgDown.Size = new Size(133, 19);
        this._chkAvgDown.TabIndex = 48;
        this._chkAvgDown.Text = "Allow average down";
        // 
        // MainForm
        // 
        this.AutoScaleDimensions = new SizeF(7F, 15F);
        this.AutoScaleMode = AutoScaleMode.Font;
        this.ClientSize = new Size(1138, 831);
        this.Controls.Add(this._tableRoot);
        this.MinimumSize = new Size(920, 720);
        this.Name = "MainForm";
        this.StartPosition = FormStartPosition.CenterScreen;
        this.Text = "EMA Strategy Simulator";
        this._tableRoot.ResumeLayout(false);
        this._tableRoot.PerformLayout();
        this._tableBottom.ResumeLayout(false);
        this._tableBottom.PerformLayout();
        this._panelRun.ResumeLayout(false);
        this._panelRun.PerformLayout();
        this._tableOptions.ResumeLayout(false);
        this._tableOptions.PerformLayout();
        ((System.ComponentModel.ISupportInitialize)this._numWarmup).EndInit();
        ((System.ComponentModel.ISupportInitialize)this._numMaxOpen).EndInit();
        ((System.ComponentModel.ISupportInitialize)this._numMaxClosed).EndInit();
        ((System.ComponentModel.ISupportInitialize)this._numOrderQty).EndInit();
        ((System.ComponentModel.ISupportInitialize)this._numChaseEma).EndInit();
        ((System.ComponentModel.ISupportInitialize)this._numMinDist).EndInit();
        ((System.ComponentModel.ISupportInitialize)this._numTpPts).EndInit();
        ((System.ComponentModel.ISupportInitialize)this._numTpUsd).EndInit();
        ((System.ComponentModel.ISupportInitialize)this._numSlPts).EndInit();
        ((System.ComponentModel.ISupportInitialize)this._numSlUsd).EndInit();
        ((System.ComponentModel.ISupportInitialize)this._numSlAnchorEma).EndInit();
        ((System.ComponentModel.ISupportInitialize)this._numDollarsPerPoint).EndInit();
        ((System.ComponentModel.ISupportInitialize)this._numEvalSec).EndInit();
        ((System.ComponentModel.ISupportInitialize)this._numCdOrder).EndInit();
        ((System.ComponentModel.ISupportInitialize)this._numCdExit).EndInit();
        ((System.ComponentModel.ISupportInitialize)this._numSyntheticCount).EndInit();
        ((System.ComponentModel.ISupportInitialize)this._numSyntheticPrice).EndInit();
        this.ResumeLayout(false);
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing && components != null)
            components.Dispose();
        base.Dispose(disposing);
    }
}
