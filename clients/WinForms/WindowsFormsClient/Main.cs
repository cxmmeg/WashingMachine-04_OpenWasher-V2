﻿using OpenWasherHardwareLibrary;
using OpenWasherHardwareLibrary.Enums;
using System;
using System.Collections.Generic;
using System.Windows.Forms;
using WindowsFormsClient.Entities;
using WindowsFormsClient.Managers;
using WindowsFormsClient.Properties;

namespace WindowsFormsClient
{
    public partial class Main : Form
    {
        HardwareLibrary _hardwareLibrary;
        ConfigManager _config = new ConfigManager();

        private bool _isProgramWorking;
        List<Log> _logs = new List<Log>();

        LogFrm logFrm;

        public Main()
        {
            InitializeComponent();
            _hardwareLibrary = new HardwareLibrary(MessageHandler, ErrorHandler, EventHandler);
        }

        private async void Main_Load(object sender, EventArgs e)
        {
            await ConnectAsync();
        }

        int i = 0;
        private async void timerPoll_Tick(object sender, EventArgs e)
        {
            try
            {
                var status = await GetStatusAsync();
                lblTemp.Text = $"{status.temperature}°C";

                if (status.program == Programs.Nothing)
                {
                    _isProgramWorking = false;
                    btnStart.Image = Resources.play;
                    btnStart.Enabled = true;

                    lblStage.Visible = false;
                    lblFinishTime.Visible = false;

                    progressBar.Enabled = false;
                }
                else
                {
                    var localizedProgram = ResourceString.GetString($"Program_{(int)status.program}", EnumManager.GetEnumDescription(status.program));
                    var localizedStage = ResourceString.GetString($"Stage_{(int)status.stage}", EnumManager.GetEnumDescription(status.stage));

                    lblStage.Text = localizedProgram + ": " + localizedStage;
                    lblStage.Visible = true;

                    trayIcon.Text = localizedStage;

                    _isProgramWorking = true;
                    btnStart.Image = Resources.stop;
                    btnStart.Enabled = true;

                    lblFinishTime.Text = $"Finish time: {DateTime.Now.AddMilliseconds(status.timefull - status.timepassed)}";
                    lblFinishTime.Visible = true;

                    progressBar.Maximum = status.timefull;
                    progressBar.Value = status.timepassed;
                    progressBar.Enabled = true;
                }

                i = 0;
            }
            catch (TimeoutException)
            {
                if (i++ == 3)
                {
                    Disconnect();
                    MessageBox.Show("No answer");
                }
            }
        }

        private async void btnStart_Click(object sender, EventArgs e)
        {
            if (_isProgramWorking)
                await _hardwareLibrary.StopProgramAsync();
            else
            {
                StartFrm startForm = new StartFrm(_hardwareLibrary);
                startForm.Show();
            }
        }

        private void settingsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SettingsFrm settingsForm = new SettingsFrm(_config);
            settingsForm.Show();
        }

        private async void ffToolStripMenuItem_Click(object sender, EventArgs e)
        {
            timerPoll.Stop();
            await _hardwareLibrary.GoToBootloaderAsync();
            Disconnect();
            //FirmwareFrm firmwareForm = new FirmwareFrm();
            //firmwareForm.Show();
        }

        private void Main_FormClosed(object sender, FormClosedEventArgs e)
        {
            _hardwareLibrary.Disconnect();
        }

        private void AboutToolStripMenuItem_Click(object sender, EventArgs e)
        {

        }

        private void Main_Resize(object sender, EventArgs e)
        {
            if (WindowState == FormWindowState.Minimized)
            {
                Hide();
                trayIcon.Visible = true;
            }
        }

        private void TrayIcon_DoubleClick(object sender, EventArgs e)
        {
            Show();
            WindowState = FormWindowState.Normal;
            trayIcon.Visible = false;
        }

        private void LogToolStripMenuItem_Click(object sender, EventArgs e)
        {
            logFrm = new LogFrm(_logs);
            logFrm.Show();
        }
    }
}
