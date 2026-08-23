(function () {
    function loadScript(src) {
        return new Promise((resolve, reject) => {
            const existing = document.querySelector(`script[src="${src}"]`);
            if (existing) {
                if (existing.dataset.loaded === "true") resolve();
                else existing.addEventListener("load", resolve, { once: true });
                return;
            }
            const script = document.createElement("script");
            script.src = src;
            script.async = true;
            script.defer = true;
            script.onload = () => { script.dataset.loaded = "true"; resolve(); };
            script.onerror = reject;
            document.head.appendChild(script);
        });
    }

    async function authorize(clientId) {
        await loadScript("https://accounts.google.com/gsi/client");
        return new Promise((resolve, reject) => {
            const client = google.accounts.oauth2.initTokenClient({
                client_id: clientId,
                scope: "openid email profile https://www.googleapis.com/auth/drive.file",
                callback: response => response.error ? reject(new Error(response.error_description || response.error)) : resolve(response.access_token),
                error_callback: error => reject(new Error(error.message || "Google authorization was cancelled."))
            });
            client.requestAccessToken({ prompt: "consent" });
        });
    }

    function getProjectNumber(clientId) {
        const match = /^(\d+)-/.exec(clientId);
        if (!match) throw new Error("The Google OAuth client ID does not contain a valid Cloud project number.");
        return match[1];
    }

    async function pick(accessToken, apiKey, projectNumber) {
        await loadScript("https://apis.google.com/js/api.js");
        await new Promise((resolve, reject) => gapi.load("picker", { callback: resolve, onerror: reject }));
        return new Promise((resolve, reject) => {
            const view = new google.picker.DocsView(google.picker.ViewId.SPREADSHEETS)
                .setIncludeFolders(false)
                .setSelectFolderEnabled(false);
            const picker = new google.picker.PickerBuilder()
                .setDeveloperKey(apiKey)
                .setOAuthToken(accessToken)
                .setAppId(projectNumber)
                .addView(view)
                .setCallback(data => {
                    if (data.action === google.picker.Action.PICKED) resolve(data.docs[0].id);
                    if (data.action === google.picker.Action.CANCEL) reject(new Error("Spreadsheet selection was cancelled."));
                })
                .build();
            picker.setVisible(true);
        });
    }

    window.f3RegionEditor = {
        authorizeAndPick: async function (clientId, apiKey) {
            if (!clientId || !apiKey) throw new Error("Google Region Editor credentials are not configured.");
            const accessToken = await authorize(clientId);
            const fileId = await pick(accessToken, apiKey, getProjectNumber(clientId));
            return { accessToken, fileId };
        },
        replaceUrl: function (url) { window.history.replaceState(null, "", url); }
    };
})();
