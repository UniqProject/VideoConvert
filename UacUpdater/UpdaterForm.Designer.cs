namespace UacUpdater
{
    partial class UpdaterForm
    {
        /// <summary>
        /// Erforderliche Designervariable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Verwendete Ressourcen bereinigen.
        /// </summary>
        /// <param name="disposing">True, wenn verwaltete Ressourcen gelöscht werden sollen; andernfalls False.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Vom Windows Form-Designer generierter Code

        /// <summary>
        /// Erforderliche Methode für die Designerunterstützung.
        /// Der Inhalt der Methode darf nicht mit dem Code-Editor geändert werden.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.LogGroup = new System.Windows.Forms.GroupBox();
            this.Log = new System.Windows.Forms.TextBox();
            this.Status = new System.Windows.Forms.Label();
            this.PackageProgress = new System.Windows.Forms.ProgressBar();
            this.UpdateTimer = new System.Windows.Forms.Timer(this.components);
            this.OverallProgress = new System.Windows.Forms.ProgressBar();
            this.ProgressLabel = new System.Windows.Forms.Label();
            this.ExitTimer = new System.Windows.Forms.Timer(this.components);
            this.LogGroup.SuspendLayout();
            this.SuspendLayout();
            // 
            // LogGroup
            // 
            this.LogGroup.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.LogGroup.Controls.Add(this.Log);
            this.LogGroup.Location = new System.Drawing.Point(12, 12);
            this.LogGroup.Name = "LogGroup";
            this.LogGroup.Size = new System.Drawing.Size(677, 162);
            this.LogGroup.TabIndex = 1;
            this.LogGroup.TabStop = false;
            this.LogGroup.Text = "Log";
            // 
            // Log
            // 
            this.Log.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.Log.Location = new System.Drawing.Point(6, 19);
            this.Log.Multiline = true;
            this.Log.Name = "Log";
            this.Log.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.Log.Size = new System.Drawing.Size(665, 137);
            this.Log.TabIndex = 1;
            // 
            // Status
            // 
            this.Status.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.Status.AutoSize = true;
            this.Status.Location = new System.Drawing.Point(15, 177);
            this.Status.Name = "Status";
            this.Status.Size = new System.Drawing.Size(37, 13);
            this.Status.TabIndex = 2;
            this.Status.Text = "Status";
            // 
            // PackageProgress
            // 
            this.PackageProgress.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.PackageProgress.Location = new System.Drawing.Point(18, 193);
            this.PackageProgress.Name = "PackageProgress";
            this.PackageProgress.Size = new System.Drawing.Size(665, 23);
            this.PackageProgress.TabIndex = 3;
            // 
            // UpdateTimer
            // 
            this.UpdateTimer.Interval = 500;
            this.UpdateTimer.Tick += new System.EventHandler(this.UpdateTimerTick);
            // 
            // OverallProgress
            // 
            this.OverallProgress.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.OverallProgress.Location = new System.Drawing.Point(16, 235);
            this.OverallProgress.Name = "OverallProgress";
            this.OverallProgress.Size = new System.Drawing.Size(665, 23);
            this.OverallProgress.Step = 1;
            this.OverallProgress.TabIndex = 4;
            // 
            // ProgressLabel
            // 
            this.ProgressLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.ProgressLabel.AutoSize = true;
            this.ProgressLabel.Location = new System.Drawing.Point(15, 219);
            this.ProgressLabel.Name = "ProgressLabel";
            this.ProgressLabel.Size = new System.Drawing.Size(113, 13);
            this.ProgressLabel.TabIndex = 5;
            this.ProgressLabel.Text = "Total Update Progress";
            // 
            // ExitTimer
            // 
            this.ExitTimer.Interval = 500;
            // 
            // UpdaterForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(701, 271);
            this.Controls.Add(this.ProgressLabel);
            this.Controls.Add(this.OverallProgress);
            this.Controls.Add(this.PackageProgress);
            this.Controls.Add(this.Status);
            this.Controls.Add(this.LogGroup);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Name = "UpdaterForm";
            this.ShowIcon = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Update";
            this.LogGroup.ResumeLayout(false);
            this.LogGroup.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.GroupBox LogGroup;
        private System.Windows.Forms.TextBox Log;
        private System.Windows.Forms.Label Status;
        private System.Windows.Forms.ProgressBar PackageProgress;
        private System.Windows.Forms.Timer UpdateTimer;
        private System.Windows.Forms.ProgressBar OverallProgress;
        private System.Windows.Forms.Label ProgressLabel;
        private System.Windows.Forms.Timer ExitTimer;
    }
}

