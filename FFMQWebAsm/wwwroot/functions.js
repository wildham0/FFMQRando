//const localforage = require("./localforage.min");
//import localforage from 'localforage.min';

async function downloadFileFromStream(fileName, contentStreamReference) {
    const arrayBuffer = await contentStreamReference.arrayBuffer();
    const blob = new Blob([arrayBuffer]);

    const url = URL.createObjectURL(blob);

    triggerFileDownload(fileName, url);

    URL.revokeObjectURL(url);
}

function triggerFileDownload(fileName, url) {
    const anchorElement = document.createElement('a');
    anchorElement.href = url;

    if (fileName) {
        anchorElement.download = fileName;
    }

    anchorElement.click();
    anchorElement.remove();
}

async function loadSavedRom() {
    localforage.config();
    value = await localforage.getItem('SaveRom');
    return value;
}

async function saveSavedRom(romToSave) {
    localforage.config();
    localforage.setItem('SaveRom', romToSave).then(function (value) { return true; }).catch(function (err) { return false; });
}