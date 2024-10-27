namespace EQTool.UI
{
    partial class SpawnTimerDialogForms
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.timerStartGroupBox = new System.Windows.Forms.GroupBox();
            this.youHaveSlainTextBox = new System.Windows.Forms.TextBox();
            this.factionMessageTextBox = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.youHaveSlainRadioButton = new System.Windows.Forms.RadioButton();
            this.factionMessageRadioButton = new System.Windows.Forms.RadioButton();
            this.expMessageRadioButton = new System.Windows.Forms.RadioButton();
            this.enableCheckBox = new System.Windows.Forms.CheckBox();
            this.timerEndGroupBox = new System.Windows.Forms.GroupBox();
            this.timerExpiredTTSCheckBox = new System.Windows.Forms.CheckBox();
            this.endingTTSTextBox = new System.Windows.Forms.TextBox();
            this.timerExpiredTextCheckBox = new System.Windows.Forms.CheckBox();
            this.endingTextTextBox = new System.Windows.Forms.TextBox();
            this.warningTTSCheckBox = new System.Windows.Forms.CheckBox();
            this.warningTTSTextBox = new System.Windows.Forms.TextBox();
            this.warningTextCheckbox = new System.Windows.Forms.CheckBox();
            this.label3 = new System.Windows.Forms.Label();
            this.warningTimeRemaining = new System.Windows.Forms.TextBox();
            this.warningTextTextBox = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.timerDurationGroupBox = new System.Windows.Forms.GroupBox();
            this.label5 = new System.Windows.Forms.Label();
            this.customDurationTextBox = new System.Windows.Forms.TextBox();
            this.radioButtonCustom = new System.Windows.Forms.RadioButton();
            this.radioButton2800 = new System.Windows.Forms.RadioButton();
            this.radioButton2200 = new System.Windows.Forms.RadioButton();
            this.label4 = new System.Windows.Forms.Label();
            this.radioButton0640 = new System.Windows.Forms.RadioButton();
            this.radioButton1430 = new System.Windows.Forms.RadioButton();
            this.radioButton0600 = new System.Windows.Forms.RadioButton();
            this.okButton = new System.Windows.Forms.Button();
            this.cancelButton = new System.Windows.Forms.Button();
            this.counterResetGroupBox = new System.Windows.Forms.GroupBox();
            this.label6 = new System.Windows.Forms.Label();
            this.counterResetTextBox = new System.Windows.Forms.TextBox();
            this.label7 = new System.Windows.Forms.Label();
            this.commentsGroupBox = new System.Windows.Forms.GroupBox();
            this.commentsRichTextBox = new System.Windows.Forms.RichTextBox();
            this.label8 = new System.Windows.Forms.Label();
            this.label9 = new System.Windows.Forms.Label();
            this.label10 = new System.Windows.Forms.Label();
            this.timerStartGroupBox.SuspendLayout();
            this.timerEndGroupBox.SuspendLayout();
            this.timerDurationGroupBox.SuspendLayout();
            this.counterResetGroupBox.SuspendLayout();
            this.commentsGroupBox.SuspendLayout();
            this.SuspendLayout();
            // 
            // timerStartGroupBox
            // 
            this.timerStartGroupBox.Controls.Add(this.youHaveSlainTextBox);
            this.timerStartGroupBox.Controls.Add(this.factionMessageTextBox);
            this.timerStartGroupBox.Controls.Add(this.label1);
            this.timerStartGroupBox.Controls.Add(this.youHaveSlainRadioButton);
            this.timerStartGroupBox.Controls.Add(this.factionMessageRadioButton);
            this.timerStartGroupBox.Controls.Add(this.expMessageRadioButton);
            this.timerStartGroupBox.Location = new System.Drawing.Point(7, 39);
            this.timerStartGroupBox.Margin = new System.Windows.Forms.Padding(8, 7, 8, 7);
            this.timerStartGroupBox.Name = "timerStartGroupBox";
            this.timerStartGroupBox.Padding = new System.Windows.Forms.Padding(8, 7, 8, 7);
            this.timerStartGroupBox.Size = new System.Drawing.Size(531, 128);
            this.timerStartGroupBox.TabIndex = 0;
            this.timerStartGroupBox.TabStop = false;
            this.timerStartGroupBox.Enter += new System.EventHandler(this.groupBox1_Enter);
            // 
            // youHaveSlainTextBox
            // 
            this.youHaveSlainTextBox.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.youHaveSlainTextBox.Location = new System.Drawing.Point(218, 69);
            this.youHaveSlainTextBox.Name = "youHaveSlainTextBox";
            this.youHaveSlainTextBox.Size = new System.Drawing.Size(293, 21);
            this.youHaveSlainTextBox.TabIndex = 5;
            this.youHaveSlainTextBox.Text = "(a pirate|a cyclops|Boog Mudtoe|an ancient cyclops)";
            // 
            // factionMessageTextBox
            // 
            this.factionMessageTextBox.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.factionMessageTextBox.Location = new System.Drawing.Point(219, 94);
            this.factionMessageTextBox.Name = "factionMessageTextBox";
            this.factionMessageTextBox.Size = new System.Drawing.Size(293, 21);
            this.factionMessageTextBox.TabIndex = 4;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.Location = new System.Drawing.Point(6, 10);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(100, 24);
            this.label1.TabIndex = 3;
            this.label1.Text = "Timer Start";
            this.label1.Click += new System.EventHandler(this.label1_Click);
            // 
            // youHaveSlainRadioButton
            // 
            this.youHaveSlainRadioButton.AutoSize = true;
            this.youHaveSlainRadioButton.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.youHaveSlainRadioButton.Location = new System.Drawing.Point(47, 71);
            this.youHaveSlainRadioButton.Margin = new System.Windows.Forms.Padding(8, 7, 8, 7);
            this.youHaveSlainRadioButton.Name = "youHaveSlainRadioButton";
            this.youHaveSlainRadioButton.Size = new System.Drawing.Size(113, 19);
            this.youHaveSlainRadioButton.TabIndex = 2;
            this.youHaveSlainRadioButton.Text = "You have slain...";
            this.youHaveSlainRadioButton.UseVisualStyleBackColor = true;
            this.youHaveSlainRadioButton.CheckedChanged += new System.EventHandler(this.radioButton3_CheckedChanged);
            // 
            // factionMessageRadioButton
            // 
            this.factionMessageRadioButton.AutoSize = true;
            this.factionMessageRadioButton.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.factionMessageRadioButton.Location = new System.Drawing.Point(47, 96);
            this.factionMessageRadioButton.Margin = new System.Windows.Forms.Padding(8, 7, 8, 7);
            this.factionMessageRadioButton.Name = "factionMessageRadioButton";
            this.factionMessageRadioButton.Size = new System.Drawing.Size(173, 19);
            this.factionMessageRadioButton.TabIndex = 1;
            this.factionMessageRadioButton.Text = "Your faction standing with...";
            this.factionMessageRadioButton.UseVisualStyleBackColor = true;
            // 
            // expMessageRadioButton
            // 
            this.expMessageRadioButton.AutoSize = true;
            this.expMessageRadioButton.Checked = true;
            this.expMessageRadioButton.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.expMessageRadioButton.Location = new System.Drawing.Point(48, 46);
            this.expMessageRadioButton.Margin = new System.Windows.Forms.Padding(8, 7, 8, 7);
            this.expMessageRadioButton.Name = "expMessageRadioButton";
            this.expMessageRadioButton.Size = new System.Drawing.Size(100, 19);
            this.expMessageRadioButton.TabIndex = 0;
            this.expMessageRadioButton.TabStop = true;
            this.expMessageRadioButton.Text = "Exp Message";
            this.expMessageRadioButton.UseVisualStyleBackColor = true;
            // 
            // enableCheckBox
            // 
            this.enableCheckBox.AutoSize = true;
            this.enableCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.enableCheckBox.Font = new System.Drawing.Font("Microsoft Sans Serif", 14.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.enableCheckBox.Location = new System.Drawing.Point(143, 16);
            this.enableCheckBox.Margin = new System.Windows.Forms.Padding(8, 7, 8, 7);
            this.enableCheckBox.Name = "enableCheckBox";
            this.enableCheckBox.Size = new System.Drawing.Size(232, 28);
            this.enableCheckBox.TabIndex = 1;
            this.enableCheckBox.Text = "Enable Spawn Timers";
            this.enableCheckBox.UseVisualStyleBackColor = true;
            this.enableCheckBox.CheckedChanged += new System.EventHandler(this.checkBox1_CheckedChanged);
            // 
            // timerEndGroupBox
            // 
            this.timerEndGroupBox.Controls.Add(this.label10);
            this.timerEndGroupBox.Controls.Add(this.label9);
            this.timerEndGroupBox.Controls.Add(this.timerExpiredTTSCheckBox);
            this.timerEndGroupBox.Controls.Add(this.endingTTSTextBox);
            this.timerEndGroupBox.Controls.Add(this.timerExpiredTextCheckBox);
            this.timerEndGroupBox.Controls.Add(this.endingTextTextBox);
            this.timerEndGroupBox.Controls.Add(this.warningTTSCheckBox);
            this.timerEndGroupBox.Controls.Add(this.warningTTSTextBox);
            this.timerEndGroupBox.Controls.Add(this.warningTextCheckbox);
            this.timerEndGroupBox.Controls.Add(this.label3);
            this.timerEndGroupBox.Controls.Add(this.warningTimeRemaining);
            this.timerEndGroupBox.Controls.Add(this.warningTextTextBox);
            this.timerEndGroupBox.Controls.Add(this.label2);
            this.timerEndGroupBox.Location = new System.Drawing.Point(7, 168);
            this.timerEndGroupBox.Margin = new System.Windows.Forms.Padding(8, 7, 8, 7);
            this.timerEndGroupBox.Name = "timerEndGroupBox";
            this.timerEndGroupBox.Padding = new System.Windows.Forms.Padding(8, 7, 8, 7);
            this.timerEndGroupBox.Size = new System.Drawing.Size(531, 215);
            this.timerEndGroupBox.TabIndex = 6;
            this.timerEndGroupBox.TabStop = false;
            // 
            // timerExpiredTTSCheckBox
            // 
            this.timerExpiredTTSCheckBox.AutoSize = true;
            this.timerExpiredTTSCheckBox.Checked = true;
            this.timerExpiredTTSCheckBox.CheckState = System.Windows.Forms.CheckState.Checked;
            this.timerExpiredTTSCheckBox.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.timerExpiredTTSCheckBox.Location = new System.Drawing.Point(80, 185);
            this.timerExpiredTTSCheckBox.Name = "timerExpiredTTSCheckBox";
            this.timerExpiredTTSCheckBox.Size = new System.Drawing.Size(107, 19);
            this.timerExpiredTTSCheckBox.TabIndex = 16;
            this.timerExpiredTTSCheckBox.Text = "Text to Speech";
            this.timerExpiredTTSCheckBox.UseVisualStyleBackColor = true;
            this.timerExpiredTTSCheckBox.CheckedChanged += new System.EventHandler(this.checkBox1_CheckedChanged_1);
            // 
            // endingTTSTextBox
            // 
            this.endingTTSTextBox.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.endingTTSTextBox.Location = new System.Drawing.Point(219, 183);
            this.endingTTSTextBox.Name = "endingTTSTextBox";
            this.endingTTSTextBox.Size = new System.Drawing.Size(293, 21);
            this.endingTTSTextBox.TabIndex = 15;
            this.endingTTSTextBox.Text = "Pop";
            this.endingTTSTextBox.TextChanged += new System.EventHandler(this.textBox1_TextChanged);
            // 
            // timerExpiredTextCheckBox
            // 
            this.timerExpiredTextCheckBox.AutoSize = true;
            this.timerExpiredTextCheckBox.Checked = true;
            this.timerExpiredTextCheckBox.CheckState = System.Windows.Forms.CheckState.Checked;
            this.timerExpiredTextCheckBox.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.timerExpiredTextCheckBox.Location = new System.Drawing.Point(80, 158);
            this.timerExpiredTextCheckBox.Name = "timerExpiredTextCheckBox";
            this.timerExpiredTextCheckBox.Size = new System.Drawing.Size(52, 19);
            this.timerExpiredTextCheckBox.TabIndex = 14;
            this.timerExpiredTextCheckBox.Text = "Text:";
            this.timerExpiredTextCheckBox.UseVisualStyleBackColor = true;
            this.timerExpiredTextCheckBox.CheckedChanged += new System.EventHandler(this.checkBox2_CheckedChanged);
            // 
            // endingTextTextBox
            // 
            this.endingTextTextBox.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.endingTextTextBox.Location = new System.Drawing.Point(218, 156);
            this.endingTextTextBox.Name = "endingTextTextBox";
            this.endingTextTextBox.Size = new System.Drawing.Size(293, 21);
            this.endingTextTextBox.TabIndex = 12;
            this.endingTextTextBox.Text = "Pop";
            this.endingTextTextBox.TextChanged += new System.EventHandler(this.textBox2_TextChanged);
            // 
            // warningTTSCheckBox
            // 
            this.warningTTSCheckBox.AutoSize = true;
            this.warningTTSCheckBox.Checked = true;
            this.warningTTSCheckBox.CheckState = System.Windows.Forms.CheckState.Checked;
            this.warningTTSCheckBox.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.warningTTSCheckBox.Location = new System.Drawing.Point(81, 97);
            this.warningTTSCheckBox.Name = "warningTTSCheckBox";
            this.warningTTSCheckBox.Size = new System.Drawing.Size(107, 19);
            this.warningTTSCheckBox.TabIndex = 11;
            this.warningTTSCheckBox.Text = "Text to Speech";
            this.warningTTSCheckBox.UseVisualStyleBackColor = true;
            // 
            // warningTTSTextBox
            // 
            this.warningTTSTextBox.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.warningTTSTextBox.Location = new System.Drawing.Point(219, 94);
            this.warningTTSTextBox.Name = "warningTTSTextBox";
            this.warningTTSTextBox.Size = new System.Drawing.Size(293, 21);
            this.warningTTSTextBox.TabIndex = 10;
            this.warningTTSTextBox.Text = "30 sec warning";
            // 
            // warningTextCheckbox
            // 
            this.warningTextCheckbox.AutoSize = true;
            this.warningTextCheckbox.Checked = true;
            this.warningTextCheckbox.CheckState = System.Windows.Forms.CheckState.Checked;
            this.warningTextCheckbox.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.warningTextCheckbox.Location = new System.Drawing.Point(81, 70);
            this.warningTextCheckbox.Name = "warningTextCheckbox";
            this.warningTextCheckbox.Size = new System.Drawing.Size(52, 19);
            this.warningTextCheckbox.TabIndex = 9;
            this.warningTextCheckbox.Text = "Text:";
            this.warningTextCheckbox.UseVisualStyleBackColor = true;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label3.Location = new System.Drawing.Point(306, 47);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(61, 15);
            this.label3.TabIndex = 8;
            this.label3.Text = "hh:mm:ss";
            // 
            // warningTimeRemaining
            // 
            this.warningTimeRemaining.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.warningTimeRemaining.Location = new System.Drawing.Point(219, 44);
            this.warningTimeRemaining.Name = "warningTimeRemaining";
            this.warningTimeRemaining.Size = new System.Drawing.Size(81, 21);
            this.warningTimeRemaining.TabIndex = 7;
            this.warningTimeRemaining.Text = "30";
            this.warningTimeRemaining.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // warningTextTextBox
            // 
            this.warningTextTextBox.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.warningTextTextBox.Location = new System.Drawing.Point(219, 69);
            this.warningTextTextBox.Name = "warningTextTextBox";
            this.warningTextTextBox.Size = new System.Drawing.Size(293, 21);
            this.warningTextTextBox.TabIndex = 4;
            this.warningTextTextBox.Text = "30 sec warning";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("Microsoft Sans Serif", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label2.Location = new System.Drawing.Point(6, 10);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(99, 24);
            this.label2.TabIndex = 3;
            this.label2.Text = "Timer End";
            // 
            // timerDurationGroupBox
            // 
            this.timerDurationGroupBox.Controls.Add(this.label5);
            this.timerDurationGroupBox.Controls.Add(this.customDurationTextBox);
            this.timerDurationGroupBox.Controls.Add(this.radioButtonCustom);
            this.timerDurationGroupBox.Controls.Add(this.radioButton2800);
            this.timerDurationGroupBox.Controls.Add(this.radioButton2200);
            this.timerDurationGroupBox.Controls.Add(this.label4);
            this.timerDurationGroupBox.Controls.Add(this.radioButton0640);
            this.timerDurationGroupBox.Controls.Add(this.radioButton1430);
            this.timerDurationGroupBox.Controls.Add(this.radioButton0600);
            this.timerDurationGroupBox.Location = new System.Drawing.Point(554, 39);
            this.timerDurationGroupBox.Margin = new System.Windows.Forms.Padding(8, 7, 8, 7);
            this.timerDurationGroupBox.Name = "timerDurationGroupBox";
            this.timerDurationGroupBox.Padding = new System.Windows.Forms.Padding(8, 7, 8, 7);
            this.timerDurationGroupBox.Size = new System.Drawing.Size(322, 194);
            this.timerDurationGroupBox.TabIndex = 6;
            this.timerDurationGroupBox.TabStop = false;
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label5.Location = new System.Drawing.Point(207, 168);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(61, 15);
            this.label5.TabIndex = 10;
            this.label5.Text = "hh:mm:ss";
            // 
            // customDurationTextBox
            // 
            this.customDurationTextBox.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.customDurationTextBox.Location = new System.Drawing.Point(120, 162);
            this.customDurationTextBox.Name = "customDurationTextBox";
            this.customDurationTextBox.Size = new System.Drawing.Size(81, 21);
            this.customDurationTextBox.TabIndex = 9;
            this.customDurationTextBox.Text = "30:00";
            this.customDurationTextBox.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // radioButtonCustom
            // 
            this.radioButtonCustom.AutoSize = true;
            this.radioButtonCustom.Checked = true;
            this.radioButtonCustom.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.radioButtonCustom.Location = new System.Drawing.Point(48, 161);
            this.radioButtonCustom.Margin = new System.Windows.Forms.Padding(8, 7, 8, 7);
            this.radioButtonCustom.Name = "radioButtonCustom";
            this.radioButtonCustom.Size = new System.Drawing.Size(67, 19);
            this.radioButtonCustom.TabIndex = 6;
            this.radioButtonCustom.TabStop = true;
            this.radioButtonCustom.Text = "Custom";
            this.radioButtonCustom.UseVisualStyleBackColor = true;
            // 
            // radioButton2800
            // 
            this.radioButton2800.AutoSize = true;
            this.radioButton2800.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.radioButton2800.Location = new System.Drawing.Point(48, 138);
            this.radioButton2800.Margin = new System.Windows.Forms.Padding(8, 7, 8, 7);
            this.radioButton2800.Name = "radioButton2800";
            this.radioButton2800.Size = new System.Drawing.Size(56, 19);
            this.radioButton2800.TabIndex = 5;
            this.radioButton2800.Text = "28:00";
            this.radioButton2800.UseVisualStyleBackColor = true;
            // 
            // radioButton2200
            // 
            this.radioButton2200.AutoSize = true;
            this.radioButton2200.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.radioButton2200.Location = new System.Drawing.Point(48, 115);
            this.radioButton2200.Margin = new System.Windows.Forms.Padding(8, 7, 8, 7);
            this.radioButton2200.Name = "radioButton2200";
            this.radioButton2200.Size = new System.Drawing.Size(56, 19);
            this.radioButton2200.TabIndex = 4;
            this.radioButton2200.Text = "22:00";
            this.radioButton2200.UseVisualStyleBackColor = true;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Font = new System.Drawing.Font("Microsoft Sans Serif", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label4.Location = new System.Drawing.Point(6, 10);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(134, 24);
            this.label4.TabIndex = 3;
            this.label4.Text = "Timer Duration";
            // 
            // radioButton0640
            // 
            this.radioButton0640.AutoSize = true;
            this.radioButton0640.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.radioButton0640.Location = new System.Drawing.Point(48, 69);
            this.radioButton0640.Margin = new System.Windows.Forms.Padding(8, 7, 8, 7);
            this.radioButton0640.Name = "radioButton0640";
            this.radioButton0640.Size = new System.Drawing.Size(56, 19);
            this.radioButton0640.TabIndex = 2;
            this.radioButton0640.Text = "06:40";
            this.radioButton0640.UseVisualStyleBackColor = true;
            // 
            // radioButton1430
            // 
            this.radioButton1430.AutoSize = true;
            this.radioButton1430.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.radioButton1430.Location = new System.Drawing.Point(48, 92);
            this.radioButton1430.Margin = new System.Windows.Forms.Padding(8, 7, 8, 7);
            this.radioButton1430.Name = "radioButton1430";
            this.radioButton1430.Size = new System.Drawing.Size(56, 19);
            this.radioButton1430.TabIndex = 1;
            this.radioButton1430.Text = "14:30";
            this.radioButton1430.UseVisualStyleBackColor = true;
            // 
            // radioButton0600
            // 
            this.radioButton0600.AutoSize = true;
            this.radioButton0600.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.radioButton0600.Location = new System.Drawing.Point(48, 46);
            this.radioButton0600.Margin = new System.Windows.Forms.Padding(8, 7, 8, 7);
            this.radioButton0600.Name = "radioButton0600";
            this.radioButton0600.Size = new System.Drawing.Size(56, 19);
            this.radioButton0600.TabIndex = 0;
            this.radioButton0600.Text = "06:00";
            this.radioButton0600.UseVisualStyleBackColor = true;
            // 
            // okButton
            // 
            this.okButton.Font = new System.Drawing.Font("Microsoft Sans Serif", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.okButton.Location = new System.Drawing.Point(287, 546);
            this.okButton.Name = "okButton";
            this.okButton.Size = new System.Drawing.Size(133, 36);
            this.okButton.TabIndex = 7;
            this.okButton.Text = "OK";
            this.okButton.UseVisualStyleBackColor = true;
            // 
            // cancelButton
            // 
            this.cancelButton.Font = new System.Drawing.Font("Microsoft Sans Serif", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.cancelButton.Location = new System.Drawing.Point(485, 546);
            this.cancelButton.Name = "cancelButton";
            this.cancelButton.Size = new System.Drawing.Size(133, 36);
            this.cancelButton.TabIndex = 8;
            this.cancelButton.Text = "Cancel";
            this.cancelButton.UseVisualStyleBackColor = true;
            // 
            // counterResetGroupBox
            // 
            this.counterResetGroupBox.Controls.Add(this.label6);
            this.counterResetGroupBox.Controls.Add(this.counterResetTextBox);
            this.counterResetGroupBox.Controls.Add(this.label7);
            this.counterResetGroupBox.Location = new System.Drawing.Point(7, 397);
            this.counterResetGroupBox.Margin = new System.Windows.Forms.Padding(8, 7, 8, 7);
            this.counterResetGroupBox.Name = "counterResetGroupBox";
            this.counterResetGroupBox.Padding = new System.Windows.Forms.Padding(8, 7, 8, 7);
            this.counterResetGroupBox.Size = new System.Drawing.Size(531, 125);
            this.counterResetGroupBox.TabIndex = 11;
            this.counterResetGroupBox.TabStop = false;
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label6.Location = new System.Drawing.Point(149, 46);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(61, 15);
            this.label6.TabIndex = 10;
            this.label6.Text = "hh:mm:ss";
            // 
            // counterResetTextBox
            // 
            this.counterResetTextBox.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.counterResetTextBox.Location = new System.Drawing.Point(62, 43);
            this.counterResetTextBox.Name = "counterResetTextBox";
            this.counterResetTextBox.Size = new System.Drawing.Size(81, 21);
            this.counterResetTextBox.TabIndex = 9;
            this.counterResetTextBox.Text = "1:00:00";
            this.counterResetTextBox.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Font = new System.Drawing.Font("Microsoft Sans Serif", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label7.Location = new System.Drawing.Point(6, 10);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(178, 24);
            this.label7.TabIndex = 3;
            this.label7.Text = "Counter Reset Time";
            this.label7.Click += new System.EventHandler(this.label7_Click);
            // 
            // commentsGroupBox
            // 
            this.commentsGroupBox.Controls.Add(this.commentsRichTextBox);
            this.commentsGroupBox.Controls.Add(this.label8);
            this.commentsGroupBox.Location = new System.Drawing.Point(554, 238);
            this.commentsGroupBox.Margin = new System.Windows.Forms.Padding(8, 7, 8, 7);
            this.commentsGroupBox.Name = "commentsGroupBox";
            this.commentsGroupBox.Padding = new System.Windows.Forms.Padding(8, 7, 8, 7);
            this.commentsGroupBox.Size = new System.Drawing.Size(322, 251);
            this.commentsGroupBox.TabIndex = 12;
            this.commentsGroupBox.TabStop = false;
            // 
            // commentsRichTextBox
            // 
            this.commentsRichTextBox.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.commentsRichTextBox.Location = new System.Drawing.Point(8, 44);
            this.commentsRichTextBox.Name = "commentsRichTextBox";
            this.commentsRichTextBox.Size = new System.Drawing.Size(314, 206);
            this.commentsRichTextBox.TabIndex = 5;
            this.commentsRichTextBox.Text = "OOT sisters/specs = 6:40\nOasis specs = 14:30\nWK guards = 6:00\nOOT AC mobs = (a pi" +
    "rate|a cyclops|Boog Mudtoe|an ancient cyclops)\n(Drelzna|a necromancer)\n";
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Font = new System.Drawing.Font("Microsoft Sans Serif", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label8.Location = new System.Drawing.Point(6, 10);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(165, 24);
            this.label8.TabIndex = 4;
            this.label8.Text = "Comments / Notes";
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label9.Location = new System.Drawing.Point(45, 44);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(148, 15);
            this.label9.TabIndex = 17;
            this.label9.Text = "Remaining Time Warning";
            this.label9.Click += new System.EventHandler(this.label9_Click);
            // 
            // label10
            // 
            this.label10.AutoSize = true;
            this.label10.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label10.Location = new System.Drawing.Point(45, 130);
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size(148, 15);
            this.label10.TabIndex = 18;
            this.label10.Text = "Timer Expired Notification";
            // 
            // SpawnTimerDialogForms
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(16F, 31F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(909, 634);
            this.Controls.Add(this.commentsGroupBox);
            this.Controls.Add(this.counterResetGroupBox);
            this.Controls.Add(this.cancelButton);
            this.Controls.Add(this.okButton);
            this.Controls.Add(this.timerDurationGroupBox);
            this.Controls.Add(this.timerEndGroupBox);
            this.Controls.Add(this.enableCheckBox);
            this.Controls.Add(this.timerStartGroupBox);
            this.Font = new System.Drawing.Font("Microsoft Sans Serif", 20F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Margin = new System.Windows.Forms.Padding(8, 7, 8, 7);
            this.Name = "SpawnTimerDialog";
            this.Text = "Spawn Timer Dialog";
            this.timerStartGroupBox.ResumeLayout(false);
            this.timerStartGroupBox.PerformLayout();
            this.timerEndGroupBox.ResumeLayout(false);
            this.timerEndGroupBox.PerformLayout();
            this.timerDurationGroupBox.ResumeLayout(false);
            this.timerDurationGroupBox.PerformLayout();
            this.counterResetGroupBox.ResumeLayout(false);
            this.counterResetGroupBox.PerformLayout();
            this.commentsGroupBox.ResumeLayout(false);
            this.commentsGroupBox.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.GroupBox timerStartGroupBox;
        private System.Windows.Forms.RadioButton youHaveSlainRadioButton;
        private System.Windows.Forms.RadioButton factionMessageRadioButton;
        private System.Windows.Forms.RadioButton expMessageRadioButton;
        private System.Windows.Forms.CheckBox enableCheckBox;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox youHaveSlainTextBox;
        private System.Windows.Forms.TextBox factionMessageTextBox;
        private System.Windows.Forms.GroupBox timerEndGroupBox;
        private System.Windows.Forms.TextBox warningTextTextBox;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox warningTimeRemaining;
        private System.Windows.Forms.CheckBox warningTextCheckbox;
        private System.Windows.Forms.CheckBox warningTTSCheckBox;
        private System.Windows.Forms.TextBox warningTTSTextBox;
        private System.Windows.Forms.CheckBox timerExpiredTTSCheckBox;
        private System.Windows.Forms.TextBox endingTTSTextBox;
        private System.Windows.Forms.CheckBox timerExpiredTextCheckBox;
        private System.Windows.Forms.TextBox endingTextTextBox;
        private System.Windows.Forms.GroupBox timerDurationGroupBox;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.RadioButton radioButton0640;
        private System.Windows.Forms.RadioButton radioButton1430;
        private System.Windows.Forms.RadioButton radioButton0600;
        private System.Windows.Forms.RadioButton radioButton2800;
        private System.Windows.Forms.RadioButton radioButton2200;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.TextBox customDurationTextBox;
        private System.Windows.Forms.RadioButton radioButtonCustom;
        private System.Windows.Forms.Button okButton;
        private System.Windows.Forms.Button cancelButton;
        private System.Windows.Forms.GroupBox counterResetGroupBox;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.TextBox counterResetTextBox;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.GroupBox commentsGroupBox;
        private System.Windows.Forms.RichTextBox commentsRichTextBox;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.Label label10;
        private System.Windows.Forms.Label label9;
    }
}