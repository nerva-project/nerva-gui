using System;
using Nerva.Toolkit.Helpers;
using Nerva.Rpc.Wallet;
using System.Collections.Generic;
using Configuration = Nerva.Toolkit.Config.Configuration;
using Nerva.Rpc;
using AngryWasp.Logger;
using System.Linq;

namespace Nerva.Toolkit.CLI
{
    public partial class WalletInterface : CliInterface
    {
        public WalletInterface() : base(Configuration.Instance.Wallet.Rpc) { }

        //todo: mute
        public bool GetAccounts(Action<GetAccountsResponseData> successAction, Action<RequestError> errorAction)
        {
            //suppress error code -13: No wallet file
            var l = Nerva.Rpc.Log.Presets.Normal;
            l.SuppressRpcCodes.Add(-13);
            return new GetAccounts(successAction, errorAction, r.Port, l).Run();
        }
            
        public bool CloseWallet(Action successAction, Action<RequestError> errorAction) =>
            new CloseWallet((string s) => {
                if (successAction != null)
                    successAction();
            }, errorAction, r.Port).Run(); 

        public bool StopWallet() =>
            new StopWallet(null, null, r.Port).Run(); 

        public bool CreateWallet(string walletName, string password,
            Action<CreateWalletResponseData> successAction, Action<RequestError> errorAction) =>
            new CreateWallet(new CreateWalletRequestData {
                FileName = walletName,
                Password = password
            }, successAction, errorAction, r.Port).Run();

        public bool CreateHwWallet(string walletName, string password,
            Action<CreateHwWalletResponseData> successAction, Action<RequestError> errorAction) =>
            new CreateHwWallet(new CreateHwWalletRequestData {
                FileName = walletName,
                Password = password
            }, successAction, errorAction, r.Port).Run();

        public bool RestoreWalletFromSeed(string walletName, string seed, string seedOffset, string password, string language, 
            Action<RestoreWalletFromSeedResponseData> successAction, Action<RequestError> errorAction) =>
            new RestoreWalletFromSeed(new RestoreWalletFromSeedRequestData {
                FileName = walletName, 
                Seed = seed,
                SeedOffset = seedOffset,
                Password = password
            }, successAction, errorAction, r.Port).Run();

        public bool RestoreWalletFromKeys(string walletName, string address, string viewKey, string spendKey, string password, string language, 
            Action<RestoreWalletFromKeysResponseData> successAction, Action<RequestError> errorAction) =>
            new RestoreWalletFromKeys(new RestoreWalletFromKeysRequestData {
                FileName = walletName, 
                Address = address,
                ViewKey = viewKey,
                SpendKey = spendKey,
                Password = password
            }, successAction, errorAction, r.Port).Run();

        public bool OpenWallet(string walletName, string password, Action successAction, Action<RequestError> errorAction) =>
            new OpenWallet(new OpenWalletRequestData {
                FileName = walletName,
                Password = password
            }, (string s) => {
                if (successAction != null)
                    successAction();
            }, errorAction, r.Port).Run();

        public bool QueryKey(string keyType, Action<QueryKeyResponseData> successAction, Action<RequestError> errorAction) =>
            new QueryKey(new QueryKeyRequestData {
                KeyType = keyType
            }, successAction, errorAction, r.Port).Run();

        public bool GetTransfers(ulong scanFromHeight, Action<GetTransfersResponseData> successAction, Action<RequestError> errorAction)
        {
            //suppress error code -13: No wallet file
            var l = Nerva.Rpc.Log.Presets.Normal;
            l.SuppressRpcCodes.Add(-13);

            return new GetTransfers(new GetTransfersRequestData {
                ScanFromHeight = scanFromHeight
            }, successAction, errorAction, r.Port, l).Run();
        }

        public bool RescanSpent() =>
            new RescanSpent(null, null, r.Port).Run();

        public bool RescanBlockchain() =>
            new RescanBlockchain(null, null, r.Port).Run();

        public bool Store() =>
            new Store(null, null, r.Port).Run();

        public bool CreateAccount(string label, Action<CreateAccountResponseData> successAction, Action<RequestError> errorAction) =>
            new CreateAccount(new CreateAccountRequestData {
                Label = label
            }, successAction, errorAction, r.Port).Run();

        public bool LabelAccount(uint index, string label) =>
            new LabelAccount(new LabelAccountRequestData {
                Index = index,
                Label = label
            }, null, null, r.Port).Run();

        public bool GetTransferByTxID(string txid, Action<GetTransferByTxIDResponseData> successAction, Action<RequestError> errorAction) =>
            new GetTransferByTxID(new GetTransferByTxIDRequestData {
                TxID = txid
            }, successAction, errorAction, r.Port).Run();

        public bool TransferFunds(SubAddressAccount acc, string address, string paymentId, double amount, Send_Priority priority, 
            Action<TransferResponseData> successAction, Action<RequestError> errorAction) =>
            new Transfer(new TransferRequestData {
                AccountIndex = acc.Index,
                Priority = (uint)priority,
                PaymentId = paymentId,
                Destinations = new List<TransferDestination> {
                    new TransferDestination {
                        Address = address,
                        Amount = Conversions.ToAtomicUnits(amount)
                    }
                }
            }, successAction, errorAction, r.Port).Run();

        public bool MakeIntegratedAddress(string address, Action<MakeIntegratedAddressResponseData> successAction, Action<RequestError> errorAction) =>
            new MakeIntegratedAddress(new MakeIntegratedAddressRequestData {
                StandardAddress = address
            }, successAction, errorAction, r.Port).Run();
    }
}