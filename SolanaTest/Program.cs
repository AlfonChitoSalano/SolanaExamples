using SolanaTest;
using Solnet.Rpc;

const Cluster currentCluster = Cluster.DevNet;
const string mnemonicWords =
    "taxi clerk disease box emerge airport loud waste attitude film army tray " +
    "father deal onion eight catalog surface unit card window walnut wealth peanut";

IExample example = new SendingTransactionExample(currentCluster, mnemonicWords);
await example.RunAsync();