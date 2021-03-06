using Eto.Forms;
using Eto.Drawing;
using Nerva.Desktop.Content;
using Nerva.Desktop.Helpers;

namespace Nerva.Desktop
{
    public partial class MainForm : Form
	{	
		#region Status Bar controls
		
		Label lblDaemonStatus = new Label { Text = "Daemon offline" };
		Label lblWalletStatus = new Label { Text = "Wallet offline" };
		Label lblVersion = new Label { Text = "Version: 0.0.0.0" };
		Label lblTaskList = new Label { Text = "Tasks: 0", Tag = -1, Visible = false };

		DaemonPage daemonPage = new DaemonPage();
		BalancesPage balancesPage = new BalancesPage();
		TransfersPage transfersPage = new TransfersPage();

		#endregion

		public void ConstructLayout()
		{
			this.Title = "NERVA Desktop Wallet and Miner " + Version.LONG_VERSION;
			this.ClientSize = new Size(640, 480);
			this.MinimumSize = new Size(560, 400);

			// Set Icon but only if found. Otherwise, app will not work correctly
			string iconFile = GlobalMethods.GetAppIcon();
			if(!string.IsNullOrEmpty(iconFile))
			{				
				Icon = new Icon(iconFile);
			}

			daemonPage.ConstructLayout();
			balancesPage.ConstructLayout();
			transfersPage.ConstructLayout();

			TabControl tabs = new TabControl
			{
				Pages = {
					new TabPage { Text = "Daemon", Content = daemonPage.MainControl, Image =  Constants.DaemonTabImage },
					new TabPage { Text = "Balances", Content = balancesPage.MainControl, Image =  Constants.BalancesTabImage },
					new TabPage { Text = "Transfers", Content = transfersPage.MainControl, Image =  Constants.TransfersTabImage }
				}
			};

			TableLayout statusBar = new TableLayout
			{
				Padding = 5,
				Rows = {
					new TableRow (
						new TableCell(lblDaemonStatus, true),
						new TableCell(lblVersion)),
					new TableRow (
						new TableCell(lblWalletStatus, true),
						new TableCell(lblTaskList))
				}
			};

			Content = new TableLayout
			{
				Rows = {
					new TableRow (
						new TableCell(tabs, true)) { ScaleHeight = true },
					new TableRow (
						new TableCell(statusBar, true))
				}
			};

			// File
			var file_Preferences = new Command { MenuText = "Preferences", ToolBarText = "Preferences", Shortcut = Application.Instance.CommonModifier | Keys.Comma };	
			file_Preferences.Executed += file_Preferences_Clicked;

			var quitCommand = new Command { MenuText = "Quit", Shortcut = Application.Instance.CommonModifier | Keys.Q };
			quitCommand.Executed += quit_Clicked;


			// Daemon
			var daemon_ToggleMining = new Command { MenuText = "Toggle Miner", ToolBarText = "Toggle Miner", Shortcut = Application.Instance.CommonModifier | Keys.G };			
			daemon_ToggleMining.Executed += daemon_ToggleMining_Clicked;

			var daemon_Restart = new Command { MenuText = "Restart", ToolBarText = "Restart", Shortcut = Application.Instance.CommonModifier | Keys.R };
			daemon_Restart.Executed += daemon_Restart_Clicked;

			var daemonRestartQuickSync = new Command { MenuText = "Restart with QuickSync", ToolBarText = "Restart with QuickSync" };
			daemonRestartQuickSync.Executed += daemonRestartQuickSync_Clicked;

			var daemonRestartWithCommand = new Command { MenuText = "Restart with Command", ToolBarText = "Restart with Command" };
			daemonRestartWithCommand.Executed += daemonRestartWithCommand_Clicked;


			// Wallet			
			var wallet_Open = new Command { MenuText = "Open", ToolBarText = "Open", Shortcut = Application.Instance.CommonModifier | Keys.O };
			wallet_Open.Executed += wallet_Open_Clicked;

			var wallet_New = new Command { MenuText = "New", ToolBarText = "New" };
			wallet_New.Executed += wallet_New_Clicked;

			var wallet_Import = new Command { MenuText = "Import", ToolBarText = "Import" };
			wallet_Import.Executed += wallet_Import_Clicked;

			var wallet_Transfer = new Command { MenuText = "Transfer Funds", ToolBarText = "Transfer Funds", Shortcut = Application.Instance.CommonModifier | Keys.T };
			wallet_Transfer.Executed += wallet_Transfer_Clicked;

			var wallet_AddressInfo = new Command { MenuText = "Address Info", ToolBarText = "Address Info", Shortcut = Application.Instance.CommonModifier | Keys.I };
			wallet_AddressInfo.Executed += wallet_AddressInfo_Clicked;

			var wallet_Store = new Command { MenuText = "Save", ToolBarText = "Save", Shortcut = Application.Instance.CommonModifier | Keys.S };
			wallet_Store.Executed += wallet_Store_Clicked;

			var wallet_Stop = new Command { MenuText = "Close", ToolBarText = "Close wallet", Shortcut = Application.Instance.CommonModifier | Keys.X };
			wallet_Stop.Executed += wallet_Stop_Clicked;

			var wallet_Account_Create = new Command { MenuText = "New Sub-Account", ToolBarText = "New Sub-Account" };
			wallet_Account_Create.Executed += wallet_Account_Create_Clicked;

			var wallet_RescanSpent = new Command { MenuText = "Spent Outputs", ToolBarText = "Spent Outputs" };
			wallet_RescanSpent.Executed += wallet_RescanSpent_Clicked;

			var wallet_RescanBlockchain = new Command { MenuText = "Blockchain", ToolBarText = "Blockchain" };
			wallet_RescanBlockchain.Executed += wallet_RescanBlockchain_Clicked;

			var wallet_Keys_View = new Command { MenuText = "View Keys", ToolBarText = "View Keys" };
			wallet_Keys_View.Executed += wallet_Keys_View_Clicked;


			// Help
			var debugFolderCommand = new Command { MenuText = "Debug Folder", Shortcut = Application.Instance.CommonModifier | Keys.D };
			debugFolderCommand.Executed += debugFolderCommand_Clicked;

			var discordCommand = new Command { MenuText = "Discord" };
			discordCommand.Executed += discord_Clicked;

			var twitterCommand = new Command { MenuText = "Twitter" };
			twitterCommand.Executed += twitter_Clicked;

			var redditCommand = new Command { MenuText = "Reddit" };
			redditCommand.Executed += reddit_Clicked;

			var file_UpdateCheck = new Command { MenuText = "Check for Updates", ToolBarText = "Check for Updates", Shortcut = Application.Instance.CommonModifier | Keys.U };	
			file_UpdateCheck.Executed += file_UpdateCheck_Clicked;

			var aboutCommand = new Command { MenuText = "About..." };
			aboutCommand.Executed += about_Clicked;

			// create menu
			Menu = new MenuBar
			{
				Items =
				{
					// File submenu
					new ButtonMenuItem
					{ 
						Text = "&File",
						Items =
						{ 
							file_Preferences
						}
					},
					new ButtonMenuItem
					{
						Text = "&Daemon",
						Items =
						{
							daemon_ToggleMining,
							new SeparatorMenuItem(),
							daemon_Restart,
							daemonRestartWithCommand,
							daemonRestartQuickSync
						}
					},
					new ButtonMenuItem
					{
						Text = "&Wallet",
						Items =
						{
							wallet_Open,
							wallet_New,
							wallet_Import,
							new SeparatorMenuItem(),
							wallet_AddressInfo,
							wallet_Transfer,
							new SeparatorMenuItem(),
							wallet_Store,
							wallet_Stop,
							new SeparatorMenuItem(),
							wallet_Account_Create,
							new ButtonMenuItem
							{
								Text = "Rescan",
								Items =
								{
									wallet_RescanSpent,
									wallet_RescanBlockchain
								}
							},
							wallet_Keys_View
						}
					},
					new ButtonMenuItem
					{
						Text = "&Help",
						Items =
						{
							debugFolderCommand,
							new SeparatorMenuItem(),
							discordCommand,
							redditCommand,
							twitterCommand,
							new SeparatorMenuItem(),
							file_UpdateCheck
						}
					}
				},
				QuitItem = quitCommand,
				AboutItem = aboutCommand
			};
		}
	}
}
