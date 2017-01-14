using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using ircCore.Controls.ChildWindows.OutputDisplay.Helpers;
using ircCore.Settings.Theming;

namespace FusionIRC.Forms
{
    public partial class FrmTest : Form
    {
        public FrmTest()
        {
            InitializeComponent();
            //WHEN I DELETE THIS FORM THE FOLLOWING SETTINGS BELOW NEED TO BE IMPLEMENTED ON THE CHILD WINDOW
            outputWindow1.BackColor = ThemeManager.GetColor(ThemeColor.WindowBackColor);
            outputWindow1.ForeColor = ThemeManager.GetColor(ThemeColor.WindowForeColor);

            outputWindow1.Font = ThemeManager.CurrentTheme.ThemeFonts[ThemeWindow.Channel];
            var bd = ThemeManager.GetBackground(ThemeWindow.Channel);
            if (bd != null && File.Exists(bd.Path))
            {
                outputWindow1.BackgroundImage = (Bitmap)Image.FromFile(bd.Path);
                outputWindow1.BackgroundImageLayout = bd.LayoutStyle;
            }
            outputWindow1.MaximumLines = 100;
            Icon = Properties.Resources.channel;
            outputWindow1.LineSpacingStyle = LineSpacingStyle.Paragraph;
            var now = DateTime.Now;
            
            outputWindow1.AddLine(3, string.Format("[{0:00}:{1:00}] {2}", now.Hour, now.Minute, "* Topic is: '9,1/x\\5,8 Welcome to #DragonsRealm - 0,12 Next holiday, 2017 - Quick Survey: In Summer 0,13you sleep less? - 0,12Funny Note: Revolver thought today was Monday 9,1/x\\'"));
            //outputWindow1.AddLine(3, string.Format("[{0:00}:{1:00}] {2}", now.Hour, now.Minute, @"* Topic is: '9,1/x\5,8 Welcome to #DragonsRealm - 0,12 Quick Survey: Fav Cartoon/Anime? | NEXT MAJOR EVENT: Inaugration Day (US) 1/20 9,1/x\'"));

            var tmd = new IncomingMessageData
                          {
                              Message = ThemeMessage.ChannelSelfKickText,
                              TimeStamp = DateTime.Now,
                              Nick = "helloWorld",
                              Address = "~helloW@inet.com.au",
                              Text = "you can fuck off",
                              //Text = TimeFunctions.AscTime("1484264346",null),
                              Target = "#test",
                              KickedNick = "lucy"
                          };

            //next line is IMPORTANT
            var pmd = ThemeManager.ParseMessage(tmd);            
            outputWindow1.AddLine(pmd.DefaultColor, pmd.Message);

            outputWindow1.AddLine(1, true, string.Format("[{0:00}:{1:00}] {2}", now.Hour, now.Minute, "<4TestNick2> Now I'm going to put bolding in the m0,12iddle of a sentence and word"));
            outputWindow1.AddLine(1, true, string.Format("[{0:00}:{1:00}] {2}", now.Hour, now.Minute, "<5~MysTicaL_DraGoN> .wz adelaide au"));
            outputWindow1.AddLine(1, true, string.Format("[{0:00}:{1:00}] {2}", now.Hour, now.Minute, "<7+Eragon> MysTicaL_DraGoN, in Adelaide Airport, Australia at 6:00 PM ACDT it was 68 degrees F (20 C), with Partly Cloudy sky and a 22 MPH (35 KPH) wind from the SSW. The humidity was at 52%."));
            outputWindow1.AddLine(1, true, string.Format("[{0:00}:{1:00}] {2}", now.Hour, now.Minute, "<7+Eragon> The forecast for Adelaide Airport, Australia Sunday is  Partly Cloudy.		High:  71 F."));
            outputWindow1.AddLine(1, true, string.Format("[{0:00}:{1:00}] {2}", now.Hour, now.Minute, "<5~MysTicaL_DraGoN> damn it is the humidity doing it"));
            outputWindow1.AddLine(1, true, string.Format("[{0:00}:{1:00}] {2}", now.Hour, now.Minute, "<4TestNick> this is a test of our new window!"));
            outputWindow1.AddLine(1, true, string.Format("[{0:00}:{1:00}] {2}", now.Hour, now.Minute, "<4TestNick2> and this is a test of our new window's wrapping functunality. So, over long lines it should break nicely and then when draw calculate the vscroll bar to the number of actual lines drawn."));
            outputWindow1.AddLine(6, true, string.Format("[{0:00}:{1:00}] {2}", now.Hour, now.Minute, "* TestNick2 this is a test action!!"));
            outputWindow1.AddLine(1, true, string.Format("[{0:00}:{1:00}] {2}", now.Hour, now.Minute, "<4TestNick> lol"));
            outputWindow1.AddLine(6, true, string.Format("[{0:00}:{1:00}] {2}", now.Hour, now.Minute, "* TestNick2 URL test: https://toshiba.semicon-storage.com/info/docget.jsp?did=14237&prodName=TLP241A"));
            outputWindow1.AddLine(1, true, string.Format("[{0:00}:{1:00}] {2}", now.Hour, now.Minute, "<4TestNick2> 09,03(00,03p01)09,03(00,03e01)09,03(00,03n01)09,03(00,03i01)09,03(00,03s01)03,03a09,03(00,03i01)09,03(00,03n01)03,03a09,03(00,03v01)09,03(00,03a01)09,03(00,03g01)09,03(00,03i01)09,03(00,03n01)09,03(00,03a01)"));
            
           
            //outputWindow1.SaveBuffer("test.buf");
            //outputWindow1.LoadBuffer("test.buf");
            //outputWindow1.AddLine(1, true, string.Format("[{0:00}:{1:00}] {2}", now.Hour, now.Minute, "<DonaldTrump> 4,1W15e 4n15eed 4t15o 4b15uild 4a w15all!"));
            //outputWindow1.Clear();                     
        }

        protected override void OnLoad(EventArgs e)
        {
            FrmTest_Resize(this, new EventArgs());
            base.OnLoad(e);
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            var now = DateTime.Now;
            outputWindow1.AddLine(1, true, string.Format("[{0:00}:{1:00}] {2}", now.Hour, now.Minute, "<4TestNick> lol"));
        }

        private void outputWindow1_OnUrlDoubleClicked(string url)
        {
            MessageBox.Show(@"URL Double-Clicked " + url);
        }

        private void outputWindow1_OnWindowDoubleClicked()
        {
            MessageBox.Show(@"Window was double-clicked");
        }

        private void outputWindow1_OnWindowRightClicked()
        {
            MessageBox.Show(@"Window was right-clicked");
        }

        private void FrmTest_Resize(object sender, EventArgs e)
        {
            outputWindow1.SetBounds(0, 0, ClientRectangle.Width,
                                    ClientRectangle.Height - inputBox1.ClientRectangle.Height - 1);
            inputBox1.SetBounds(0, ClientRectangle.Height - inputBox1.ClientRectangle.Height, ClientRectangle.Width,
                                inputBox1.ClientRectangle.Height);
        }
    }
}
