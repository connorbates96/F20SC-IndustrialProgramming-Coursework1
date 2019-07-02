using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WebBrowser
{
    public partial class Form1 : Form
    {

        private BrowserData browserData = new BrowserData();

        //boolean values to indicate when a button has been pressed
        private bool searchClick = false;
        private bool homeClick = false;
        private bool backClick = false;
        private bool forwardClick = false;
        private bool refreshClick = false;

        /// <summary>
        /// Contains information about threads that are being run. Contains the tab the thread is working on, the Thread itself, and the
        /// corresponding BrowserCommunication object.
        /// Has a constructor in the form of ThreadInfo(Thread thread, TabPage tab, BrowserCommunication bc)
        /// </summary>
        public struct ThreadInfo
        {
            public TabPage tab;
            public Thread thread;
            public BrowserCommunication bc;
            public bool addToHistory;

            public ThreadInfo(Thread thread, TabPage tab, BrowserCommunication bc, bool addToHistory)
            {
                this.thread = thread;
                this.tab = tab;
                this.bc = bc;
                this.addToHistory = addToHistory;
            }
        }





        public Form1()
        {
            InitializeComponent();
            this.Text = "Web Browser";
            this.tabControl1.TabPages.Add(createTab());
            Task.Run(() => startBrowserOperations());
        }

        /// <summary>
        /// Before the form closes, the data is serialised and stored in MyFile.bin
        /// </summary>
        private void Form1_FormClosing(Object sender, FormClosingEventArgs e)
        {
            IFormatter formatter = new BinaryFormatter();
            Stream stream = new FileStream("MyFile.bin", FileMode.Create, FileAccess.Write, FileShare.None);
            formatter.Serialize(stream, browserData);
            stream.Close();
        }

        /// <summary>
        /// When the form loads, myFile.bin is deserialised and the data is stored in browserData
        /// </summary>
        private void Form1_Load(Object sender, EventArgs e)
        {
            IFormatter formatter = new BinaryFormatter();
            Stream stream = null;
            try
            {
                stream = new FileStream("MyFile.bin", FileMode.Open, FileAccess.Read, FileShare.Read);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception raised: {0}", ex.Message);
            }
            
            if (stream != null)
            {
                browserData = (BrowserData)formatter.Deserialize(stream);
                stream.Close();
            }
        }





        /// <summary>
        /// Creates a list that contains ThreadInfo objects then starts a while(true) loop that constantly checks if a button has been pressed.
        /// If a button has been pressed, the correct code is executed to carry out the function. At the end of the while loop, all the running
        /// threads are checked to see if they have finished. If they are, the tab is updated accordingly and the thread is removed from the list.
        /// </summary>
        private async void startBrowserOperations()
        {
            List<ThreadInfo> threads = new List<ThreadInfo>();
            Thread.Sleep(100);
            homeClick = true;
            while (true)
            {

                if (searchClick == true) //Gets URL from searchBar, creates a BrowserCommunication thread, adds threadInfo to list and starts execution.
                {
                    string currentURL = null;
                    tabControl1.Invoke(new Action(() => currentURL = tabControl1.SelectedTab.Controls.Find("searchBar", true).First().Text));
                    BrowserCommunication searchComm = new BrowserCommunication(currentURL);
                    Thread searchThread = new Thread(searchComm.searchURL);
                    TabPage currentTab = null;
                    tabControl1.Invoke(new Action(() => currentTab = tabControl1.SelectedTab));
                    ThreadInfo searchTI = new ThreadInfo(searchThread, currentTab, searchComm, true);
                    threads.Add(searchTI);
                    searchThread.Start();
                    searchClick = false;

                }
                if (homeClick == true) //Creates BrowserCommunication thread with home url, adds threadInfo to list and starts execution.
                {
                    BrowserCommunication homeComm = new BrowserCommunication(browserData.home);
                    Thread homeThread = new Thread(homeComm.searchURL);
                    TabPage currentTab = null;
                    tabControl1.Invoke(new Action(() => currentTab = tabControl1.SelectedTab));
                    ThreadInfo homeTI = new ThreadInfo(homeThread, currentTab, homeComm, true);
                    threads.Add(homeTI);
                    homeThread.Start();
                    homeClick = false;
                }
                if (refreshClick == true) //Creates BrowserCommunication with current URL and refreshes the page.
                {
                    string url = "";
                    tabControl1.Invoke(new Action(() => url = tabControl1.SelectedTab.Controls.Find("searchBar", true).First().Text));
                    BrowserCommunication refreshComm = new BrowserCommunication(url);
                    Thread refreshThread = new Thread(refreshComm.searchURL);
                    TabPage currentTab = null;
                    tabControl1.Invoke(new Action(() => currentTab = tabControl1.SelectedTab));
                    ThreadInfo refreshTI = new ThreadInfo(refreshThread, currentTab, refreshComm, false);
                    threads.Add(refreshTI);
                    refreshThread.Start();
                    refreshClick = false;
                }
                if (forwardClick == true) //Uses the traverseHistory function within BrowserData to find the next url in the history list
                                          //and creates a new thread for the request.
                {
                    if (browserData.history.Count == (0 | 1)) break;

                    string currentURL = null;
                    tabControl1.Invoke(new Action(() => currentURL = tabControl1.SelectedTab.Controls.Find("searchBar", true).First().Text));
                    string nextURL = browserData.traverseHistory(currentURL, true);
                    if (nextURL == "") break;

                    BrowserCommunication forwardComm = new BrowserCommunication(nextURL);
                    Thread forwardThread = new Thread(forwardComm.searchURL);
                    TabPage currentTab = null;
                    tabControl1.Invoke(new Action(() => currentTab = tabControl1.SelectedTab));
                    ThreadInfo forwardTI = new ThreadInfo(forwardThread, currentTab, forwardComm, false);
                    threads.Add(forwardTI);
                    forwardThread.Start();
                    forwardClick = false;
                    
                }
                
                if (backClick == true) //Uses the traverseHistory function within BrowserData to find the previous url in the history list
                                       //and creates a new thread for the request.
                {
                    if (browserData.history.Count == (0 | 1)) break;

                    string currentURL = null;
                    tabControl1.Invoke(new Action(() => currentURL = tabControl1.SelectedTab.Controls.Find("searchBar", true).First().Text));
                    string nextURL = browserData.traverseHistory(currentURL, false);
                    if (nextURL == "") break;

                    BrowserCommunication backComm = new BrowserCommunication(nextURL);
                    Thread backThread = new Thread(backComm.searchURL);
                    TabPage currentTab = null;
                    tabControl1.Invoke(new Action(() => currentTab = tabControl1.SelectedTab));
                    ThreadInfo backTI = new ThreadInfo(backThread, currentTab, backComm, false);
                    threads.Add(backTI);
                    backThread.Start();
                    backClick = false;
                }

                for (int i = 0; i < threads.Count; i++) //Checks if all threads in the threads list are finished and if they are, the page is
                                                        //updated and the thread removed.
                {
                    ThreadInfo curr = threads[i];
                    if (curr.thread.IsAlive == false)
                    {
                        updatePage(curr.bc, curr.tab, curr.addToHistory);
                        threads.Remove(curr);
                    }
                }

            }
        }





        //Button Click Events----------------------------------------------------------------------------------------------------
        //-----------------------------------------------------------------------------------------------------------------------

        /// <summary>
        /// Sets searchClick to true for the browser opertaions to perform the search.
        /// </summary>
        private void searchButton_Click(object sender, EventArgs e)
        {
            searchClick = true;
        }

        /// <summary>
        /// Sets homeClick to true for the browser to navigate back to the home page.
        /// </summary>
        private void homeButton_Click(object sender, EventArgs e)
        {
            homeClick = true;
        }

        /// <summary>
        /// Sets refreshClick to true for the browser to refresh the current page.
        /// </summary>
        private void refreshButton_Click(object sender, EventArgs e)
        {
            refreshClick = true;
        }

        /// <summary>
        /// Sets backClick to true for the browser to go back a page.
        /// </summary>
        private void backButton_Click(object sender, EventArgs e)
        {
            backClick = true;
        }

        /// <summary>
        /// Sets forwardClick to true for the browser to go forward a page.
        /// </summary>
        private void forwardButton_Click(object sender, EventArgs e)
        {
            forwardClick = true;
        }





        /// <summary>
        /// Creates a new tab that contains all of the required controls and adds it to the tabcontrol.
        /// </summary>
        private void newTabButton_click(object sender, EventArgs e)
        {
            tabControl1.TabPages.Add(createTab());
        }

        /// <summary>
        /// Closes the current tab that is selected unless it is the final tab.
        /// </summary>
        private void closeTabButton_Click(object sender, EventArgs e)
        {
            if (tabControl1.TabCount > 1)
            {
                tabControl1.TabPages.Remove(tabControl1.SelectedTab);
            }
            else System.Windows.Forms.MessageBox.Show("You can't close the last tab!");
        }





        /// <summary>
        /// Brings the history panel into view and makes it visible. Updates the history panel so it contains the history.
        /// </summary>
        private void historyButton_Click(object sender, EventArgs e)
        {
            Panel historyPanel = (Panel)this.tabControl1.SelectedTab.Controls.Find("histPanel", true).First();
            updateHistoryTable(historyPanel);
            historyPanel.Visible = true;
            historyPanel.BringToFront();
        }

        /// <summary>
        /// Closes the history panel by setting its visibility to false.
        /// </summary>
        private void hpClose_Click(object sender, EventArgs e)
        {
            Panel historyPanel = (Panel)this.tabControl1.SelectedTab.Controls.Find("histPanel", true).First();
            historyPanel.Visible = false;
            historyPanel.SendToBack();
        }

        /// <summary>
        /// Brings the favourite panel into view and make it visible. Updates the favourite panel so it contains the favourited pages
        /// </summary>
        private void favouriteButton_Click(object sender, EventArgs e)
        {
            Panel favouritePanel = (Panel)this.tabControl1.SelectedTab.Controls.Find("favPanel", true).First();
            updateFavouriteTable(favouritePanel);
            favouritePanel.Visible = true;
            favouritePanel.BringToFront();
        }

        /// <summary>
        /// Closes the favourite panel by setting its visibility to false.
        /// </summary>
        private void fpClose_Click(object sender, EventArgs e)
        {
            Panel favouritePanel = (Panel)this.tabControl1.SelectedTab.Controls.Find("favPanel", true).First();
            favouritePanel.Visible = false;
            favouritePanel.SendToBack();
        }

        /// <summary>
        /// Adds the current page to the favourite list
        /// </summary>
        private void fpAdd_Click(object sender, EventArgs e)
        {
            TabPage currentTab = (TabPage)this.tabControl1.SelectedTab;
            string url = currentTab.Controls.Find("searchBar", true).First().Text;
            string title = currentTab.Text;
            browserData.addFT(title, url);
            Panel favouritePanel = (Panel)this.tabControl1.SelectedTab.Controls.Find("favPanel", true).First();
            updateFavouriteTable(favouritePanel);
        }

        /// <summary>
        /// Removes the page corresponding to the button from the favourites list.
        /// </summary>
        private void fpRemove_Click(object sender, EventArgs e)
        {
            object tag = (sender as Button).Tag;
            browserData.removeFT(tag.ToString());
            Panel favouritePanel = (Panel)this.tabControl1.SelectedTab.Controls.Find("favPanel", true).First();
            updateFavouriteTable(favouritePanel);
        }

        private void fpEdit_Click(object sender, EventArgs e)
        {
            string newTitle = Microsoft.VisualBasic.Interaction.InputBox("Please input a new title.","Edit Favourite Title","");
            browserData.editFT((sender as Button).Tag as string, newTitle);
            Panel favouritePanel = (Panel)this.tabControl1.SelectedTab.Controls.Find("favPanel", true).First();
            updateFavouriteTable(favouritePanel);
        }

        /// <summary>
        /// Takes the url from the history or favourite page and searches it.
        /// </summary>
        private void favhistLink_Click(object sender, EventArgs e)
        {
            TextBox seturl = (TextBox)this.tabControl1.SelectedTab.Controls.Find("searchBar", true).First();
            object url = (sender as Button).Tag;
            seturl.Text = (string)url;
            searchClick = true;
        }





        /// <summary>
        /// Brings the settings panel into view and makes it visible
        /// </summary>
        private void settingsButton_Click(object sender, EventArgs e)
        {
            Panel settingsPanel = (Panel)this.tabControl1.SelectedTab.Controls.Find("settingsPanel", true).First();
            settingsPanel.Visible = true;
            settingsPanel.BringToFront();
        }

        /// <summary>
        /// Closes the settings panel by setting its visibility to false
        /// </summary>
        private void spClose_Click(object sender, EventArgs e)
        {
            Panel settingsPanel = (Panel)this.tabControl1.SelectedTab.Controls.Find("settingsPanel", true).First();
            settingsPanel.Visible = false;
            settingsPanel.SendToBack();
        }

        /// <summary>
        /// Sets the homepage to the url that is inputted.
        /// </summary>
        private void setHome_Click(object sender, EventArgs e)
        {
            TextBox homeURL = (TextBox)this.tabControl1.SelectedTab.Controls.Find("setHomeText", true).First();
            browserData.home = homeURL.Text;
            System.Windows.Forms.MessageBox.Show("Home page has been updated.");
        }

        /// <summary>
        /// Clears the browsing history.
        /// </summary>
        private void clearHistory_Click(object sender, EventArgs e)
        {
            browserData.history.Clear();
            System.Windows.Forms.MessageBox.Show("Browsing history cleared.");
        }

        //-------------------------------------------------------------------------------------------------------------
        //-------------------------------------------------------------------------------------------------------------





        /// <summary>
        /// Updates the tab page once the http request has been completed and a response has arrived.
        /// </summary>
        /// <param name="bc">BrowserCommunication object that contains information about the request/response</param>
        /// <param name="tab">Tab to be updated</param>
        private void updatePage(BrowserCommunication bc, TabPage tab, bool addToHistory)
        {
            //Checks if there has been a problem getting the response. If an error has occurred, the status code is
            //printed in the tab title and nothing else is updated.
            if (bc.error == true) tab.Invoke(new Action(() => tab.Text = bc.title + " " + bc.statusCodeString));

            else
            {
                //Update Page Information
                tab.Invoke(new Action(() => tab.Text = bc.title + " " + bc.statusCodeString));
                tab.Invoke(new Action(() => tab.Controls.Find("mainTextBox", true).First().Text = bc.htmlCode));
                tab.Invoke(new Action(() => tab.Controls.Find("searchBar", true).First().Text = bc.url));

                //Update History List
                if (addToHistory == true) browserData.addHT(bc.url, DateTime.Now);
            }

        }

        /// <summary>
        /// Updates the history table by adding in each history entry to the table.
        /// </summary>
        /// <param name="currPanel">The panel to be updated</param>
        private void updateHistoryTable(Panel currPanel)
        {
            TableLayoutPanel table = (TableLayoutPanel)currPanel.Controls.Find("hpTable", true).First();
            table.Controls.Clear();

            //Adds titles to the history table
            Label urlLabel = new Label();
            urlLabel.Text = "URL";
            urlLabel.Width = 370;
            urlLabel.Font = new Font(urlLabel.Font.FontFamily, 15);
            urlLabel.BackColor = Color.DarkSlateBlue;
            urlLabel.ForeColor = Color.White;
            Label timestampLabel = new Label();
            timestampLabel.Text = "Timestamp";
            timestampLabel.Width = 110;
            timestampLabel.Font = new Font(timestampLabel.Font.FontFamily, 15);
            timestampLabel.BackColor = Color.DarkSlateBlue;
            timestampLabel.ForeColor = Color.White;
            table.Controls.Add(urlLabel, 0, 0);
            table.Controls.Add(timestampLabel, 1, 0);


            //For each entry in browserData's history list, add the url and timestamp to the table
            for (int i = 0; i < browserData.history.Count(); i++)
            {
                Button link = new Button();
                link.Text = browserData.history.ElementAt(i).url;
                link.Width = 320;
                link.FlatStyle = FlatStyle.Flat;
                link.ForeColor = Color.White;
                link.Tag = browserData.history.ElementAt(i).url;
                table.Controls.Add(link, 0, i+1);
                link.Click += new EventHandler(favhistLink_Click);
                Label timestamp = new Label();
                timestamp.Text = browserData.history.ElementAt(i).dateTime.ToString();
                timestamp.ForeColor = Color.White;
                timestamp.Width = 120;
                timestamp.Height = 20;
                table.Controls.Add(timestamp, 1, i+1);
            }
        }


        /// <summary>
        /// Updates the favourite table by adding in each entry to the table.
        /// </summary>
        /// <param name="currPanel">The panel to be updated</param>
        private void updateFavouriteTable(Panel currPanel)
        {
            TableLayoutPanel table = (TableLayoutPanel)currPanel.Controls.Find("fpTable", true).First();
            table.Controls.Clear();

            //Adds titles to the favourite table
            Label titleLabel = new Label();
            titleLabel.Text = "Title";
            titleLabel.Width = 370;
            titleLabel.Font = new Font(titleLabel.Font.FontFamily, 15);
            titleLabel.BackColor = Color.DarkSlateBlue;
            titleLabel.ForeColor = Color.White;
            table.Controls.Add(titleLabel, 0, 0);


            //For each entry in browserData's favourite list, add the url and timestamp to the table
            for (int i = 0; i < browserData.favourites.Count(); i++)
            {
                Button link = new Button();
                if (browserData.favourites.ElementAt(i).customTitle == null)
                {
                    link.Text = browserData.favourites.ElementAt(i).title;
                }
                else
                {
                    link.Text = browserData.favourites.ElementAt(i).customTitle;
                }
                link.Width = 320;
                link.FlatStyle = FlatStyle.Flat;
                link.ForeColor = Color.White;
                link.Tag = browserData.favourites.ElementAt(i).url;
                table.Controls.Add(link, 0, i + 1);
                link.Click += new EventHandler(favhistLink_Click);
                Button remove = new Button();
                remove.Text = "Remove";
                remove.ForeColor = Color.White;
                remove.Width = 80;
                remove.Height = 20;
                remove.Tag = link.Text;
                remove.Click += new EventHandler(fpRemove_Click);
                table.Controls.Add(remove, 1, i + 1);
                Button edit = new Button();
                edit.Text = "Edit Title";
                edit.ForeColor = Color.White;
                edit.Width = 80;
                edit.Height = 20;
                edit.Tag = link.Tag;
                edit.Click += new EventHandler(fpEdit_Click);
                table.Controls.Add(edit, 2, i + 1);
            }
        }



        /// <summary>
        /// Creates a new tab, add all of the elements that make up the tab, then adds
        /// the new tab to the tabControl.
        /// </summary>
        /// <returns>new TabPage</returns>
        private TabPage createTab()
        {
            TabPage tp = new TabPage("New Tab");

            //BackGround
            tp.BackColor = Color.DarkSlateBlue;
            tp.Width = 1227;
            tp.Height = 676;
            
            //Main Text Box
            TextBox textBox = new TextBox();
            textBox.Location = new Point(64, 50);
            textBox.Height = 475;
            textBox.Width = 834;
            textBox.Font = new Font(textBox.Font.FontFamily, 15);
            textBox.BackColor = Color.LightSlateGray;
            textBox.ForeColor = Color.White;
            textBox.Multiline = true;
            textBox.ReadOnly = true;
            textBox.ScrollBars = ScrollBars.Vertical;
            textBox.Name = "mainTextBox";
            tp.Controls.Add(textBox);



            //Search Bar and Page Control
                //Back Button
                Button backButton = new Button();
                tp.Controls.Add(backButton);
                backButton.Height = 28;
                backButton.Width = 28;
                backButton.Location = new Point(0, 0);
                backButton.BackColor = Color.LightSlateGray;
                backButton.ForeColor = Color.White;
                backButton.Text = "B";
                backButton.FlatStyle = FlatStyle.Flat;
                backButton.Click += new EventHandler(backButton_Click);
                //Forward Button
                Button forwardButton = new Button();
                tp.Controls.Add(forwardButton);
                forwardButton.Height = 28;
                forwardButton.Width = 28;
                forwardButton.Location = new Point(28, 0);
                forwardButton.BackColor = Color.LightSlateGray;
                forwardButton.ForeColor = Color.White;
                forwardButton.Text = "F";
                forwardButton.FlatStyle = FlatStyle.Flat;
                forwardButton.Click += new EventHandler(forwardButton_Click);
                //Refresh Button
                Button refreshButton = new Button();
                tp.Controls.Add(refreshButton);
                refreshButton.Height = 28;
                refreshButton.Width = 28;
                refreshButton.Location = new Point(56, 0);
                refreshButton.BackColor = Color.LightSlateGray;
                refreshButton.ForeColor = Color.White;
                refreshButton.Text = "R";
                refreshButton.FlatStyle = FlatStyle.Flat;
                refreshButton.Click += new EventHandler(refreshButton_Click);
                //Search Bar
                TextBox searchBar = new TextBox();
                searchBar.Location = new Point(84, 0);
                searchBar.Width = 766;
                searchBar.Font = new Font(searchBar.Font.FontFamily, 14);
                searchBar.BackColor = Color.LightSlateGray;
                searchBar.ForeColor = Color.White;
                searchBar.Text = "http://";
                searchBar.Name = "searchBar";
                tp.Controls.Add(searchBar);
                //Search Button
                Button searchButton = new Button();
                searchButton.Location = new Point(850, 1);
                searchButton.Width = 68;
                searchButton.Height = 27;
                searchButton.Font = new Font(searchButton.Font.FontFamily, 12);
                searchButton.Text = "Go!";
                searchButton.BackColor = Color.LightSlateGray;
                searchButton.ForeColor = Color.White;
                searchButton.FlatStyle = FlatStyle.Flat;
                searchButton.Click += new EventHandler(searchButton_Click);
                searchButton.Name = "searchButton";
                tp.Controls.Add(searchButton);



            //Control Panel and Components
                //Control Panel
                Panel controlPanel = new Panel();
                controlPanel.Location = new Point(0, 30);
                controlPanel.Height = 600;
                controlPanel.Width = 44;
                controlPanel.BackColor = Color.LightSlateGray;
                controlPanel.Name = "controlPanel";
                //Home Button
                Button homeButton = new Button();
                controlPanel.Controls.Add(homeButton);
                homeButton.Height = 30;
                homeButton.Width = 30;
                homeButton.Location = new Point(7, 5);
                homeButton.BackColor = Color.DarkSlateBlue;
                homeButton.ForeColor = Color.White;
                homeButton.FlatStyle = FlatStyle.Flat;
                homeButton.Click += new EventHandler(homeButton_Click);
                Label homeCaption = new Label();
                controlPanel.Controls.Add(homeCaption);
                homeCaption.Text = "Home";
                homeCaption.Font = new Font(homeCaption.Font.FontFamily, 8);
                homeCaption.ForeColor = Color.White;
                homeCaption.Location = new Point(4, 40);
                //Favourite Button
                Button favouriteButton = new Button();
                controlPanel.Controls.Add(favouriteButton);
                favouriteButton.Height = 30;
                favouriteButton.Width = 30;
                favouriteButton.Location = new Point(7, 70);
                favouriteButton.BackColor = Color.DarkSlateBlue;
                favouriteButton.ForeColor = Color.White;
                favouriteButton.FlatStyle = FlatStyle.Flat;
                favouriteButton.Click += new EventHandler(favouriteButton_Click);
                Label favouriteCaption = new Label();
                controlPanel.Controls.Add(favouriteCaption);
                favouriteCaption.Text = "Favs";
                favouriteCaption.Font = new Font(favouriteCaption.Font.FontFamily, 8);
                favouriteCaption.ForeColor = Color.White;
                favouriteCaption.Location = new Point(7, 105);
                //New Tab
                Button newTabButton = new Button();
                controlPanel.Controls.Add(newTabButton);
                newTabButton.Height = 30;
                newTabButton.Width = 30;
                newTabButton.Location = new Point(7, 135);
                newTabButton.BackColor = Color.DarkSlateBlue;
                newTabButton.ForeColor = Color.White;
                newTabButton.FlatStyle = FlatStyle.Flat;
                newTabButton.Click += new EventHandler(newTabButton_click);
                Label newTabCaption = new Label();
                controlPanel.Controls.Add(newTabCaption);
                newTabCaption.Text = "Add" + Environment.NewLine + "Tab";
                newTabCaption.Font = new Font(newTabCaption.Font.FontFamily, 8);
                newTabCaption.Height = 25;
                newTabCaption.ForeColor = Color.White;
                newTabCaption.Location = new Point(9, 170);
                //Close Tab
                Button closeTabButton = new Button();
                controlPanel.Controls.Add(closeTabButton);
                closeTabButton.Height = 30;
                closeTabButton.Width = 30;
                closeTabButton.Location = new Point(7, 210);
                closeTabButton.BackColor = Color.DarkSlateBlue;
                closeTabButton.ForeColor = Color.White;
                closeTabButton.FlatStyle = FlatStyle.Flat;
                closeTabButton.Click += new EventHandler(closeTabButton_Click);
                Label closeTabCaption = new Label();
                controlPanel.Controls.Add(closeTabCaption);
                closeTabCaption.Text = "Close" + Environment.NewLine + " Tab";
                closeTabCaption.Font = new Font(closeTabCaption.Font.FontFamily, 8);
                closeTabCaption.Height = 25;
                closeTabCaption.ForeColor = Color.White;
                closeTabCaption.Location = new Point(7, 245);
                //History Button
                Button historyButton = new Button();
                controlPanel.Controls.Add(historyButton);
                historyButton.Height = 30;
                historyButton.Width = 30;
                historyButton.Location = new Point(7, 285);
                historyButton.BackColor = Color.DarkSlateBlue;
                historyButton.ForeColor = Color.White;
                historyButton.FlatStyle = FlatStyle.Flat;
                historyButton.Click += new EventHandler(historyButton_Click);
                Label historyCaption = new Label();
                controlPanel.Controls.Add(historyCaption);
                historyCaption.Text = "History";
                historyCaption.Font = new Font(historyCaption.Font.FontFamily, 8);
                historyCaption.Height = 25;
                historyCaption.ForeColor = Color.White;
                historyCaption.Location = new Point(3, 320);
                //Settings Button
                Button settingsButton = new Button();
                controlPanel.Controls.Add(settingsButton);
                settingsButton.Height = 30;
                settingsButton.Width = 30;
                settingsButton.Location = new Point(7, 460);
                settingsButton.BackColor = Color.DarkSlateBlue;
                settingsButton.ForeColor = Color.White;
                settingsButton.FlatStyle = FlatStyle.Flat;
                settingsButton.Click += new EventHandler(settingsButton_Click);
                Label settingsCaption = new Label();
                controlPanel.Controls.Add(settingsCaption);
                settingsCaption.Text = "Settings";
                settingsCaption.Font = new Font(settingsCaption.Font.FontFamily, 8);
                settingsCaption.Height = 25;
                settingsCaption.ForeColor = Color.White;
                settingsCaption.Location = new Point(1, 495);

                tp.Controls.Add(controlPanel);


            //Favourites
                //Favourites Panel
                Panel favPanel = new Panel();
                favPanel.Location = new Point(164, 100);
                favPanel.Height = 375;
                favPanel.Width = 634;
                favPanel.Name = "favPanel";
                tp.Controls.Add(favPanel);
                favPanel.Visible = false;
                favPanel.SendToBack();
                //Favourites Panel Title
                Label fpTitle = new Label();
                favPanel.Controls.Add(fpTitle);
                fpTitle.Font = new Font(fpTitle.Font.FontFamily, 18);
                fpTitle.BackColor = Color.DarkSlateBlue;
                fpTitle.ForeColor = Color.White;
                fpTitle.Location = new Point(250, 10);
                fpTitle.Width = 125;
                fpTitle.Text = "Favourites";
                //Favourites Panel Table
                TableLayoutPanel fpTable = new TableLayoutPanel();
                fpTable.Width = 600;
                fpTable.Height = 300;
                fpTable.ColumnCount = 3;
                fpTable.RowCount = 0;
                fpTable.Name = "fpTable";
                fpTable.AutoSize = false;
                fpTable.AutoScroll = true;
                favPanel.Controls.Add(fpTable);
                fpTable.Location = new Point(50, 50);
                //Favourite Panel Close Button
                Button fpClose = new Button();
                fpClose.Height = 30;
                fpClose.Width = 30;
                fpClose.Location = new Point(599, 5);
                fpClose.BackColor = Color.DarkSlateBlue;
                fpClose.ForeColor = Color.White;
                fpClose.Text = "X";
                fpClose.FlatStyle = FlatStyle.Flat;
                fpClose.Click += new EventHandler(fpClose_Click);
                favPanel.Controls.Add(fpClose);
                //Favourite Panel Add Button
                Button fpAdd = new Button();
                fpAdd.Height = 30;
                fpAdd.Width = 200;
                fpAdd.Location = new Point(5, 5);
                fpAdd.BackColor = Color.DarkSlateBlue;
                fpAdd.ForeColor = Color.White;
                fpAdd.Text = "Add Current Page to Favourites";
                fpAdd.FlatStyle = FlatStyle.Flat;
                fpAdd.Click += new EventHandler(fpAdd_Click);
                favPanel.Controls.Add(fpAdd);

            //History
                //History Panel
                Panel histPanel = new Panel();
                histPanel.Location = new Point(164, 100);
                histPanel.Height = 375;
                histPanel.Width = 634;
                histPanel.Name = "histPanel";
                tp.Controls.Add(histPanel);
                histPanel.Visible = false;
                histPanel.SendToBack();
                //History Panel Title
                Label hpTitle = new Label();
                histPanel.Controls.Add(hpTitle);
                hpTitle.Font = new Font(hpTitle.Font.FontFamily, 18);
                hpTitle.BackColor = Color.DarkSlateBlue;
                hpTitle.ForeColor = Color.White;
                hpTitle.Location = new Point(250, 10);
                hpTitle.Width = 100;
                hpTitle.Height = 30;
                hpTitle.Text = "History";
                //History Panel Table
                TableLayoutPanel hpTable = new TableLayoutPanel();
                hpTable.Width = 580;
                hpTable.Height = 300;
                hpTable.ColumnCount = 2;
                hpTable.RowCount = 0;
                hpTable.Name = "hpTable";
                hpTable.AutoSize = false;
                hpTable.AutoScroll = true;
                histPanel.Controls.Add(hpTable);
                hpTable.Location = new Point(50, 50);
                //History Panel Close Button
                Button hpClose = new Button();
                hpClose.Height = 30;
                hpClose.Width = 30;
                hpClose.Location = new Point(599, 5);
                hpClose.BackColor = Color.DarkSlateBlue;
                hpClose.ForeColor = Color.White;
                hpClose.Text = "X";
                hpClose.FlatStyle = FlatStyle.Flat;
                hpClose.Click += new EventHandler(hpClose_Click);
                histPanel.Controls.Add(hpClose);

            //Settings
                //Settings Panel
                Panel settingsPanel = new Panel();
                settingsPanel.Location = new Point(164, 100);
                settingsPanel.Height = 375;
                settingsPanel.Width = 634;
                settingsPanel.Name = "settingsPanel";
                tp.Controls.Add(settingsPanel);
                settingsPanel.Visible = false;
                settingsPanel.SendToBack();
                //Settings Panel Title
                Label spTitle = new Label();
                settingsPanel.Controls.Add(spTitle);
                spTitle.Font = new Font(spTitle.Font.FontFamily, 18);
                spTitle.BackColor = Color.DarkSlateBlue;
                spTitle.ForeColor = Color.White;
                spTitle.Location = new Point(250, 10);
                spTitle.Width = 100;
                spTitle.Height = 30;
                spTitle.Text = "Settings";
                //Settings Panel Close Button
                Button spClose = new Button();
                spClose.Height = 30;
                spClose.Width = 30;
                spClose.Location = new Point(599, 5);
                spClose.BackColor = Color.DarkSlateBlue;
                spClose.ForeColor = Color.White;
                spClose.Text = "X";
                spClose.FlatStyle = FlatStyle.Flat;
                spClose.Click += new EventHandler(spClose_Click);
                settingsPanel.Controls.Add(spClose);
                //SetHome Label
                Label setHomeLabel = new Label();
                setHomeLabel.Text = "Change home page";
                setHomeLabel.Font = new Font(setHomeLabel.Font.FontFamily, 12);
                setHomeLabel.BackColor = Color.DarkSlateBlue;
                setHomeLabel.ForeColor = Color.White;
                setHomeLabel.Location = new Point(60, 100);
                setHomeLabel.Width = 200;
                setHomeLabel.Height = 30;
                settingsPanel.Controls.Add(setHomeLabel);
                //SetHome TextBox
                TextBox setHomeText = new TextBox();
                setHomeText.Font = new Font(setHomeText.Font.FontFamily, 12);
                setHomeText.Location = new Point(280, 100);
                setHomeText.Width = 300;
                setHomeText.BackColor = Color.LightSlateGray;
                setHomeText.ForeColor = Color.White;
                setHomeText.Name = "setHomeText";
                setHomeText.Text = "http://";
                settingsPanel.Controls.Add(setHomeText);
                //SetHome Button
                Button setHomeButton = new Button();
                setHomeButton.Height = 30;
                setHomeButton.Width = 100;
                setHomeButton.Location = new Point(350, 130);
                setHomeButton.BackColor = Color.DarkSlateBlue;
                setHomeButton.ForeColor = Color.White;
                setHomeButton.Text = "Confirm Change";
                setHomeButton.FlatStyle = FlatStyle.Flat;
                setHomeButton.Click += new EventHandler(setHome_Click);
                settingsPanel.Controls.Add(setHomeButton);
                //Clear History
                Button clearHistoryButton = new Button();
                clearHistoryButton.Height = 30;
                clearHistoryButton.Width = 200;
                clearHistoryButton.Location = new Point(200, 230);
                clearHistoryButton.BackColor = Color.DarkSlateBlue;
                clearHistoryButton.ForeColor = Color.White;
                clearHistoryButton.Text = "Clear Browsing History";
                clearHistoryButton.FlatStyle = FlatStyle.Flat;
                clearHistoryButton.Click += new EventHandler(clearHistory_Click);
                settingsPanel.Controls.Add(clearHistoryButton);

            return tp;
        }

    }

    /// <summary>
    /// Class that handles the http communication and allows for the browser to be multithreaded. Contains fields; 
    /// string url, htmlCode, statusCodeString, timestamp, title; int statusCode; bool error
    /// </summary>
    public class BrowserCommunication
    {
        public string url;
        public string htmlCode;
        public string statusCodeString;
        public string timestamp;
        public string title;
        public int statusCode;
        public bool error;

        /// <summary>
        /// Constructor for BrowserCommunication. Takes only the url as a string.
        /// </summary>
        /// <param name="url">Url to be search</param>
        public BrowserCommunication(string url)
        {
            this.url = url;
        }

        /// <summary>
        /// Function for performing the http request and receiving a response.
        /// </summary>
        public void searchURL()
        {

            if (url == null)
            {
                System.Console.WriteLine("Cannot find url to search.");
                return;
            }


            //Creates the request and sets method to GET
            HttpWebRequest request = null;
            try
            {
                request = (HttpWebRequest)WebRequest.Create(url);
                request.Method = "GET";
            }
            catch (Exception e) { error = true; statusCodeString = e.Message; }


            //Gets the response message from the request
            HttpWebResponse res = null;
            res = request.GetResponseNoException();


            //Gets the statusCode and htmlCode for the communication
            if (res != null)
            {
                statusCode = (int)res.StatusCode;
                Stream stream = res.GetResponseStream();
                StreamReader reader = new StreamReader(stream);
                string responseText = reader.ReadToEnd();
                reader.Close();
                stream.Close();
                htmlCode = responseText;
            }
            else
            {
                statusCode = -1;
                htmlCode = "There has been a problem while getting the response message. Unable to load html code";
            }


            //Creates a timestamp for the communication
            timestamp = DateTime.Now.ToString();

            
            //Creates a statusCodeString from the status code received
            switch (statusCode)
            {
                case 200:
                    statusCodeString = "200 OK";
                    break;
                case 400:
                    statusCodeString = "400 Bad Request";
                    break;
                case 403:
                    statusCodeString = "403 Forbidden";
                    break;
                case 404:
                    statusCodeString = "404 Not Found";
                    break;
                default:
                    statusCodeString = "" + statusCode;
                    break;
            }
        

            //Finds the title within the htmlCode using a regular expression
            if (htmlCode != null)
            {
                Match m = Regex.Match(htmlCode, @"<title>\s*(.+?)\s*</title>");
                if (m.Success)
                {
                    title = m.Groups[1].Value;
                }
                else
                {
                    title = "";
                }
            }

        }

    }




    /// <summary>
    /// BrowserData is a class that can store all of the information that the browser needs to store (history, favourites, home page).
    /// The class is serializable to allow for the data to be stored when the window is closed and read when the application starts.
    /// </summary>
    [Serializable]
    public class BrowserData
    {
        
        public LinkedList<HistoryToken> history = new LinkedList<HistoryToken>();
        public List<FavouriteToken> favourites = new List<FavouriteToken>();
        public string home = "http://www.google.co.uk";

        /// <summary>
        /// Structure that can store all the required information for the history
        /// </summary>
        [Serializable]
        public struct HistoryToken
        {
            public string url;
            public DateTime dateTime;

            public HistoryToken(string url, DateTime dateTime)
            {
                this.url = url;
                this.dateTime = dateTime;
            }
        }

        /// <summary>
        /// Structure that can create all the required information for the favourites
        /// </summary>
        [Serializable]
        public struct FavouriteToken
        {
            public string title;
            public string url;
            public string customTitle;

            public FavouriteToken(string title, string url)
            {
                this.title = title;
                this.url = url;
                this.customTitle = null;
            }
        }

        /// <summary>
        /// Adds a new HistoryToken to the history list
        /// </summary>
        /// <param name="url">Url that was accessed</param>
        /// <param name="dateTime">When the page was visited</param>
        public void addHT(string url, DateTime dateTime)
        {
            HistoryToken current = new HistoryToken(url, dateTime);
            history.AddLast(current);
        }

        /// <summary>
        /// Adds a new FavouriteToken to the favourite list
        /// </summary>
        /// <param name="title">Title for the favourited webpage</param>
        /// <param name="url">The url for the webpage</param>
        public void addFT(string title, string url)
        {
            FavouriteToken current = new FavouriteToken(title, url);
            if (favourites.Contains(current) != true)
            {
                favourites.Add(current);
            }
        }

        /// <summary>
        /// Removes a FavouriteToken from the favourites list by searchng for its title
        /// </summary>
        /// <param name="title">Title of the FavouriteToken to be removed</param>
        public void removeFT(string title)
        {
            for (int i = 0; i < favourites.Count(); i++)
            {
                FavouriteToken current = favourites.ElementAt(i);
                if (current.title == title)
                {
                    favourites.Remove(current);
                    return;
                }
            }
        }

        /// <summary>
        /// Edits a FavouriteToken by editing the title associated to it
        /// </summary>
        /// <param name="oldTitle">Old title to be changed</param>
        /// <param name="newTitle">New title to be assigned</param>
        public void editFT(string url, string newTitle)
        {
            for (int i = 0; i < favourites.Count(); i++)
            {
                FavouriteToken current = favourites.ElementAt(i);
                if (current.url == url)
                {
                    favourites.Remove(current);
                    favourites.Insert(i, new FavouriteToken(newTitle, current.url));
                    return;
                }
            }
        }

        /// <summary>
        /// Traverses the history to find either the next or the previous node(page)
        /// </summary>
        /// <param name="url">The current url</param>
        /// <param name="next">True if looking for the next page, false if we want to go back</param>
        /// <returns></returns>
        public string traverseHistory(string url, bool next)
        {
            string returnUrl = "";
            for (int i = history.Count-1; i >= 0; i--)
            {
                if (history.ElementAt(i).url == url)
                {
                    HistoryToken ht = history.ElementAt(i);
                    HistoryToken temp;
                    if (next == true)
                    {
                        if (history.Find(ht).Next == null) break;

                        temp = history.Find(ht).Next.Value;
                        returnUrl = temp.url;
                        return returnUrl;
                    }
                    else
                    {
                        if (history.Find(ht).Previous == null) break;

                        temp = history.Find(ht).Previous.Value;
                        returnUrl = temp.url;
                        return returnUrl;
                    }
                }
            }
            return returnUrl;
        }

    }



    /// <summary>
    /// Extensions for HttpWebResponse that returns a response even when an exception has been thrown.
    /// Allows for all status codes to be returned and used.
    /// </summary>
    public static class HttpWebResponseExt
    {
        public static HttpWebResponse GetResponseNoException(this HttpWebRequest req)
        {
            HttpWebResponse webResponse;
            try
            {
                webResponse = (HttpWebResponse)req.GetResponse();
                return webResponse;
            }
            catch (WebException we)
            {
                var resp = we.Response as HttpWebResponse;
                if (resp == null)
                    throw;
                return resp;
            }
        }
    }

}

