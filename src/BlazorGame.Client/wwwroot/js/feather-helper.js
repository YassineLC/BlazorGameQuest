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