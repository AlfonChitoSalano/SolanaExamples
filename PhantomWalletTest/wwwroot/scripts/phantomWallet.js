(function () {
    // Global export
    window.solanaPhantom = {
        //https://github.com/phantom-labs/sandbox/blob/b57fdd0e65ce4f01290141a01e33d17fd2f539b9/src/App.tsx#L263
        login: async function (dotNetInstance) {
            if (!('phantom' in window)) {
                window.open('https://phantom.app/', '_blank');
                return;
            }

            let provider = getProvider();

            if (!(provider?.isPhantom)) {
                console.log("wallet not phantom");
                return;
            }

            provider.connect({ onlyIfTrusted: true })
                .then(({ publicKey }) => {
                    console.log("wallet is already connected");
                    dotNetInstance.invokeMethodAsync('OnConnected', publicKey.toString());
                })
                .catch(() => {
                    console.log("fail connecting");
                });

            if (provider.isConnected) {
                return;
            }

            try {
                console.log("connecting to wallet main");
                let resp = await provider.connect();
                let publicKey = resp.publicKey.toString();
                dotNetInstance.invokeMethodAsync('OnConnected', publicKey);
            } catch (error) {
                console.log("error log.");
                console.log(error);
            }
        },

        signOut: async function (dotNetInstance) {
            let provider = getProvider();

            if (!(provider?.isPhantom)) {
                console.log("wallet not phantom");
                return;
            }

            await provider.disconnect();
            console.log("wallet disconnected.");

            if (provider.isConnected) {
                console.log("wallet is not disconnected well.");
                return;
            }

            dotNetInstance.invokeMethodAsync('OnDisConnected');
        }
    };

    function getProvider() {
        return window.phantom?.solana;
    }
})();