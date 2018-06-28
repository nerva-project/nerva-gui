using System;
using System.Collections.Generic;
using Eto.Forms;
using Eto.Drawing;
using AngryWasp.Logger;
using Nerva.Toolkit.Helpers;
using Nerva.Toolkit.CLI.Structures.Response;
using Nerva.Toolkit.Config;
using Nerva.Toolkit.CLI;
using Nerva.Toolkit.Content.Dialogs;

namespace Nerva.Toolkit.Content
{	
	public partial class BalancesPage
	{
		private StackLayout mainControl;
        public StackLayout MainControl => mainControl;

		GridView grid;

		Label lblTotalXnv = new Label();
		Label lblUnlockedXnv = new Label();

		private List<SubAddressAccount> accounts;

		public BalancesPage() { }

        public void ConstructLayout()
		{
			var ctx_Mine = new Command { MenuText = "Mine" };
			var ctx_Transfer = new Command { MenuText = "Transfer" };
			var ctx_Rename = new Command { MenuText = "Rename" };

			ctx_Mine.Executed += (s, e) =>
			{
				if (grid.SelectedRow == -1)
					return;

				SubAddressAccount a = accounts[grid.SelectedRow];
				Configuration.Instance.Daemon.MiningAddress = a.BaseAddress;
				Configuration.Save();

				Cli.Instance.Daemon.StopMining();
				Log.Instance.Write("Mining stopped");

				if (Cli.Instance.Daemon.StartMining(Configuration.Instance.Daemon.MiningThreads))
					Log.Instance.Write("Mining started for @ {0} on {1} threads", 
						Conversions.WalletAddressShortForm(Configuration.Instance.Daemon.MiningAddress),
						Configuration.Instance.Daemon.MiningThreads);
			};

			ctx_Transfer.Executed += (s, e) =>
			{

			};

			ctx_Rename.Executed += (s, e) =>
			{
				if (grid.SelectedRow == -1)
					return;

				EnterTextDialog d = new EnterTextDialog("Select Account Name");

				if (d.ShowModal() == DialogResult.Ok)
					if (!Cli.Instance.Wallet.LabelAccount((uint)grid.SelectedRow, d.Text))
						MessageBox.Show("Failed to rename account", MessageBoxType.Error);
			};

			grid = new GridView
			{
				GridLines = GridLines.Horizontal,
				Columns = 
				{
					new GridColumn { DataCell = new TextBoxCell { Binding = Binding.Property<SubAddressAccount, string>(r => r.Index.ToString())}, HeaderText = "#" },
					new GridColumn { DataCell = new TextBoxCell { Binding = Binding.Property<SubAddressAccount, string>(r => r.Label)}, HeaderText = "Label" },
					new GridColumn { DataCell = new TextBoxCell { Binding = Binding.Property<SubAddressAccount, string>(r => Conversions.WalletAddressShortForm(r.BaseAddress))}, HeaderText = "Address" },
					new GridColumn { DataCell = new TextBoxCell { Binding = Binding.Property<SubAddressAccount, string>(r => Conversions.FromAtomicUnits(r.Balance).ToString())}, HeaderText = "Balance" },
					new GridColumn { DataCell = new TextBoxCell { Binding = Binding.Property<SubAddressAccount, string>(r => Conversions.FromAtomicUnits(r.UnlockedBalance).ToString())}, HeaderText = "Unlocked" },
				}
			};

			grid.ContextMenu = new ContextMenu
			{
				Items = 
				{
					ctx_Mine,
					ctx_Transfer,
					ctx_Rename
				}
			};

			mainControl = new StackLayout
			{
				Orientation = Orientation.Vertical,
				HorizontalContentAlignment = HorizontalAlignment.Stretch,
				VerticalContentAlignment = VerticalAlignment.Stretch,
				Items = 
				{
					new StackLayoutItem(new TableLayout
					{
						Padding = 10,
						Spacing = new Eto.Drawing.Size(10, 10),
						Rows =
						{
							new TableRow(
								new TableCell(new Label { Text = "Total XNV" }),
								new TableCell(lblTotalXnv, true),
								new TableCell(null)),
							new TableRow(
								new TableCell(new Label { Text = "Unlocked XNV" }),
								new TableCell(lblUnlockedXnv, true),
								new TableCell(null))
						}
					}, false),
					new StackLayoutItem(grid, true)
				}
			};
		}

		public void Update(Account a)
		{
			if (a != null)
			{
				lblTotalXnv.Text = Conversions.FromAtomicUnits(a.TotalBalance).ToString();
				lblUnlockedXnv.Text = Conversions.FromAtomicUnits(a.TotalUnlockedBalance).ToString();
				accounts = a.Accounts;
			}
			else
			{
				lblTotalXnv.Text = "-";
				lblUnlockedXnv.Text = "-";
				accounts = null;
			}

			grid.DataStore = accounts;
		}
    }
}