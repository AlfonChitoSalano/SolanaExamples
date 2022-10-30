using Solnet.Programs;
using Solnet.Rpc;
using Solnet.Rpc.Builders;
using Solnet.Wallet;

namespace SolanaTest
{
    public class SendingTransactionExample : IExample
    {
        private const string ToMnemonicWords =
            "taxi clerk disease box emerge airport loud waste attitude film army tray " +
            "father deal club seven catalog surface unit card window walnut wealth peanut";

        private readonly Cluster _currentCluster;
        private readonly Wallet _senderWallet;
        private readonly Wallet _receiverWallet;

        public SendingTransactionExample(Cluster currentCluster, string mnemonicWords)
        {
            _currentCluster = currentCluster;
            _senderWallet = new Wallet(mnemonicWords);
            _receiverWallet = new Wallet(ToMnemonicWords);
        }

        public async Task RunAsync()
        {
            var fromAccount = _senderWallet.GetAccount(0);
            var senderBalance = GetBalanceThenShow(fromAccount.PublicKey, "Sender");

            if (senderBalance < 1)
            {
                await AirDroppingSolAsync(fromAccount);
                Console.WriteLine("Sender don't have any balance. Re run the app");
                return;
            }

            var toAccount = _receiverWallet.GetAccount(0);

            if (GetBalanceThenShow(toAccount.PublicKey, "Receiver") < 1)
            {
                await AirDroppingSolAsync(toAccount);
                Console.WriteLine("Receiver don't have any balance. Re run the app");
                return;
            }

            var rpcClient = ClientFactory.GetClient(_currentCluster);
            var blockHash = await rpcClient.GetLatestBlockHashAsync();
            Console.WriteLine($"BlockHash >> {blockHash.Result.Value.Blockhash}\n");

            var lamPortToSend = senderBalance / 2;
            var transactionByte = new TransactionBuilder()
                .SetRecentBlockHash(blockHash.Result.Value.Blockhash)
                .SetFeePayer(fromAccount)
                .AddInstruction(MemoProgram.NewMemo(fromAccount, $"We send {lamPortToSend.ToString()}."))
                .AddInstruction(SystemProgram.Transfer(fromAccount, toAccount.PublicKey, lamPortToSend))
                .Build(fromAccount);
            var signedTransaction = await rpcClient.SendTransactionAsync(transactionByte);

            if (!signedTransaction.WasSuccessful)
            {
                Console.WriteLine("Not success");
                return;
            }

            Console.WriteLine($"Success in this tx: {signedTransaction.Result}.");
            Console.WriteLine("Wait for a moment so that we get the updated balance from the rpc provider.\n");
            await Task.Delay(20000);
            var toWallet = new Wallet(ToMnemonicWords);
            var toAccountAfter = toWallet.GetAccount(0);
            GetBalanceThenShow(toAccountAfter.PublicKey, "Receiver");
        }

        private ulong GetBalanceThenShow(string publicKey, string toOrFrom)
        {
            var rpcClient = ClientFactory.GetClient(_currentCluster);
            var balance = rpcClient.GetBalance(publicKey);
            Console.WriteLine($"{toOrFrom} balance: {balance.Result.Value}");
            return balance.Result.Value;
        }

        private async Task AirDroppingSolAsync(Account account)
        {
            const ulong lamPort = 10000000; //1Lamport = 0.000000001SOL

            var publicKey = account.PublicKey;
            Console.WriteLine($"Air dropping to this account {publicKey} because no balance.");
            var rpcClient = ClientFactory.GetClient(_currentCluster);
            var transactionHash = await rpcClient.RequestAirdropAsync(publicKey, lamPort);
            Console.WriteLine($"TxHash: {transactionHash.Result}.");

            var streamingRpcClient = ClientFactory.GetStreamingClient(_currentCluster);
            await streamingRpcClient.ConnectAsync();
            await streamingRpcClient.SubscribeSignatureAsync(transactionHash.Result, (_, data) =>
            {
                if (data.Value.Error == null)
                {
                    var balance = rpcClient.GetBalance(publicKey);
                    Console.WriteLine($"Balance: {balance.Result.Value}");

                    var memoInstruction = MemoProgram.NewMemoV2("Hello Solana World, using Solnet Ch :)");
                    var recentHash = rpcClient.GetLatestBlockHash();

                    var tx = new TransactionBuilder().AddInstruction(memoInstruction)
                        .SetFeePayer(account)
                        .SetRecentBlockHash(recentHash.Result.Value.Blockhash)
                        .Build(account);

                    var txHash = rpcClient.SendTransaction(tx);
                    Console.WriteLine($"TxHash: {txHash.Result}");
                }
                else
                {
                    Console.WriteLine($"Transaction error: {data.Value.Error.Type}");
                    Console.WriteLine(
                        "Please check the reason at explorer.solana.com with the above transaction hash.");
                }
            });
        }
    }
}