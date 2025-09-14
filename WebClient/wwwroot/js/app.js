window.initJalaliDatePicker = () => {
    jalaliDatepicker.startWatch({
        date: true,
        time: false,
        persianDigits: true,
        autoShow: true,
        autoHide: true,
        separatorChars: {
            date: '/',
            time: ':',
            between: ' '
        }
    });
};
window.addEventListener('DOMContentLoaded', () => {
    jalaliDatepicker.startWatch();
})
window.openModal = (id) => {
    const modal = document.getElementById(id);
    if (modal) {
        modal.show();
        const input = modal.querySelector('[data-jdp]');
        if (input) {
            jalaliDatepicker.startWatch(input, {
                container: document.querySelector('dialog'),
                autoShow: true,
                autoHide: true,
                persianDigits: true
            });
        }
    }
}
window.closeModal = (id) => {
    const modal = document.getElementById(id);
    if (modal) {
        modal.close();
    }
}

// === toggle sidebar ===
window.toggleSidebar = () => {
    const wrapper = document.getElementById("layoutWrapper");
    if (wrapper) {
        wrapper.classList.toggle("sidebar-collapsed");
    }
}

window.downloadFileFromBytes = (fileName, byteArray) => {
    const blob = new Blob([new Uint8Array(byteArray)], { type: "application/octet-stream" });
    const url = URL.createObjectURL(blob);

    const link = document.createElement("a");
    link.href = url;
    link.download = fileName;
    document.body.appendChild(link);
    link.click();

    URL.revokeObjectURL(url);
    link.remove();
};
