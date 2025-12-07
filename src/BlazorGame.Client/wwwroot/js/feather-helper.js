window.FeatherIcons = {
    replace: function() {
        if (typeof feather !== 'undefined') {
            feather.replace({
                'stroke-width': 2,
                'width': 20,
                'height': 20
            });
        }
    },
    
    replaceDelayed: function(delay = 50) {
        setTimeout(() => {
            this.replace();
        }, delay);
    }
};

window.DownloadHelpers = {
    saveBase64File: function (fileName, contentType, base64Data) {
        try {
            const byteCharacters = atob(base64Data);
            const byteNumbers = new Array(byteCharacters.length);

            for (let i = 0; i < byteCharacters.length; i++) {
                byteNumbers[i] = byteCharacters.charCodeAt(i);
            }

            const byteArray = new Uint8Array(byteNumbers);
            const blob = new Blob([byteArray], { type: contentType || 'application/octet-stream' });
            const url = URL.createObjectURL(blob);
            const anchor = document.createElement('a');

            anchor.href = url;
            anchor.download = fileName || 'export.json';
            document.body.appendChild(anchor);
            anchor.click();
            document.body.removeChild(anchor);
            URL.revokeObjectURL(url);
        } catch (error) {
            console.error('Erreur lors de la sauvegarde du fichier', error);
        }
    }
};