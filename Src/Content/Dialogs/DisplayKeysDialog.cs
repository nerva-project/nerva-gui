using Eto.Forms;
using Eto.Drawing;
using Nerva.Rpc;
using Nerva.Rpc.Wallet;
using Nerva.Desktop.CLI;
using Nerva.Desktop.Helpers;

namespace Nerva.Desktop.Content.Dialogs
{
    public class DisplayKeysDialog : DialogBase<DialogResult>
	{
        private const string INVALID_KEY_1 = "0000000000000000000000000000000000000000000000000000000000000000";
        private const string INVALID_KEY_2 = "ffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffff";

        TextBox txtPublicViewKey = new TextBox { ReadOnly = true };
        TextBox txtPrivateViewKey = new TextBox { ReadOnly = true };

        TextBox txtPublicSpendKey = new TextBox { ReadOnly = true };
        TextBox txtPrivateSpendKey = new TextBox { ReadOnly = true };

        TextArea txtSeed = new TextArea { ReadOnly = true, Wrap = true };

        public DisplayKeysDialog() : base("Restore Info")
        {
            this.MinimumSize = new Size(300, 400);

            Helpers.TaskFactory.Instance.RunTask("getkeys", $"Retrieving wallet keys", () =>
            {
                WalletRpc.QueryKey("all_keys", (QueryKeyResponseData r) =>
                {
                    Application.Instance.AsyncInvoke(() =>
                    {
                        txtPublicViewKey.Text = r.PublicViewKey;
                        txtPrivateViewKey.Text = IsKeyValid(r.PrivateViewKey) ? r.PrivateViewKey : "Not available";

                        txtPublicSpendKey.Text = r.PublicSpendKey;
                        txtPrivateSpendKey.Text = IsKeyValid(r.PrivateSpendKey) ? r.PrivateSpendKey : "Not available";
                    });  
                }, (RequestError e) =>
                {
                    Application.Instance.AsyncInvoke(() =>
                    {
                        txtPublicViewKey.Text = "Not available";
                        txtPrivateViewKey.Text = "Not available";

                        txtPublicSpendKey.Text = "Not available";
                        txtPrivateSpendKey.Text = "Not available";
                    });
                });

                WalletRpc.QueryKey("mnemonic", (QueryKeyResponseData r) =>
                {
                    Application.Instance.AsyncInvoke(() =>
                    {
                        txtSeed.Text = r.MnemonicSeed;
                    });
                }, (RequestError e) =>
                {
                    Application.Instance.AsyncInvoke(() =>
                    {
                        txtSeed.Text = e.Message;
                    });
                });
            });

            //reuse ok and cancel buttons but give a more meaningful label
            btnOk.Visible = false;
            btnCancel.Text = "Close";

            this.DefaultButton = btnCancel;
        }

        private Control CreateKeyItem(TextBox txtPubCtrl, TextBox txtPvtCtrl)
        {
            return new TableLayout
            {
                Spacing = new Eto.Drawing.Size(10, 10),
                
                Rows =
                {
                    new TableRow(
                        new TableCell(new Label { Text = "Public:" }),
                        new TableCell(txtPubCtrl, true)),
                    new TableRow(
                        new TableCell(new Label { Text = "Private:" }),
                        new TableCell(txtPvtCtrl, true)),
                }
            };
        }

        protected override Control ConstructChildContent()
        {
            return new StackLayout
            {
                Padding = 10,
                Spacing = 10,
                Orientation = Orientation.Vertical,
				HorizontalContentAlignment = HorizontalAlignment.Stretch,
				VerticalContentAlignment = VerticalAlignment.Stretch,
                Items = 
                {
                    new StackLayoutItem(new Label { Text = "View Key" }),
                    CreateKeyItem(txtPublicViewKey, txtPrivateViewKey),
                    new StackLayoutItem(new Label { Text = "Spend Key" }),
                    CreateKeyItem(txtPublicSpendKey, txtPrivateSpendKey),
                    new StackLayoutItem(new Label { Text = "Mnemonic Seed" }),
                    new StackLayoutItem(txtSeed, true),
                }
            };
        }

        protected override void OnOk()
        {
            // Saving keys does not apply and button is invisible. This should never log
            Logger.LogDebug("DKD.OOk", "Saving keys disabled.  Why are you here?");
            this.Close(DialogResult.Ok);
        }

        protected override void OnCancel()
        {
            this.Close(DialogResult.Cancel);
        }

        private bool IsKeyValid(string key)
        {
            if (key == INVALID_KEY_1 || key == INVALID_KEY_2)
                return false;

            return true;
        }
    }
}