/*
Copyright (c) 2011 Ben Barron

Permission is hereby granted, free of charge, to any person obtaining a copy 
of this software and associated documentation files (the "Software"), to deal 
in the Software without restriction, including without limitation the rights 
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell 
copies of the Software, and to permit persons to whom the Software is furnished 
to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all 
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, 
INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A 
PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT 
HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION 
OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE 
SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
*/

using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;
using System.Xml;

using WindowsAPI;

namespace PS3BluMote
{
    public partial class SettingsForm : Form
    {
        private readonly String SETTINGS_DIRECTORY = System.Environment.GetFolderPath(System.Environment.SpecialFolder.LocalApplicationData) + "\\PS3BluMote\\";
        private readonly String SETTINGS_FILE = System.Environment.GetFolderPath(System.Environment.SpecialFolder.LocalApplicationData) + "\\PS3BluMote\\settings.ini";
        private const String SETTINGS_VERSION = "1.1";

        private List<SendInputAPI.Keyboard.KeyCode>[] buttonMappings = new List<SendInputAPI.Keyboard.KeyCode>[51];
        private PS3Remote remote = null;
        private SendInputAPI.Keyboard keyboard = null;

        public SettingsForm()
        {
            for (int i = 0; i < buttonMappings.Length; i++)
            {
                buttonMappings[i] = new List<SendInputAPI.Keyboard.KeyCode>();
            }

            InitializeComponent();

            ListViewItem lvItem;
            foreach (PS3Remote.Button button in Enum.GetValues(typeof(PS3Remote.Button)))
            {
                lvItem = new ListViewItem(button.ToString());
                lvItem.SubItems.Add("");
                lvButtons.Items.Add(lvItem);
            }

            foreach (SendInputAPI.Keyboard.KeyCode key in Enum.GetValues(typeof(SendInputAPI.Keyboard.KeyCode)))
            {
                lvKeys.Items.Add(new ListViewItem(key.ToString()));
            }

            if (!loadSettings())
            {
                saveSettings();
            }

            try
            {
                remote = new PS3Remote(int.Parse(txtVendorId.Text.Remove(0, 2), System.Globalization.NumberStyles.HexNumber), int.Parse(txtProductId.Text.Remove(0, 2), System.Globalization.NumberStyles.HexNumber), cbHibernation.Checked);
                remote.BatteryLifeChanged += new EventHandler<EventArgs>(remote_BatteryLifeChanged);
                remote.ButtonDown += new EventHandler<PS3Remote.ButtonData>(remote_ButtonDown);
                remote.ButtonReleased += new EventHandler<PS3Remote.ButtonData>(remote_ButtonReleased);
                remote.Connected += new EventHandler<EventArgs>(remote_Connected);
                remote.Disconnected += new EventHandler<EventArgs>(remote_Disconnected);
                remote.connect();
            }
            catch
            {
                MessageBox.Show("An error occured whilst attempting to load the remote.", "PS3BluMote: Remote error!", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            keyboard = new SendInputAPI.Keyboard(cbSms.Checked);
        }

        private void cbHibernation_CheckedChanged(object sender, EventArgs e)
        {
            remote.hibernationEnabled = cbHibernation.Checked;
        }

        private void cbSms_CheckedChanged(object sender, EventArgs e)
        {
            keyboard.isSmsEnabled = cbSms.Checked;
        }

        private bool loadSettings()
        {
            String errorMessage;

            if (File.Exists(SETTINGS_FILE))
            {
                XmlDocument rssDoc = new XmlDocument();

                try
                {
                    rssDoc.Load(SETTINGS_FILE);

                    XmlNode rssNode = rssDoc.SelectSingleNode("PS3BluMote");

                    if (rssNode.Attributes["version"].InnerText == SETTINGS_VERSION)
                    {
                        cbSms.Checked = (rssNode.SelectSingleNode("settings/smsinput").InnerText.ToLower() == "true") ? true : false;
                        cbHibernation.Checked = (rssNode.SelectSingleNode("settings/hibernation").InnerText.ToLower() == "true") ? true : false;
                        txtVendorId.Text = rssNode.SelectSingleNode("settings/vendorid").InnerText;
                        txtProductId.Text = rssNode.SelectSingleNode("settings/productid").InnerText;

                        foreach (XmlNode buttonNode in rssNode.SelectNodes("mappings/button"))
                        {
                            int index = (int)Enum.Parse(typeof(PS3Remote.Button), buttonNode.Attributes["name"].InnerText, true);
                            List<SendInputAPI.Keyboard.KeyCode> mappedKeys = buttonMappings[index];

                            if (buttonNode.InnerText.Length > 0)
                            {
                                foreach (string keyCode in buttonNode.InnerText.Split(','))
                                {
                                    mappedKeys.Add((SendInputAPI.Keyboard.KeyCode)Enum.Parse(typeof(SendInputAPI.Keyboard.KeyCode), keyCode, true));
                                }

                                lvButtons.Items[index].SubItems[1].Text = buttonNode.InnerText.Replace(",", " + ");
                            }
                        }

                        return true;
                    }

                    errorMessage = "Incorrect settings file version.";
                }
                catch
                {
                    errorMessage = "An error occured whilst attempting to load settings.";
                }
            }
            else
            {
                errorMessage = "Unable to locate the settings file.";
            }

            MessageBox.Show(errorMessage, "PS3BluMote: Settings load error!", MessageBoxButtons.OK, MessageBoxIcon.Error);
            
            return false;
        }

        private void lvButtons_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (lvButtons.SelectedItems.Count == 0) return;

            lvButtons.Tag = true;

            int index = (int)Enum.Parse(typeof(PS3Remote.Button), lvButtons.SelectedItems[0].SubItems[0].Text, true);
            List<SendInputAPI.Keyboard.KeyCode> mappedKeys = buttonMappings[index];

            foreach (ListViewItem lvItem in lvKeys.Items)
            {
                if (mappedKeys.Contains((SendInputAPI.Keyboard.KeyCode)Enum.Parse(typeof(SendInputAPI.Keyboard.KeyCode), lvItem.Text, true)))
                {
                    lvItem.Checked = true;
                }
                else
                {
                    lvItem.Checked = false;
                }
            }

            lvButtons.Tag = false;
        }

        private void lvKeys_ItemCheck(object sender, ItemCheckEventArgs e)
        {
            if ((bool)lvButtons.Tag) return;

            int index = (int)Enum.Parse(typeof(PS3Remote.Button), lvButtons.SelectedItems[0].SubItems[0].Text, true);
            List<SendInputAPI.Keyboard.KeyCode> mappedKeys = buttonMappings[index];
            SendInputAPI.Keyboard.KeyCode code = (SendInputAPI.Keyboard.KeyCode)Enum.Parse(typeof(SendInputAPI.Keyboard.KeyCode), lvKeys.Items[e.Index].Text, true);

            if (e.NewValue == CheckState.Checked && !mappedKeys.Contains(code))
            {
                mappedKeys.Add(code);
            }
            else
            {
                mappedKeys.Remove(code);
            }

            String text = "";
            foreach (SendInputAPI.Keyboard.KeyCode key in mappedKeys)
            {
                text += key.ToString() + " + ";
            }

            lvButtons.SelectedItems[0].SubItems[1].Text = (mappedKeys.Count > 0) ? text.Substring(0, text.Length - 3) : "";
        }

        private void menuNotifyIcon_ItemClick(object sender, EventArgs e)
        {
            if (sender.Equals(mitemSettings))
            {
                this.Show();
            }
            else if (sender.Equals(mitemExit))
            {
                Application.Exit();
            }
        }

        private void notifyIcon_DoubleClick(object sender, EventArgs e)
        {
            this.Show();
        }

        private void remote_BatteryLifeChanged(object sender, EventArgs e)
        {
            notifyIcon.Text = "PS3BluMote: Connected + (Battery: " + remote.getBatteryLife.ToString() + "%).";
        }

        private void remote_ButtonDown(object sender, PS3Remote.ButtonData e)
        {
            keyboard.sendKeysDown(buttonMappings[(int)e.button]);
        }

        private void remote_ButtonReleased(object sender, PS3Remote.ButtonData e)
        {
            keyboard.releaseLastKeys();
        }

        private void remote_Connected(object sender, EventArgs e)
        {
            notifyIcon.Text = "PS3BluMote: Connected + (Battery: " + remote.getBatteryLife.ToString() + "%).";
            notifyIcon.Icon = Properties.Resources.Icon_Connected;
        }

        private void remote_Disconnected(object sender, EventArgs e)
        {
            notifyIcon.Text = "PS3BluMote: Disconnected.";
            notifyIcon.Icon = Properties.Resources.Icon_Disconnected;
        }

        private bool saveSettings()
        {
            string text = "<PS3BluMote version=\"" + SETTINGS_VERSION + "\">\r\n";
            text += "\t<settings>\r\n";
            text += "\t\t<vendorid>" + txtVendorId.Text.ToLower() + "</vendorid>\r\n";
            text += "\t\t<productid>" + txtProductId.Text.ToLower() + "</productid>\r\n";
            text += "\t\t<smsinput>" + cbSms.Checked.ToString().ToLower() + "</smsinput>\r\n";
            text += "\t\t<hibernation>" + cbHibernation.Checked.ToString().ToLower() + "</hibernation>\r\n";
            text += "\t</settings>\r\n";
            text += "\t<mappings>\r\n";

            for (int i = 0; i < buttonMappings.Length; i++)
            {
                text += "\t\t<button name=\"" + ((PS3Remote.Button)i).ToString() + "\">";

                foreach (SendInputAPI.Keyboard.KeyCode key in buttonMappings[i])
                {
                    text += key.ToString() + ",";
                }

                text = text.TrimEnd(',') + "</button>\r\n";
            }

            text += "\t</mappings>\r\n";
            text += "</PS3BluMote>";

            try
            {
                if (!Directory.Exists(SETTINGS_DIRECTORY))
                {
                    Directory.CreateDirectory(SETTINGS_DIRECTORY);
                }

                TextWriter tw = new StreamWriter(SETTINGS_FILE, false);
                tw.WriteLine(text);
                tw.Close();

                return true;
            }
            catch
            {
                MessageBox.Show("An error occured whilst attempting to save settings.", "PS3BluMote: Saving settings error!", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            return false;
        }

        private void SettingsForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            saveSettings();

            if (e.CloseReason != CloseReason.UserClosing) return;
            
            e.Cancel = true;

            this.Hide();
        }

        private void SettingsForm_Shown(object sender, EventArgs e)
        {
            lvButtons.Items[0].Selected = true;
        }

        private void txtProductId_Validating(object sender, System.ComponentModel.CancelEventArgs e)
        {
            try
            {
                int i = int.Parse(txtProductId.Text.Remove(0, 2), System.Globalization.NumberStyles.HexNumber);
            }
            catch
            {
                e.Cancel = true;   
            }
        }

        private void txtVendorId_Validating(object sender, System.ComponentModel.CancelEventArgs e)
        {
            try
            {
                int i = int.Parse(txtVendorId.Text.Remove(0, 2), System.Globalization.NumberStyles.HexNumber);
            }
            catch
            {
                e.Cancel = true;
            }
        }
    }
}
