using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading;
using AngryWasp.Logger;
using Eto.Forms;
using Nerva.Toolkit.CLI;
using Nerva.Toolkit.CLI.Structures.Response;
using Nerva.Toolkit.Config;
using Nerva.Toolkit.Content.Dialogs;
using Nerva.Toolkit.Helpers;

namespace Nerva.Toolkit
{
    public partial class MainForm : Form
	{	
		Thread updateDaemonThread;
		bool shouldUpdateDaemon = true;

		Thread updateWalletThread;
		bool shouldUpdateWallet = true;

		Thread pingThread;

		bool pingSuccess = true;

		public MainForm()
		{
			SuspendLayout();
			ConstructLayout();
			ResumeLayout();

			Cli.Instance.Start();

			Application.Instance.Initialized += (s, e) =>
			{
				StartUpdateThread();
			};

			this.Closing += (s, e) =>
			{
				shouldUpdateDaemon = false;
				shouldUpdateWallet = false;
				Cli.Instance.KillRunningProcesses(FileNames.RPC_WALLET, -1);
			};
		}

		private void StartUpdateThread()
		{
			updateDaemonThread = new Thread(new ThreadStart(UpdateDaemonUI));
			shouldUpdateDaemon = true;
			updateDaemonThread.Start();

			updateWalletThread = new Thread(new ThreadStart(UpdateWalletUI));
			shouldUpdateWallet = true;
			updateWalletThread.Start();

			pingThread = new Thread(new ThreadStart(CheckConnection));
			pingSuccess = true;
			pingThread.Start();
		}

		int lastHeight = -1;
		private void UpdateDaemonUI()
		{
			while (shouldUpdateDaemon)
			{
				Thread.Sleep(Constants.DAEMON_POLL_INTERVAL);

				//spin the wheels for a bit if we should be updating, but have no daemon
				while(shouldUpdateDaemon && Cli.Instance.DaemonPid == -1)
					Thread.Sleep(Constants.DAEMON_RESTART_THREAD_INTERVAL);

				//Condition may have changed. 5 seconds is a long time
				if (!shouldUpdateDaemon)
					break;

				Info info = null;
				List<Connection> connections = null;
				int height = -1;
				MiningStatus mStatus = null;

				try
				{
					height = Cli.Instance.Daemon.GetBlockCount();
					info = Cli.Instance.Daemon.GetInfo();
					connections = Cli.Instance.Daemon.GetConnections();
					mStatus = Cli.Instance.Daemon.GetMiningStatus();
				}
				catch (Exception) { }

				//Double check we want to update before we do
				if (!shouldUpdateDaemon)
					break;

				if (info != null && connections != null && height != -1)
				{
					Application.Instance.AsyncInvoke ( () =>
					{
						lblStatus.Text = $"Height: {height} | Connections (In/Out): {info.IncomingConnectionsCount} / {info.OutgoingConnectionsCount}";
						lblVersion.Text = $"Version: {info.Version}";
						ad.Version = $"GUI: {Constants.VERSION}\r\nCLI: {info.Version}";

						if (info.TargetHeight != 0 && info.Height < info.TargetHeight)
							lblStatus.Text += " | Syncing";
						else
							lblStatus.Text += " | Sync OK";

						daemonPage.Update(info, connections, mStatus);
					});
				}
				else
				{
					Application.Instance.AsyncInvoke ( () =>
					{
						lblStatus.Text = "NOT CONNECTED TO DAEMON";
						daemonPage.Update(null, null, null);
					});
				}
			}
		}

		private void UpdateWalletUI()
		{
			uint lastTxHeight = 0;
			while (shouldUpdateWallet)
			{
				Thread.Sleep(Constants.DAEMON_POLL_INTERVAL);

				//spin the wheels for a bit if we should be updating, but have no wallet
				while(shouldUpdateWallet && Cli.Instance.WalletPid == -1)
					Thread.Sleep(Constants.DAEMON_RESTART_THREAD_INTERVAL);

				//Condition may have changed. 5 seconds is a long time
				if (!shouldUpdateWallet)
					break;

				Account account = null;
				TransferList transfers = null;

				try
				{
					account = Cli.Instance.Wallet.GetAccounts();
					transfers = Cli.Instance.Wallet.GetTransfers(lastTxHeight, out lastTxHeight);
				}
				catch (Exception ex)
				{
					Log.Instance.WriteNonFatalException(ex);
				}

				//Double check we want to update before we do
				if (!shouldUpdateWallet)
					break;

				if (account != null)
				{
					Application.Instance.AsyncInvoke ( () =>
					{
						balancesPage.Update(account);
						transfersPage.Update(transfers);
						sendPage.Update(account);
					});
				}
				else
				{
					lastTxHeight = 0;
					Application.Instance.AsyncInvoke ( () =>
					{
						balancesPage.Update(null);
						transfersPage.Update(null);
						sendPage.Update(null);
					});
				}
			}
		}

		private void CheckConnection()
		{
			while (true)
			{
				//If last ping failed. speed up this wait time. Do the online check as quick as reasonably possible
				Thread.Sleep(pingSuccess ? Constants.DAEMON_POLL_INTERVAL * 5 : Constants.DAEMON_POLL_INTERVAL);

				//Try pinging all 3 seed nodes before calling it disconnected
				bool pingOk = NetHelper.PingServer(SeedNodes.XNV1);

				if (!pingOk)
					pingOk = NetHelper.PingServer(SeedNodes.XNV2);

				if (!pingOk)
					pingOk = NetHelper.PingServer(SeedNodes.XNV3);

				pingSuccess = pingOk;

				if (!pingOk)
				{
					shouldUpdateDaemon = false;
					shouldUpdateWallet = false;

					Application.Instance.AsyncInvoke(() =>
					{
						lblStatus.Text = "NOT CONNECTED TO INTERNET";
						daemonPage.Update(null, null, null);
					});

					continue;
				}
				else
				{
					shouldUpdateDaemon = true;
					shouldUpdateWallet = true;
				}	
			}
		}

		protected void daemon_ToggleMining_Clicked(object sender, EventArgs e)
		{
			MiningStatus ms = Cli.Instance.Daemon.GetMiningStatus();

			if (ms.Active)
			{
				Cli.Instance.Daemon.StopMining();
				Log.Instance.Write("Mining stopped"); 
			}
			else
				if (Cli.Instance.Daemon.StartMining(Configuration.Instance.Daemon.MiningThreads))
					Log.Instance.Write("Mining started for @ {0} on {1} threads", 
						Conversions.WalletAddressShortForm(Configuration.Instance.Daemon.MiningAddress),
						Configuration.Instance.Daemon.MiningThreads);
		}

		protected void daemon_Restart_Clicked(object sender, EventArgs e)
		{
			//Log the restart and kill the daemon
			Log.Instance.Write("Restarting daemon");
			Cli.Instance.Daemon.StopDaemon();
			//From here the crash handler should reboot the daemon
		}

		protected void wallet_Select_Clicked(object sender, EventArgs e)
		{
			Wallet_Wizard_Result result = Wallet_Wizard_Result.Undefined;
            WalletHelper.ShowWalletWizard(out result);

			if (result != Wallet_Wizard_Result.Cancelled && result != Wallet_Wizard_Result.Undefined)
				transfersPage.Update(null);
				
            Log.Instance.Write(result.ToString());
		}

		protected void wallet_Store_Clicked(object sender, EventArgs e)
		{
			Log.Instance.Write("Storing blockchain");
			Cli.Instance.Wallet.Store();
		}

		protected void wallet_RescanSpent_Clicked(object sender, EventArgs e)
		{
			BackgroundWorker w = new BackgroundWorker();

			w.DoWork += (ws, we) =>
			{
				Log.Instance.Write("Rescanning spent outputs");
				if (!Cli.Instance.Wallet.RescanSpent())
					Log.Instance.Write("Rescanning spent outputs failed");
				else
					Log.Instance.Write("Rescanning spent outputs success");
			};

			w.RunWorkerCompleted += (ws, we) =>
			{
				MessageBox.Show("Rescanning spent outputs complete");
			};

			w.RunWorkerAsync();
		}

		protected void wallet_RescanBlockchain_Clicked(object sender, EventArgs e)
		{
			BackgroundWorker w = new BackgroundWorker();

			w.DoWork += (ws, we) =>
			{
				Log.Instance.Write("Rescanning blockchain");
				if (!Cli.Instance.Wallet.RescanBlockchain())
					Log.Instance.Write("Rescanning blockchain failed");
				else
					Log.Instance.Write("Rescanning blockchain success");
			};

			w.RunWorkerCompleted += (ws, we) =>
			{
				MessageBox.Show("Rescanning blockchain complete");
			};

			w.RunWorkerAsync();
		}

		protected void wallet_Keys_View_Clicked(object sender, EventArgs e)
		{
			new DisplaySeedDialog(Key_Type.View_Key).ShowModal();
		}

		protected void wallet_Keys_Mnemonic_Clicked(object sender, EventArgs e)
		{
			new DisplaySeedDialog(Key_Type.Mnemonic).ShowModal();
		}

		protected void about_Clicked(object sender, EventArgs e)
		{
			ad.ShowDialog(this);
		}

		protected void quit_Clicked(object sender, EventArgs e)
		{
			Application.Instance.Quit();
		}
    }
}